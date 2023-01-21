namespace ImageEditor
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
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.button1 = new System.Windows.Forms.Button();
			this.BackgroundColorIndicator = new System.Windows.Forms.PictureBox();
			this.Tool = new System.Windows.Forms.ListBox();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.ColorButton = new System.Windows.Forms.Button();
			this.ColorIndicator = new System.Windows.Forms.PictureBox();
			this.pictureBox2 = new System.Windows.Forms.PictureBox();
			this.Colors = new System.Windows.Forms.ColorDialog();
			this.TableMain.SuspendLayout();
			this.TableEdit.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BackgroundColorIndicator)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ColorIndicator)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
			this.SuspendLayout();
			// 
			// TableMain
			// 
			this.TableMain.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Inset;
			this.TableMain.ColumnCount = 2;
			this.TableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 85F));
			this.TableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
			this.TableMain.Controls.Add(this.TableEdit, 1, 0);
			this.TableMain.Controls.Add(this.pictureBox2, 0, 0);
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
			this.TableEdit.Controls.Add(this.tableLayoutPanel2, 0, 2);
			this.TableEdit.Controls.Add(this.Tool, 0, 0);
			this.TableEdit.Controls.Add(this.tableLayoutPanel1, 0, 1);
			this.TableEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TableEdit.Location = new System.Drawing.Point(1076, 5);
			this.TableEdit.Name = "TableEdit";
			this.TableEdit.RowCount = 4;
			this.TableEdit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.19403F));
			this.TableEdit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.49925F));
			this.TableEdit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15.09716F));
			this.TableEdit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 59.04335F));
			this.TableEdit.Size = new System.Drawing.Size(183, 671);
			this.TableEdit.TabIndex = 1;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 1;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.Controls.Add(this.button1, 0, 1);
			this.tableLayoutPanel2.Controls.Add(this.BackgroundColorIndicator, 0, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(5, 178);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 2;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(173, 93);
			this.tableLayoutPanel2.TabIndex = 4;
			// 
			// button1
			// 
			this.button1.BackColor = System.Drawing.Color.Black;
			this.button1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.button1.Location = new System.Drawing.Point(3, 49);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(167, 41);
			this.button1.TabIndex = 4;
			this.button1.Text = "Background Color";
			this.button1.UseVisualStyleBackColor = false;
			this.button1.Click += new System.EventHandler(this.OnBackgroundColorClick);
			// 
			// BackgroundColorIndicator
			// 
			this.BackgroundColorIndicator.BackColor = System.Drawing.Color.White;
			this.BackgroundColorIndicator.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BackgroundColorIndicator.Location = new System.Drawing.Point(3, 3);
			this.BackgroundColorIndicator.Name = "BackgroundColorIndicator";
			this.BackgroundColorIndicator.Size = new System.Drawing.Size(167, 40);
			this.BackgroundColorIndicator.TabIndex = 3;
			this.BackgroundColorIndicator.TabStop = false;
			// 
			// Tool
			// 
			this.Tool.BackColor = System.Drawing.Color.Black;
			this.Tool.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Tool.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.Tool.ForeColor = System.Drawing.Color.White;
			this.Tool.FormattingEnabled = true;
			this.Tool.ItemHeight = 21;
			this.Tool.Items.AddRange(new object[] {
            "Point",
            "Square",
            "Line"});
			this.Tool.Location = new System.Drawing.Point(5, 5);
			this.Tool.Name = "Tool";
			this.Tool.Size = new System.Drawing.Size(173, 68);
			this.Tool.TabIndex = 0;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.Controls.Add(this.ColorButton, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.ColorIndicator, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(5, 81);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(173, 89);
			this.tableLayoutPanel1.TabIndex = 3;
			// 
			// ColorButton
			// 
			this.ColorButton.BackColor = System.Drawing.Color.Black;
			this.ColorButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ColorButton.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.ColorButton.Location = new System.Drawing.Point(3, 47);
			this.ColorButton.Name = "ColorButton";
			this.ColorButton.Size = new System.Drawing.Size(167, 39);
			this.ColorButton.TabIndex = 4;
			this.ColorButton.Text = "Brush Color";
			this.ColorButton.UseVisualStyleBackColor = false;
			this.ColorButton.Click += new System.EventHandler(this.OnBrushColorClick);
			// 
			// ColorIndicator
			// 
			this.ColorIndicator.BackColor = System.Drawing.Color.White;
			this.ColorIndicator.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ColorIndicator.Location = new System.Drawing.Point(3, 3);
			this.ColorIndicator.Name = "ColorIndicator";
			this.ColorIndicator.Size = new System.Drawing.Size(167, 38);
			this.ColorIndicator.TabIndex = 3;
			this.ColorIndicator.TabStop = false;
			// 
			// pictureBox2
			// 
			this.pictureBox2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pictureBox2.Location = new System.Drawing.Point(5, 5);
			this.pictureBox2.Name = "pictureBox2";
			this.pictureBox2.Size = new System.Drawing.Size(1063, 671);
			this.pictureBox2.TabIndex = 2;
			this.pictureBox2.TabStop = false;
			// 
			// Colors
			// 
			this.Colors.AnyColor = true;
			this.Colors.FullOpen = true;
			this.Colors.SolidColorOnly = true;
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
			this.Text = "Pure - Image Editor";
			this.TableMain.ResumeLayout(false);
			this.TableEdit.ResumeLayout(false);
			this.tableLayoutPanel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.BackgroundColorIndicator)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ColorIndicator)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private TableLayoutPanel TableMain;
		private TableLayoutPanel TableEdit;
		private ListBox Tool;
		private ColorDialog Colors;
		private TableLayoutPanel tableLayoutPanel2;
		private Button button1;
		private PictureBox BackgroundColorIndicator;
		private TableLayoutPanel tableLayoutPanel1;
		private Button ColorButton;
		private PictureBox ColorIndicator;
		private PictureBox pictureBox2;
	}
}