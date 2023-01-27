namespace TilemapEditor
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
			this.TableMain = new System.Windows.Forms.TableLayoutPanel();
			this.TableEdit = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
			this.label5 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.TileOffsetHeight = new System.Windows.Forms.NumericUpDown();
			this.TileOffsetWidth = new System.Windows.Forms.NumericUpDown();
			this.TileHeight = new System.Windows.Forms.NumericUpDown();
			this.TileWidth = new System.Windows.Forms.NumericUpDown();
			this.button3 = new System.Windows.Forms.Button();
			this.Set = new System.Windows.Forms.PictureBox();
			this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			this.ColorBackground = new System.Windows.Forms.PictureBox();
			this.ColorGrid5 = new System.Windows.Forms.PictureBox();
			this.label9 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.ColorGrid1 = new System.Windows.Forms.PictureBox();
			this.label6 = new System.Windows.Forms.Label();
			this.ColorSelection = new System.Windows.Forms.PictureBox();
			this.label2 = new System.Windows.Forms.Label();
			this.ColorBrush = new System.Windows.Forms.PictureBox();
			this.label1 = new System.Windows.Forms.Label();
			this.Layers = new System.Windows.Forms.CheckedListBox();
			this.LayerMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.renameRToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.moveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.topToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.upToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.downToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.bottomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
			this.Map = new System.Windows.Forms.PictureBox();
			this.tableLayoutPanel8 = new System.Windows.Forms.TableLayoutPanel();
			this.label8 = new System.Windows.Forms.Label();
			this.MapHeight = new System.Windows.Forms.NumericUpDown();
			this.MapWidth = new System.Windows.Forms.NumericUpDown();
			this.TileHovered = new System.Windows.Forms.Label();
			this.Colors = new System.Windows.Forms.ColorDialog();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.button1 = new System.Windows.Forms.Button();
			this.button4 = new System.Windows.Forms.Button();
			this.LoadTileset = new System.Windows.Forms.OpenFileDialog();
			this.TableMain.SuspendLayout();
			this.TableEdit.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel5.SuspendLayout();
			this.tableLayoutPanel6.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TileOffsetHeight)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TileOffsetWidth)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TileHeight)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TileWidth)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Set)).BeginInit();
			this.tableLayoutPanel3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ColorBackground)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ColorGrid5)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ColorGrid1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ColorSelection)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ColorBrush)).BeginInit();
			this.LayerMenu.SuspendLayout();
			this.tableLayoutPanel7.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Map)).BeginInit();
			this.tableLayoutPanel8.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MapHeight)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MapWidth)).BeginInit();
			this.tableLayoutPanel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// TableMain
			// 
			this.TableMain.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Inset;
			this.TableMain.ColumnCount = 2;
			this.TableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 330F));
			this.TableMain.Controls.Add(this.TableEdit, 1, 0);
			this.TableMain.Controls.Add(this.tableLayoutPanel7, 0, 0);
			this.TableMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TableMain.Location = new System.Drawing.Point(0, 0);
			this.TableMain.Name = "TableMain";
			this.TableMain.RowCount = 1;
			this.TableMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TableMain.Size = new System.Drawing.Size(1264, 681);
			this.TableMain.TabIndex = 0;
			// 
			// TableEdit
			// 
			this.TableEdit.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Inset;
			this.TableEdit.ColumnCount = 1;
			this.TableEdit.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TableEdit.Controls.Add(this.tableLayoutPanel1, 0, 2);
			this.TableEdit.Controls.Add(this.tableLayoutPanel3, 0, 1);
			this.TableEdit.Controls.Add(this.Layers, 0, 0);
			this.TableEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TableEdit.Location = new System.Drawing.Point(935, 5);
			this.TableEdit.Name = "TableEdit";
			this.TableEdit.RowCount = 3;
			this.TableEdit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 186F));
			this.TableEdit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 125F));
			this.TableEdit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TableEdit.Size = new System.Drawing.Size(324, 671);
			this.TableEdit.TabIndex = 1;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Inset;
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel5, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.Set, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(5, 320);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 71F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(314, 346);
			this.tableLayoutPanel1.TabIndex = 7;
			// 
			// tableLayoutPanel5
			// 
			this.tableLayoutPanel5.ColumnCount = 2;
			this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 62.82895F));
			this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 37.17105F));
			this.tableLayoutPanel5.Controls.Add(this.tableLayoutPanel6, 0, 0);
			this.tableLayoutPanel5.Controls.Add(this.button3, 1, 0);
			this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel5.Location = new System.Drawing.Point(5, 276);
			this.tableLayoutPanel5.Name = "tableLayoutPanel5";
			this.tableLayoutPanel5.RowCount = 1;
			this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel5.Size = new System.Drawing.Size(304, 65);
			this.tableLayoutPanel5.TabIndex = 9;
			// 
			// tableLayoutPanel6
			// 
			this.tableLayoutPanel6.ColumnCount = 3;
			this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel6.Controls.Add(this.label5, 0, 1);
			this.tableLayoutPanel6.Controls.Add(this.label4, 0, 0);
			this.tableLayoutPanel6.Controls.Add(this.TileOffsetHeight, 2, 1);
			this.tableLayoutPanel6.Controls.Add(this.TileOffsetWidth, 1, 1);
			this.tableLayoutPanel6.Controls.Add(this.TileHeight, 2, 0);
			this.tableLayoutPanel6.Controls.Add(this.TileWidth, 1, 0);
			this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel6.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanel6.Name = "tableLayoutPanel6";
			this.tableLayoutPanel6.RowCount = 2;
			this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel6.Size = new System.Drawing.Size(185, 59);
			this.tableLayoutPanel6.TabIndex = 7;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label5.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.label5.Location = new System.Drawing.Point(3, 29);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(55, 30);
			this.label5.TabIndex = 5;
			this.label5.Text = "Offset";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label4.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.label4.Location = new System.Drawing.Point(3, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(55, 29);
			this.label4.TabIndex = 4;
			this.label4.Text = "Size";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// TileOffsetHeight
			// 
			this.TileOffsetHeight.BackColor = System.Drawing.Color.Black;
			this.TileOffsetHeight.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TileOffsetHeight.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.TileOffsetHeight.ForeColor = System.Drawing.Color.White;
			this.TileOffsetHeight.Location = new System.Drawing.Point(125, 32);
			this.TileOffsetHeight.Maximum = new decimal(new int[] {
            16,
            0,
            0,
            0});
			this.TileOffsetHeight.Name = "TileOffsetHeight";
			this.TileOffsetHeight.Size = new System.Drawing.Size(57, 27);
			this.TileOffsetHeight.TabIndex = 3;
			this.TileOffsetHeight.TabStop = false;
			this.TileOffsetHeight.ValueChanged += new System.EventHandler(this.OnNumericValueChange);
			// 
			// TileOffsetWidth
			// 
			this.TileOffsetWidth.BackColor = System.Drawing.Color.Black;
			this.TileOffsetWidth.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TileOffsetWidth.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.TileOffsetWidth.ForeColor = System.Drawing.Color.White;
			this.TileOffsetWidth.Location = new System.Drawing.Point(64, 32);
			this.TileOffsetWidth.Maximum = new decimal(new int[] {
            16,
            0,
            0,
            0});
			this.TileOffsetWidth.Name = "TileOffsetWidth";
			this.TileOffsetWidth.Size = new System.Drawing.Size(55, 27);
			this.TileOffsetWidth.TabIndex = 2;
			this.TileOffsetWidth.TabStop = false;
			this.TileOffsetWidth.ValueChanged += new System.EventHandler(this.OnNumericValueChange);
			// 
			// TileHeight
			// 
			this.TileHeight.BackColor = System.Drawing.Color.Black;
			this.TileHeight.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TileHeight.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.TileHeight.ForeColor = System.Drawing.Color.White;
			this.TileHeight.Increment = new decimal(new int[] {
            4,
            0,
            0,
            0});
			this.TileHeight.Location = new System.Drawing.Point(125, 3);
			this.TileHeight.Maximum = new decimal(new int[] {
            128,
            0,
            0,
            0});
			this.TileHeight.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.TileHeight.Name = "TileHeight";
			this.TileHeight.Size = new System.Drawing.Size(57, 27);
			this.TileHeight.TabIndex = 1;
			this.TileHeight.TabStop = false;
			this.TileHeight.Value = new decimal(new int[] {
            16,
            0,
            0,
            0});
			this.TileHeight.ValueChanged += new System.EventHandler(this.OnNumericValueChange);
			// 
			// TileWidth
			// 
			this.TileWidth.BackColor = System.Drawing.Color.Black;
			this.TileWidth.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TileWidth.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.TileWidth.ForeColor = System.Drawing.Color.White;
			this.TileWidth.Increment = new decimal(new int[] {
            4,
            0,
            0,
            0});
			this.TileWidth.Location = new System.Drawing.Point(64, 3);
			this.TileWidth.Maximum = new decimal(new int[] {
            128,
            0,
            0,
            0});
			this.TileWidth.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.TileWidth.Name = "TileWidth";
			this.TileWidth.Size = new System.Drawing.Size(55, 27);
			this.TileWidth.TabIndex = 0;
			this.TileWidth.TabStop = false;
			this.TileWidth.Value = new decimal(new int[] {
            16,
            0,
            0,
            0});
			this.TileWidth.ValueChanged += new System.EventHandler(this.OnNumericValueChange);
			// 
			// button3
			// 
			this.button3.BackColor = System.Drawing.Color.Black;
			this.button3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button3.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.button3.ForeColor = System.Drawing.Color.White;
			this.button3.Location = new System.Drawing.Point(194, 3);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(107, 59);
			this.button3.TabIndex = 2;
			this.button3.TabStop = false;
			this.button3.Text = "Tileset";
			this.button3.UseVisualStyleBackColor = false;
			this.button3.Click += new System.EventHandler(this.OnSetLoadClick);
			// 
			// Set
			// 
			this.Set.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Set.Location = new System.Drawing.Point(5, 5);
			this.Set.Name = "Set";
			this.Set.Size = new System.Drawing.Size(304, 263);
			this.Set.TabIndex = 10;
			this.Set.TabStop = false;
			this.Set.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnSetPress);
			this.Set.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnSetMouseMove);
			// 
			// tableLayoutPanel3
			// 
			this.tableLayoutPanel3.ColumnCount = 2;
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 76.08069F));
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 23.91931F));
			this.tableLayoutPanel3.Controls.Add(this.ColorBackground, 1, 4);
			this.tableLayoutPanel3.Controls.Add(this.ColorGrid5, 1, 3);
			this.tableLayoutPanel3.Controls.Add(this.label9, 0, 4);
			this.tableLayoutPanel3.Controls.Add(this.label7, 0, 3);
			this.tableLayoutPanel3.Controls.Add(this.ColorGrid1, 1, 2);
			this.tableLayoutPanel3.Controls.Add(this.label6, 0, 2);
			this.tableLayoutPanel3.Controls.Add(this.ColorSelection, 1, 1);
			this.tableLayoutPanel3.Controls.Add(this.label2, 0, 1);
			this.tableLayoutPanel3.Controls.Add(this.ColorBrush, 1, 0);
			this.tableLayoutPanel3.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel3.Location = new System.Drawing.Point(5, 193);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			this.tableLayoutPanel3.RowCount = 5;
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel3.Size = new System.Drawing.Size(314, 119);
			this.tableLayoutPanel3.TabIndex = 6;
			// 
			// ColorBackground
			// 
			this.ColorBackground.BackColor = System.Drawing.Color.Black;
			this.ColorBackground.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.ColorBackground.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ColorBackground.Location = new System.Drawing.Point(241, 95);
			this.ColorBackground.Name = "ColorBackground";
			this.ColorBackground.Size = new System.Drawing.Size(70, 21);
			this.ColorBackground.TabIndex = 16;
			this.ColorBackground.TabStop = false;
			this.ColorBackground.Click += new System.EventHandler(this.OnColorBackgroundClick);
			// 
			// ColorGrid5
			// 
			this.ColorGrid5.BackColor = System.Drawing.Color.Brown;
			this.ColorGrid5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.ColorGrid5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ColorGrid5.Location = new System.Drawing.Point(241, 72);
			this.ColorGrid5.Name = "ColorGrid5";
			this.ColorGrid5.Size = new System.Drawing.Size(70, 17);
			this.ColorGrid5.TabIndex = 15;
			this.ColorGrid5.TabStop = false;
			this.ColorGrid5.Click += new System.EventHandler(this.OnColorGrid5Click);
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label9.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.label9.Location = new System.Drawing.Point(3, 92);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(232, 27);
			this.label9.TabIndex = 14;
			this.label9.Text = "Background";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.label9.Click += new System.EventHandler(this.OnColorBackgroundClick);
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label7.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.label7.Location = new System.Drawing.Point(3, 69);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(232, 23);
			this.label7.TabIndex = 13;
			this.label7.Text = "Grid 5";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.label7.Click += new System.EventHandler(this.OnColorGrid5Click);
			// 
			// ColorGrid1
			// 
			this.ColorGrid1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.ColorGrid1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.ColorGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ColorGrid1.Location = new System.Drawing.Point(241, 49);
			this.ColorGrid1.Name = "ColorGrid1";
			this.ColorGrid1.Size = new System.Drawing.Size(70, 17);
			this.ColorGrid1.TabIndex = 12;
			this.ColorGrid1.TabStop = false;
			this.ColorGrid1.Click += new System.EventHandler(this.OnColorGrid1Click);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label6.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.label6.Location = new System.Drawing.Point(3, 46);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(232, 23);
			this.label6.TabIndex = 11;
			this.label6.Text = "Grid 1";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.label6.Click += new System.EventHandler(this.OnColorGrid1Click);
			// 
			// ColorSelection
			// 
			this.ColorSelection.BackColor = System.Drawing.Color.LightSkyBlue;
			this.ColorSelection.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.ColorSelection.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ColorSelection.Location = new System.Drawing.Point(241, 26);
			this.ColorSelection.Name = "ColorSelection";
			this.ColorSelection.Size = new System.Drawing.Size(70, 17);
			this.ColorSelection.TabIndex = 6;
			this.ColorSelection.TabStop = false;
			this.ColorSelection.Click += new System.EventHandler(this.OnColorSelectionClick);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label2.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.label2.Location = new System.Drawing.Point(3, 23);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(232, 23);
			this.label2.TabIndex = 5;
			this.label2.Text = "Selection";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.label2.Click += new System.EventHandler(this.OnColorSelectionClick);
			// 
			// ColorBrush
			// 
			this.ColorBrush.BackColor = System.Drawing.Color.White;
			this.ColorBrush.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.ColorBrush.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ColorBrush.Location = new System.Drawing.Point(241, 3);
			this.ColorBrush.Name = "ColorBrush";
			this.ColorBrush.Size = new System.Drawing.Size(70, 17);
			this.ColorBrush.TabIndex = 4;
			this.ColorBrush.TabStop = false;
			this.ColorBrush.Click += new System.EventHandler(this.OnColorBrushClick);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.label1.Location = new System.Drawing.Point(3, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(232, 23);
			this.label1.TabIndex = 0;
			this.label1.Text = "Brush";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.label1.Click += new System.EventHandler(this.OnColorBrushClick);
			// 
			// Layers
			// 
			this.Layers.BackColor = System.Drawing.Color.Black;
			this.Layers.ContextMenuStrip = this.LayerMenu;
			this.Layers.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Layers.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.Layers.ForeColor = System.Drawing.Color.White;
			this.Layers.FormattingEnabled = true;
			this.Layers.Location = new System.Drawing.Point(5, 5);
			this.Layers.Name = "Layers";
			this.Layers.Size = new System.Drawing.Size(314, 180);
			this.Layers.TabIndex = 9;
			this.Layers.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnLayerPress);
			// 
			// LayerMenu
			// 
			this.LayerMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.toolStripSeparator1,
            this.renameRToolStripMenuItem,
            this.moveToolStripMenuItem,
            this.removeToolStripMenuItem});
			this.LayerMenu.Name = "LayersMenu";
			this.LayerMenu.Size = new System.Drawing.Size(162, 98);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(161, 22);
			this.toolStripMenuItem1.Text = "Add";
			this.toolStripMenuItem1.Click += new System.EventHandler(this.OnLayerAdd);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(158, 6);
			// 
			// renameRToolStripMenuItem
			// 
			this.renameRToolStripMenuItem.Name = "renameRToolStripMenuItem";
			this.renameRToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
			this.renameRToolStripMenuItem.Text = "Rename";
			this.renameRToolStripMenuItem.Click += new System.EventHandler(this.OnLayerRename);
			// 
			// moveToolStripMenuItem
			// 
			this.moveToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.topToolStripMenuItem,
            this.upToolStripMenuItem,
            this.downToolStripMenuItem,
            this.bottomToolStripMenuItem});
			this.moveToolStripMenuItem.Name = "moveToolStripMenuItem";
			this.moveToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
			this.moveToolStripMenuItem.Text = "Move";
			// 
			// topToolStripMenuItem
			// 
			this.topToolStripMenuItem.Name = "topToolStripMenuItem";
			this.topToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
			this.topToolStripMenuItem.Text = "Top";
			this.topToolStripMenuItem.Click += new System.EventHandler(this.OnLayerMoveTop);
			// 
			// upToolStripMenuItem
			// 
			this.upToolStripMenuItem.Name = "upToolStripMenuItem";
			this.upToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
			this.upToolStripMenuItem.Text = "Up";
			this.upToolStripMenuItem.Click += new System.EventHandler(this.OnLayerMoveUp);
			// 
			// downToolStripMenuItem
			// 
			this.downToolStripMenuItem.Name = "downToolStripMenuItem";
			this.downToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
			this.downToolStripMenuItem.Text = "Down";
			this.downToolStripMenuItem.Click += new System.EventHandler(this.OnLayerMoveDown);
			// 
			// bottomToolStripMenuItem
			// 
			this.bottomToolStripMenuItem.Name = "bottomToolStripMenuItem";
			this.bottomToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
			this.bottomToolStripMenuItem.Text = "Bottom";
			this.bottomToolStripMenuItem.Click += new System.EventHandler(this.OnLayerMoveBottom);
			// 
			// removeToolStripMenuItem
			// 
			this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
			this.removeToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
			this.removeToolStripMenuItem.Text = "Remove [Delete]";
			this.removeToolStripMenuItem.Click += new System.EventHandler(this.OnLayerRemove);
			// 
			// tableLayoutPanel7
			// 
			this.tableLayoutPanel7.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Inset;
			this.tableLayoutPanel7.ColumnCount = 1;
			this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel7.Controls.Add(this.Map, 0, 0);
			this.tableLayoutPanel7.Controls.Add(this.tableLayoutPanel8, 0, 1);
			this.tableLayoutPanel7.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel7.Location = new System.Drawing.Point(5, 5);
			this.tableLayoutPanel7.Name = "tableLayoutPanel7";
			this.tableLayoutPanel7.RowCount = 2;
			this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 39F));
			this.tableLayoutPanel7.Size = new System.Drawing.Size(922, 671);
			this.tableLayoutPanel7.TabIndex = 2;
			// 
			// Map
			// 
			this.Map.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Map.Location = new System.Drawing.Point(5, 5);
			this.Map.Name = "Map";
			this.Map.Size = new System.Drawing.Size(912, 620);
			this.Map.TabIndex = 3;
			this.Map.TabStop = false;
			this.Map.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnMapPress);
			this.Map.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMapMouseMove);
			this.Map.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnMapRelease);
			// 
			// tableLayoutPanel8
			// 
			this.tableLayoutPanel8.ColumnCount = 5;
			this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
			this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
			this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 449F));
			this.tableLayoutPanel8.Controls.Add(this.label8, 0, 0);
			this.tableLayoutPanel8.Controls.Add(this.MapHeight, 2, 0);
			this.tableLayoutPanel8.Controls.Add(this.MapWidth, 1, 0);
			this.tableLayoutPanel8.Controls.Add(this.TileHovered, 4, 0);
			this.tableLayoutPanel8.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel8.Location = new System.Drawing.Point(5, 633);
			this.tableLayoutPanel8.Name = "tableLayoutPanel8";
			this.tableLayoutPanel8.RowCount = 1;
			this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel8.Size = new System.Drawing.Size(912, 33);
			this.tableLayoutPanel8.TabIndex = 4;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label8.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.label8.Location = new System.Drawing.Point(3, 0);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(44, 33);
			this.label8.TabIndex = 8;
			this.label8.Text = "Size";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// MapHeight
			// 
			this.MapHeight.BackColor = System.Drawing.Color.Black;
			this.MapHeight.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MapHeight.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.MapHeight.ForeColor = System.Drawing.Color.White;
			this.MapHeight.Location = new System.Drawing.Point(113, 3);
			this.MapHeight.Maximum = new decimal(new int[] {
            99999999,
            0,
            0,
            0});
			this.MapHeight.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.MapHeight.Name = "MapHeight";
			this.MapHeight.Size = new System.Drawing.Size(54, 27);
			this.MapHeight.TabIndex = 7;
			this.MapHeight.TabStop = false;
			this.MapHeight.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
			this.MapHeight.ValueChanged += new System.EventHandler(this.OnNumericValueChange);
			// 
			// MapWidth
			// 
			this.MapWidth.BackColor = System.Drawing.Color.Black;
			this.MapWidth.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MapWidth.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.MapWidth.ForeColor = System.Drawing.Color.White;
			this.MapWidth.Location = new System.Drawing.Point(53, 3);
			this.MapWidth.Maximum = new decimal(new int[] {
            99999999,
            0,
            0,
            0});
			this.MapWidth.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.MapWidth.Name = "MapWidth";
			this.MapWidth.Size = new System.Drawing.Size(54, 27);
			this.MapWidth.TabIndex = 6;
			this.MapWidth.TabStop = false;
			this.MapWidth.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
			this.MapWidth.ValueChanged += new System.EventHandler(this.OnNumericValueChange);
			// 
			// TileHovered
			// 
			this.TileHovered.AutoSize = true;
			this.TileHovered.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TileHovered.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.TileHovered.Location = new System.Drawing.Point(466, 0);
			this.TileHovered.Name = "TileHovered";
			this.TileHovered.Size = new System.Drawing.Size(443, 33);
			this.TileHovered.TabIndex = 5;
			this.TileHovered.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// Colors
			// 
			this.Colors.AnyColor = true;
			this.Colors.FullOpen = true;
			this.Colors.SolidColorOnly = true;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 1;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Controls.Add(this.button1, 0, 2);
			this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 3;
			this.tableLayoutPanel2.Size = new System.Drawing.Size(200, 100);
			this.tableLayoutPanel2.TabIndex = 0;
			// 
			// button1
			// 
			this.button1.BackColor = System.Drawing.Color.Black;
			this.button1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button1.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.button1.ForeColor = System.Drawing.Color.White;
			this.button1.Location = new System.Drawing.Point(3, 3);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(194, 94);
			this.button1.TabIndex = 6;
			this.button1.TabStop = false;
			this.button1.Text = "Load";
			this.button1.UseVisualStyleBackColor = false;
			// 
			// button4
			// 
			this.button4.BackColor = System.Drawing.Color.Black;
			this.button4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button4.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.button4.ForeColor = System.Drawing.Color.White;
			this.button4.Location = new System.Drawing.Point(3, 3);
			this.button4.Name = "button4";
			this.button4.Size = new System.Drawing.Size(194, 46);
			this.button4.TabIndex = 2;
			this.button4.TabStop = false;
			this.button4.Text = "New";
			this.button4.UseVisualStyleBackColor = false;
			// 
			// LoadTileset
			// 
			this.LoadTileset.DefaultExt = "png";
			this.LoadTileset.FileName = "tileset";
			this.LoadTileset.Filter = "Image|*.png|Image|*.jpg|Image|*.bmp";
			this.LoadTileset.Title = "Load Tileset";
			// 
			// Window
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Black;
			this.ClientSize = new System.Drawing.Size(1264, 681);
			this.Controls.Add(this.TableMain);
			this.ForeColor = System.Drawing.Color.White;
			this.MinimumSize = new System.Drawing.Size(800, 600);
			this.Name = "Window";
			this.Text = "Pure - Tilemap Editor";
			this.TableMain.ResumeLayout(false);
			this.TableEdit.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel5.ResumeLayout(false);
			this.tableLayoutPanel6.ResumeLayout(false);
			this.tableLayoutPanel6.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TileOffsetHeight)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TileOffsetWidth)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TileHeight)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TileWidth)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Set)).EndInit();
			this.tableLayoutPanel3.ResumeLayout(false);
			this.tableLayoutPanel3.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ColorBackground)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ColorGrid5)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ColorGrid1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ColorSelection)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ColorBrush)).EndInit();
			this.LayerMenu.ResumeLayout(false);
			this.tableLayoutPanel7.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.Map)).EndInit();
			this.tableLayoutPanel8.ResumeLayout(false);
			this.tableLayoutPanel8.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MapHeight)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MapWidth)).EndInit();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private TableLayoutPanel TableMain;
		private TableLayoutPanel TableEdit;
		private ColorDialog Colors;
		private TableLayoutPanel tableLayoutPanel3;
		private PictureBox ColorSelection;
		private Label label2;
		private PictureBox ColorBrush;
		private Label label1;
		private TableLayoutPanel tableLayoutPanel1;
		private TableLayoutPanel tableLayoutPanel2;
		private Button button1;
		private Button button4;
		private TableLayoutPanel tableLayoutPanel5;
		private Button button3;
		private TableLayoutPanel tableLayoutPanel6;
		private NumericUpDown TileHeight;
		private NumericUpDown TileWidth;
		private PictureBox Set;
		private NumericUpDown TileOffsetWidth;
		private NumericUpDown TileOffsetHeight;
		private Label label4;
		private Label label5;
		private Label label6;
		private PictureBox ColorGrid1;
		private TableLayoutPanel tableLayoutPanel7;
		private PictureBox Map;
		private TableLayoutPanel tableLayoutPanel8;
		private Label TileHovered;
		private NumericUpDown MapHeight;
		private NumericUpDown MapWidth;
		private Label label8;
		private OpenFileDialog LoadTileset;
		private PictureBox ColorBackground;
		private PictureBox ColorGrid5;
		private Label label9;
		private Label label7;
		private ContextMenuStrip LayerMenu;
		private ToolStripMenuItem removeToolStripMenuItem;
		private ToolStripMenuItem moveToolStripMenuItem;
		private ToolStripMenuItem upToolStripMenuItem;
		private ToolStripMenuItem downToolStripMenuItem;
		private ToolStripMenuItem topToolStripMenuItem;
		private ToolStripMenuItem bottomToolStripMenuItem;
		private ToolStripMenuItem toolStripMenuItem1;
		private ToolStripSeparator toolStripSeparator1;
		private ToolStripMenuItem renameRToolStripMenuItem;
		private CheckedListBox Layers;
	}
}