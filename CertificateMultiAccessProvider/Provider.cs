using System.Security.Cryptography.X509Certificates;
using CertificateMultiAccessProvider.CertStore;
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

    public CertProviderConfiguration _certProviderConfig;
    public Settings Settings { get; private set; }

    private readonly IPluginHost _host;

    public CertificateMultiAccessProvider(IPluginHost host)
    {
        this._host = host;
        this.Settings = new Settings(host.CustomConfig);
    }

    public override string Name
    {
        get { return "CertificateMultiAccess Provider"; }
    }

    public override bool SecureDesktopCompatible => true;

    public override bool GetKeyMightShowGui => true;

    public override byte[] GetKey(KeyProviderQueryContext ctx)
    {
        if (ctx == null) throw new ArgumentNullException(nameof(ctx));

        // The key file is expected to be next to the database by default:
        var keyFilePath = UrlUtil.StripExtension(ctx.DatabasePath) + DefaultKeyExtension;

        _certProviderConfig = new CertProviderConfiguration();

        if (ctx.CreatingNewKey)
        {
            using var form = new KeyManagementForm(keyFilePath, _certProviderConfig, this, keyCreation: true);
            var res = form.ShowDialog();
            if (res != DialogResult.OK)
            {
                return null;
            }
        }
        else
        {
            // Load allowed keys from header data
            var dbHeader = PwDatabase.LoadHeader(ctx.DatabaseIOInfo);
            ReadCertificatesConfig(dbHeader);
        }


        X509Certificate2 cert = null;
        var type = CertProvType.CAPI;

        //capi
        if (Settings.UseCAPIByDefault && _certProviderConfig.AllowedCertificates.Any(k => k is AllowedCertificateRSA))
        {
            cert = CertStoreCAPI.SelectCertificate("Select a certificate.", _certProviderConfig.AllowedCertificates.Select(k => k.Thumbprint), GlobalWindowManager.TopWindow?.Handle);
            type = CertProvType.CAPI;
        }

        if (cert == null)
        {
            (cert, type) = CryptoHelpers.ShowCertificateSelection("Select a current valid certificate.", _certProviderConfig, Settings);
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
        catch (Exception)
        {
            throw;
        }
        finally
        {
            topwindow.Enabled = true;
        }

        if (secretKey == null) return null;
        return secretKey.ReadData();
    }

    internal bool ReadCertificatesConfig(PwDatabase database)
    {
        var compressedBytes = database.PublicCustomData.GetByteArray($"{ConfigPrefix}.config");

        if (compressedBytes == null) return false;


        using var memoryStream = new MemoryStream(compressedBytes);
        var configBytes = MemUtil.Decompress(memoryStream.ToArray());

        _certProviderConfig = XmlUtilEx.Deserialize<CertProviderConfiguration>(new MemoryStream(configBytes));
        return true;
    }

    internal void SaveCertificatesConfig(PwDatabase database)
    {
        using var memoryStream = new MemoryStream();
        XmlUtilEx.Serialize(memoryStream, _certProviderConfig);

        var configBytes = memoryStream.ToArray();
        var compressedBytes = MemUtil.Compress(configBytes);

        database.PublicCustomData.SetByteArray($"{ConfigPrefix}.config", compressedBytes);

        var hash = BitConverter.ToString(CryptoUtil.HashSha256(configBytes)).Replace("-", string.Empty);
        database.CustomData.Set("certmultiaccess.config.hash", hash);
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
}
