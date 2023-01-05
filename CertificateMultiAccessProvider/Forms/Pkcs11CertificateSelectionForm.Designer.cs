namespace CertificateMultiAccessProvider;

partial class Pkcs11CertificateSelectionForm
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            this.textBoxLibraryPath = new System.Windows.Forms.TextBox();
            this.buttonLoadLibrary = new System.Windows.Forms.Button();
            this.treeViewPKCS11 = new System.Windows.Forms.TreeView();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBoxLibraryPath
            // 
            this.textBoxLibraryPath.Location = new System.Drawing.Point(12, 30);
            this.textBoxLibraryPath.Name = "textBoxLibraryPath";
            this.textBoxLibraryPath.Size = new System.Drawing.Size(301, 20);
            this.textBoxLibraryPath.TabIndex = 0;
            // 
            // buttonLoadLibrary
            // 
            this.buttonLoadLibrary.Location = new System.Drawing.Point(403, 29);
            this.buttonLoadLibrary.Name = "buttonLoadLibrary";
            this.buttonLoadLibrary.Size = new System.Drawing.Size(78, 21);
            this.buttonLoadLibrary.TabIndex = 1;
            this.buttonLoadLibrary.Text = "Load";
            this.buttonLoadLibrary.UseVisualStyleBackColor = true;
            this.buttonLoadLibrary.Click += new System.EventHandler(this.buttonLoadLibrary_Click);
            // 
            // treeViewPKCS11
            // 
            this.treeViewPKCS11.Location = new System.Drawing.Point(12, 74);
            this.treeViewPKCS11.Name = "treeViewPKCS11";
            this.treeViewPKCS11.Size = new System.Drawing.Size(382, 263);
            this.treeViewPKCS11.TabIndex = 2;
            this.treeViewPKCS11.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewPKCS11_AfterSelect);
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.Location = new System.Drawing.Point(519, 353);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 3;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(600, 353);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // label1
            // 
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label1.Location = new System.Drawing.Point(403, 74);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(272, 42);
            this.label1.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label2.Location = new System.Drawing.Point(403, 116);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(274, 221);
            this.label2.TabIndex = 6;
            this.label2.Text = "()";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label4.Location = new System.Drawing.Point(1, 61);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(684, 2);
            this.label4.TabIndex = 30;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 14);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(113, 13);
            this.label5.TabIndex = 31;
            this.label5.Text = "PCKS#11 Library Path";
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Location = new System.Drawing.Point(319, 29);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(78, 21);
            this.buttonBrowse.TabIndex = 32;
            this.buttonBrowse.Text = "Browse...";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // Pkcs11CertificateSelectionForm
            // 
            this.AcceptButton = this.buttonLoadLibrary;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(687, 388);
            this.Controls.Add(this.buttonBrowse);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.treeViewPKCS11);
            this.Controls.Add(this.buttonLoadLibrary);
            this.Controls.Add(this.textBoxLibraryPath);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "Pkcs11CertificateSelectionForm";
            this.Text = "Pkcs#11 Certificates";
            this.Load += new System.EventHandler(this.Pkcs11CertificateSelectionForm_Load);
            this.Shown += new System.EventHandler(this.Pkcs11CertificateSelectionForm_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private TextBox textBoxLibraryPath;
    private Button buttonLoadLibrary;
    private TreeView treeViewPKCS11;
    private Button buttonOk;
    private Button buttonCancel;
    private Label label1;
    private Label label2;
    private Label label4;
    private Label label5;
    private Button buttonBrowse;
}
