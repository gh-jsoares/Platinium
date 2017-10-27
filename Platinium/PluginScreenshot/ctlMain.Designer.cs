namespace PluginScreenshot
{
    partial class ctlMain
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonTakeScreenshot = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonSaveScreenshot = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonTakeScreenshot
            // 
            this.buttonTakeScreenshot.Location = new System.Drawing.Point(3, 3);
            this.buttonTakeScreenshot.Name = "buttonTakeScreenshot";
            this.buttonTakeScreenshot.Size = new System.Drawing.Size(135, 23);
            this.buttonTakeScreenshot.TabIndex = 0;
            this.buttonTakeScreenshot.Text = "Take Screenshot";
            this.buttonTakeScreenshot.UseVisualStyleBackColor = true;
            this.buttonTakeScreenshot.Click += new System.EventHandler(this.button1_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(0, 30);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(343, 284);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.buttonSaveScreenshot);
            this.panel1.Controls.Add(this.buttonTakeScreenshot);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(343, 30);
            this.panel1.TabIndex = 2;
            // 
            // buttonSaveScreenshot
            // 
            this.buttonSaveScreenshot.Location = new System.Drawing.Point(144, 4);
            this.buttonSaveScreenshot.Name = "buttonSaveScreenshot";
            this.buttonSaveScreenshot.Size = new System.Drawing.Size(139, 23);
            this.buttonSaveScreenshot.TabIndex = 1;
            this.buttonSaveScreenshot.Text = "Save Screenshot";
            this.buttonSaveScreenshot.UseVisualStyleBackColor = true;
            this.buttonSaveScreenshot.Click += new System.EventHandler(this.buttonSaveScreenshot_Click);
            // 
            // ctlMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.panel1);
            this.Name = "ctlMain";
            this.Size = new System.Drawing.Size(343, 314);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonTakeScreenshot;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button buttonSaveScreenshot;
    }
}
