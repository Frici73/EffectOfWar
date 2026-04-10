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
            Starter = new Button();
            pictureBox1 = new PictureBox();
            Selector = new ComboBox();
            notesBox = new RichTextBox();
            Refresh = new Button();
            haladas = new Label();
            groupBox1 = new GroupBox();
            Rewrite = new RadioButton();
            Norewrite = new RadioButton();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // Starter
            // 
            Starter.Location = new Point(12, 202);
            Starter.Margin = new Padding(3, 4, 3, 4);
            Starter.Name = "Starter";
            Starter.Size = new Size(175, 35);
            Starter.TabIndex = 0;
            Starter.Text = "Starter";
            Starter.UseVisualStyleBackColor = true;
            Starter.Click += Starter_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new Point(12, 15);
            pictureBox1.Margin = new Padding(3, 4, 3, 4);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(175, 180);
            pictureBox1.TabIndex = 1;
            pictureBox1.TabStop = false;
            // 
            // Selector
            // 
            Selector.FormattingEnabled = true;
            Selector.Location = new Point(357, 15);
            Selector.Margin = new Padding(3, 4, 3, 4);
            Selector.Name = "Selector";
            Selector.Size = new Size(268, 28);
            Selector.TabIndex = 2;
            // 
            // notesBox
            // 
            notesBox.Location = new Point(357, 81);
            notesBox.Margin = new Padding(3, 4, 3, 4);
            notesBox.Name = "notesBox";
            notesBox.Size = new Size(268, 406);
            notesBox.TabIndex = 3;
            notesBox.Text = "";
            // 
            // Refresh
            // 
            Refresh.Location = new Point(12, 246);
            Refresh.Margin = new Padding(3, 4, 3, 4);
            Refresh.Name = "Refresh";
            Refresh.Size = new Size(175, 29);
            Refresh.TabIndex = 5;
            Refresh.Text = "Refresh";
            Refresh.UseVisualStyleBackColor = true;
            Refresh.Click += Refresh_Click;
            // 
            // haladas
            // 
            haladas.Location = new Point(12, 279);
            haladas.Name = "haladas";
            haladas.Size = new Size(175, 25);
            haladas.TabIndex = 6;
            haladas.Text = "haladas";
            haladas.TextAlign = ContentAlignment.MiddleCenter;
            haladas.Visible = false;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(Norewrite);
            groupBox1.Controls.Add(Rewrite);
            groupBox1.Location = new Point(12, 321);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(175, 125);
            groupBox1.TabIndex = 7;
            groupBox1.TabStop = false;
            groupBox1.Text = "Letöltési beállítás";
            // 
            // Rewrite
            // 
            Rewrite.Checked = true;
            Rewrite.Location = new Point(6, 42);
            Rewrite.Name = "Rewrite";
            Rewrite.Size = new Size(163, 30);
            Rewrite.TabIndex = 0;
            Rewrite.TabStop = true;
            Rewrite.Text = "Újra írással";
            Rewrite.UseVisualStyleBackColor = true;
            // 
            // Norewrite
            // 
            Norewrite.Location = new Point(6, 78);
            Norewrite.Name = "Norewrite";
            Norewrite.Size = new Size(163, 30);
            Norewrite.TabIndex = 1;
            Norewrite.Text = "Újra írás nélkül";
            Norewrite.UseVisualStyleBackColor = true;
            // 
            // Launcher
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(782, 504);
            Controls.Add(groupBox1);
            Controls.Add(haladas);
            Controls.Add(Refresh);
            Controls.Add(notesBox);
            Controls.Add(Selector);
            Controls.Add(pictureBox1);
            Controls.Add(Starter);
            Margin = new Padding(3, 4, 3, 4);
            MaximizeBox = false;
            Name = "Launcher";
            Text = "Effect of War Launcher";
            Load += Launcher_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            groupBox1.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button Starter;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ComboBox Selector;
        private System.Windows.Forms.RichTextBox notesBox;
        private System.Windows.Forms.Button Refresh;
        private Label haladas;
        private GroupBox groupBox1;
        private RadioButton Norewrite;
        private RadioButton Rewrite;
    }
}

