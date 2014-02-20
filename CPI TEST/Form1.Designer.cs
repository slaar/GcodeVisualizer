namespace CPI_TEST
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.showButton = new System.Windows.Forms.Button();
            this.lineButton = new System.Windows.Forms.Button();
            this.loadButton = new System.Windows.Forms.Button();
            this.forwardButton = new System.Windows.Forms.Button();
            this.debugOut = new System.Windows.Forms.Label();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.Arena = new OpenTK.GLControl();
            this.debugText = new System.Windows.Forms.RichTextBox();
            this.LoadGCODEFile = new System.Windows.Forms.OpenFileDialog();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(816, 392);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.showButton);
            this.flowLayoutPanel1.Controls.Add(this.lineButton);
            this.flowLayoutPanel1.Controls.Add(this.loadButton);
            this.flowLayoutPanel1.Controls.Add(this.forwardButton);
            this.flowLayoutPanel1.Controls.Add(this.debugOut);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 360);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(810, 29);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // showButton
            // 
            this.showButton.Location = new System.Drawing.Point(3, 3);
            this.showButton.Name = "showButton";
            this.showButton.Size = new System.Drawing.Size(75, 23);
            this.showButton.TabIndex = 0;
            this.showButton.Text = "ALIEN";
            this.showButton.UseVisualStyleBackColor = true;
            this.showButton.Click += new System.EventHandler(this.showButton_Click);
            // 
            // lineButton
            // 
            this.lineButton.Location = new System.Drawing.Point(84, 3);
            this.lineButton.Name = "lineButton";
            this.lineButton.Size = new System.Drawing.Size(75, 23);
            this.lineButton.TabIndex = 1;
            this.lineButton.Text = "SMILE";
            this.lineButton.UseVisualStyleBackColor = true;
            this.lineButton.Click += new System.EventHandler(this.lineButton_Click);
            // 
            // loadButton
            // 
            this.loadButton.Location = new System.Drawing.Point(165, 3);
            this.loadButton.Name = "loadButton";
            this.loadButton.Size = new System.Drawing.Size(75, 23);
            this.loadButton.TabIndex = 2;
            this.loadButton.Text = "Load";
            this.loadButton.UseVisualStyleBackColor = true;
            this.loadButton.Click += new System.EventHandler(this.loadButton_Click);
            // 
            // forwardButton
            // 
            this.forwardButton.Location = new System.Drawing.Point(246, 3);
            this.forwardButton.Name = "forwardButton";
            this.forwardButton.Size = new System.Drawing.Size(75, 23);
            this.forwardButton.TabIndex = 3;
            this.forwardButton.Text = "Forward";
            this.forwardButton.UseVisualStyleBackColor = true;
            this.forwardButton.Click += new System.EventHandler(this.forwardButton_Click);
            // 
            // debugOut
            // 
            this.debugOut.AutoSize = true;
            this.debugOut.Location = new System.Drawing.Point(327, 0);
            this.debugOut.Name = "debugOut";
            this.debugOut.Size = new System.Drawing.Size(35, 13);
            this.debugOut.TabIndex = 4;
            this.debugOut.Text = "label1";
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.Arena);
            this.flowLayoutPanel2.Controls.Add(this.debugText);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(810, 351);
            this.flowLayoutPanel2.TabIndex = 1;
            // 
            // Arena
            // 
            this.Arena.BackColor = System.Drawing.Color.Black;
            this.Arena.Location = new System.Drawing.Point(3, 3);
            this.Arena.Name = "Arena";
            this.Arena.Size = new System.Drawing.Size(417, 348);
            this.Arena.TabIndex = 0;
            this.Arena.VSync = false;
            this.Arena.Load += new System.EventHandler(this.Arena_Load);
            this.Arena.Paint += new System.Windows.Forms.PaintEventHandler(this.Arena_Paint);
            this.Arena.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Arena_MouseClick);
            this.Arena.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Arena_MouseDown);
            this.Arena.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Arena_MouseMove);
            this.Arena.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Arena_MouseUp);
            this.Arena.Resize += new System.EventHandler(this.Arena_Resize);
            // 
            // debugText
            // 
            this.debugText.Location = new System.Drawing.Point(426, 3);
            this.debugText.Name = "debugText";
            this.debugText.Size = new System.Drawing.Size(375, 348);
            this.debugText.TabIndex = 1;
            this.debugText.Text = "";
            // 
            // LoadGCODEFile
            // 
            this.LoadGCODEFile.FileName = "openFileDialog1";
            this.LoadGCODEFile.Filter = "GCODE files (*.NC)|*.NC|All files (*.*)|*.*";
            this.LoadGCODEFile.FileOk += new System.ComponentModel.CancelEventHandler(this.LoadGCODEFile_FileOk);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(816, 392);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "MainForm";
            this.Text = "CPI Test";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button showButton;
        private System.Windows.Forms.Button lineButton;
        private System.Windows.Forms.Button loadButton;
        private System.Windows.Forms.OpenFileDialog LoadGCODEFile;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Button forwardButton;
        private System.Windows.Forms.Label debugOut;
        private OpenTK.GLControl Arena;
        public System.Windows.Forms.RichTextBox debugText;
    }
}

