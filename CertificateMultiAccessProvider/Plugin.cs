using System.Windows.Forms;
using CertificateMultiAccessProvider.Forms;
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
        _host.MainWindow.FileOpened += this.FileOpened;
        _host.MainWindow.FileCreated += this.FileCreated;
        _host.MainWindow.MasterKeyChanged += this.MasterKeyChanged;
        _host.MainWindow.FileClosed += this.FileClosed;
        _host.KeyProviderPool.Add(_provider);

        return true;
    }

    private void MasterKeyChanged(object sender, MasterKeyChangedEventArgs e)
    {
    }

    private void FileCreated(object sender, FileCreatedEventArgs e)
    {
        if (e.Database.IsOpen)
        {
            _provider.OnDatabaseFileCreated(e.Database);
        }
    }

    private void FileOpened(object sender, FileOpenedEventArgs e)
    {
        _provider.ValidateConfig(e.Database);
    }

    private void FileClosed(object sender, FileClosedEventArgs e)
    {
        _provider.OnFileClosed(e.IOConnectionInfo.Path, e.Flags);
    }

    /// <summary>
    /// The <c>Terminate</c> method is called by KeePass when
    /// you should free all resources, close files/streams,
    /// remove event handlers, etc.
    /// </summary>
    public override void Terminate()
    {
        if (_host == null)
        {
            return;
        }

        _host.MainWindow.FileOpened -= this.FileOpened;
        _host.MainWindow.FileCreated -= this.FileCreated;
        _host.MainWindow.MasterKeyChanged -= this.MasterKeyChanged;
        _host.MainWindow.FileClosed -= this.FileClosed;

        _host.KeyProviderPool.Remove(_provider);

        _provider = null;
        _host = null;
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
                Text = "CertificateMultiAccess",
                Image = _host.MainWindow.ClientIcons.Images[(int)PwIcon.Certificate]
            };

            var settingsSubMenuItem = new ToolStripMenuItem("Settings...");
            settingsSubMenuItem.Image = _host.MainWindow.ClientIcons.Images[(int)PwIcon.Configuration];
            settingsSubMenuItem.Click += OnOptionsClicked;
            tsmi.DropDownItems.Add(settingsSubMenuItem);

            var aboutSubMenuItem = new ToolStripMenuItem("About");
            aboutSubMenuItem.Image = _host.MainWindow.ClientIcons.Images[(int)PwIcon.Info];
            aboutSubMenuItem.Click += OnAboutClicked;
            tsmi.DropDownItems.Add(aboutSubMenuItem);

            return tsmi;
        }
        return null;
    }

    private void OnOptionsClicked(object sender, EventArgs e)
    {
        if (!_host.Database.IsOpen)
        {
            MessageBox.Show("No database open.");
            return;
        }

        if (!_provider.IsDatabaseOpen(_host.Database.IOConnectionInfo.Path))
        {
            MessageBox.Show($"This database is not using {Version.Name}.");
            return;
        }

        var _certProviderConfig = _provider.ReadCertificatesConfig(_host.Database);
        if (_certProviderConfig == null)
        {
            return;
        }

        using var form = new KeyManagementForm(_provider, _certProviderConfig, _host.Database);
        form.ShowDialog();
    }

    private void OnAboutClicked(object sender, EventArgs e)
    {
        var about = new About();
        about.ShowDialog(_host.MainWindow);
    }
}
