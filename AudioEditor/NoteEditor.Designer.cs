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
			this.Menu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.playToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.TableMain = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.Octave = new System.Windows.Forms.NumericUpDown();
			this.Note = new System.Windows.Forms.DomainUpDown();
			this.LabelNote = new System.Windows.Forms.Label();
			this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			this.label3 = new System.Windows.Forms.Label();
			this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
			this.TextboxMain = new System.Windows.Forms.TextBox();
			this.Menu.SuspendLayout();
			this.TableMain.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Octave)).BeginInit();
			this.tableLayoutPanel3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
			this.SuspendLayout();
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
			// TableMain
			// 
			this.TableMain.ColumnCount = 2;
			this.TableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80.21047F));
			this.TableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 19.78953F));
			this.TableMain.Controls.Add(this.tableLayoutPanel1, 1, 0);
			this.TableMain.Controls.Add(this.TextboxMain, 0, 0);
			this.TableMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TableMain.Location = new System.Drawing.Point(0, 0);
			this.TableMain.Name = "TableMain";
			this.TableMain.RowCount = 1;
			this.TableMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TableMain.Size = new System.Drawing.Size(1262, 673);
			this.TableMain.TabIndex = 1;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Outset;
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(1015, 3);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 5;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20.45113F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.218045F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 35.48872F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.54135F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 19.84962F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(244, 667);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.Controls.Add(this.label2, 0, 2);
			this.tableLayoutPanel2.Controls.Add(this.label1, 0, 1);
			this.tableLayoutPanel2.Controls.Add(this.checkBox1, 1, 2);
			this.tableLayoutPanel2.Controls.Add(this.Octave, 1, 1);
			this.tableLayoutPanel2.Controls.Add(this.Note, 1, 0);
			this.tableLayoutPanel2.Controls.Add(this.LabelNote, 0, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(5, 5);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 3;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(234, 128);
			this.tableLayoutPanel2.TabIndex = 4;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.label2.ForeColor = System.Drawing.Color.White;
			this.label2.Location = new System.Drawing.Point(4, 85);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(109, 42);
			this.label2.TabIndex = 6;
			this.label2.Text = "Sharp";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.label1.ForeColor = System.Drawing.Color.White;
			this.label1.Location = new System.Drawing.Point(4, 43);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(109, 41);
			this.label1.TabIndex = 5;
			this.label1.Text = "Octave";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// checkBox1
			// 
			this.checkBox1.AutoSize = true;
			this.checkBox1.Dock = System.Windows.Forms.DockStyle.Right;
			this.checkBox1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.checkBox1.ForeColor = System.Drawing.Color.White;
			this.checkBox1.Location = new System.Drawing.Point(154, 88);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.checkBox1.Size = new System.Drawing.Size(76, 36);
			this.checkBox1.TabIndex = 3;
			this.checkBox1.TabStop = false;
			this.checkBox1.Text = "#      ";
			this.checkBox1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox1.UseVisualStyleBackColor = true;
			// 
			// Octave
			// 
			this.Octave.BackColor = System.Drawing.Color.Black;
			this.Octave.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Octave.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.Octave.ForeColor = System.Drawing.Color.White;
			this.Octave.Location = new System.Drawing.Point(120, 46);
			this.Octave.Maximum = new decimal(new int[] {
            7,
            0,
            0,
            0});
			this.Octave.Name = "Octave";
			this.Octave.Size = new System.Drawing.Size(110, 34);
			this.Octave.TabIndex = 2;
			this.Octave.TabStop = false;
			this.Octave.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
			// 
			// Note
			// 
			this.Note.BackColor = System.Drawing.Color.Black;
			this.Note.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Note.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.Note.ForeColor = System.Drawing.Color.White;
			this.Note.Items.Add("A");
			this.Note.Items.Add("B");
			this.Note.Items.Add("C");
			this.Note.Items.Add("D");
			this.Note.Items.Add("E");
			this.Note.Items.Add("F");
			this.Note.Items.Add("G");
			this.Note.Location = new System.Drawing.Point(120, 4);
			this.Note.Name = "Note";
			this.Note.Size = new System.Drawing.Size(110, 34);
			this.Note.TabIndex = 1;
			this.Note.TabStop = false;
			this.Note.Text = "A";
			this.Note.Wrap = true;
			// 
			// LabelNote
			// 
			this.LabelNote.AutoSize = true;
			this.LabelNote.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelNote.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.LabelNote.ForeColor = System.Drawing.Color.White;
			this.LabelNote.Location = new System.Drawing.Point(4, 1);
			this.LabelNote.Name = "LabelNote";
			this.LabelNote.Size = new System.Drawing.Size(109, 41);
			this.LabelNote.TabIndex = 4;
			this.LabelNote.Text = "Note";
			this.LabelNote.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// tableLayoutPanel3
			// 
			this.tableLayoutPanel3.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
			this.tableLayoutPanel3.ColumnCount = 2;
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel3.Controls.Add(this.label3, 0, 0);
			this.tableLayoutPanel3.Controls.Add(this.numericUpDown1, 1, 0);
			this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel3.Location = new System.Drawing.Point(5, 141);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			this.tableLayoutPanel3.RowCount = 1;
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel3.Size = new System.Drawing.Size(234, 41);
			this.tableLayoutPanel3.TabIndex = 5;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label3.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.label3.ForeColor = System.Drawing.Color.White;
			this.label3.Location = new System.Drawing.Point(4, 1);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(109, 39);
			this.label3.TabIndex = 5;
			this.label3.Text = "Repeat";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// numericUpDown1
			// 
			this.numericUpDown1.BackColor = System.Drawing.Color.Black;
			this.numericUpDown1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.numericUpDown1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.numericUpDown1.ForeColor = System.Drawing.Color.White;
			this.numericUpDown1.Location = new System.Drawing.Point(120, 4);
			this.numericUpDown1.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
			this.numericUpDown1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericUpDown1.Name = "numericUpDown1";
			this.numericUpDown1.Size = new System.Drawing.Size(110, 34);
			this.numericUpDown1.TabIndex = 6;
			this.numericUpDown1.TabStop = false;
			this.numericUpDown1.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// TextboxMain
			// 
			this.TextboxMain.AcceptsReturn = true;
			this.TextboxMain.AcceptsTab = true;
			this.TextboxMain.BackColor = System.Drawing.Color.Black;
			this.TextboxMain.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.TextboxMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TextboxMain.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.TextboxMain.ForeColor = System.Drawing.Color.White;
			this.TextboxMain.Location = new System.Drawing.Point(3, 3);
			this.TextboxMain.Multiline = true;
			this.TextboxMain.Name = "TextboxMain";
			this.TextboxMain.Size = new System.Drawing.Size(1006, 667);
			this.TextboxMain.TabIndex = 1;
			this.TextboxMain.TabStop = false;
			// 
			// Window
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Black;
			this.ClientSize = new System.Drawing.Size(1262, 673);
			this.Controls.Add(this.TableMain);
			this.Name = "Window";
			this.Text = "Pure - Note Editor";
			this.Menu.ResumeLayout(false);
			this.TableMain.ResumeLayout(false);
			this.TableMain.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.Octave)).EndInit();
			this.tableLayoutPanel3.ResumeLayout(false);
			this.tableLayoutPanel3.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
		private ContextMenuStrip Menu;
		private ToolStripMenuItem playToolStripMenuItem;
		private TableLayoutPanel TableMain;
		private TableLayoutPanel tableLayoutPanel1;
		private TextBox TextboxMain;
		private DomainUpDown Note;
		private NumericUpDown Octave;
		private CheckBox checkBox1;
		private TableLayoutPanel tableLayoutPanel2;
		private Label LabelNote;
		private Label label2;
		private Label label1;
		private TableLayoutPanel tableLayoutPanel3;
		private Label label3;
		private NumericUpDown numericUpDown1;
	}
}