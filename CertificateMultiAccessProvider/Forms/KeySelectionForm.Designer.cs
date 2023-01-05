using KeePass.UI;

namespace CertificateMultiAccessProvider
{
    partial class KeySelectionForm
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
            this.lblMessage = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.listViewCertificate = new System.Windows.Forms.ListView();
            this.displayCertificateDetailsButton = new System.Windows.Forms.Button();
            this.selectCertificateButton = new System.Windows.Forms.Button();
            this.buttonPkcs11 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblMessage
            // 
            this.lblMessage.AutoSize = true;
            this.lblMessage.Location = new System.Drawing.Point(8, 9);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(95, 13);
            this.lblMessage.TabIndex = 1;
            this.lblMessage.Text = "Select a certificate";
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
            this.listViewCertificate.Size = new System.Drawing.Size(484, 127);
            this.listViewCertificate.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listViewCertificate.TabIndex = 5;
            this.listViewCertificate.UseCompatibleStateImageBehavior = false;
            this.listViewCertificate.View = System.Windows.Forms.View.Details;
            this.listViewCertificate.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listViewCertificate_MouseDoubleClick);
            // 
            // displayCertificateDetailsButton
            // 
            this.displayCertificateDetailsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.displayCertificateDetailsButton.Location = new System.Drawing.Point(422, 204);
            this.displayCertificateDetailsButton.Name = "displayCertificateDetailsButton";
            this.displayCertificateDetailsButton.Size = new System.Drawing.Size(70, 23);
            this.displayCertificateDetailsButton.TabIndex = 20;
            this.displayCertificateDetailsButton.Text = "View...";
            this.displayCertificateDetailsButton.UseVisualStyleBackColor = true;
            this.displayCertificateDetailsButton.Click += new System.EventHandler(this.displayCertificateDetails_Click);
            // 
            // selectCertificateButton
            // 
            this.selectCertificateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.selectCertificateButton.Location = new System.Drawing.Point(274, 204);
            this.selectCertificateButton.Name = "selectCertificateButton";
            this.selectCertificateButton.Size = new System.Drawing.Size(146, 23);
            this.selectCertificateButton.TabIndex = 10;
            this.selectCertificateButton.Text = "Use selected certificate";
            this.selectCertificateButton.UseVisualStyleBackColor = true;
            this.selectCertificateButton.Click += new System.EventHandler(this.selectCertificateButton_Click);
            // 
            // buttonPkcs11
            // 
            this.buttonPkcs11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonPkcs11.Location = new System.Drawing.Point(11, 204);
            this.buttonPkcs11.Name = "buttonPkcs11";
            this.buttonPkcs11.Size = new System.Drawing.Size(203, 23);
            this.buttonPkcs11.TabIndex = 21;
            this.buttonPkcs11.Text = "Use certificate from PKCS#11 token...";
            this.buttonPkcs11.UseVisualStyleBackColor = true;
            this.buttonPkcs11.Click += new System.EventHandler(this.buttonPkcs11_Click);
            // 
            // KeySelectionForm
            // 
            this.AcceptButton = this.selectCertificateButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(504, 237);
            this.Controls.Add(this.buttonPkcs11);
            this.Controls.Add(this.selectCertificateButton);
            this.Controls.Add(this.displayCertificateDetailsButton);
            this.Controls.Add(this.listViewCertificate);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblMessage);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(520, 276);
            this.Name = "KeySelectionForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Certificate MultiAccess Provider";
            this.Load += new System.EventHandler(this.KeyCreationForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.Label label3;
        private ListView listViewCertificate;
        private Button displayCertificateDetailsButton;
        private Button selectCertificateButton;
        private Button buttonPkcs11;
    }
}
