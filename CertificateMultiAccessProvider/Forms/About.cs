using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CertificateMultiAccessProvider.Forms;
public partial class About : Form
{
    public About()
    {
        InitializeComponent();
    }

    private void About_Load(object sender, EventArgs e)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);

        labelName.Text = Version.Name;
        labelVersion.Text = String.Format($"Version: {Version.Number}");
    }
}
