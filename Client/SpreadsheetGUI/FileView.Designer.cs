namespace SS
{
    partial class FileView
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
            this.filesListBox = new System.Windows.Forms.ListBox();
            this.filenameTextBox = new System.Windows.Forms.TextBox();
            this.openButton = new System.Windows.Forms.Button();
            this.createButton = new System.Windows.Forms.Button();
            this.filesListLabel = new System.Windows.Forms.Label();
            this.openProgressBar = new System.Windows.Forms.ProgressBar();
            this.createProgressBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // filesListBox
            // 
            this.filesListBox.FormattingEnabled = true;
            this.filesListBox.Location = new System.Drawing.Point(12, 31);
            this.filesListBox.Name = "filesListBox";
            this.filesListBox.ScrollAlwaysVisible = true;
            this.filesListBox.Size = new System.Drawing.Size(223, 186);
            this.filesListBox.TabIndex = 0;
            this.filesListBox.Enter += new System.EventHandler(this.filesListBox_Enter);
            // 
            // filenameTextBox
            // 
            this.filenameTextBox.Location = new System.Drawing.Point(12, 255);
            this.filenameTextBox.Name = "filenameTextBox";
            this.filenameTextBox.Size = new System.Drawing.Size(223, 20);
            this.filenameTextBox.TabIndex = 2;
            this.filenameTextBox.Enter += new System.EventHandler(this.filenameTextBox_Enter);
            // 
            // openButton
            // 
            this.openButton.Location = new System.Drawing.Point(90, 223);
            this.openButton.Name = "openButton";
            this.openButton.Size = new System.Drawing.Size(145, 26);
            this.openButton.TabIndex = 1;
            this.openButton.Text = "Open Spreadsheet";
            this.openButton.UseVisualStyleBackColor = true;
            this.openButton.Click += new System.EventHandler(this.openButton_Click);
            // 
            // createButton
            // 
            this.createButton.Location = new System.Drawing.Point(90, 281);
            this.createButton.Name = "createButton";
            this.createButton.Size = new System.Drawing.Size(145, 26);
            this.createButton.TabIndex = 3;
            this.createButton.Text = "Create New Spreadsheet";
            this.createButton.UseVisualStyleBackColor = true;
            this.createButton.Click += new System.EventHandler(this.createButton_Click);
            // 
            // filesListLabel
            // 
            this.filesListLabel.AutoSize = true;
            this.filesListLabel.Location = new System.Drawing.Point(12, 9);
            this.filesListLabel.Name = "filesListLabel";
            this.filesListLabel.Size = new System.Drawing.Size(109, 13);
            this.filesListLabel.TabIndex = 4;
            this.filesListLabel.Text = "Select a Spreadsheet";
            // 
            // openProgressBar
            // 
            this.openProgressBar.Location = new System.Drawing.Point(12, 223);
            this.openProgressBar.MarqueeAnimationSpeed = 50;
            this.openProgressBar.Name = "openProgressBar";
            this.openProgressBar.Size = new System.Drawing.Size(72, 26);
            this.openProgressBar.TabIndex = 5;
            // 
            // createProgressBar
            // 
            this.createProgressBar.Location = new System.Drawing.Point(12, 281);
            this.createProgressBar.MarqueeAnimationSpeed = 50;
            this.createProgressBar.Name = "createProgressBar";
            this.createProgressBar.Size = new System.Drawing.Size(72, 26);
            this.createProgressBar.TabIndex = 6;
            // 
            // FileView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(249, 319);
            this.Controls.Add(this.createProgressBar);
            this.Controls.Add(this.openProgressBar);
            this.Controls.Add(this.filesListLabel);
            this.Controls.Add(this.createButton);
            this.Controls.Add(this.openButton);
            this.Controls.Add(this.filenameTextBox);
            this.Controls.Add(this.filesListBox);
            this.Name = "FileView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Spreadsheet";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox filesListBox;
        private System.Windows.Forms.TextBox filenameTextBox;
        private System.Windows.Forms.Button openButton;
        private System.Windows.Forms.Button createButton;
        private System.Windows.Forms.Label filesListLabel;
        private System.Windows.Forms.ProgressBar openProgressBar;
        private System.Windows.Forms.ProgressBar createProgressBar;
    }
}