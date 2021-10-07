namespace Imagon
{
    partial class OcrForm
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
            this.cboLanguage = new System.Windows.Forms.ComboBox();
            this.btnRun = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.txtResult = new System.Windows.Forms.TextBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnRemoveLineBreaks = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lnkTesseractDotNet = new System.Windows.Forms.LinkLabel();
            this.lnkTesseractOcr = new System.Windows.Forms.LinkLabel();
            this.groupBox1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cboLanguage
            // 
            this.cboLanguage.FormattingEnabled = true;
            this.cboLanguage.Items.AddRange(new object[] {
            "English",
            "Čeština"});
            this.cboLanguage.Location = new System.Drawing.Point(3, 3);
            this.cboLanguage.Name = "cboLanguage";
            this.cboLanguage.Size = new System.Drawing.Size(173, 21);
            this.cboLanguage.TabIndex = 0;
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(182, 1);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(75, 23);
            this.btnRun.TabIndex = 1;
            this.btnRun.Text = "Run OCR";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.panel2);
            this.groupBox1.Controls.Add(this.panel3);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 39);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(800, 411);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Recognized Text";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.txtResult);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 16);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(672, 392);
            this.panel2.TabIndex = 0;
            // 
            // txtResult
            // 
            this.txtResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtResult.Location = new System.Drawing.Point(0, 0);
            this.txtResult.Multiline = true;
            this.txtResult.Name = "txtResult";
            this.txtResult.Size = new System.Drawing.Size(672, 392);
            this.txtResult.TabIndex = 0;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.btnRemoveLineBreaks);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel3.Location = new System.Drawing.Point(675, 16);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(122, 392);
            this.panel3.TabIndex = 1;
            // 
            // btnRemoveLineBreaks
            // 
            this.btnRemoveLineBreaks.Location = new System.Drawing.Point(3, 3);
            this.btnRemoveLineBreaks.Name = "btnRemoveLineBreaks";
            this.btnRemoveLineBreaks.Size = new System.Drawing.Size(116, 23);
            this.btnRemoveLineBreaks.TabIndex = 0;
            this.btnRemoveLineBreaks.Text = "Remove line breaks";
            this.btnRemoveLineBreaks.UseVisualStyleBackColor = true;
            this.btnRemoveLineBreaks.Click += new System.EventHandler(this.btnRemoveLineBreaks_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lnkTesseractDotNet);
            this.panel1.Controls.Add(this.lnkTesseractOcr);
            this.panel1.Controls.Add(this.btnRun);
            this.panel1.Controls.Add(this.cboLanguage);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(800, 39);
            this.panel1.TabIndex = 3;
            // 
            // lnkTesseractDotNet
            // 
            this.lnkTesseractDotNet.AutoSize = true;
            this.lnkTesseractDotNet.Location = new System.Drawing.Point(690, 16);
            this.lnkTesseractDotNet.Name = "lnkTesseractDotNet";
            this.lnkTesseractDotNet.Size = new System.Drawing.Size(107, 13);
            this.lnkTesseractDotNet.TabIndex = 3;
            this.lnkTesseractDotNet.TabStop = true;
            this.lnkTesseractDotNet.Text = "and its .NET wrapper";
            this.lnkTesseractDotNet.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkTesseractDotNet_LinkClicked);
            // 
            // lnkTesseractOcr
            // 
            this.lnkTesseractOcr.AutoSize = true;
            this.lnkTesseractOcr.Location = new System.Drawing.Point(658, 3);
            this.lnkTesseractOcr.Name = "lnkTesseractOcr";
            this.lnkTesseractOcr.Size = new System.Drawing.Size(139, 13);
            this.lnkTesseractOcr.TabIndex = 2;
            this.lnkTesseractOcr.TabStop = true;
            this.lnkTesseractOcr.Text = "Powered by Tesseract OCR";
            this.lnkTesseractOcr.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkTesseractOcr_LinkClicked);
            // 
            // OcrForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.panel1);
            this.Name = "OcrForm";
            this.Text = "OCR";
            this.groupBox1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox cboLanguage;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TextBox txtResult;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.LinkLabel lnkTesseractDotNet;
        private System.Windows.Forms.LinkLabel lnkTesseractOcr;
        private System.Windows.Forms.Button btnRemoveLineBreaks;
    }
}