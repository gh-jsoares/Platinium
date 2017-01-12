namespace PlatiniumMasterWF
{
    partial class Form1
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
            this.richTextBox = new System.Windows.Forms.RichTextBox();
            this.listboxPlugins = new System.Windows.Forms.ListBox();
            this.datagridClients = new System.Windows.Forms.DataGridView();
            this.buttonGetClients = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.datagridClients)).BeginInit();
            this.SuspendLayout();
            // 
            // richTextBox
            // 
            this.richTextBox.Location = new System.Drawing.Point(12, 378);
            this.richTextBox.Name = "richTextBox";
            this.richTextBox.Size = new System.Drawing.Size(463, 59);
            this.richTextBox.TabIndex = 0;
            this.richTextBox.Text = "";
            // 
            // listboxPlugins
            // 
            this.listboxPlugins.FormattingEnabled = true;
            this.listboxPlugins.Location = new System.Drawing.Point(12, 12);
            this.listboxPlugins.Name = "listboxPlugins";
            this.listboxPlugins.Size = new System.Drawing.Size(183, 355);
            this.listboxPlugins.TabIndex = 1;
            // 
            // datagridClients
            // 
            this.datagridClients.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.datagridClients.Location = new System.Drawing.Point(481, 3);
            this.datagridClients.Name = "datagridClients";
            this.datagridClients.Size = new System.Drawing.Size(348, 434);
            this.datagridClients.TabIndex = 2;
            // 
            // buttonGetClients
            // 
            this.buttonGetClients.Location = new System.Drawing.Point(354, 12);
            this.buttonGetClients.Name = "buttonGetClients";
            this.buttonGetClients.Size = new System.Drawing.Size(121, 23);
            this.buttonGetClients.TabIndex = 3;
            this.buttonGetClients.Text = "Update Client List";
            this.buttonGetClients.UseVisualStyleBackColor = true;
            this.buttonGetClients.Click += new System.EventHandler(this.buttonGetClients_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(201, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(147, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "Update Plugin List";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(841, 449);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.buttonGetClients);
            this.Controls.Add(this.datagridClients);
            this.Controls.Add(this.listboxPlugins);
            this.Controls.Add(this.richTextBox);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.datagridClients)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox;
        private System.Windows.Forms.ListBox listboxPlugins;
        private System.Windows.Forms.DataGridView datagridClients;
        private System.Windows.Forms.Button buttonGetClients;
        private System.Windows.Forms.Button button1;
    }
}

