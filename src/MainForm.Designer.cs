namespace WinDynamicDesktop
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.setLocationButton = new System.Windows.Forms.Button();
            this.locationInput = new System.Windows.Forms.TextBox();
            this.logOutput = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // setLocationButton
            // 
            this.setLocationButton.Location = new System.Drawing.Point(324, 20);
            this.setLocationButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.setLocationButton.Name = "setLocationButton";
            this.setLocationButton.Size = new System.Drawing.Size(132, 35);
            this.setLocationButton.TabIndex = 1;
            this.setLocationButton.Text = "Set Location";
            this.setLocationButton.UseVisualStyleBackColor = true;
            this.setLocationButton.Click += new System.EventHandler(this.setLocationButton_Click);
            // 
            // locationInput
            // 
            this.locationInput.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.locationInput.Location = new System.Drawing.Point(28, 23);
            this.locationInput.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.locationInput.Name = "locationInput";
            this.locationInput.Size = new System.Drawing.Size(265, 30);
            this.locationInput.TabIndex = 0;
            // 
            // logOutput
            // 
            this.logOutput.Location = new System.Drawing.Point(28, 80);
            this.logOutput.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.logOutput.Multiline = true;
            this.logOutput.Name = "logOutput";
            this.logOutput.ReadOnly = true;
            this.logOutput.Size = new System.Drawing.Size(428, 148);
            this.logOutput.TabIndex = 2;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 23F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(486, 253);
            this.Controls.Add(this.logOutput);
            this.Controls.Add(this.setLocationButton);
            this.Controls.Add(this.locationInput);
            this.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.Text = "WinDynamicDesktop";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button setLocationButton;
        private System.Windows.Forms.TextBox locationInput;
        private System.Windows.Forms.TextBox logOutput;
    }
}

