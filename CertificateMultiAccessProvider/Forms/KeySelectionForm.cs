using System.Security.Cryptography.X509Certificates;
using KeePass.App;
using KeePass.UI;

namespace CertificateMultiAccessProvider;

public partial class KeySelectionForm : Form
{
    private readonly CertProviderConfig userPublicKeys;

    public X509Certificate2 SelectedCertificate { get; set; }
    public CertProvType SelectedType { get; internal set; }

    public KeySelectionForm(CertProviderConfig publicKeys, string message = null)
    {
        InitializeComponent();

        userPublicKeys = publicKeys;

        listViewCertificate.Columns.Add("Type");
        listViewCertificate.Columns.Add("Subject");
        listViewCertificate.Columns.Add("Issuer");
        listViewCertificate.Columns.Add("Thumbprint");

        if (message != null)
        {
            lblMessage.Text = message;
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

        listViewCertificate.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        listViewCertificate.EndUpdate();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);

        GlobalWindowManager.RemoveWindow(this);
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


    private void selectCertificateButton_Click(object sender, EventArgs e)
    {
        if (listViewCertificate.SelectedItems.Count == 0) { return; }

        var certificateInfo = (AllowedCertificate)listViewCertificate.SelectedItems[0]?.Tag;
        if (certificateInfo != null)
        {
            OpenCertificate(certificateInfo);
        }
    }

    private void listViewCertificate_MouseDoubleClick(object sender, MouseEventArgs e)
    {
        var certificateInfo = (AllowedCertificate)listViewCertificate.SelectedItems[0]?.Tag;
        if (certificateInfo != null)
        {
            OpenCertificate(certificateInfo);
        }
    }

    private void OpenCertificate(AllowedCertificate certificateInfo)
    {
        SelectedCertificate = certificateInfo.ReadCertificate();
        SelectedType = certificateInfo is AllowedCertificateRSAInternal ? CertProvType.InternalCertificate : CertProvType.CAPI;
        DialogResult = DialogResult.OK;
    }

    private void buttonPkcs11_Click(object sender, EventArgs e)
    {
        var form = new Pkcs11CertificateSelectionForm(null, userPublicKeys.AllowedCertificates.Select(k => k.Thumbprint));
        var dialogResult = form.ShowDialog(this);

        if (dialogResult == DialogResult.OK)
        {
            SelectedCertificate = form.SelectedCertificate.Info.ParsedCertificate;
            SelectedType = CertProvType.Pkcs11;
            DialogResult = DialogResult.OK;
        }
    }
}

