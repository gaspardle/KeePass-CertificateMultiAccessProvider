using System.Security.Cryptography.X509Certificates;
using CertificateMultiAccessProvider.CertProvider;
using CertificateMultiAccessProvider.CertStore;
using KeePass.Forms;
using KeePass.Plugins;
using KeePass.UI;
using KeePassLib;
using KeePassLib.Cryptography;
using KeePassLib.Keys;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace CertificateMultiAccessProvider;

public enum CertProvType
{
    None,
    CAPI,
    InternalCertificate,
    Pkcs11
}

public sealed class CertificateMultiAccessProvider : KeyProvider
{
    public const string DefaultKeyExtension = ".mcspkey";
    public const string ConfigPrefix = "certmultiaccess";

    public Settings Settings { get; private set; }

    private readonly IPluginHost _host;

    private readonly Dictionary<string, CertProviderConfiguration> _temporaryCreatedConfigs = new();
    private readonly HashSet<string> _openedDb = new();

    public CertificateMultiAccessProvider(IPluginHost host)
    {
        this._host = host;
        this.Settings = new Settings(host.CustomConfig);
    }

    public override string Name
    {
        get { return "CertificateMultiAccess Provider"; }
    }

    public override bool SecureDesktopCompatible => false;

    public override bool GetKeyMightShowGui => true;

    public override byte[] GetKey(KeyProviderQueryContext ctx)
    {
        //ctx.IsOnSecureDesktop;

        if (ctx == null) throw new ArgumentNullException(nameof(ctx));

        CertProviderConfiguration _certProviderConfig;

        if (ctx.CreatingNewKey)
        {
            using var form = new KeyManagementForm(this, new CertProviderConfiguration(), ctx.DatabasePath);
            var res = form.ShowDialog();
            if (res != DialogResult.OK)
            {
                return null;
            }
            _certProviderConfig = form.CertProviderConfig;
        }
        else
        {
            // Load allowed keys from header data
            _certProviderConfig = ReadCertificatesConfig(PwDatabase.LoadHeader(ctx.DatabaseIOInfo));
        }


        X509Certificate2 cert = null;
        var type = CertProvType.CAPI;

        //capi
        if (Settings.UseCAPIByDefault && _certProviderConfig.AllowedCertificates.Any(k => k is AllowedCertificateRSA))
        {
            cert = CertStoreCAPI.SelectCertificate("Please select the certificate that you would like to use to unlock the database.", _certProviderConfig.AllowedCertificates.Select(k => k.Thumbprint).Reverse(), GlobalWindowManager.TopWindow?.Handle);
            type = CertProvType.CAPI;
        }

        if (cert == null)
        {
            (cert, type) = CryptoHelpers.ShowCertificateSelection("Please select the certificate that you would like to use to unlock the database.", _certProviderConfig, Settings);
        }

        if (cert == null)
        {
            MessageService.ShowWarning("You must select a certificate.");
            return null;
        }

        AllowedCertificate certInfo = _certProviderConfig.AllowedCertificates.Where(p => p.Thumbprint == cert.Thumbprint).FirstOrDefault();

        if (certInfo == null)
        {
            MessageService.ShowWarning("This certificate is not the allowed list.");
            return null;
        }

        var topwindow = GlobalWindowManager.TopWindow;
        topwindow.Enabled = false;
        ProtectedBinary secretKey = null;
        try
        {
            secretKey = CryptoHelpers.DecryptSecretFromConfig(certInfo, type, Settings);
        }
        catch (Exception ex)
        {
            MessageService.ShowFatal(ex);
            return null;
        }
        finally
        {
            topwindow.Enabled = true;
        }

        if (secretKey == null) return null;

        _openedDb.Add(ctx.DatabasePath);
        return secretKey.ReadData();
    }

    internal CertProviderConfiguration ReadCertificatesConfig(PwDatabase database)
    {
        var compressedBytes = database.PublicCustomData.GetByteArray($"{ConfigPrefix}.config");
        if (compressedBytes == null) return null;

        try
        {
            using var memoryStream = new MemoryStream(compressedBytes);
            var configBytes = MemUtil.Decompress(memoryStream.ToArray());
            var certProviderConfig = XmlUtilEx.Deserialize<CertProviderConfiguration>(new MemoryStream(configBytes));
            return certProviderConfig;
        }
        catch (Exception ex)
        {
            MessageService.ShowFatal(ex);
            throw;
        }
    }

    internal void SaveCertificatesConfig(CertProviderConfiguration certProviderConfig, PwDatabase database)
    {
        using var memoryStream = new MemoryStream();
        try
        {
            XmlUtilEx.Serialize(memoryStream, certProviderConfig);

            var configBytes = memoryStream.ToArray();
            var compressedBytes = MemUtil.Compress(configBytes);

            database.PublicCustomData.SetByteArray($"{ConfigPrefix}.config", compressedBytes);

            var hash = BitConverter.ToString(CryptoUtil.HashSha256(configBytes)).Replace("-", string.Empty);
            database.CustomData.Set($"{ConfigPrefix}.config.hash", hash);
        }
        catch (Exception ex)
        {
            MessageService.ShowFatal(ex);
            return;
        }
    }

    internal void RemoveCertificatesConfig(PwDatabase database)
    {
        database.PublicCustomData.Remove($"{ConfigPrefix}.config");
        database.CustomData.Remove($"{ConfigPrefix}.config.hash");
    }

    public void SetModified()
    {
        this._host.MainWindow.UpdateUI(false, null, false, null, false, null, true);
    }

    internal bool ValidateConfig(PwDatabase database)
    {
        return true;
        /*var configBytes = database.PublicCustomData.GetByteArray($"{ConfigPrefix}.config");
        if (configBytes == null)
        {
            throw new Exception();
        }

        var saved_hash = database.CustomData.Get($"{ConfigPrefix}.config.hash");
        var actual_hash = BitConverter.ToString(CryptoUtil.HashSha256(configBytes)).Replace("-", string.Empty);
        if (actual_hash != saved_hash)
        {
            throw new Exception($"Can't verify the integrity of Certificate MultiAccess configuration. Hash value is not expected.");
        }*/
    }

    internal static CertProviderConfiguration ImportConfig(string path)
    {
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
        return XmlUtilEx.Deserialize<CertProviderConfiguration>(fs);
    }

    internal static void ExportConfig(string path, CertProviderConfiguration config)
    {
        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
        XmlUtilEx.Serialize(fs, config);
    }

    internal void OnFileClosed(string path, FileEventFlags flag)
    {
        _openedDb.Remove(path);
    }

    internal void OnDatabaseFileCreated(PwDatabase database)
    {
        var path = database.IOConnectionInfo.Path;
        if (_temporaryCreatedConfigs.ContainsKey(path))
        {
            SaveCertificatesConfig(_temporaryCreatedConfigs[path], database);
            _temporaryCreatedConfigs.Remove(path);
        }
    }

    internal void AddTemporaryCreatedConfig(string dbpath, CertProviderConfiguration certProviderConfig)
    {
        _temporaryCreatedConfigs[dbpath] = certProviderConfig;
    }

    internal bool IsDatabaseOpen(string path)
    {
        return _openedDb.Contains(path);
    }
}
