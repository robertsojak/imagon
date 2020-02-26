namespace ViewTool
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            this.pnlImageView = new System.Windows.Forms.Panel();
            this.pbImageView = new System.Windows.Forms.PictureBox();
            this.msMainMenu = new System.Windows.Forms.MenuStrip();
            this.cmsContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.pnlImageView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbImageView)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlImageView
            // 
            this.pnlImageView.Controls.Add(this.pbImageView);
            this.pnlImageView.Controls.Add(this.msMainMenu);
            this.pnlImageView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlImageView.Location = new System.Drawing.Point(0, 0);
            this.pnlImageView.Name = "pnlImageView";
            this.pnlImageView.Size = new System.Drawing.Size(434, 411);
            this.pnlImageView.TabIndex = 0;
            // 
            // pbImageView
            // 
            this.pbImageView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbImageView.Location = new System.Drawing.Point(0, 24);
            this.pbImageView.Name = "pbImageView";
            this.pbImageView.Size = new System.Drawing.Size(434, 387);
            this.pbImageView.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbImageView.TabIndex = 0;
            this.pbImageView.TabStop = false;
            this.pbImageView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pbImageView_MouseDown);
            // 
            // msMainMenu
            // 
            this.msMainMenu.Location = new System.Drawing.Point(0, 0);
            this.msMainMenu.Name = "msMainMenu";
            this.msMainMenu.Size = new System.Drawing.Size(434, 24);
            this.msMainMenu.TabIndex = 1;
            this.msMainMenu.Text = "menuStrip1";
            // 
            // cmsContextMenu
            // 
            this.cmsContextMenu.Name = "cmsContextMenu";
            this.cmsContextMenu.Size = new System.Drawing.Size(61, 4);
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(434, 411);
            this.ContextMenuStrip = this.cmsContextMenu;
            this.Controls.Add(this.pnlImageView);
            this.MainMenuStrip = this.msMainMenu;
            this.Name = "MainForm";
            this.Text = "ViewTool";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.DragOver += new System.Windows.Forms.DragEventHandler(this.MainForm_DragOver);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.pnlImageView.ResumeLayout(false);
            this.pnlImageView.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbImageView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlImageView;
        private System.Windows.Forms.PictureBox pbImageView;
        private System.Windows.Forms.MenuStrip msMainMenu;
        private System.Windows.Forms.ContextMenuStrip cmsContextMenu;
    }
}

