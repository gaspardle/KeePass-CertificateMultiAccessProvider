using System.Windows.Forms;
using KeePass.Forms;
using KeePass.Plugins;
using KeePass.UI;
using KeePassLib;
using KeePassLib.Utility;

// The namespace must be named like the DLL file without extension.
namespace CertificateMultiAccessProvider;

// The main plugin class (which KeePass will instantiate when it loads your plugin) must be called exactly the same as the namespace plus "Ext". 
public sealed class CertificateMultiAccessProviderExt : Plugin
{
    private IPluginHost _host = null;
    private CertificateMultiAccessProvider _provider;

    //public override string UpdateUrl => "https://raw.githubusercontent.com/gaspardle/KeePass-CertificateMultiAccessProvider/master/plugin-version.txt";

    /// <summary>
    /// The <c>Initialize</c> method is called by KeePass when
    /// you should initialize your plugin.
    /// </summary>
    /// <param name="host">Plugin host interface. Through this
    /// interface you can access the KeePass main window, the
    /// currently opened database, etc.</param>
    /// <returns>You must return <c>true</c> in order to signal
    /// successful initialization. If you return <c>false</c>,
    /// KeePass unloads your plugin (without calling the
    /// <c>Terminate</c> method of your plugin).</returns>
    public override bool Initialize(IPluginHost host)
    {
        if (host == null) return false; // Fail; we need the host

        _provider = new(host);
        _host = host;
        _host.MainWindow.FileSaving += this.FileSaving;
        _host.MainWindow.FileOpened += this.FileOpened;
        _host.KeyProviderPool.Add(_provider);

        return true;
    }

    private void FileOpened(object sender, FileOpenedEventArgs e)
    {
        _provider.ValidateConfig(e.Database);
    }

    private void FileSaving(object sender, KeePass.Forms.FileSavingEventArgs e)
    {
        if (e.Database.IsOpen)
        {
            _provider.SaveCertificatesConfig(e.Database);
        }
    }

    /// <summary>
    /// The <c>Terminate</c> method is called by KeePass when
    /// you should free all resources, close files/streams,
    /// remove event handlers, etc.
    /// </summary>
    public override void Terminate()
    {
        _host.KeyProviderPool.Remove(_provider);
    }

    /// <summary>
    /// Get a menu item of the plugin. See
    /// https://keepass.info/help/v2_dev/plg_index.html#co_menuitem
    /// </summary>
    /// <param name="t">Type of the menu that the plugin should
    /// return an item for.</param>
    public override ToolStripMenuItem GetMenuItem(PluginMenuType t)
    {
        if (t == PluginMenuType.Main)
        {
            var tsmi = new ToolStripMenuItem
            {
                Text = "CertificateMultiAccess - Settings...",
            };
            tsmi.Click += OnOptionsClicked;
            return tsmi;
        }
        return null;
    }

    private void OnOptionsClicked(object sender, EventArgs e)
    {
        if (!_host.Database.IsOpen)
        {
            MessageBox.Show("No database open");
            return;
        }

        var databasePath = UrlUtil.StripExtension(_host.Database.IOConnectionInfo.Path);

        if (string.IsNullOrWhiteSpace(databasePath))
        {
            databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Database");
        }

        var keyFilePath = databasePath + CertificateMultiAccessProvider.DefaultKeyExtension;

        using var form = new KeyManagementForm(keyFilePath, _provider._certProviderConfig, _provider, false);
        form.ShowDialog();
    }
}
