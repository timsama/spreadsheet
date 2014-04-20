namespace BC
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
            this.player_textbox = new System.Windows.Forms.TextBox();
            this.Player_label = new System.Windows.Forms.Label();
            this.Play_button = new System.Windows.Forms.Button();
            this.receive_label = new System.Windows.Forms.Label();
            this.receive_textbox = new System.Windows.Forms.RichTextBox();
            this.send_label = new System.Windows.Forms.Label();
            this.send_button = new System.Windows.Forms.Button();
            this.opponent_label = new System.Windows.Forms.Label();
            this.opponent_name_label = new System.Windows.Forms.Label();
            this.send_textbox = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // player_textbox
            // 
            this.player_textbox.Location = new System.Drawing.Point(55, 10);
            this.player_textbox.Margin = new System.Windows.Forms.Padding(2);
            this.player_textbox.Name = "player_textbox";
            this.player_textbox.Size = new System.Drawing.Size(116, 20);
            this.player_textbox.TabIndex = 0;
            // 
            // Player_label
            // 
            this.Player_label.AutoSize = true;
            this.Player_label.Location = new System.Drawing.Point(14, 12);
            this.Player_label.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.Player_label.Name = "Player_label";
            this.Player_label.Size = new System.Drawing.Size(36, 13);
            this.Player_label.TabIndex = 1;
            this.Player_label.Text = "Player";
            // 
            // Play_button
            // 
            this.Play_button.Location = new System.Drawing.Point(183, 7);
            this.Play_button.Margin = new System.Windows.Forms.Padding(2);
            this.Play_button.Name = "Play_button";
            this.Play_button.Size = new System.Drawing.Size(42, 24);
            this.Play_button.TabIndex = 2;
            this.Play_button.Text = "PLAY";
            this.Play_button.UseVisualStyleBackColor = true;
            this.Play_button.Click += new System.EventHandler(this.Play_button_Click);
            // 
            // receive_label
            // 
            this.receive_label.AutoSize = true;
            this.receive_label.Location = new System.Drawing.Point(14, 162);
            this.receive_label.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.receive_label.Name = "receive_label";
            this.receive_label.Size = new System.Drawing.Size(104, 13);
            this.receive_label.TabIndex = 3;
            this.receive_label.Text = "Received Messages";
            // 
            // receive_textbox
            // 
            this.receive_textbox.Location = new System.Drawing.Point(12, 180);
            this.receive_textbox.Margin = new System.Windows.Forms.Padding(2);
            this.receive_textbox.Name = "receive_textbox";
            this.receive_textbox.ReadOnly = true;
            this.receive_textbox.Size = new System.Drawing.Size(419, 180);
            this.receive_textbox.TabIndex = 4;
            this.receive_textbox.Text = "";
            // 
            // send_label
            // 
            this.send_label.AutoSize = true;
            this.send_label.Location = new System.Drawing.Point(14, 56);
            this.send_label.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.send_label.Name = "send_label";
            this.send_label.Size = new System.Drawing.Size(78, 13);
            this.send_label.TabIndex = 5;
            this.send_label.Text = "Send to Server";
            // 
            // send_button
            // 
            this.send_button.Location = new System.Drawing.Point(94, 54);
            this.send_button.Margin = new System.Windows.Forms.Padding(2);
            this.send_button.Name = "send_button";
            this.send_button.Size = new System.Drawing.Size(45, 19);
            this.send_button.TabIndex = 7;
            this.send_button.Text = "SEND";
            this.send_button.UseVisualStyleBackColor = true;
            this.send_button.Click += new System.EventHandler(this.send_button_Click);
            // 
            // opponent_label
            // 
            this.opponent_label.AutoSize = true;
            this.opponent_label.Location = new System.Drawing.Point(238, 11);
            this.opponent_label.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.opponent_label.Name = "opponent_label";
            this.opponent_label.Size = new System.Drawing.Size(70, 13);
            this.opponent_label.TabIndex = 8;
            this.opponent_label.Text = "OPPONENT:";
            // 
            // opponent_name_label
            // 
            this.opponent_name_label.AutoSize = true;
            this.opponent_name_label.Location = new System.Drawing.Point(310, 11);
            this.opponent_name_label.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.opponent_name_label.Name = "opponent_name_label";
            this.opponent_name_label.Size = new System.Drawing.Size(52, 13);
            this.opponent_name_label.TabIndex = 9;
            this.opponent_name_label.Text = "Waiting...";
            // 
            // send_textbox
            // 
            this.send_textbox.Location = new System.Drawing.Point(12, 75);
            this.send_textbox.Margin = new System.Windows.Forms.Padding(2);
            this.send_textbox.Name = "send_textbox";
            this.send_textbox.Size = new System.Drawing.Size(418, 84);
            this.send_textbox.TabIndex = 6;
            this.send_textbox.Text = "";
            this.send_textbox.TextChanged += new System.EventHandler(this.send_textbox_TextChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(439, 366);
            this.Controls.Add(this.opponent_name_label);
            this.Controls.Add(this.opponent_label);
            this.Controls.Add(this.send_button);
            this.Controls.Add(this.send_textbox);
            this.Controls.Add(this.send_label);
            this.Controls.Add(this.receive_textbox);
            this.Controls.Add(this.receive_label);
            this.Controls.Add(this.Play_button);
            this.Controls.Add(this.Player_label);
            this.Controls.Add(this.player_textbox);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox player_textbox;
        private System.Windows.Forms.Label Player_label;
        private System.Windows.Forms.Button Play_button;
        private System.Windows.Forms.Label receive_label;
        private System.Windows.Forms.RichTextBox receive_textbox;
        private System.Windows.Forms.Label send_label;
        private System.Windows.Forms.Button send_button;
        private System.Windows.Forms.Label opponent_label;
        private System.Windows.Forms.Label opponent_name_label;
        private System.Windows.Forms.RichTextBox send_textbox;
    }
}

