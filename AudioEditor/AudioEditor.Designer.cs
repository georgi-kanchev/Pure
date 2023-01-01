namespace AudioEditor
{
	partial class Window
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
			if(disposing && (components != null))
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
			this.components = new System.ComponentModel.Container();
			this.Screen = new System.Windows.Forms.PictureBox();
			this.Menu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.playToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			((System.ComponentModel.ISupportInitialize)(this.Screen)).BeginInit();
			this.Menu.SuspendLayout();
			this.SuspendLayout();
			// 
			// Screen
			// 
			this.Screen.ContextMenuStrip = this.Menu;
			this.Screen.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Screen.Location = new System.Drawing.Point(0, 0);
			this.Screen.Name = "Screen";
			this.Screen.Size = new System.Drawing.Size(1262, 673);
			this.Screen.TabIndex = 0;
			this.Screen.TabStop = false;
			this.Screen.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
			// 
			// Menu
			// 
			this.Menu.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.Menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.playToolStripMenuItem});
			this.Menu.Name = "Menu";
			this.Menu.Size = new System.Drawing.Size(106, 28);
			// 
			// playToolStripMenuItem
			// 
			this.playToolStripMenuItem.Name = "playToolStripMenuItem";
			this.playToolStripMenuItem.Size = new System.Drawing.Size(105, 24);
			this.playToolStripMenuItem.Text = "Play";
			this.playToolStripMenuItem.Click += new System.EventHandler(this.OnMenuPlay);
			// 
			// Window
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Black;
			this.ClientSize = new System.Drawing.Size(1262, 673);
			this.Controls.Add(this.Screen);
			this.Name = "Window";
			this.Text = "Pure - Audio Editor";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnClose);
			((System.ComponentModel.ISupportInitialize)(this.Screen)).EndInit();
			this.Menu.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private PictureBox Screen;
		private ContextMenuStrip Menu;
		private ToolStripMenuItem playToolStripMenuItem;
	}
}