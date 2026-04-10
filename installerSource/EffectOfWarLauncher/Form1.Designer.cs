namespace EffectOfWarLauncher
{
    partial class Launcher
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Launcher));
            this.Starter = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.Selector = new System.Windows.Forms.ComboBox();
            this.notesBox = new System.Windows.Forms.RichTextBox();
            this.cacheCleaner = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // Starter
            // 
            this.Starter.Location = new System.Drawing.Point(12, 162);
            this.Starter.Name = "Starter";
            this.Starter.Size = new System.Drawing.Size(175, 28);
            this.Starter.TabIndex = 0;
            this.Starter.Text = "Starter";
            this.Starter.UseVisualStyleBackColor = true;
            this.Starter.Click += new System.EventHandler(this.Starter_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(175, 144);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // Selector
            // 
            this.Selector.FormattingEnabled = true;
            this.Selector.Location = new System.Drawing.Point(357, 12);
            this.Selector.Name = "Selector";
            this.Selector.Size = new System.Drawing.Size(268, 24);
            this.Selector.TabIndex = 2;
            // 
            // notesBox
            // 
            this.notesBox.Location = new System.Drawing.Point(357, 65);
            this.notesBox.Name = "notesBox";
            this.notesBox.Size = new System.Drawing.Size(268, 326);
            this.notesBox.TabIndex = 3;
            this.notesBox.Text = "";
            // 
            // cacheCleaner
            // 
            this.cacheCleaner.Location = new System.Drawing.Point(647, 12);
            this.cacheCleaner.Name = "cacheCleaner";
            this.cacheCleaner.Size = new System.Drawing.Size(123, 23);
            this.cacheCleaner.TabIndex = 4;
            this.cacheCleaner.Text = "Clear cache";
            this.cacheCleaner.UseVisualStyleBackColor = true;
            // 
            // Launcher
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(782, 403);
            this.Controls.Add(this.cacheCleaner);
            this.Controls.Add(this.notesBox);
            this.Controls.Add(this.Selector);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.Starter);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Launcher";
            this.Text = "Effect of War Launcher";
            this.Load += new System.EventHandler(this.Launcher_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button Starter;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ComboBox Selector;
        private System.Windows.Forms.RichTextBox notesBox;
        private System.Windows.Forms.Button cacheCleaner;
    }
}

