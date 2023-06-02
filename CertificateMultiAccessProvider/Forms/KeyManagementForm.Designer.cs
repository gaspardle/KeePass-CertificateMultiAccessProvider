using KeePass.UI;

namespace CertificateMultiAccessProvider
{
    partial class KeyManagementForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonOk = new System.Windows.Forms.Button();
            this.addCertFromStoreButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.exportButton = new System.Windows.Forms.Button();
            this.addCertInternalButton = new System.Windows.Forms.Button();
            this.listViewCertificate = new System.Windows.Forms.ListView();
            this.displayCertificateDetailsButton = new System.Windows.Forms.Button();
            this.removeCertButton = new System.Windows.Forms.Button();
            this.importButton = new System.Windows.Forms.Button();
            this.buttonAddPkcs11 = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonMore = new System.Windows.Forms.Button();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemExportKey = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemCustomKey = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(379, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Certificate MultiAccess Provider uses certificates to allow access the database.\r" +
    "\n";
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.Location = new System.Drawing.Point(411, 283);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(92, 23);
            this.buttonOk.TabIndex = 50;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.ButtonOk_Click);
            // 
            // addCertFromStoreButton
            // 
            this.addCertFromStoreButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.addCertFromStoreButton.Location = new System.Drawing.Point(11, 194);
            this.addCertFromStoreButton.Name = "addCertFromStoreButton";
            this.addCertFromStoreButton.Size = new System.Drawing.Size(156, 23);
            this.addCertFromStoreButton.TabIndex = 15;
            this.addCertFromStoreButton.Text = "Add a certificate...";
            this.addCertFromStoreButton.UseVisualStyleBackColor = true;
            this.addCertFromStoreButton.Click += new System.EventHandler(this.AddCertFromStoreButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 54);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(101, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "Allowed certificates:";
            // 
            // exportButton
            // 
            this.exportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.exportButton.Location = new System.Drawing.Point(117, 283);
            this.exportButton.Name = "exportButton";
            this.exportButton.Size = new System.Drawing.Size(96, 23);
            this.exportButton.TabIndex = 60;
            this.exportButton.Text = "Export config...";
            this.exportButton.UseVisualStyleBackColor = true;
            this.exportButton.Click += new System.EventHandler(this.ExportButton_Click);
            // 
            // addCertInternalButton
            // 
            this.addCertInternalButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.addCertInternalButton.Location = new System.Drawing.Point(11, 249);
            this.addCertInternalButton.Name = "addCertInternalButton";
            this.addCertInternalButton.Size = new System.Drawing.Size(156, 24);
            this.addCertInternalButton.TabIndex = 20;
            this.addCertInternalButton.Text = "Add internal certificate...";
            this.addCertInternalButton.UseVisualStyleBackColor = true;
            this.addCertInternalButton.Visible = false;
            this.addCertInternalButton.Click += new System.EventHandler(this.AddCertInternalButton_Click);
            // 
            // listViewCertificate
            // 
            this.listViewCertificate.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewCertificate.FullRowSelect = true;
            this.listViewCertificate.GridLines = true;
            this.listViewCertificate.HideSelection = false;
            this.listViewCertificate.Location = new System.Drawing.Point(11, 70);
            this.listViewCertificate.MultiSelect = false;
            this.listViewCertificate.Name = "listViewCertificate";
            this.listViewCertificate.Size = new System.Drawing.Size(590, 118);
            this.listViewCertificate.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listViewCertificate.TabIndex = 5;
            this.listViewCertificate.UseCompatibleStateImageBehavior = false;
            this.listViewCertificate.View = System.Windows.Forms.View.Details;
            this.listViewCertificate.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ListViewCertificate_MouseDoubleClick);
            // 
            // displayCertificateDetailsButton
            // 
            this.displayCertificateDetailsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.displayCertificateDetailsButton.Location = new System.Drawing.Point(509, 194);
            this.displayCertificateDetailsButton.Name = "displayCertificateDetailsButton";
            this.displayCertificateDetailsButton.Size = new System.Drawing.Size(92, 23);
            this.displayCertificateDetailsButton.TabIndex = 35;
            this.displayCertificateDetailsButton.Text = "View...";
            this.displayCertificateDetailsButton.UseVisualStyleBackColor = true;
            this.displayCertificateDetailsButton.Click += new System.EventHandler(this.DisplayCertificateDetails_Click);
            // 
            // removeCertButton
            // 
            this.removeCertButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.removeCertButton.Location = new System.Drawing.Point(377, 194);
            this.removeCertButton.Name = "removeCertButton";
            this.removeCertButton.Size = new System.Drawing.Size(126, 23);
            this.removeCertButton.TabIndex = 30;
            this.removeCertButton.Text = "Remove selected...";
            this.removeCertButton.UseVisualStyleBackColor = true;
            this.removeCertButton.Click += new System.EventHandler(this.RemoveCertButton_Click);
            // 
            // importButton
            // 
            this.importButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.importButton.Location = new System.Drawing.Point(13, 283);
            this.importButton.Name = "importButton";
            this.importButton.Size = new System.Drawing.Size(98, 23);
            this.importButton.TabIndex = 55;
            this.importButton.Text = "Import config...";
            this.importButton.UseVisualStyleBackColor = true;
            this.importButton.Click += new System.EventHandler(this.ImportButton_Click);
            // 
            // buttonAddPkcs11
            // 
            this.buttonAddPkcs11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAddPkcs11.Location = new System.Drawing.Point(11, 221);
            this.buttonAddPkcs11.Name = "buttonAddPkcs11";
            this.buttonAddPkcs11.Size = new System.Drawing.Size(156, 23);
            this.buttonAddPkcs11.TabIndex = 25;
            this.buttonAddPkcs11.Text = "Add a certificate (PKCS11)...";
            this.buttonAddPkcs11.UseVisualStyleBackColor = true;
            this.buttonAddPkcs11.Click += new System.EventHandler(this.ButtonAddPkcs11_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(510, 283);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(92, 23);
            this.buttonCancel.TabIndex = 61;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // buttonMore
            // 
            this.buttonMore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonMore.Location = new System.Drawing.Point(218, 283);
            this.buttonMore.Name = "buttonMore";
            this.buttonMore.Size = new System.Drawing.Size(92, 23);
            this.buttonMore.TabIndex = 63;
            this.buttonMore.Text = "Advanced   ▼";
            this.buttonMore.UseVisualStyleBackColor = true;
            this.buttonMore.Click += new System.EventHandler(this.ButtonMore_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemExportKey,
            this.toolStripMenuItemCustomKey});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(217, 48);
            // 
            // toolStripMenuItemExportKey
            // 
            this.toolStripMenuItemExportKey.Name = "toolStripMenuItemExportKey";
            this.toolStripMenuItemExportKey.Size = new System.Drawing.Size(216, 22);
            this.toolStripMenuItemExportKey.Text = "Export backing key as file...";
            this.toolStripMenuItemExportKey.Click += new System.EventHandler(this.ToolStripMenuItemExportKey_Click);
            // 
            // toolStripMenuItemCustomKey
            // 
            this.toolStripMenuItemCustomKey.Name = "toolStripMenuItemCustomKey";
            this.toolStripMenuItemCustomKey.Size = new System.Drawing.Size(216, 22);
            this.toolStripMenuItemCustomKey.Text = "Set custom key...";
            this.toolStripMenuItemCustomKey.Click += new System.EventHandler(this.ToolStripMenuItemCustomKey_Click);
            // 
            // KeyManagementForm
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(614, 316);
            this.Controls.Add(this.buttonMore);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.addCertInternalButton);
            this.Controls.Add(this.buttonAddPkcs11);
            this.Controls.Add(this.importButton);
            this.Controls.Add(this.removeCertButton);
            this.Controls.Add(this.displayCertificateDetailsButton);
            this.Controls.Add(this.listViewCertificate);
            this.Controls.Add(this.exportButton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.addCertFromStoreButton);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(630, 355);
            this.Name = "KeyManagementForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Certificate MultiAccess Provider";
            this.Load += new System.EventHandler(this.KeyCreationForm_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button addCertFromStoreButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button exportButton;
        private Button addCertInternalButton;
        private ListView listViewCertificate;
        private Button displayCertificateDetailsButton;
        private Button removeCertButton;
        private Button importButton;
        private Button buttonAddPkcs11;
        private Button buttonCancel;
        private Button buttonMore;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem toolStripMenuItemExportKey;
        private ToolStripMenuItem toolStripMenuItemCustomKey;
    }
}
