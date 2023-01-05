using KeePass.App;
using KeePass.UI;
using KeePassLib.Security;

namespace CertificateMultiAccessProvider;

public partial class PasswordDialog : Form
{
    public ProtectedString Passphrase => securePassphraseTextBox.TextEx;
    public string Subject { get; set; }
    private readonly bool _keyCreation = false;

    public PasswordDialog(string subject, bool keyCreation)
    {
        InitializeComponent();
        Subject = subject;
        _keyCreation = keyCreation;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        GlobalWindowManager.AddWindow(this);

        Icon = AppIcons.Default;

        if (!_keyCreation)
        {
            subjectTextBox.Enabled = false;
            subjectTextBox.Text = Subject;
        }
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

    private void okButton_Click(object sender, EventArgs e)
    {

        if (_keyCreation && subjectTextBox.Text.Length == 0)
        {
            MessageBox.Show("Enter the key name.");
            return;
        }

        if (securePassphraseTextBox.TextLength == 0)
        {
            MessageBox.Show("Invalid passphrase.");
            return;
        }

        this.Subject = subjectTextBox.Text;
        DialogResult = DialogResult.OK;
    }

    private void cancelButton_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
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
    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        if (!_keyCreation && securePassphraseTextBox.CanFocus) UIUtil.ResetFocus(securePassphraseTextBox, this, true);
    }

    private void PasswordDialog_Load(object sender, EventArgs e)
    {

    }
}

