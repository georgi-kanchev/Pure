namespace StorageEditor
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
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.button1 = new System.Windows.Forms.Button();
			this.listBox1 = new System.Windows.Forms.ListBox();
			this.button2 = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			this.button3 = new System.Windows.Forms.Button();
			this.listBox2 = new System.Windows.Forms.ListBox();
			this.label2 = new System.Windows.Forms.Label();
			this.button4 = new System.Windows.Forms.Button();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.tableLayoutPanel3.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.BackColor = System.Drawing.Color.Black;
			this.tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
			this.tableLayoutPanel1.ColumnCount = 4;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.ForeColor = System.Drawing.Color.Black;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(784, 561);
			this.tableLayoutPanel1.TabIndex = 2;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Inset;
			this.tableLayoutPanel2.ColumnCount = 1;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Controls.Add(this.button1, 0, 2);
			this.tableLayoutPanel2.Controls.Add(this.listBox1, 0, 1);
			this.tableLayoutPanel2.Controls.Add(this.button2, 0, 3);
			this.tableLayoutPanel2.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(4, 4);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 4;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 4.949945F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 79.19914F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.925457F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.925457F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(149, 553);
			this.tableLayoutPanel2.TabIndex = 0;
			// 
			// button1
			// 
			this.button1.BackColor = System.Drawing.Color.Black;
			this.button1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button1.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.button1.ForeColor = System.Drawing.Color.White;
			this.button1.Location = new System.Drawing.Point(5, 465);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(139, 37);
			this.button1.TabIndex = 1;
			this.button1.Text = "Create";
			this.button1.UseVisualStyleBackColor = false;
			// 
			// listBox1
			// 
			this.listBox1.BackColor = System.Drawing.Color.Black;
			this.listBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listBox1.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.listBox1.ForeColor = System.Drawing.Color.White;
			this.listBox1.FormattingEnabled = true;
			this.listBox1.ItemHeight = 25;
			this.listBox1.Items.AddRange(new object[] {
            "test",
            "aaa",
            "bbb"});
			this.listBox1.Location = new System.Drawing.Point(5, 33);
			this.listBox1.Name = "listBox1";
			this.listBox1.Size = new System.Drawing.Size(139, 424);
			this.listBox1.TabIndex = 0;
			// 
			// button2
			// 
			this.button2.BackColor = System.Drawing.Color.Black;
			this.button2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button2.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.button2.ForeColor = System.Drawing.Color.White;
			this.button2.Location = new System.Drawing.Point(5, 510);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(139, 38);
			this.button2.TabIndex = 2;
			this.button2.Text = "Delete";
			this.button2.UseVisualStyleBackColor = false;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.label1.ForeColor = System.Drawing.Color.White;
			this.label1.Location = new System.Drawing.Point(5, 2);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(139, 26);
			this.label1.TabIndex = 3;
			this.label1.Text = "Instances";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// tableLayoutPanel3
			// 
			this.tableLayoutPanel3.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Inset;
			this.tableLayoutPanel3.ColumnCount = 1;
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel3.Controls.Add(this.button3, 0, 2);
			this.tableLayoutPanel3.Controls.Add(this.listBox2, 0, 1);
			this.tableLayoutPanel3.Controls.Add(this.button4, 0, 3);
			this.tableLayoutPanel3.Controls.Add(this.label2, 0, 0);
			this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel3.Location = new System.Drawing.Point(160, 4);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			this.tableLayoutPanel3.RowCount = 4;
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 4.949945F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 79.19914F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.925457F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.925457F));
			this.tableLayoutPanel3.Size = new System.Drawing.Size(149, 553);
			this.tableLayoutPanel3.TabIndex = 1;
			// 
			// button3
			// 
			this.button3.BackColor = System.Drawing.Color.Black;
			this.button3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button3.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.button3.ForeColor = System.Drawing.Color.White;
			this.button3.Location = new System.Drawing.Point(5, 465);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(139, 37);
			this.button3.TabIndex = 1;
			this.button3.Text = "Create";
			this.button3.UseVisualStyleBackColor = false;
			// 
			// listBox2
			// 
			this.listBox2.BackColor = System.Drawing.Color.Black;
			this.listBox2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listBox2.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.listBox2.ForeColor = System.Drawing.Color.White;
			this.listBox2.FormattingEnabled = true;
			this.listBox2.ItemHeight = 25;
			this.listBox2.Items.AddRange(new object[] {
            "test",
            "aaa",
            "bbb"});
			this.listBox2.Location = new System.Drawing.Point(5, 33);
			this.listBox2.Name = "listBox2";
			this.listBox2.Size = new System.Drawing.Size(139, 424);
			this.listBox2.TabIndex = 0;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label2.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.label2.ForeColor = System.Drawing.Color.White;
			this.label2.Location = new System.Drawing.Point(5, 2);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(139, 26);
			this.label2.TabIndex = 3;
			this.label2.Text = "Properties";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// button4
			// 
			this.button4.BackColor = System.Drawing.Color.Black;
			this.button4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button4.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.button4.ForeColor = System.Drawing.Color.White;
			this.button4.Location = new System.Drawing.Point(5, 510);
			this.button4.Name = "button4";
			this.button4.Size = new System.Drawing.Size(139, 38);
			this.button4.TabIndex = 2;
			this.button4.Text = "Delete";
			this.button4.UseVisualStyleBackColor = false;
			// 
			// Window
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Black;
			this.ClientSize = new System.Drawing.Size(784, 561);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "Window";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Purity - Storage Editor";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			this.tableLayoutPanel3.ResumeLayout(false);
			this.tableLayoutPanel3.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion
		private TableLayoutPanel tableLayoutPanel1;
		private TableLayoutPanel tableLayoutPanel2;
		private Button button1;
		private ListBox listBox1;
		private Button button2;
		private Label label1;
		private TableLayoutPanel tableLayoutPanel3;
		private Button button3;
		private ListBox listBox2;
		private Label label2;
		private Button button4;
	}
}