using System.Security.Cryptography.X509Certificates;
using KeePass.UI;
using KeePassLib.Utility;
using Net.Pkcs11Interop.X509Store;

namespace CertificateMultiAccessProvider;
public partial class Pkcs11CertificateSelectionForm : Form
{
    private Pkcs11X509Store _store;
    private readonly IEnumerable<string> _thumbprints;
    private readonly Settings _settings;

    public Pkcs11X509Certificate SelectedCertificate { get; internal set; } //XXX X509Certificate2

    public Pkcs11CertificateSelectionForm(string title, Settings settings, IEnumerable<string> thumbprints = null)
    {
        InitializeComponent();
        _thumbprints = thumbprints ?? Array.Empty<string>();
        _settings = settings;
    }

    private void Pkcs11CertificateSelectionForm_Load(object sender, EventArgs e)
    {
        textBoxLibraryPath.Text = _settings.Pkcs11LibPath;
    }

    private void Pkcs11CertificateSelectionForm_Shown(object sender, EventArgs e)
    {
        if (File.Exists(textBoxLibraryPath.Text))
        {
            LoadPKCS11Library();
        }
    }

    private void RefreshList()
    {
        treeViewPKCS11.BeginUpdate();
        treeViewPKCS11.Nodes.Clear();
        treeViewPKCS11.TreeViewNodeSorter = null;

        if (_store.Slots.Count == 0)
        {
            treeViewPKCS11.Nodes.Add(new TreeNode($"(No slot or reader available)"));
        }
        foreach (var slot in _store.Slots)
        {
            var newSlotNode = new TreeNode($"Slot: {slot.Info.Manufacturer} {slot.Info.Description}");
            treeViewPKCS11.Nodes.Add(newSlotNode);

            var newTokenNode = new TreeNode($"Token: {slot.Token.Info.Label} {slot.Token.Info.Manufacturer}");
            newSlotNode.Nodes.Add(newTokenNode);

            foreach (var certificate in slot.Token.Certificates)
            {
                var parsedCertificate = certificate.Info.ParsedCertificate;
                var newCertNode = new TreeNode($"({certificate.Info.Id}) - {certificate.Info.Label} - {certificate.Info.KeyType}")
                {
                    Tag = certificate,
                    BackColor = _thumbprints.Contains(parsedCertificate.Thumbprint) ? Color.LightGreen : Color.Empty
                };

                newTokenNode.Nodes.Add(newCertNode);
            }
        }
        treeViewPKCS11.ExpandAll();
        treeViewPKCS11.EndUpdate();

        label2.Text = "(Select a certificate)";
    }

    private void ButtonLoadLibrary_Click(object sender, EventArgs e)
    {
        LoadPKCS11Library();
    }

    private void LoadPKCS11Library()
    {
        var libPath = textBoxLibraryPath.Text;
        _settings.Pkcs11LibPath = libPath;

        treeViewPKCS11.Nodes.Clear();

        try
        {
            _store = new Pkcs11X509Store(libPath, new PinProvider());
        }
        catch (Exception ex)
        {
            MessageService.ShowWarning(ex.Message);
            return;
        }
        label1.Text = _store.Info.Manufacturer + Environment.NewLine + _store.Info.Description;

        RefreshList();

        treeViewPKCS11.Focus();
        this.AcceptButton = buttonOk;
    }

    //private void ShowWindowsX509Certificate2UI()
    //{
    //    var collection = new X509Certificate2Collection();

    //    foreach (var slot in _store.Slots)
    //    {
    //        foreach (var certificate in slot.Token.Certificates)
    //        {
    //            var parsedCertificate = certificate.Info.ParsedCertificate;
    //            collection.Add(parsedCertificate);
    //        }
    //    }

    //    var selection = X509Certificate2UI.SelectFromCollection(
    //        collection,
    //        _store.Info.Manufacturer,
    //        "Select a certificate",
    //        X509SelectionFlag.SingleSelection, this.Handle)
    //        .Cast<X509Certificate2>().SingleOrDefault();

    //    if (selection != null)
    //    {
    //        var pkcsCert = (Pkcs11X509Certificate)_store.Slots
    //            .SelectMany(slot => slot.Token.Certificates
    //            .Where(cert => selection.Thumbprint == cert.Info.ParsedCertificate.Thumbprint))
    //            .FirstOrDefault();
    //        SelectedCertificate = pkcsCert;
    //        DialogResult = DialogResult.OK;
    //    }
    //}

    private void ButtonOk_Click(object sender, EventArgs e)
    {
        if (treeViewPKCS11.SelectedNode != null && treeViewPKCS11.SelectedNode.Tag is Pkcs11X509Certificate pkcsCert)
        {
            SelectedCertificate = pkcsCert;
            DialogResult = DialogResult.OK;
        }
    }

    private void ButtonCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
    }

    private void TreeViewPKCS11_AfterSelect(object sender, TreeViewEventArgs e)
    {
        if (e.Node.Tag is Pkcs11X509Certificate pkcsCert)
        {
            var cert = pkcsCert.Info.ParsedCertificate;

            label2.Text = "Subject: " + cert.Subject + Environment.NewLine + "Issuer: " + cert.Issuer + "\nThumbprint: " + cert.Thumbprint + "\nHas private key: " + pkcsCert.HasPrivateKeyObject;
        }
        else
        {
            label2.Text = "(Select a certificate)";
        }
    }

    private void ButtonBrowse_Click(object sender, EventArgs e)
    {
        var ofd = new OpenFileDialogEx("");
        if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
        {
            ofd.Filter = $".so files|*.so;*.so.*|All files (*.*)|*.*";
        }
        else
        {
            ofd.Filter = $"Dll files|*.dll|All files (*.*)|*.*";
        }
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            textBoxLibraryPath.Text = ofd.FileName;
        }
    }


}
