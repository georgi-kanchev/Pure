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
			this.components = new System.ComponentModel.Container();
			this.TableMain = new System.Windows.Forms.TableLayoutPanel();
			this.MenuGlobal = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.MenuGlobalSave = new System.Windows.Forms.ToolStripMenuItem();
			this.MenuGlobalLoad = new System.Windows.Forms.ToolStripMenuItem();
			this.TableValues = new System.Windows.Forms.TableLayoutPanel();
			this.LabelValues = new System.Windows.Forms.Label();
			this.ListValues = new System.Windows.Forms.ListBox();
			this.TableSubProperties = new System.Windows.Forms.TableLayoutPanel();
			this.LabelSubProperties = new System.Windows.Forms.Label();
			this.ListSubProperties = new System.Windows.Forms.ListBox();
			this.TableProperties = new System.Windows.Forms.TableLayoutPanel();
			this.LabelProperties = new System.Windows.Forms.Label();
			this.ListProperties = new System.Windows.Forms.ListBox();
			this.TableObjects = new System.Windows.Forms.TableLayoutPanel();
			this.LabelObjects = new System.Windows.Forms.Label();
			this.ListObjects = new System.Windows.Forms.ListBox();
			this.MenuEdit = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.MenuEditEdit = new System.Windows.Forms.ToolStripMenuItem();
			this.MenuEditRemove = new System.Windows.Forms.ToolStripMenuItem();
			this.MenuEditSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.MenuEditCreate = new System.Windows.Forms.ToolStripMenuItem();
			this.MenuEditSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.MenuEditSave = new System.Windows.Forms.ToolStripMenuItem();
			this.MenuEditLoad = new System.Windows.Forms.ToolStripMenuItem();
			this.Load = new System.Windows.Forms.OpenFileDialog();
			this.TableMain.SuspendLayout();
			this.MenuGlobal.SuspendLayout();
			this.TableValues.SuspendLayout();
			this.TableSubProperties.SuspendLayout();
			this.TableProperties.SuspendLayout();
			this.TableObjects.SuspendLayout();
			this.MenuEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// TableMain
			// 
			this.TableMain.ColumnCount = 4;
			this.TableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.TableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.TableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.TableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.TableMain.ContextMenuStrip = this.MenuGlobal;
			this.TableMain.Controls.Add(this.TableValues, 3, 0);
			this.TableMain.Controls.Add(this.TableSubProperties, 2, 0);
			this.TableMain.Controls.Add(this.TableProperties, 1, 0);
			this.TableMain.Controls.Add(this.TableObjects, 0, 0);
			this.TableMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TableMain.Location = new System.Drawing.Point(0, 0);
			this.TableMain.Name = "TableMain";
			this.TableMain.RowCount = 1;
			this.TableMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TableMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.TableMain.Size = new System.Drawing.Size(784, 561);
			this.TableMain.TabIndex = 0;
			// 
			// MenuGlobal
			// 
			this.MenuGlobal.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuGlobalSave,
            this.MenuGlobalLoad});
			this.MenuGlobal.Name = "contextMenuStrip1";
			this.MenuGlobal.Size = new System.Drawing.Size(144, 48);
			// 
			// MenuGlobalSave
			// 
			this.MenuGlobalSave.Name = "MenuGlobalSave";
			this.MenuGlobalSave.Size = new System.Drawing.Size(143, 22);
			this.MenuGlobalSave.Text = "Save Storage";
			// 
			// MenuGlobalLoad
			// 
			this.MenuGlobalLoad.Name = "MenuGlobalLoad";
			this.MenuGlobalLoad.Size = new System.Drawing.Size(143, 22);
			this.MenuGlobalLoad.Text = "Load Storage";
			this.MenuGlobalLoad.Click += new System.EventHandler(this.Load_Click);
			// 
			// TableValues
			// 
			this.TableValues.ColumnCount = 1;
			this.TableValues.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TableValues.Controls.Add(this.LabelValues, 0, 0);
			this.TableValues.Controls.Add(this.ListValues, 0, 1);
			this.TableValues.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TableValues.Location = new System.Drawing.Point(591, 3);
			this.TableValues.Name = "TableValues";
			this.TableValues.RowCount = 2;
			this.TableValues.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
			this.TableValues.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 95F));
			this.TableValues.Size = new System.Drawing.Size(190, 555);
			this.TableValues.TabIndex = 3;
			this.TableValues.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Global_Click);
			// 
			// LabelValues
			// 
			this.LabelValues.AutoSize = true;
			this.LabelValues.ContextMenuStrip = this.MenuGlobal;
			this.LabelValues.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelValues.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.LabelValues.ForeColor = System.Drawing.Color.White;
			this.LabelValues.Location = new System.Drawing.Point(3, 0);
			this.LabelValues.Name = "LabelValues";
			this.LabelValues.Size = new System.Drawing.Size(184, 27);
			this.LabelValues.TabIndex = 0;
			this.LabelValues.Text = "Values";
			this.LabelValues.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ListValues
			// 
			this.ListValues.BackColor = System.Drawing.Color.Black;
			this.ListValues.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ListValues.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.ListValues.ForeColor = System.Drawing.Color.White;
			this.ListValues.FormattingEnabled = true;
			this.ListValues.ItemHeight = 21;
			this.ListValues.Location = new System.Drawing.Point(3, 30);
			this.ListValues.Name = "ListValues";
			this.ListValues.Size = new System.Drawing.Size(184, 522);
			this.ListValues.TabIndex = 1;
			this.ListValues.Tag = "Value";
			this.ListValues.MouseUp += new System.Windows.Forms.MouseEventHandler(this.List_Click);
			// 
			// TableSubProperties
			// 
			this.TableSubProperties.ColumnCount = 1;
			this.TableSubProperties.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TableSubProperties.Controls.Add(this.LabelSubProperties, 0, 0);
			this.TableSubProperties.Controls.Add(this.ListSubProperties, 0, 1);
			this.TableSubProperties.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TableSubProperties.Location = new System.Drawing.Point(395, 3);
			this.TableSubProperties.Name = "TableSubProperties";
			this.TableSubProperties.RowCount = 2;
			this.TableSubProperties.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
			this.TableSubProperties.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 95F));
			this.TableSubProperties.Size = new System.Drawing.Size(190, 555);
			this.TableSubProperties.TabIndex = 2;
			this.TableSubProperties.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Global_Click);
			// 
			// LabelSubProperties
			// 
			this.LabelSubProperties.AutoSize = true;
			this.LabelSubProperties.ContextMenuStrip = this.MenuGlobal;
			this.LabelSubProperties.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelSubProperties.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.LabelSubProperties.ForeColor = System.Drawing.Color.White;
			this.LabelSubProperties.Location = new System.Drawing.Point(3, 0);
			this.LabelSubProperties.Name = "LabelSubProperties";
			this.LabelSubProperties.Size = new System.Drawing.Size(184, 27);
			this.LabelSubProperties.TabIndex = 0;
			this.LabelSubProperties.Text = "Sub Properties";
			this.LabelSubProperties.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ListSubProperties
			// 
			this.ListSubProperties.BackColor = System.Drawing.Color.Black;
			this.ListSubProperties.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ListSubProperties.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.ListSubProperties.ForeColor = System.Drawing.Color.White;
			this.ListSubProperties.FormattingEnabled = true;
			this.ListSubProperties.ItemHeight = 21;
			this.ListSubProperties.Location = new System.Drawing.Point(3, 30);
			this.ListSubProperties.Name = "ListSubProperties";
			this.ListSubProperties.Size = new System.Drawing.Size(184, 522);
			this.ListSubProperties.TabIndex = 1;
			this.ListSubProperties.Tag = "Sub Property";
			this.ListSubProperties.SelectedIndexChanged += new System.EventHandler(this.ListSubProperties_SelectedIndexChanged);
			this.ListSubProperties.MouseUp += new System.Windows.Forms.MouseEventHandler(this.List_Click);
			// 
			// TableProperties
			// 
			this.TableProperties.ColumnCount = 1;
			this.TableProperties.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TableProperties.Controls.Add(this.LabelProperties, 0, 0);
			this.TableProperties.Controls.Add(this.ListProperties, 0, 1);
			this.TableProperties.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TableProperties.Location = new System.Drawing.Point(199, 3);
			this.TableProperties.Name = "TableProperties";
			this.TableProperties.RowCount = 2;
			this.TableProperties.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
			this.TableProperties.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 95F));
			this.TableProperties.Size = new System.Drawing.Size(190, 555);
			this.TableProperties.TabIndex = 1;
			this.TableProperties.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Global_Click);
			// 
			// LabelProperties
			// 
			this.LabelProperties.AutoSize = true;
			this.LabelProperties.ContextMenuStrip = this.MenuGlobal;
			this.LabelProperties.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelProperties.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.LabelProperties.ForeColor = System.Drawing.Color.White;
			this.LabelProperties.Location = new System.Drawing.Point(3, 0);
			this.LabelProperties.Name = "LabelProperties";
			this.LabelProperties.Size = new System.Drawing.Size(184, 27);
			this.LabelProperties.TabIndex = 0;
			this.LabelProperties.Text = "Properties";
			this.LabelProperties.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ListProperties
			// 
			this.ListProperties.BackColor = System.Drawing.Color.Black;
			this.ListProperties.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ListProperties.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.ListProperties.ForeColor = System.Drawing.Color.White;
			this.ListProperties.FormattingEnabled = true;
			this.ListProperties.ItemHeight = 21;
			this.ListProperties.Location = new System.Drawing.Point(3, 30);
			this.ListProperties.Name = "ListProperties";
			this.ListProperties.Size = new System.Drawing.Size(184, 522);
			this.ListProperties.TabIndex = 1;
			this.ListProperties.Tag = "Property";
			this.ListProperties.SelectedIndexChanged += new System.EventHandler(this.ListProperties_SelectedIndexChanged);
			this.ListProperties.MouseUp += new System.Windows.Forms.MouseEventHandler(this.List_Click);
			// 
			// TableObjects
			// 
			this.TableObjects.ColumnCount = 1;
			this.TableObjects.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TableObjects.Controls.Add(this.LabelObjects, 0, 0);
			this.TableObjects.Controls.Add(this.ListObjects, 0, 1);
			this.TableObjects.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TableObjects.Location = new System.Drawing.Point(3, 3);
			this.TableObjects.Name = "TableObjects";
			this.TableObjects.RowCount = 2;
			this.TableObjects.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
			this.TableObjects.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 95F));
			this.TableObjects.Size = new System.Drawing.Size(190, 555);
			this.TableObjects.TabIndex = 0;
			this.TableObjects.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Global_Click);
			// 
			// LabelObjects
			// 
			this.LabelObjects.AutoSize = true;
			this.LabelObjects.ContextMenuStrip = this.MenuGlobal;
			this.LabelObjects.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelObjects.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.LabelObjects.ForeColor = System.Drawing.Color.White;
			this.LabelObjects.Location = new System.Drawing.Point(3, 0);
			this.LabelObjects.Name = "LabelObjects";
			this.LabelObjects.Size = new System.Drawing.Size(184, 27);
			this.LabelObjects.TabIndex = 0;
			this.LabelObjects.Text = "Objects";
			this.LabelObjects.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ListObjects
			// 
			this.ListObjects.BackColor = System.Drawing.Color.Black;
			this.ListObjects.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ListObjects.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.ListObjects.ForeColor = System.Drawing.Color.White;
			this.ListObjects.FormattingEnabled = true;
			this.ListObjects.ItemHeight = 21;
			this.ListObjects.Location = new System.Drawing.Point(3, 30);
			this.ListObjects.Name = "ListObjects";
			this.ListObjects.Size = new System.Drawing.Size(184, 522);
			this.ListObjects.TabIndex = 1;
			this.ListObjects.Tag = "Object";
			this.ListObjects.SelectedIndexChanged += new System.EventHandler(this.ListObjects_SelectedIndexChanged);
			this.ListObjects.MouseUp += new System.Windows.Forms.MouseEventHandler(this.List_Click);
			// 
			// MenuEdit
			// 
			this.MenuEdit.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuEditEdit,
            this.MenuEditRemove,
            this.MenuEditSeparator1,
            this.MenuEditCreate,
            this.MenuEditSeparator2,
            this.MenuEditSave,
            this.MenuEditLoad});
			this.MenuEdit.Name = "contextMenuStrip1";
			this.MenuEdit.Size = new System.Drawing.Size(181, 148);
			// 
			// MenuEditEdit
			// 
			this.MenuEditEdit.Name = "MenuEditEdit";
			this.MenuEditEdit.Size = new System.Drawing.Size(180, 22);
			this.MenuEditEdit.Text = "Edit";
			this.MenuEditEdit.Click += new System.EventHandler(this.MenuEditEdit_Click);
			// 
			// MenuEditRemove
			// 
			this.MenuEditRemove.Name = "MenuEditRemove";
			this.MenuEditRemove.Size = new System.Drawing.Size(180, 22);
			this.MenuEditRemove.Text = "Remove";
			this.MenuEditRemove.Click += new System.EventHandler(this.MenuEditRemove_Click);
			// 
			// MenuEditSeparator1
			// 
			this.MenuEditSeparator1.Name = "MenuEditSeparator1";
			this.MenuEditSeparator1.Size = new System.Drawing.Size(177, 6);
			// 
			// MenuEditCreate
			// 
			this.MenuEditCreate.Name = "MenuEditCreate";
			this.MenuEditCreate.Size = new System.Drawing.Size(180, 22);
			this.MenuEditCreate.Text = "Create";
			this.MenuEditCreate.Click += new System.EventHandler(this.MenuEditCreate_Click);
			// 
			// MenuEditSeparator2
			// 
			this.MenuEditSeparator2.Name = "MenuEditSeparator2";
			this.MenuEditSeparator2.Size = new System.Drawing.Size(177, 6);
			// 
			// MenuEditSave
			// 
			this.MenuEditSave.Name = "MenuEditSave";
			this.MenuEditSave.Size = new System.Drawing.Size(180, 22);
			this.MenuEditSave.Text = "Save Storage";
			// 
			// MenuEditLoad
			// 
			this.MenuEditLoad.Name = "MenuEditLoad";
			this.MenuEditLoad.Size = new System.Drawing.Size(180, 22);
			this.MenuEditLoad.Text = "Load Storage";
			this.MenuEditLoad.Click += new System.EventHandler(this.Load_Click);
			// 
			// Load
			// 
			this.Load.AddExtension = false;
			this.Load.Title = "Load Purity Storage File";
			// 
			// Window
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Black;
			this.ClientSize = new System.Drawing.Size(784, 561);
			this.Controls.Add(this.TableMain);
			this.Name = "Window";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Purity - Storage Editor";
			this.TableMain.ResumeLayout(false);
			this.MenuGlobal.ResumeLayout(false);
			this.TableValues.ResumeLayout(false);
			this.TableValues.PerformLayout();
			this.TableSubProperties.ResumeLayout(false);
			this.TableSubProperties.PerformLayout();
			this.TableProperties.ResumeLayout(false);
			this.TableProperties.PerformLayout();
			this.TableObjects.ResumeLayout(false);
			this.TableObjects.PerformLayout();
			this.MenuEdit.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private TableLayoutPanel TableMain;
		private TableLayoutPanel TableObjects;
		private Label LabelObjects;
		private ListBox ListObjects;
		private TableLayoutPanel TableValues;
		private Label LabelValues;
		private ListBox ListValues;
		private TableLayoutPanel TableSubProperties;
		private Label LabelSubProperties;
		private ListBox ListSubProperties;
		private TableLayoutPanel TableProperties;
		private Label LabelProperties;
		private ListBox ListProperties;
		private ContextMenuStrip MenuEdit;
		private ToolStripMenuItem MenuEditCreate;
		private ToolStripMenuItem MenuEditRemove;
		private ToolStripSeparator MenuEditSeparator1;
		private ToolStripSeparator MenuEditSeparator2;
		private ToolStripMenuItem MenuEditSave;
		private ToolStripMenuItem MenuEditLoad;
		private ContextMenuStrip MenuGlobal;
		private ToolStripMenuItem MenuGlobalSave;
		private ToolStripMenuItem MenuGlobalLoad;
		private ToolStripMenuItem MenuEditEdit;
		private OpenFileDialog Load;
	}
}