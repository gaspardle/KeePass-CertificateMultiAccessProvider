using KeePass.App;
using KeePass.UI;
using KeePassLib.Security;

namespace CertificateMultiAccessProvider;
public partial class PinDialog : Form
{
    public ProtectedString Pin { get; private set; }

    public PinDialog(string caption = null, string text = null)
    {
        InitializeComponent();
        if (caption != null) labelTitle.Text = caption;
        if (text != null) labelTitle.Text = text;
    }

    private void okButton_Click(object sender, EventArgs e)
    {
        if (securePassphraseTextBox.TextLength == 0)
        {
            MessageBox.Show("Can't be empty");
            return;
        }

        this.Pin = securePassphraseTextBox.TextEx;
        DialogResult = DialogResult.OK;
    }

    private void PinDialog_Load(object sender, EventArgs e)
    {
        GlobalWindowManager.AddWindow(this);
    }
    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);

        GlobalWindowManager.RemoveWindow(this);
    }

    private void hidePasswordCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        bool hide = hidePasswordCheckBox.Checked;
        if (!hide && !AppPolicy.Try(AppPolicyId.UnhidePasswords))
        {
            hidePasswordCheckBox.Checked = true;
            return;
        }

        securePassphraseTextBox.EnableProtection(hide);

    }
}
