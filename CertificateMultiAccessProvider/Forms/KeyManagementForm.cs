using System.Runtime.ConstrainedExecution;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using CertificateMultiAccessProvider.CertProvider;
using CertificateMultiAccessProvider.CertStore;
using KeePass.App;
using KeePass.UI;
using KeePassLib;
using KeePassLib.Cryptography;
using KeePassLib.Security;
using KeePassLib.Serialization;
using KeePassLib.Utility;
using static CertificateMultiAccessProvider.CryptoHelpers;

namespace CertificateMultiAccessProvider;

public partial class KeyManagementForm : Form
{
    private ProtectedBinary _secretKey;
    //private readonly CertProviderConfiguration _certProviderConfig;
    private readonly bool _isKeyCreation = false;
    private readonly bool _allowInsecureRemoval = false;
    private readonly CertificateMultiAccessProvider _provider;
    private string _keyFileDefaultLocation;
    private readonly PwDatabase _database;
    private readonly string _dbpath;

    public CertProviderConfiguration CertProviderConfig { get; }


    public KeyManagementForm(CertificateMultiAccessProvider provider, CertProviderConfiguration certProviderConfig, PwDatabase database)
    {
        InitializeComponent();

        _provider = provider;
        CertProviderConfig = certProviderConfig.Copy();
        //_allowNotSecureRemoval = provider.Settings.AllowUnsecureDeletion;

        _database = database;

    }
    public KeyManagementForm(CertificateMultiAccessProvider provider, CertProviderConfiguration certProviderConfig, string dbpath)
    {
        InitializeComponent();

        _provider = provider;
        CertProviderConfig = certProviderConfig.Copy();
        //_allowNotSecureRemoval = provider.Settings.AllowUnsecureDeletion;

        _dbpath = dbpath;
        _isKeyCreation = true;
        _secretKey = new ProtectedBinary(true, CryptoRandom.Instance.GetRandomBytes(32));
        if ((_secretKey == null) || (_secretKey.Length != 32))
        {
            throw new SecurityException();
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        GlobalWindowManager.AddWindow(this);

        Icon = AppIcons.Default;

        listViewCertificate.Columns.Add("Subject");
        listViewCertificate.Columns.Add("Issuer");
        listViewCertificate.Columns.Add("Thumbprint");

        RefreshList();
    }

    private void RefreshList()
    {
        listViewCertificate.BeginUpdate();
        listViewCertificate.Items.Clear();
        listViewCertificate.ListViewItemSorter = null;

        foreach (var p in CertProviderConfig.AllowedCertificates)
        {
            var cert = p.ReadCertificate();

            var subject = cert.GetNameInfo(X509NameType.SimpleName, forIssuer: false);
            var issuer = cert.GetNameInfo(X509NameType.SimpleName, forIssuer: true);
            var item = new ListViewItem(subject);
            item.SubItems.AddRange(new List<string>() { issuer, cert.Thumbprint }.ToArray());
            item.Tag = p;
            listViewCertificate.Items.Add(item);
        }

        //listView1.ListViewItemSorter = sorter;
        listViewCertificate.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        listViewCertificate.EndUpdate();

    }
    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);

