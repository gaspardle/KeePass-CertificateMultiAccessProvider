using System.Runtime.ConstrainedExecution;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using CertificateMultiAccessProvider.CertStore;
using KeePass.App;
using KeePass.UI;
using KeePassLib.Cryptography;
using KeePassLib.Security;
using KeePassLib.Serialization;
using KeePassLib.Utility;
using static CertificateMultiAccessProvider.CryptoHelpers;

namespace CertificateMultiAccessProvider;

public partial class KeyManagementForm : Form
{
    private ProtectedBinary _secretKey;
    private readonly CertProviderConfiguration _certProviderConfigCopy;
    private readonly CertProviderConfiguration _certProviderConfig;
    private readonly bool _isKeyCreation;
    private readonly bool _allowNotSecureRemoval = false;
    private readonly CertificateMultiAccessProvider _provider;
    private string _keyFileDefaultLocation;

    public X509Certificate2 SelectedCertificate { get; set; }


    public KeyManagementForm(string keyFileDefaultLocation, CertProviderConfiguration certProviderConfig, CertificateMultiAccessProvider provider, bool keyCreation = false)
    {
        InitializeComponent();

        _certProviderConfigCopy = certProviderConfig.Copy();
        _certProviderConfig = certProviderConfig;
        _isKeyCreation = keyCreation;
        _allowNotSecureRemoval = provider.Settings.AllowUnsecureDeletion;
        _keyFileDefaultLocation = keyFileDefaultLocation;
        _provider = provider;

        listViewCertificate.Columns.Add("Subject");
        listViewCertificate.Columns.Add("Issuer");
        listViewCertificate.Columns.Add("Thumbprint");

        if (keyCreation)
        {
            _secretKey = new ProtectedBinary(true, CryptoRandom.Instance.GetRandomBytes(32));
            if ((_secretKey == null) || (_secretKey.Length != 32))
            {
                throw new SecurityException();
            }
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        GlobalWindowManager.AddWindow(this);

        Icon = AppIcons.Default;

        RefreshList();
    }

    private void RefreshList()
    {
        if (_certProviderConfigCopy.AllowedCertificates.Count == 0) return;

        listViewCertificate.BeginUpdate();
        listViewCertificate.Items.Clear();
        listViewCertificate.ListViewItemSorter = null;

        foreach (var p in _certProviderConfigCopy.AllowedCertificates)
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
            var (cert, certProvType) = CryptoHelpers.ShowCertificateSelection("Select a current certificate to confirm the addition of a new certificate.", _certProviderConfigCopy, _provider.Settings);
            if (cert == null)
            {
                MessageService.ShowWarning("You must select a certificate.");
                return false;
            }

            var certInfo = _certProviderConfigCopy.AllowedCertificates.Where(p => p.Thumbprint == cert.Thumbprint).FirstOrDefault();

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
        var certificate = CertStoreCAPI.SelectCertificate("Select the certificate to add as an authentication method.", hwnd: this.Handle);
        if (certificate == null) return;

        if (_certProviderConfigCopy.AllowedCertificates.Any(p => p.Thumbprint == certificate.Thumbprint))
        {
            MessageBox.Show("Certificate is already present.");
            return;
        }

        RSA rsa;
        if ((rsa = certificate.GetRSAPublicKey()) != null)
        {
            if (rsa.KeySize < 2048)
            {
                MessageService.ShowWarning("Certificates which the key size are lesser than 2048 bits are not supported.");
                return;
            }
        }

        if (!GetSecretKey())
        {
            return;
        }

        Add(AllowedCertificateRSA.Create(certificate, AllowedRSAEncryptionPadding.GetFromName(_provider.Settings.DefaultRsaPaddingName)));
    }

    private void ButtonAddPkcs11_Click(object sender, EventArgs e)
    {
        var form = new Pkcs11CertificateSelectionForm(null, _provider.Settings, _certProviderConfigCopy.AllowedCertificates.Select(k => k.Thumbprint));
        var dialogResult = form.ShowDialog();

        if (dialogResult == DialogResult.OK)
        {
            var pkcsCertificate = form.SelectedCertificate;
            if (pkcsCertificate == null) return;

            var certificate = pkcsCertificate.Info.ParsedCertificate;
            if (_certProviderConfigCopy.AllowedCertificates.Any(p => p.Thumbprint == certificate.Thumbprint))
            {
                MessageBox.Show("Certificate is already present.");
                return;
            }
            Add(AllowedCertificateRSA.Create(certificate, AllowedRSAEncryptionPadding.GetFromName(_provider.Settings.DefaultRsaPaddingName)));
        }
    }

    private void AddCertInternalButton_Click(object sender, EventArgs e)
    {
        var form = new PasswordDialog("", true);
        var dialogResult = form.ShowDialog();
        var passphrase = form.Passphrase;
        var subject = form.Subject;

        if (passphrase == null || passphrase.Length == 0)
        {
            MessageService.ShowWarning("The password is not valid.");
            return;
        }

        if (dialogResult == DialogResult.OK && passphrase.Length > 0)
        {
            var newCertificate = CryptoHelpers.GenerateSelfSignedCertificate(subject);
            Add(AllowedCertificateRSAInternal.Create(newCertificate, passphrase));
        }
    }

    private void Add(AllowedCertificate certInfo)
    {
        if (!GetSecretKey())
        {
            return;
        }
        try
        {
            CryptoHelpers.SetSecretKeyFromNewUserKey(certInfo, _secretKey);
            _certProviderConfigCopy.AllowedCertificates.Add(certInfo);
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
        if (_certProviderConfigCopy.AllowedCertificates.Count == 0)
        {
            MessageService.ShowWarning("You must add at least one certificate.");
            return;
        }

        if (!_isKeyCreation)
        {
            MessageService.ShowWarning("Changes will be committed when the database is saved in KeePass.");
        }

        _certProviderConfig.AllowedCertificates.Clear();
        _certProviderConfig.AllowedCertificates.AddRange(_certProviderConfigCopy.AllowedCertificates);

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
        if (!_isKeyCreation && !_allowNotSecureRemoval)
        {
            MessageBox.Show("To remove a certificate, go to File -> Change master key", "CertificateMultiAccess");
            return;
        }
        if (listViewCertificate.SelectedIndices.Count == 0)
        {
            MessageBox.Show("No certificate selected", "CertificateMultiAccess");
            return;
        }

        var item = (AllowedCertificate)listViewCertificate.SelectedItems[0]?.Tag;
        if (item != null)
        {
            var cert = item.ReadCertificate();
            var result = MessageBox.Show($"Are you sure you want to remove from the access list this certificate?" +
                $"\n\n - Subject: {cert.Subject}\n - Thumbprint: {cert.Thumbprint}", "CertificateMultiAccess",
                MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                _certProviderConfigCopy.AllowedCertificates.Remove(item);
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

            CertificateMultiAccessProvider.ExportConfig(sfd.FileName, _certProviderConfigCopy);
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
                    var newCertInfo = certInfo.Clone();
                    if (!_certProviderConfigCopy.AllowedCertificates.Any(c => c.Thumbprint == newCertInfo.Thumbprint))
                    {
                        CryptoHelpers.SetSecretKeyFromNewUserKey(newCertInfo, _secretKey);
                        _certProviderConfigCopy.AllowedCertificates.Add(newCertInfo);
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
        MessageBox.Show("The exported file can be used as a key file for this database.", "Export key file", MessageBoxButtons.OK, MessageBoxIcon.Warning);

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
        var form = new PinDialog("(Experimental) Set Provider Key", "Set custom value for the key used in the database composite key, instead of a random value. (32 bytes hex)");
        var dialogResult = form.ShowDialog();
        ;
        if (form.Pin != null && dialogResult == DialogResult.OK)
        {
            var newKeyHex = form.Pin.ReadString();
            var newKey = MemUtil.HexStringToByteArray(newKeyHex);
            _secretKey = new ProtectedBinary(true, newKey);
            MemUtil.ZeroByteArray(newKey);
        }
    }


}

