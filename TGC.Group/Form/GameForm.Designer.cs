using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TGC.Group.Form
{
    partial class GameForm
    {
        private Panel panel3D;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
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
            this.panel3D = new Panel();
            this.SuspendLayout();

            // Panel3D
            this.panel3D.Dock = DockStyle.Fill;
            this.panel3D.Location = new Point(0, 0);
            this.panel3D.Name = "panel3D";
            this.panel3D.Size = new Size(784, 561);
            this.panel3D.TabIndex = 0;

            // GameForm
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(784, 561);
            this.Controls.Add(this.panel3D);
            this.Name = "GameForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Form";
            this.WindowState = FormWindowState.Maximized;
            this.FormClosing += new FormClosingEventHandler(this.GameForm_FormClosing);
            this.Load += new EventHandler(this.GameForm_Load);
            this.ResumeLayout(false);
        }

        #endregion
    }
}

