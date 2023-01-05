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

namespace CertificateMultiAccessProvider;

public partial class KeyManagementForm : Form
{
    private ProtectedBinary SecretKey;
    private readonly CertProviderConfig userPublicKeys;
    private readonly bool _isKeyCreation;
    private string _keyFileDefaultLocation;
    private readonly CertificateMultiAccessProvider _provider;
    private const bool AllowNotSecureRemoval = false;

    public X509Certificate2 SelectedCertificate { get; set; }


    public KeyManagementForm(string keyFileDefaultLocation, CertProviderConfig certProviderConfig, CertificateMultiAccessProvider provider, bool keyCreation = false)
    {
        InitializeComponent();

        userPublicKeys = certProviderConfig; //.Copy();
        _isKeyCreation = keyCreation;
        _keyFileDefaultLocation = keyFileDefaultLocation;
        _provider = provider;

        listViewCertificate.Columns.Add("Type");
        listViewCertificate.Columns.Add("Subject");
        listViewCertificate.Columns.Add("Issuer");
        listViewCertificate.Columns.Add("Thumbprint");

        if (keyCreation)
        {
            SecretKey = new ProtectedBinary(true, CryptoRandom.Instance.GetRandomBytes(32));
            if ((SecretKey == null) || (SecretKey.Length != 32))
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
        if (userPublicKeys.AllowedCertificates.Count == 0) return;

        listViewCertificate.BeginUpdate();
        listViewCertificate.Items.Clear();
        listViewCertificate.ListViewItemSorter = null;
        foreach (var p in userPublicKeys.AllowedCertificates)
        {
            var cert = p.ReadCertificate();

            var subject = cert.GetNameInfo(X509NameType.SimpleName, forIssuer: false);
            var issuer = cert.GetNameInfo(X509NameType.SimpleName, forIssuer: true);
            var item = new ListViewItem("type");
            item.SubItems.AddRange(new List<string>() { subject, issuer, cert.Thumbprint }.ToArray());
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
        if (SecretKey == null)
        {
            var (cert, certProvType) = CryptoHelpers.ShowCertificateSelection("Select a current valid certificate to complete the creation of a new certificate.", userPublicKeys);
            if (cert == null)
            {
                MessageService.ShowWarning("You must select a certificate.");
                return false;
            }

            var certInfo = userPublicKeys.AllowedCertificates.Where(p => p.Thumbprint == cert.Thumbprint).FirstOrDefault();

            this.Enabled = false;
            try
            {
                SecretKey = CryptoHelpers.DecryptSecretFromConfig(certInfo, certProvType);
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

            if (SecretKey == null)
            {
                MessageService.ShowWarning("Error while retrieving key.");
                return false;
            }

        }
        return true;
    }

    private void addCertFromStoreButton_Click(object sender, EventArgs e)
    {
        var certificate = CertStoreCAPI.SelectCertificate("Select the certificate to add as an authentication method.", hwnd: this.Handle);
        if (certificate == null) return;

        if (userPublicKeys.AllowedCertificates.Any(p => p.Thumbprint == certificate.Thumbprint))
        {
            MessageBox.Show("Certificate is already present.");
            return;
        }

        RSA rsa;
        if ((rsa = certificate.GetRSAPublicKey()) != null)
        {
            if (rsa.KeySize < 2048)
            {
                MessageService.ShowWarning("Certificate key size which are < 2048 not supported.");
                return;
            }
        }

        if (!GetSecretKey())
        {
            return;
        }

        Add(AllowedCertificateRSA.Create(certificate));
    }

    private void buttonAddPkcs11_Click(object sender, EventArgs e)
    {
        var form = new Pkcs11CertificateSelectionForm(null, userPublicKeys.AllowedCertificates.Select(k => k.Thumbprint));
        var dialogResult = form.ShowDialog();

        if (dialogResult == DialogResult.OK)
        {
            var pkcsCertificate = form.SelectedCertificate;
            if (pkcsCertificate == null) return;

            var certificate = pkcsCertificate.Info.ParsedCertificate;
            if (userPublicKeys.AllowedCertificates.Any(p => p.Thumbprint == certificate.Thumbprint))
            {
                MessageBox.Show("Certificate is already present.");
                return;
            }
            Add(AllowedCertificateRSA.Create(certificate));
        }
    }

    private void addCertInternalButton_Click(object sender, EventArgs e)
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
            var newCertificate = CryptoHelpers.buildSelfSignedServerCertificate(subject);
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
            CryptoHelpers.SetSecretKeyFromNewUserKey(certInfo, SecretKey);
            userPublicKeys.AllowedCertificates.Add(certInfo);
        }
        catch (Exception ex)
        {
            MessageService.ShowWarning(ex.Message);
            return;
        }

        RefreshList();
    }

    private void buttonOk_Click(object sender, EventArgs e)
    {
        if (userPublicKeys.AllowedCertificates.Count == 0)
        {
            MessageService.ShowWarning("You must add at least one certificate.");
            return;
        }

        MessageService.ShowWarning("Changes will be committed when the database is saved in KeePass.");

        //if (string.IsNullOrWhiteSpace(keyFileLocationTextBox.Text))
        //{
        //    MessageService.ShowWarning("File location required.");
        //    return;
        //}

        //bool overwrite = false;
        //if (File.Exists(keyFileLocationTextBox.Text))
        //{
        //    overwrite = MessageService.AskYesNo($"The file '{keyFileLocationTextBox.Text}' already exists. Overwrite?", "Warning");
        //    if (overwrite != true)
        //    {
        //        return;
        //    }
        //}

        //using (var fs = new FileStream(keyFileLocationTextBox.Text, overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.Read))
        //{
        //    XmlUtilEx.Serialize(fs, UserPublicKeys);
        //}

        _provider.SetModified();
        DialogResult = DialogResult.OK;
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

    private void displayCertificateDetails_Click(object sender, EventArgs e)
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

    private void removeCertButton_Click(object sender, EventArgs e)
    {
        if (!_isKeyCreation && !AllowNotSecureRemoval)
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
                userPublicKeys.AllowedCertificates.Remove(item);
                RefreshList();
            }
        }
    }

