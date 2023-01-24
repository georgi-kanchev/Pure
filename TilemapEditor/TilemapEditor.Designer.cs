﻿namespace ImageEditor
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
			this.TableEdit = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
			this.button6 = new System.Windows.Forms.Button();
			this.button7 = new System.Windows.Forms.Button();
			this.button8 = new System.Windows.Forms.Button();
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
			this.SetBgColor = new System.Windows.Forms.PictureBox();
			this.label6 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.MapBgColor = new System.Windows.Forms.PictureBox();
			this.GridColor = new System.Windows.Forms.PictureBox();
			this.label2 = new System.Windows.Forms.Label();
			this.BrushColor = new System.Windows.Forms.PictureBox();
			this.label1 = new System.Windows.Forms.Label();
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
			this.tableLayoutPanel4.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel5.SuspendLayout();
			this.tableLayoutPanel6.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TileOffsetHeight)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TileOffsetWidth)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TileHeight)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TileWidth)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Set)).BeginInit();
			this.tableLayoutPanel3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SetBgColor)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MapBgColor)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.GridColor)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BrushColor)).BeginInit();
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
			this.TableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 71.15689F));
			this.TableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 28.84311F));
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
			this.TableEdit.Controls.Add(this.tableLayoutPanel4, 0, 2);
			this.TableEdit.Controls.Add(this.tableLayoutPanel1, 0, 0);
			this.TableEdit.Controls.Add(this.tableLayoutPanel3, 0, 1);
			this.TableEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TableEdit.Location = new System.Drawing.Point(902, 5);
			this.TableEdit.Name = "TableEdit";
			this.TableEdit.RowCount = 3;
			this.TableEdit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 59.56006F));
			this.TableEdit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 19.43199F));
			this.TableEdit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 21.67414F));
			this.TableEdit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.TableEdit.Size = new System.Drawing.Size(357, 671);
			this.TableEdit.TabIndex = 1;
			// 
			// tableLayoutPanel4
			// 
			this.tableLayoutPanel4.ColumnCount = 1;
			this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel4.Controls.Add(this.button6, 0, 2);
			this.tableLayoutPanel4.Controls.Add(this.button7, 0, 0);
			this.tableLayoutPanel4.Controls.Add(this.button8, 0, 1);
			this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel4.Location = new System.Drawing.Point(5, 528);
			this.tableLayoutPanel4.Name = "tableLayoutPanel4";
			this.tableLayoutPanel4.RowCount = 3;
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel4.Size = new System.Drawing.Size(347, 138);
			this.tableLayoutPanel4.TabIndex = 8;
			// 
			// button6
			// 
			this.button6.BackColor = System.Drawing.Color.Black;
			this.button6.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button6.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.button6.ForeColor = System.Drawing.Color.White;
			this.button6.Location = new System.Drawing.Point(3, 95);
			this.button6.Name = "button6";
			this.button6.Size = new System.Drawing.Size(341, 40);
			this.button6.TabIndex = 6;
			this.button6.TabStop = false;
			this.button6.Text = "Load";
			this.button6.UseVisualStyleBackColor = false;
			// 
			// button7
			// 
			this.button7.BackColor = System.Drawing.Color.Black;
			this.button7.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button7.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.button7.ForeColor = System.Drawing.Color.White;
			this.button7.Location = new System.Drawing.Point(3, 3);
			this.button7.Name = "button7";
			this.button7.Size = new System.Drawing.Size(341, 40);
			this.button7.TabIndex = 2;
			this.button7.TabStop = false;
			this.button7.Text = "New";
			this.button7.UseVisualStyleBackColor = false;
			// 
			// button8
			// 
			this.button8.BackColor = System.Drawing.Color.Black;
			this.button8.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button8.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.button8.ForeColor = System.Drawing.Color.White;
			this.button8.Location = new System.Drawing.Point(3, 49);
			this.button8.Name = "button8";
			this.button8.Size = new System.Drawing.Size(341, 40);
			this.button8.TabIndex = 1;
			this.button8.TabStop = false;
			this.button8.Text = "Save";
			this.button8.UseVisualStyleBackColor = false;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Inset;
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel5, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.Set, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(5, 5);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80.72916F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 19.27083F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(347, 386);
			this.tableLayoutPanel1.TabIndex = 7;
			// 
			// tableLayoutPanel5
			// 
			this.tableLayoutPanel5.ColumnCount = 2;
			this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 72.01365F));
			this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 27.98635F));
			this.tableLayoutPanel5.Controls.Add(this.tableLayoutPanel6, 0, 0);
			this.tableLayoutPanel5.Controls.Add(this.button3, 1, 0);
			this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel5.Location = new System.Drawing.Point(5, 313);
			this.tableLayoutPanel5.Name = "tableLayoutPanel5";
			this.tableLayoutPanel5.RowCount = 1;
			this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel5.Size = new System.Drawing.Size(337, 68);
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
			this.tableLayoutPanel6.Size = new System.Drawing.Size(236, 62);
			this.tableLayoutPanel6.TabIndex = 7;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label5.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.label5.Location = new System.Drawing.Point(3, 31);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(72, 31);
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
			this.label4.Size = new System.Drawing.Size(72, 31);
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
			this.TileOffsetHeight.Location = new System.Drawing.Point(159, 34);
			this.TileOffsetHeight.Maximum = new decimal(new int[] {
            16,
            0,
            0,
            0});
			this.TileOffsetHeight.Name = "TileOffsetHeight";
			this.TileOffsetHeight.Size = new System.Drawing.Size(74, 27);
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
			this.TileOffsetWidth.Location = new System.Drawing.Point(81, 34);
			this.TileOffsetWidth.Maximum = new decimal(new int[] {
            16,
            0,
            0,
            0});
			this.TileOffsetWidth.Name = "TileOffsetWidth";
			this.TileOffsetWidth.Size = new System.Drawing.Size(72, 27);
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
			this.TileHeight.Location = new System.Drawing.Point(159, 3);
			this.TileHeight.Maximum = new decimal(new int[] {
            128,
            0,
            0,
            0});
			this.TileHeight.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
			this.TileHeight.Name = "TileHeight";
			this.TileHeight.Size = new System.Drawing.Size(74, 27);
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
			this.TileWidth.Location = new System.Drawing.Point(81, 3);
			this.TileWidth.Maximum = new decimal(new int[] {
            128,
            0,
            0,
            0});
			this.TileWidth.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
			this.TileWidth.Name = "TileWidth";
			this.TileWidth.Size = new System.Drawing.Size(72, 27);
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
			this.button3.Location = new System.Drawing.Point(245, 3);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(89, 62);
			this.button3.TabIndex = 2;
			this.button3.TabStop = false;
			this.button3.Text = "Tileset";
			this.button3.UseVisualStyleBackColor = false;
			this.button3.Click += new System.EventHandler(this.OnTilesetLoadClick);
			// 
			// Set
			// 
			this.Set.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Set.Location = new System.Drawing.Point(5, 5);
			this.Set.Name = "Set";
			this.Set.Size = new System.Drawing.Size(337, 300);
			this.Set.TabIndex = 10;
			this.Set.TabStop = false;
			this.Set.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnSetPress);
			this.Set.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMoveSet);
			// 
			// tableLayoutPanel3
			// 
			this.tableLayoutPanel3.ColumnCount = 2;
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel3.Controls.Add(this.SetBgColor, 1, 3);
			this.tableLayoutPanel3.Controls.Add(this.label6, 0, 3);
			this.tableLayoutPanel3.Controls.Add(this.label3, 0, 2);
			this.tableLayoutPanel3.Controls.Add(this.MapBgColor, 0, 2);
			this.tableLayoutPanel3.Controls.Add(this.GridColor, 1, 1);
			this.tableLayoutPanel3.Controls.Add(this.label2, 0, 1);
			this.tableLayoutPanel3.Controls.Add(this.BrushColor, 1, 0);
			this.tableLayoutPanel3.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel3.Location = new System.Drawing.Point(5, 399);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			this.tableLayoutPanel3.RowCount = 4;
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.tableLayoutPanel3.Size = new System.Drawing.Size(347, 121);
			this.tableLayoutPanel3.TabIndex = 6;
			// 
			// SetBgColor
			// 
			this.SetBgColor.BackColor = System.Drawing.Color.Black;
			this.SetBgColor.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.SetBgColor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SetBgColor.Location = new System.Drawing.Point(176, 93);
			this.SetBgColor.Name = "SetBgColor";
			this.SetBgColor.Size = new System.Drawing.Size(168, 25);
			this.SetBgColor.TabIndex = 12;
			this.SetBgColor.TabStop = false;
			this.SetBgColor.Click += new System.EventHandler(this.OnSetBackgroundColorClick);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label6.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.label6.Location = new System.Drawing.Point(3, 90);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(167, 31);
			this.label6.TabIndex = 11;
			this.label6.Text = "Set Background";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label3.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.label3.Location = new System.Drawing.Point(3, 60);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(167, 30);
			this.label3.TabIndex = 10;
			this.label3.Text = "Map Background";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// MapBgColor
			// 
			this.MapBgColor.BackColor = System.Drawing.Color.Black;
			this.MapBgColor.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.MapBgColor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MapBgColor.Location = new System.Drawing.Point(176, 63);
			this.MapBgColor.Name = "MapBgColor";
			this.MapBgColor.Size = new System.Drawing.Size(168, 24);
			this.MapBgColor.TabIndex = 9;
			this.MapBgColor.TabStop = false;
			this.MapBgColor.Click += new System.EventHandler(this.OnMapBackgroundColorClick);
			// 
			// GridColor
			// 
			this.GridColor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.GridColor.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.GridColor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GridColor.Location = new System.Drawing.Point(176, 33);
			this.GridColor.Name = "GridColor";
			this.GridColor.Size = new System.Drawing.Size(168, 24);
			this.GridColor.TabIndex = 6;
			this.GridColor.TabStop = false;
			this.GridColor.Click += new System.EventHandler(this.OnGridColorClick);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label2.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.label2.Location = new System.Drawing.Point(3, 30);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(167, 30);
			this.label2.TabIndex = 5;
			this.label2.Text = "Grid";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// BrushColor
			// 
			this.BrushColor.BackColor = System.Drawing.Color.White;
			this.BrushColor.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.BrushColor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BrushColor.Location = new System.Drawing.Point(176, 3);
			this.BrushColor.Name = "BrushColor";
			this.BrushColor.Size = new System.Drawing.Size(168, 24);
			this.BrushColor.TabIndex = 4;
			this.BrushColor.TabStop = false;
			this.BrushColor.Click += new System.EventHandler(this.OnBrushColorClick);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.label1.Location = new System.Drawing.Point(3, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(167, 30);
			this.label1.TabIndex = 0;
			this.label1.Text = "Brush";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
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
			this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 94.33681F));
			this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5.663189F));
			this.tableLayoutPanel7.Size = new System.Drawing.Size(889, 671);
			this.tableLayoutPanel7.TabIndex = 2;
			// 
			// Map
			// 
			this.Map.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Map.Location = new System.Drawing.Point(5, 5);
			this.Map.Name = "Map";
			this.Map.Size = new System.Drawing.Size(879, 621);
			this.Map.TabIndex = 3;
			this.Map.TabStop = false;
			this.Map.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMoveMap);
			// 
			// tableLayoutPanel8
			// 
			this.tableLayoutPanel8.ColumnCount = 5;
			this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8F));
			this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8F));
			this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8F));
			this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 56F));
			this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.tableLayoutPanel8.Controls.Add(this.label8, 0, 0);
			this.tableLayoutPanel8.Controls.Add(this.MapHeight, 2, 0);
			this.tableLayoutPanel8.Controls.Add(this.MapWidth, 1, 0);
			this.tableLayoutPanel8.Controls.Add(this.TileHovered, 4, 0);
			this.tableLayoutPanel8.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel8.Location = new System.Drawing.Point(5, 634);
			this.tableLayoutPanel8.Name = "tableLayoutPanel8";
			this.tableLayoutPanel8.RowCount = 1;
			this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel8.Size = new System.Drawing.Size(879, 32);
			this.tableLayoutPanel8.TabIndex = 4;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label8.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.label8.Location = new System.Drawing.Point(3, 0);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(64, 32);
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
			this.MapHeight.Location = new System.Drawing.Point(143, 3);
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
			this.MapHeight.Size = new System.Drawing.Size(64, 27);
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
			this.MapWidth.Location = new System.Drawing.Point(73, 3);
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
			this.MapWidth.Size = new System.Drawing.Size(64, 27);
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
			this.TileHovered.Location = new System.Drawing.Point(705, 0);
			this.TileHovered.Name = "TileHovered";
			this.TileHovered.Size = new System.Drawing.Size(171, 32);
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
			this.Name = "Window";
			this.Text = "Pure - Tilemap Editor";
			this.TableMain.ResumeLayout(false);
			this.TableEdit.ResumeLayout(false);
			this.tableLayoutPanel4.ResumeLayout(false);
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
			((System.ComponentModel.ISupportInitialize)(this.SetBgColor)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MapBgColor)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.GridColor)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BrushColor)).EndInit();
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
		private Label label3;
		private PictureBox MapBgColor;
		private PictureBox GridColor;
		private Label label2;
		private PictureBox BrushColor;
		private Label label1;
		private TableLayoutPanel tableLayoutPanel1;
		private TableLayoutPanel tableLayoutPanel4;
		private Button button6;
		private Button button7;
		private Button button8;
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
		private PictureBox SetBgColor;
		private TableLayoutPanel tableLayoutPanel7;
		private PictureBox Map;
		private TableLayoutPanel tableLayoutPanel8;
		private Label TileHovered;
		private NumericUpDown MapHeight;
		private NumericUpDown MapWidth;
		private Label label8;
		private OpenFileDialog LoadTileset;
	}
}