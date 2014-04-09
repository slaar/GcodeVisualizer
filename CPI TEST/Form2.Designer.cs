namespace GcodeVisualizer
{
    partial class EditForm
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
            this.EditTextBox = new System.Windows.Forms.TextBox();
            this.SaveChangeButton = new System.Windows.Forms.Button();
            this.CancelChangeButton = new System.Windows.Forms.Button();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.EditDebugOutput = new System.Windows.Forms.Label();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // EditTextBox
            // 
            this.EditTextBox.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EditTextBox.Location = new System.Drawing.Point(12, 12);
            this.EditTextBox.Name = "EditTextBox";
            this.EditTextBox.Size = new System.Drawing.Size(530, 26);
            this.EditTextBox.TabIndex = 0;
            this.EditTextBox.TextChanged += new System.EventHandler(this.EditTextBox_TextChanged);
            // 
            // SaveChangeButton
            // 
            this.SaveChangeButton.Location = new System.Drawing.Point(12, 52);
            this.SaveChangeButton.Name = "SaveChangeButton";
            this.SaveChangeButton.Size = new System.Drawing.Size(75, 23);
            this.SaveChangeButton.TabIndex = 1;
            this.SaveChangeButton.Text = "Save";
            this.SaveChangeButton.UseVisualStyleBackColor = true;
            this.SaveChangeButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            // 
            // CancelChangeButton
            // 
            this.CancelChangeButton.Location = new System.Drawing.Point(93, 52);
            this.CancelChangeButton.Name = "CancelChangeButton";
            this.CancelChangeButton.Size = new System.Drawing.Size(75, 23);
            this.CancelChangeButton.TabIndex = 2;
            this.CancelChangeButton.Text = "Cancel";
            this.CancelChangeButton.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.EditDebugOutput);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(174, 52);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(368, 61);
            this.flowLayoutPanel1.TabIndex = 3;
            // 
            // EditDebugOutput
            // 
            this.EditDebugOutput.AutoSize = true;
            this.EditDebugOutput.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.EditDebugOutput.Location = new System.Drawing.Point(3, 0);
            this.EditDebugOutput.Name = "EditDebugOutput";
            this.EditDebugOutput.Size = new System.Drawing.Size(0, 13);
            this.EditDebugOutput.TabIndex = 0;
            // 
            // EditForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(554, 120);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.CancelChangeButton);
            this.Controls.Add(this.SaveChangeButton);
            this.Controls.Add(this.EditTextBox);
            this.Name = "EditForm";
            this.Text = "Edit Command";
            this.Load += new System.EventHandler(this.EditForm_Load);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.TextBox EditTextBox;
        private System.Windows.Forms.Button SaveChangeButton;
        private System.Windows.Forms.Button CancelChangeButton;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label EditDebugOutput;
    }
}