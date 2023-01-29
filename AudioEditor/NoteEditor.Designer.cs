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
			this.TableMain = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.Octave = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.IsSharp = new System.Windows.Forms.CheckBox();
			this.LabelNote = new System.Windows.Forms.Label();
			this.Note = new System.Windows.Forms.ComboBox();
			this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
			this.button4 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
			this.button3 = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			this.Repeat = new System.Windows.Forms.NumericUpDown();
			this.label3 = new System.Windows.Forms.Label();
			this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
			this.Preview = new System.Windows.Forms.Button();
			this.Play = new System.Windows.Forms.Button();
			this.button5 = new System.Windows.Forms.Button();
			this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel9 = new System.Windows.Forms.TableLayoutPanel();
			this.Tempo = new System.Windows.Forms.NumericUpDown();
			this.label4 = new System.Windows.Forms.Label();
			this.tableLayoutPanel8 = new System.Windows.Forms.TableLayoutPanel();
			this.label5 = new System.Windows.Forms.Label();
			this.Wave = new System.Windows.Forms.ComboBox();
			this.Notes = new System.Windows.Forms.TextBox();
			this.SaveNotes = new System.Windows.Forms.SaveFileDialog();
			this.LoadNotes = new System.Windows.Forms.OpenFileDialog();
			this.TableMain.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.tableLayoutPanel6.SuspendLayout();
			this.tableLayoutPanel5.SuspendLayout();
			this.tableLayoutPanel3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Repeat)).BeginInit();
			this.tableLayoutPanel4.SuspendLayout();
			this.tableLayoutPanel7.SuspendLayout();
			this.tableLayoutPanel9.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Tempo)).BeginInit();
			this.tableLayoutPanel8.SuspendLayout();
			this.SuspendLayout();
			// 
			// TableMain
			// 
			this.TableMain.ColumnCount = 2;
			this.TableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 250F));
			this.TableMain.Controls.Add(this.tableLayoutPanel1, 1, 0);
			this.TableMain.Controls.Add(this.Notes, 0, 0);
			this.TableMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TableMain.Location = new System.Drawing.Point(0, 0);
			this.TableMain.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.TableMain.Name = "TableMain";
			this.TableMain.RowCount = 1;
			this.TableMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TableMain.Size = new System.Drawing.Size(984, 571);
			this.TableMain.TabIndex = 1;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.InsetDouble;
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel6, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel5, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel4, 0, 2);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(737, 2);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 5;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 101F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 134F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 222F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 95F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 0F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(244, 567);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 73.30508F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 26.69492F));
			this.tableLayoutPanel2.Controls.Add(this.Octave, 1, 1);
			this.tableLayoutPanel2.Controls.Add(this.label2, 0, 2);
			this.tableLayoutPanel2.Controls.Add(this.label1, 0, 1);
			this.tableLayoutPanel2.Controls.Add(this.IsSharp, 1, 2);
			this.tableLayoutPanel2.Controls.Add(this.LabelNote, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.Note, 1, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(6, 5);
			this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 3;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 34.4086F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 38.70968F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25.80645F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(232, 97);
			this.tableLayoutPanel2.TabIndex = 4;
			// 
			// Octave
			// 
			this.Octave.BackColor = System.Drawing.Color.White;
			this.Octave.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Octave.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.Octave.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.Octave.ForeColor = System.Drawing.Color.Black;
			this.Octave.FormattingEnabled = true;
			this.Octave.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7"});
			this.Octave.Location = new System.Drawing.Point(173, 35);
			this.Octave.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.Octave.Name = "Octave";
			this.Octave.Size = new System.Drawing.Size(56, 28);
			this.Octave.TabIndex = 9;
			this.Octave.TabStop = false;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.label2.ForeColor = System.Drawing.Color.White;
			this.label2.Location = new System.Drawing.Point(3, 70);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(164, 27);
			this.label2.TabIndex = 6;
			this.label2.Text = "Is Sharp";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.label1.ForeColor = System.Drawing.Color.White;
			this.label1.Location = new System.Drawing.Point(3, 33);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(164, 37);
			this.label1.TabIndex = 5;
			this.label1.Text = "Octave";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// IsSharp
			// 
			this.IsSharp.AutoSize = true;
			this.IsSharp.Dock = System.Windows.Forms.DockStyle.Right;
			this.IsSharp.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.IsSharp.ForeColor = System.Drawing.Color.White;
			this.IsSharp.Location = new System.Drawing.Point(173, 72);
			this.IsSharp.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.IsSharp.Name = "IsSharp";
			this.IsSharp.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.IsSharp.Size = new System.Drawing.Size(56, 23);
			this.IsSharp.TabIndex = 3;
			this.IsSharp.TabStop = false;
			this.IsSharp.Text = "#      ";
			this.IsSharp.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.IsSharp.UseVisualStyleBackColor = true;
			// 
			// LabelNote
			// 
			this.LabelNote.AutoSize = true;
			this.LabelNote.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelNote.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.LabelNote.ForeColor = System.Drawing.Color.White;
			this.LabelNote.Location = new System.Drawing.Point(3, 0);
			this.LabelNote.Name = "LabelNote";
			this.LabelNote.Size = new System.Drawing.Size(164, 33);
			this.LabelNote.TabIndex = 4;
			this.LabelNote.Text = "Note";
			this.LabelNote.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// Note
			// 
			this.Note.BackColor = System.Drawing.Color.White;
			this.Note.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Note.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.Note.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.Note.ForeColor = System.Drawing.Color.Black;
			this.Note.FormattingEnabled = true;
			this.Note.Items.AddRange(new object[] {
            "A",
            "B",
            "C",
            "D",
            "E",
            "F",
            "G"});
			this.Note.Location = new System.Drawing.Point(173, 2);
			this.Note.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.Note.Name = "Note";
			this.Note.Size = new System.Drawing.Size(56, 28);
			this.Note.TabIndex = 7;
			this.Note.TabStop = false;
			// 
			// tableLayoutPanel6
			// 
			this.tableLayoutPanel6.ColumnCount = 1;
			this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel6.Controls.Add(this.button4, 0, 1);
			this.tableLayoutPanel6.Controls.Add(this.button2, 0, 0);
			this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel6.Location = new System.Drawing.Point(6, 471);
			this.tableLayoutPanel6.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.tableLayoutPanel6.Name = "tableLayoutPanel6";
			this.tableLayoutPanel6.RowCount = 2;
			this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel6.Size = new System.Drawing.Size(232, 91);
			this.tableLayoutPanel6.TabIndex = 9;
			// 
			// button4
			// 
			this.button4.BackColor = System.Drawing.Color.Black;
			this.button4.Cursor = System.Windows.Forms.Cursors.Hand;
			this.button4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button4.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.button4.ForeColor = System.Drawing.Color.White;
			this.button4.Location = new System.Drawing.Point(3, 47);
			this.button4.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.button4.Name = "button4";
			this.button4.Size = new System.Drawing.Size(226, 42);
			this.button4.TabIndex = 11;
			this.button4.TabStop = false;
			this.button4.Text = "Load";
			this.button4.UseVisualStyleBackColor = false;
			this.button4.Click += new System.EventHandler(this.OnLoad);
			// 
			// button2
			// 
			this.button2.BackColor = System.Drawing.Color.Black;
			this.button2.Cursor = System.Windows.Forms.Cursors.Hand;
			this.button2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.button2.ForeColor = System.Drawing.Color.White;
			this.button2.Location = new System.Drawing.Point(3, 2);
			this.button2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(226, 41);
			this.button2.TabIndex = 9;
			this.button2.TabStop = false;
			this.button2.Text = "Save";
			this.button2.UseVisualStyleBackColor = false;
			this.button2.Click += new System.EventHandler(this.OnSave);
			// 
			// tableLayoutPanel5
			// 
			this.tableLayoutPanel5.ColumnCount = 1;
			this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel5.Controls.Add(this.button3, 0, 1);
			this.tableLayoutPanel5.Controls.Add(this.button1, 0, 0);
			this.tableLayoutPanel5.Controls.Add(this.tableLayoutPanel3, 0, 2);
			this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel5.Location = new System.Drawing.Point(6, 109);
			this.tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.tableLayoutPanel5.Name = "tableLayoutPanel5";
			this.tableLayoutPanel5.RowCount = 3;
			this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 34.18803F));
			this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 35.04274F));
			this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 29.46428F));
			this.tableLayoutPanel5.Size = new System.Drawing.Size(232, 130);
			this.tableLayoutPanel5.TabIndex = 8;
			// 
			// button3
			// 
			this.button3.BackColor = System.Drawing.Color.Black;
			this.button3.Cursor = System.Windows.Forms.Cursors.Hand;
			this.button3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button3.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.button3.ForeColor = System.Drawing.Color.White;
			this.button3.Location = new System.Drawing.Point(3, 47);
			this.button3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(226, 42);
			this.button3.TabIndex = 10;
			this.button3.TabStop = false;
			this.button3.Text = "Add Pause";
			this.button3.UseVisualStyleBackColor = false;
			this.button3.Click += new System.EventHandler(this.OnAddPause);
			// 
			// button1
			// 
			this.button1.BackColor = System.Drawing.Color.Black;
			this.button1.Cursor = System.Windows.Forms.Cursors.Hand;
			this.button1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.button1.ForeColor = System.Drawing.Color.White;
			this.button1.Location = new System.Drawing.Point(3, 2);
			this.button1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(226, 41);
			this.button1.TabIndex = 8;
			this.button1.TabStop = false;
			this.button1.Text = "Add Note";
			this.button1.UseVisualStyleBackColor = false;
			this.button1.Click += new System.EventHandler(this.OnAddNote);
			// 
			// tableLayoutPanel3
			// 
			this.tableLayoutPanel3.ColumnCount = 2;
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 76.54868F));
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 23.45133F));
			this.tableLayoutPanel3.Controls.Add(this.Repeat, 0, 0);
			this.tableLayoutPanel3.Controls.Add(this.label3, 0, 0);
			this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 93);
			this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			this.tableLayoutPanel3.RowCount = 1;
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
			this.tableLayoutPanel3.Size = new System.Drawing.Size(226, 35);
			this.tableLayoutPanel3.TabIndex = 5;
			// 
			// Repeat
			// 
			this.Repeat.BackColor = System.Drawing.Color.Black;
			this.Repeat.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Repeat.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.Repeat.ForeColor = System.Drawing.Color.White;
			this.Repeat.Location = new System.Drawing.Point(176, 2);
			this.Repeat.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.Repeat.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
			this.Repeat.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.Repeat.Name = "Repeat";
			this.Repeat.Size = new System.Drawing.Size(47, 27);
			this.Repeat.TabIndex = 7;
			this.Repeat.TabStop = false;
			this.Repeat.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label3.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.label3.ForeColor = System.Drawing.Color.White;
			this.label3.Location = new System.Drawing.Point(3, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(167, 35);
			this.label3.TabIndex = 5;
			this.label3.Text = "Repeat";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// tableLayoutPanel4
			// 
			this.tableLayoutPanel4.ColumnCount = 1;
			this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel4.Controls.Add(this.Preview, 0, 0);
			this.tableLayoutPanel4.Controls.Add(this.Play, 0, 1);
			this.tableLayoutPanel4.Controls.Add(this.button5, 0, 2);
			this.tableLayoutPanel4.Controls.Add(this.tableLayoutPanel7, 0, 3);
			this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel4.Location = new System.Drawing.Point(6, 246);
			this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.tableLayoutPanel4.Name = "tableLayoutPanel4";
			this.tableLayoutPanel4.RowCount = 4;
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.12102F));
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.12102F));
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.75796F));
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 76F));
			this.tableLayoutPanel4.Size = new System.Drawing.Size(232, 218);
			this.tableLayoutPanel4.TabIndex = 7;
			// 
			// Preview
			// 
			this.Preview.BackColor = System.Drawing.Color.Black;
			this.Preview.Cursor = System.Windows.Forms.Cursors.Hand;
			this.Preview.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Preview.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.Preview.ForeColor = System.Drawing.Color.White;
			this.Preview.Location = new System.Drawing.Point(3, 2);
			this.Preview.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.Preview.Name = "Preview";
			this.Preview.Size = new System.Drawing.Size(226, 43);
			this.Preview.TabIndex = 7;
			this.Preview.TabStop = false;
			this.Preview.Text = "Play Note";
			this.Preview.UseVisualStyleBackColor = false;
			this.Preview.Click += new System.EventHandler(this.OnPlayNote);
			// 
			// Play
			// 
			this.Play.BackColor = System.Drawing.Color.Black;
			this.Play.Cursor = System.Windows.Forms.Cursors.Hand;
			this.Play.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Play.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.Play.ForeColor = System.Drawing.Color.White;
			this.Play.Location = new System.Drawing.Point(3, 49);
			this.Play.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.Play.Name = "Play";
			this.Play.Size = new System.Drawing.Size(226, 43);
			this.Play.TabIndex = 6;
			this.Play.TabStop = false;
			this.Play.Text = "Play From Cursor";
			this.Play.UseVisualStyleBackColor = false;
			this.Play.Click += new System.EventHandler(this.OnPlayFromCursor);
			// 
			// button5
			// 
			this.button5.BackColor = System.Drawing.Color.Black;
			this.button5.Cursor = System.Windows.Forms.Cursors.Hand;
			this.button5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button5.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.button5.ForeColor = System.Drawing.Color.White;
			this.button5.Location = new System.Drawing.Point(3, 96);
			this.button5.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.button5.Name = "button5";
			this.button5.Size = new System.Drawing.Size(226, 43);
			this.button5.TabIndex = 8;
			this.button5.TabStop = false;
			this.button5.Text = "Stop";
			this.button5.UseVisualStyleBackColor = false;
			this.button5.Click += new System.EventHandler(this.OnStop);
			// 
			// tableLayoutPanel7
			// 
			this.tableLayoutPanel7.ColumnCount = 1;
			this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 58.26087F));
			this.tableLayoutPanel7.Controls.Add(this.tableLayoutPanel9, 0, 0);
			this.tableLayoutPanel7.Controls.Add(this.tableLayoutPanel8, 0, 1);
			this.tableLayoutPanel7.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel7.Location = new System.Drawing.Point(3, 143);
			this.tableLayoutPanel7.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.tableLayoutPanel7.Name = "tableLayoutPanel7";
			this.tableLayoutPanel7.RowCount = 2;
			this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 46.47887F));
			this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 53.52113F));
			this.tableLayoutPanel7.Size = new System.Drawing.Size(226, 73);
			this.tableLayoutPanel7.TabIndex = 0;
			// 
			// tableLayoutPanel9
			// 
			this.tableLayoutPanel9.ColumnCount = 2;
			this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 69.54546F));
			this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30.45455F));
			this.tableLayoutPanel9.Controls.Add(this.Tempo, 1, 0);
			this.tableLayoutPanel9.Controls.Add(this.label4, 0, 0);
			this.tableLayoutPanel9.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel9.Location = new System.Drawing.Point(3, 2);
			this.tableLayoutPanel9.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.tableLayoutPanel9.Name = "tableLayoutPanel9";
			this.tableLayoutPanel9.RowCount = 1;
			this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel9.Size = new System.Drawing.Size(220, 29);
			this.tableLayoutPanel9.TabIndex = 12;
			// 
			// Tempo
			// 
			this.Tempo.BackColor = System.Drawing.Color.Black;
			this.Tempo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Tempo.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.Tempo.ForeColor = System.Drawing.Color.White;
			this.Tempo.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.Tempo.Location = new System.Drawing.Point(156, 2);
			this.Tempo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.Tempo.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
			this.Tempo.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.Tempo.Name = "Tempo";
			this.Tempo.Size = new System.Drawing.Size(61, 27);
			this.Tempo.TabIndex = 7;
			this.Tempo.TabStop = false;
			this.Tempo.Value = new decimal(new int[] {
            120,
            0,
            0,
            0});
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label4.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.label4.ForeColor = System.Drawing.Color.White;
			this.label4.Location = new System.Drawing.Point(3, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(147, 29);
			this.label4.TabIndex = 8;
			this.label4.Text = "Tempo (BPM)";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// tableLayoutPanel8
			// 
			this.tableLayoutPanel8.ColumnCount = 2;
			this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 42.41071F));
			this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 57.58929F));
			this.tableLayoutPanel8.Controls.Add(this.label5, 0, 0);
			this.tableLayoutPanel8.Controls.Add(this.Wave, 1, 0);
			this.tableLayoutPanel8.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel8.Location = new System.Drawing.Point(3, 35);
			this.tableLayoutPanel8.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.tableLayoutPanel8.Name = "tableLayoutPanel8";
			this.tableLayoutPanel8.RowCount = 1;
			this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel8.Size = new System.Drawing.Size(220, 36);
			this.tableLayoutPanel8.TabIndex = 11;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label5.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.label5.ForeColor = System.Drawing.Color.White;
			this.label5.Location = new System.Drawing.Point(3, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(87, 36);
			this.label5.TabIndex = 10;
			this.label5.Text = "Wave";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// Wave
			// 
			this.Wave.BackColor = System.Drawing.Color.White;
			this.Wave.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Wave.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.Wave.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.Wave.ForeColor = System.Drawing.Color.Black;
			this.Wave.FormattingEnabled = true;
			this.Wave.Items.AddRange(new object[] {
            "Sine",
            "Square",
            "Triangle",
            "Sawtooth",
            "Noise"});
			this.Wave.Location = new System.Drawing.Point(96, 2);
			this.Wave.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.Wave.Name = "Wave";
			this.Wave.Size = new System.Drawing.Size(121, 28);
			this.Wave.TabIndex = 10;
			this.Wave.TabStop = false;
			// 
			// Notes
			// 
			this.Notes.AcceptsReturn = true;
			this.Notes.AcceptsTab = true;
			this.Notes.BackColor = System.Drawing.Color.Black;
			this.Notes.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.Notes.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Notes.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.Notes.ForeColor = System.Drawing.Color.White;
			this.Notes.Location = new System.Drawing.Point(3, 2);
			this.Notes.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.Notes.Multiline = true;
			this.Notes.Name = "Notes";
			this.Notes.Size = new System.Drawing.Size(728, 567);
			this.Notes.TabIndex = 1;
			this.Notes.TabStop = false;
			// 
			// SaveNotes
			// 
			this.SaveNotes.Title = "Save Notes";
			// 
			// LoadNotes
			// 
			this.LoadNotes.FileName = "notes";
			this.LoadNotes.Title = "Load Notes";
			// 
			// Window
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Black;
			this.ClientSize = new System.Drawing.Size(984, 571);
			this.Controls.Add(this.TableMain);
			this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.MinimumSize = new System.Drawing.Size(500, 610);
			this.Name = "Window";
			this.Text = "Pure - Note Editor";
			this.TableMain.ResumeLayout(false);
			this.TableMain.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			this.tableLayoutPanel6.ResumeLayout(false);
			this.tableLayoutPanel5.ResumeLayout(false);
			this.tableLayoutPanel3.ResumeLayout(false);
			this.tableLayoutPanel3.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.Repeat)).EndInit();
			this.tableLayoutPanel4.ResumeLayout(false);
			this.tableLayoutPanel7.ResumeLayout(false);
			this.tableLayoutPanel9.ResumeLayout(false);
			this.tableLayoutPanel9.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.Tempo)).EndInit();
			this.tableLayoutPanel8.ResumeLayout(false);
			this.tableLayoutPanel8.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion
		private TableLayoutPanel TableMain;
		private TableLayoutPanel tableLayoutPanel1;
		private TextBox Notes;
		private CheckBox IsSharp;
		private TableLayoutPanel tableLayoutPanel2;
		private Label LabelNote;
		private Label label2;
		private Label label1;
		private TableLayoutPanel tableLayoutPanel3;
		private Label label3;
		private Button Play;
		private TableLayoutPanel tableLayoutPanel4;
		private Button Preview;
		private TableLayoutPanel tableLayoutPanel5;
		private Button button3;
		private Button button1;
		private ComboBox Note;
		private ComboBox Octave;
		private TableLayoutPanel tableLayoutPanel6;
		private Button button4;
		private Button button2;
		private TableLayoutPanel tableLayoutPanel7;
		private NumericUpDown Tempo;
		private Label label4;
		private ComboBox Wave;
		private TableLayoutPanel tableLayoutPanel9;
		private TableLayoutPanel tableLayoutPanel8;
		private Label label5;
		private Button button5;
		private SaveFileDialog SaveNotes;
		private OpenFileDialog LoadNotes;
		private NumericUpDown Repeat;
	}
}