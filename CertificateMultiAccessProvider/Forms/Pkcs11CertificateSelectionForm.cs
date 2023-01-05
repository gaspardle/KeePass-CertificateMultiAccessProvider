using System.Security.Cryptography.X509Certificates;
using KeePass.UI;
using KeePassLib.Utility;
using Net.Pkcs11Interop.X509Store;

namespace CertificateMultiAccessProvider;
public partial class Pkcs11CertificateSelectionForm : Form
{
    private Pkcs11X509Store _store;
    private IEnumerable<string> _thumbprints;

    public Pkcs11X509Certificate SelectedCertificate { get; internal set; } //XXX X509Certificate2

    public Pkcs11CertificateSelectionForm(string title, IEnumerable<string> thumbprints = null)
    {
        InitializeComponent();
        _thumbprints = thumbprints ?? new string[] { };
    }

    private void Pkcs11CertificateSelectionForm_Load(object sender, EventArgs e)
    {
        textBoxLibraryPath.Text = @"C:\Program Files\Yubico\Yubico PIV Tool\bin\libykcs11.dll";
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
        foreach (var s in _store.Slots)
        {
            TreeNode newSlotNode = new TreeNode($"Slot: {s.Info.Manufacturer} {s.Info.Description}");
            treeViewPKCS11.Nodes.Add(newSlotNode);

            TreeNode newTokenNode = new TreeNode($"Token: {s.Token.Info.Label} {s.Token.Info.Manufacturer} - {s.Token.Info.Initialized}");
            newSlotNode.Nodes.Add(newTokenNode);
            //newSlotNode.ExpandAll();
            foreach (var c in s.Token.Certificates)
            {
                var parsedCertificate = c.Info.ParsedCertificate;
                TreeNode newCertNode = new TreeNode($"({c.Info.Id}) - {c.Info.Label} - {c.Info.KeyType}");
                newCertNode.Tag = c;
                if (_thumbprints.Contains(parsedCertificate.Thumbprint))
                {
                    newCertNode.BackColor = Color.LightGreen;
                }
                newTokenNode.Nodes.Add(newCertNode);
            }
        }
        treeViewPKCS11.ExpandAll();
        treeViewPKCS11.EndUpdate();

        label2.Text = "(Select a certificate)";
    }

    private void buttonLoadLibrary_Click(object sender, EventArgs e)
    {
        LoadPKCS11Library();
    }

    private void LoadPKCS11Library()
    {
        treeViewPKCS11.Nodes.Clear();

        try
        {
            _store = new Pkcs11X509Store(textBoxLibraryPath.Text, new PinProvider());
        }
        catch (Exception ex)
        {
            MessageService.ShowWarning(ex.Message);
            return;
        }
        label1.Text = _store.Info.Manufacturer + Environment.NewLine + _store.Info.Description;
        //provider.SaveCustomSetting("pkcs11_lib_path", textBoxLibraryPath.Text);

        //----------------
        //try
        //{
        //    ShowWindowsX509Certificate2UI();
        //}
        //catch (Exception ex)
        //{
        //    MessageService.ShowWarning(ex.Message);
        //}
        //return;

        //---------------

        RefreshList();

        treeViewPKCS11.Focus();
        this.AcceptButton = buttonOk;
    }

    private void ShowWindowsX509Certificate2UI()
    {
        X509Certificate2Collection collection = new X509Certificate2Collection();

        foreach (var s in _store.Slots)
        {
            foreach (var c in s.Token.Certificates)
            {
                var parsedCertificate = c.Info.ParsedCertificate;
                collection.Add(parsedCertificate);
            }
        }

        var selection = X509Certificate2UI.SelectFromCollection(
            collection,
            _store.Info.Manufacturer,
            "Select a certificate",
            X509SelectionFlag.SingleSelection, this.Handle)
            .Cast<X509Certificate2>().SingleOrDefault();

        if (selection != null)
        {
            var pkcsCert = (Pkcs11X509Certificate)_store.Slots.SelectMany(v => v.Token.Certificates.Where(c => selection.Thumbprint == c.Info.ParsedCertificate.Thumbprint)).FirstOrDefault();
            SelectedCertificate = pkcsCert;
            DialogResult = DialogResult.OK;
        }
    }

    private void buttonOk_Click(object sender, EventArgs e)
    {
        if (treeViewPKCS11.SelectedNode != null && treeViewPKCS11.SelectedNode.Tag is Pkcs11X509Certificate)
        {
            var pkcsCert = (Pkcs11X509Certificate)treeViewPKCS11.SelectedNode.Tag;
            SelectedCertificate = pkcsCert;
            DialogResult = DialogResult.OK;
        }
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
    }

    private void treeViewPKCS11_AfterSelect(object sender, TreeViewEventArgs e)
    {
        if (e.Node.Tag is Pkcs11X509Certificate)
        {
            var pkcsCert = (Pkcs11X509Certificate)e.Node.Tag;
            var cert = pkcsCert.Info.ParsedCertificate;

            label2.Text = "Subject: " + cert.Subject + Environment.NewLine + "Issuer: " + cert.Issuer + "\nThumbprint: " + cert.Thumbprint + "\nHas private key: " + pkcsCert.HasPrivateKeyObject;
        }
        else
        {
            label2.Text = "(Select a certificate)";
        }
    }

    private void buttonBrowse_Click(object sender, EventArgs e)
    {
        var ofd = new OpenFileDialogEx("");
        if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
        {
            ofd.Filter = $"SO files|*so|All files (*.*)|*.*";
        }
        else
        {
            ofd.Filter = $"DLL files|*dll|All files (*.*)|*.*";
        }
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            textBoxLibraryPath.Text = ofd.FileName;
        }
    }


}