    private void listViewCertificate_MouseDoubleClick(object sender, MouseEventArgs e)
    {
        var certificateInfo = (AllowedCertificate)listViewCertificate.SelectedItems[0]?.Tag;
        if (certificateInfo != null)
        {
            displayCertificateDetails_Click(sender, e);
        }
    }
    private void exportButton_Click(object sender, EventArgs e)
    {
        var sfd = new SaveFileDialogEx("Export CertificateMultiAccess Config file")
        {
            Filter = $"CertificateMultiAccess Config files (*{CertificateMultiAccessProvider.DefaultKeyExtension})|*{CertificateMultiAccessProvider.DefaultKeyExtension}|All files (*.*)|*.*",
            FileName = _keyFileDefaultLocation
        };
        if (sfd.ShowDialog() == DialogResult.OK)
        {
            _keyFileDefaultLocation = sfd.FileName;

            CertificateMultiAccessProvider.ExportConfig(sfd.FileName, userPublicKeys);
        }
    }
    private void importButton_Click(object sender, EventArgs e)
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
                    if (!userPublicKeys.AllowedCertificates.Any(c => c.Thumbprint == newCertInfo.Thumbprint))
                    {
                        CryptoHelpers.SetSecretKeyFromNewUserKey(newCertInfo, SecretKey);
                        userPublicKeys.AllowedCertificates.Add(newCertInfo);
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
        MessageBox.Show("Attention, ...", "Export key file", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        var sfd = new SaveFileDialogEx("Export Key file")
        {
            Filter = $"Key files (*.keyx,*.key)|*.keyx;*.key|All files (*.*)|*.*",
            FileName = _keyFileDefaultLocation
        };
        if (sfd.ShowDialog() == DialogResult.OK)
        {
            GetSecretKey();

            var data = CryptoUtil.HashSha256(SecretKey.ReadData());

            var kf = KeePassLib.Keys.KfxFile.Create(0, data, null);
            var ioc = IOConnectionInfo.FromPath(sfd.FileName);
            using (Stream s = IOConnection.OpenWrite(ioc))
            {
                kf.Save(s);
            }

            MemUtil.ZeroByteArray(data);
        }
    }

    private void buttonMore_Click(object sender, EventArgs e)
    {
        contextMenuStrip1.Show(buttonMore, new Point(0, buttonMore.Height));
    }

    private void toolStripMenuItemExportKey_Click(object sender, EventArgs e)
    {
        ExportBackingKey();
    }

    private void toolStripMenuItemCustomKey_Click(object sender, EventArgs e)
    {
        var form = new PinDialog("(Experimental) Set Provider Key", "Set custom value for the key used in the database composite key, instead of a random value. (32 bytes hex)");
        var dialogResult = form.ShowDialog();
        ;
        if (form.Pin != null && dialogResult == DialogResult.OK)
        {
            //validation..

            //
            var newKeyHex = form.Pin.ReadString();
            var neyKey = MemUtil.HexStringToByteArray(newKeyHex);
            SecretKey = new ProtectedBinary(true, neyKey);
            MemUtil.ZeroByteArray(neyKey);
        }
    }
}

