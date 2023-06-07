using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using CertificateMultiAccessProvider.CertProvider;
using KeePass.App;
using KeePass.UI;

namespace CertificateMultiAccessProvider;

public partial class KeySelectionForm : Form
{
    private readonly CertProviderConfiguration _certProviderConfig;
    private readonly Settings _settings;

    public X509Certificate2 SelectedCertificate { get; set; }
    public CertProvType SelectedType { get; internal set; }

    public KeySelectionForm(CertProviderConfiguration certProviderConfig, Settings settings, string message = null)
    {
        InitializeComponent();

        _certProviderConfig = certProviderConfig;
        _settings = settings;

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
        if (_certProviderConfig.AllowedCertificates.Count == 0) return;

        listViewCertificate.BeginUpdate();
        listViewCertificate.Items.Clear();
        listViewCertificate.ListViewItemSorter = null;
        foreach (var p in _certProviderConfig.AllowedCertificates)
        {
            var cert = p.ReadCertificate();

            var subject = cert.GetNameInfo(X509NameType.SimpleName, forIssuer: false);
            var issuer = cert.GetNameInfo(X509NameType.SimpleName, forIssuer: true);
            var item = new ListViewItem(subject);
            item.SubItems.AddRange(new List<string>() { issuer, cert.Thumbprint }.ToArray());
            item.Tag = p;
            listViewCertificate.Items.Add(item);
        }

        listViewCertificate.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        listViewCertificate.EndUpdate();

        listViewCertificate.Items[0].Selected = true;
        listViewCertificate.Select();
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


    private void SelectCertificateButton_Click(object sender, EventArgs e)
    {
        if (listViewCertificate.SelectedItems.Count == 0) { return; }

        var certificateInfo = (AllowedCertificate)listViewCertificate.SelectedItems[0]?.Tag;
        if (certificateInfo != null)
        {
            OpenCertificate(certificateInfo);
        }
    }

    private void ListViewCertificate_MouseDoubleClick(object sender, MouseEventArgs e)
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
        SelectedType = CertProvType.CAPI;
        DialogResult = DialogResult.OK;
    }

    private void ButtonPkcs11_Click(object sender, EventArgs e)
    {
        var form = new Pkcs11CertificateSelectionForm(null, _settings, _certProviderConfig.AllowedCertificates.Select(k => k.Thumbprint));
        var dialogResult = form.ShowDialog(this);

        if (dialogResult == DialogResult.OK)
        {
            SelectedCertificate = form.SelectedCertificate.Info.ParsedCertificate;
            SelectedType = CertProvType.Pkcs11;
            DialogResult = DialogResult.OK;
        }
    }
}

