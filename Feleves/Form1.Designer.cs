namespace Feleves
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Browse = new System.Windows.Forms.Button();
            this.originalImage = new System.Windows.Forms.PictureBox();
            this.newImage = new System.Windows.Forms.PictureBox();
            this.ASCIIArt = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.originalImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.newImage)).BeginInit();
            this.SuspendLayout();
            // 
            // Browse
            // 
            this.Browse.Location = new System.Drawing.Point(418, 12);
            this.Browse.Name = "Browse";
            this.Browse.Size = new System.Drawing.Size(75, 23);
            this.Browse.TabIndex = 0;
            this.Browse.Text = "Browse";
            this.Browse.UseVisualStyleBackColor = true;
            this.Browse.Click += new System.EventHandler(this.Browse_Click);
            // 
            // originalImage
            // 
            this.originalImage.Location = new System.Drawing.Point(12, 12);
            this.originalImage.Name = "originalImage";
            this.originalImage.Size = new System.Drawing.Size(400, 400);
            this.originalImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.originalImage.TabIndex = 1;
            this.originalImage.TabStop = false;
            // 
            // newImage
            // 
            this.newImage.Location = new System.Drawing.Point(499, 12);
            this.newImage.Name = "newImage";
            this.newImage.Size = new System.Drawing.Size(400, 400);
            this.newImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.newImage.TabIndex = 2;
            this.newImage.TabStop = false;
            // 
            // ASCIIArt
            // 
            this.ASCIIArt.Location = new System.Drawing.Point(418, 41);
            this.ASCIIArt.Name = "ASCIIArt";
            this.ASCIIArt.Size = new System.Drawing.Size(75, 23);
            this.ASCIIArt.TabIndex = 3;
            this.ASCIIArt.Text = "ASCII Art";
            this.ASCIIArt.UseVisualStyleBackColor = true;
            this.ASCIIArt.Click += new System.EventHandler(this.ASCIIArt_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(912, 425);
            this.Controls.Add(this.ASCIIArt);
            this.Controls.Add(this.newImage);
            this.Controls.Add(this.originalImage);
            this.Controls.Add(this.Browse);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.originalImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.newImage)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Button Browse;
        private PictureBox originalImage;
        private PictureBox newImage;
        private Button ASCIIArt;
    }
}