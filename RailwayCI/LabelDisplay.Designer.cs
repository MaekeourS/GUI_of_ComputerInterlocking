namespace RailwayCI
{
    partial class LabelDisplay
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
            this.SignalName = new System.Windows.Forms.CheckBox();
            this.TurningName = new System.Windows.Forms.CheckBox();
            this.RailName = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // SignalName
            // 
            this.SignalName.AutoSize = true;
            this.SignalName.Font = new System.Drawing.Font("宋体", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.SignalName.Location = new System.Drawing.Point(12, 12);
            this.SignalName.Name = "SignalName";
            this.SignalName.Size = new System.Drawing.Size(169, 37);
            this.SignalName.TabIndex = 0;
            this.SignalName.Text = "信号名称";
            this.SignalName.UseVisualStyleBackColor = true;
            this.SignalName.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // TurningName
            // 
            this.TurningName.AutoSize = true;
            this.TurningName.Font = new System.Drawing.Font("宋体", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.TurningName.Location = new System.Drawing.Point(12, 55);
            this.TurningName.Name = "TurningName";
            this.TurningName.Size = new System.Drawing.Size(169, 37);
            this.TurningName.TabIndex = 1;
            this.TurningName.Text = "渡线名称";
            this.TurningName.UseVisualStyleBackColor = true;
            this.TurningName.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // RailName
            // 
            this.RailName.AutoSize = true;
            this.RailName.Font = new System.Drawing.Font("宋体", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.RailName.Location = new System.Drawing.Point(12, 98);
            this.RailName.Name = "RailName";
            this.RailName.Size = new System.Drawing.Size(169, 37);
            this.RailName.TabIndex = 2;
            this.RailName.Text = "区段名称";
            this.RailName.UseVisualStyleBackColor = true;
            this.RailName.Click += new System.EventHandler(this.CheckedChanged);
            // 
            // LabelDisplay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(268, 153);
            this.Controls.Add(this.RailName);
            this.Controls.Add(this.TurningName);
            this.Controls.Add(this.SignalName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LabelDisplay";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "内容显示开关";
            this.Load += new System.EventHandler(this.LabelDisplay_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox SignalName;
        private System.Windows.Forms.CheckBox TurningName;
        private System.Windows.Forms.CheckBox RailName;
    }
}