        GlobalWindowManager.RemoveWindow(this);
    }


    private bool GetSecretKey()
    {
        if (_secretKey == null)
        {
            var (cert, certProvType) = CryptoHelpers.ShowCertificateSelection("Select a current certificate to confirm the addition of a new certificate.", CertProviderConfig, _provider.Settings);
            if (cert == null)
            {
                MessageService.ShowWarning("You must select a certificate.");
                return false;
            }

            var certInfo = CertProviderConfig.AllowedCertificates.Where(p => p.Thumbprint == cert.Thumbprint).FirstOrDefault();

            this.Enabled = false;
            try
            {
                _secretKey = CryptoHelpers.DecryptSecretFromConfig(certInfo, certProvType, _provider.Settings);
            }
            catch (Exception ex)
            {
                MessageService.ShowWarning(ex.Message);
                return false;
            }
            finally
            {
                this.Enabled = true;
            }

            if (_secretKey == null)
            {
                MessageService.ShowWarning("Error while retrieving key.");
                return false;
            }

        }
        return true;
    }

    private void AddCertFromStoreButton_Click(object sender, EventArgs e)
    {
        var certificate = CertStoreCAPI.SelectCertificate("Please select the certificate that you would like to add.", hwnd: this.Handle);
        if (certificate == null) return;

        if (CertProviderConfig.AllowedCertificates.Any(p => p.Thumbprint == certificate.Thumbprint))
        {
            MessageBox.Show("Certificate is already present.");
            return;
        }

        AddCertificate(certificate);
    }

    private void ButtonAddPkcs11_Click(object sender, EventArgs e)
    {
        var form = new Pkcs11CertificateSelectionForm(null, _provider.Settings, CertProviderConfig.AllowedCertificates.Select(k => k.Thumbprint));
        var dialogResult = form.ShowDialog();

        if (dialogResult == DialogResult.OK)
        {
            var pkcsCertificate = form.SelectedCertificate;
            if (pkcsCertificate == null) return;

            var certificate = pkcsCertificate.Info.ParsedCertificate;
            if (CertProviderConfig.AllowedCertificates.Any(p => p.Thumbprint == certificate.Thumbprint))
            {
                MessageBox.Show("Certificate is already present.");
                return;
            }
            AddCertificate(certificate);
        }
    }

    private void AddCertificate(X509Certificate2 certificate)
    {
        switch (certificate.GetKeyAlgorithm())
        {
            case "1.2.840.113549.1.1.1": //rsa
                if ((certificate.GetRSAPublicKey()) is RSA rsa)
                {
                    if (rsa.KeySize < 2048)
                    {
                        MessageService.ShowWarning("Certificates with a key size lesser than 2048 bits are not supported.");
                        return;
                    }
                }
                AddAllowedCertificateToConfig(AllowedCertificateRSA.Create(certificate, AllowedRSAEncryptionPadding.GetFromName(_provider.Settings.DefaultRsaPaddingName)));
                break;
            case "1.2.840.10045.2.1": //ecc
                MessageService.ShowWarning("EC certificate are not supported");
                break;
            default:
                MessageService.ShowWarning("Certificate's key type not supported");
                break;
        }
    }

    private void AddAllowedCertificateToConfig(AllowedCertificate certInfo)
    {
        if (!GetSecretKey())
        {
            return;
        }
        try
        {
            CryptoHelpers.SetSecretKeyFromNewUserKey(certInfo, _secretKey);
            CertProviderConfig.AllowedCertificates.Add(certInfo);
        }
        catch (Exception ex)
        {
            MessageService.ShowWarning(ex.Message);
            return;
        }

        RefreshList();
    }

    private void ButtonOk_Click(object sender, EventArgs e)
    {
        if (CertProviderConfig.AllowedCertificates.Count == 0)
        {
            MessageService.ShowWarning("You must add at least one certificate.");
            return;
        }

        if (!_isKeyCreation)
        {
            MessageService.ShowWarning("Changes will be committed when the database is saved in KeePass.");
        }

        if (_isKeyCreation)
        {
            _provider.AddTemporaryCreatedConfig(_dbpath, CertProviderConfig);
        }
        else
        {
            _provider.SaveCertificatesConfig(CertProviderConfig, _database);
        }

        _provider.SetModified();
        DialogResult = DialogResult.OK;
    }

    private void ButtonCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void KeyCreationForm_Load(object sender, EventArgs e)
    {

    }

    private void DisplayCertificateDetails_Click(object sender, EventArgs e)
    {
        if (listViewCertificate.SelectedIndices.Count == 0)
        {
            MessageBox.Show("No certificate selected", "CertificateMultiAccess");
            return;
        }

        var certificateInfo = (AllowedCertificate)listViewCertificate.SelectedItems[0]?.Tag;
        if (certificateInfo != null)
        {
            var cert = certificateInfo.ReadCertificate();

            if (certificateInfo is AllowedCertificateRSA)
            {
                var certCapi = CryptoHelpers.LoadCertificateFromStore(cert);
                if (certCapi != null) { cert = certCapi; }
            }

            X509Certificate2UI.DisplayCertificate(cert, this.Handle);
        }
    }

    private void RemoveCertButton_Click(object sender, EventArgs e)
    {
        if (!_isKeyCreation && !_allowInsecureRemoval)
        {
            MessageBox.Show("To remove a certificate or disable this plugin for this database, go to File -> Change master key", "CertificateMultiAccess");
            return;
        }

        if (CertProviderConfig.AllowedCertificates.Count <= 1)
        {
            MessageService.ShowWarning("You must have at least one certificate in the list.");
            return;
        }

        if (listViewCertificate.SelectedIndices.Count == 0)
        {
            MessageBox.Show("No certificate selected", "CertificateMultiAccess");
            return;
        }

        var certificateInfo = (AllowedCertificate)listViewCertificate.SelectedItems[0]?.Tag;
        if (certificateInfo != null)
        {
            var certificate = certificateInfo.ReadCertificate();
            var result = MessageBox.Show($"Are you sure you want to remove from the access list this certificate?" +
                $"\n\n - Subject: {certificate.Subject}\n - Thumbprint: {certificate.Thumbprint}", "CertificateMultiAccess",
                MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                CertProviderConfig.AllowedCertificates.RemoveAll(c => c.Thumbprint == certificate.Thumbprint);
                RefreshList();
            }
        }
    }

    private void ListViewCertificate_MouseDoubleClick(object sender, MouseEventArgs e)
    {
        var certificateInfo = (AllowedCertificate)listViewCertificate.SelectedItems[0]?.Tag;
        if (certificateInfo != null)
        {
            DisplayCertificateDetails_Click(sender, e);
        }
    }
    private void ExportButton_Click(object sender, EventArgs e)
    {
        var sfd = new SaveFileDialogEx("Export CertificateMultiAccess Config file")
        {
            Filter = $"CertificateMultiAccess Config files (*{CertificateMultiAccessProvider.DefaultKeyExtension})|*{CertificateMultiAccessProvider.DefaultKeyExtension}|All files (*.*)|*.*",
            FileName = _keyFileDefaultLocation
        };
        if (sfd.ShowDialog() == DialogResult.OK)
        {
            _keyFileDefaultLocation = sfd.FileName;

            CertificateMultiAccessProvider.ExportConfig(sfd.FileName, CertProviderConfig);
        }
    }
    private void ImportButton_Click(object sender, EventArgs e)
    {
        var sfd = new OpenFileDialogEx("Import CertificateMultiAccess Config file")
        {
            Filter = $"CertificateMultiAccess Config files (*{CertificateMultiAccessProvider.DefaultKeyExtension})|*{CertificateMultiAccessProvider.DefaultKeyExtension}|All files (*.*)|*.*",
            FileName = _keyFileDefaultLocation
        };

        try
        {
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                if (!GetSecretKey())
                {
                    return;
                }

                var count = 0;

                var config = CertificateMultiAccessProvider.ImportConfig(sfd.FileName);
                foreach (var certInfo in config.AllowedCertificates)
                {
                    var newCertInfo = certInfo.Copy();
                    if (!CertProviderConfig.AllowedCertificates.Any(c => c.Thumbprint == newCertInfo.Thumbprint))
                    {
                        CryptoHelpers.SetSecretKeyFromNewUserKey(newCertInfo, _secretKey);
                        CertProviderConfig.AllowedCertificates.Add(newCertInfo);
                        count++;
                    }
                }
                RefreshList();
                MessageService.ShowInfo($"{count} entries imported.");
            }
        }
        catch (Exception ex)
        {
            MessageService.ShowWarning(ex.Message);
            return;
        }
    }

    private void ExportBackingKey()
    {
        if (!_provider.Settings.AllowKeyExport)
        {
            MessageBox.Show("This feature is disabled.");
            return;
        }

        MessageBox.Show("The exported file can be used as a key file for this database in place of a certificate. Store safely.", "Export key file", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        var sfd = new SaveFileDialogEx("Export Key file")
        {
            Filter = $"Key files (*.keyx,*.key)|*.keyx;*.key|All files (*.*)|*.*",
            FileName = _keyFileDefaultLocation
        };

        if (sfd.ShowDialog() == DialogResult.OK)
        {
            GetSecretKey();

            var data = CryptoUtil.HashSha256(_secretKey.ReadData());

            var kf = KeePassLib.Keys.KfxFile.Create(0, data, null);
            var ioc = IOConnectionInfo.FromPath(sfd.FileName);
            using (var s = IOConnection.OpenWrite(ioc))
            {
                kf.Save(s);
            }

            MemUtil.ZeroByteArray(data);
        }
    }

    private void ButtonMore_Click(object sender, EventArgs e)
    {
        contextMenuStrip1.Show(buttonMore, new Point(0, buttonMore.Height));
    }

    private void ToolStripMenuItemExportKey_Click(object sender, EventArgs e)
    {
        ExportBackingKey();
    }

    private void ToolStripMenuItemCustomKey_Click(object sender, EventArgs e)
    {
        if (_isKeyCreation)
        {
            var form = new PinDialog("(danger) Set Provider Key", "Set custom value for the key used in the database composite key, instead of a random value. (32 bytes hex)");
            var dialogResult = form.ShowDialog();

            if (form.Pin != null && dialogResult == DialogResult.OK)
            {
                var newKeyHex = form.Pin.ReadString();
                var newKey = MemUtil.HexStringToByteArray(newKeyHex);
                _secretKey = new ProtectedBinary(true, newKey);
                MemUtil.ZeroByteArray(newKey);
            }
        }
    }

    private void toolStripMenuItemRemovedConfig_Click(object sender, EventArgs e)
    {
        if (!_provider.Settings.AllowConfigurationRemoval)
        {
            MessageBox.Show("This feature is disabled.");
            return;

        }
        if (_database != null)
        {
            var result = MessageBox.Show($"(danger) Are you sure you want to remove the plugin configuration from this database?" +
               $"\n\n{_database.IOConnectionInfo.Path}", "CertificateMultiAccess",
               MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                _provider.RemoveCertificatesConfig(_database);
                DialogResult = DialogResult.OK;
            }
        }
    }
}

