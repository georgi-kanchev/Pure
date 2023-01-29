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
			this.Menu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.MenuEdit = new System.Windows.Forms.ToolStripMenuItem();
			this.MenuDuplicate = new System.Windows.Forms.ToolStripMenuItem();
			this.MenuCopyText = new System.Windows.Forms.ToolStripMenuItem();
			this.MenuRemove = new System.Windows.Forms.ToolStripMenuItem();
			this.MenuSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.MenuCreate = new System.Windows.Forms.ToolStripMenuItem();
			this.MenuSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.MenuStorage = new System.Windows.Forms.ToolStripMenuItem();
			this.MenuStorageSave = new System.Windows.Forms.ToolStripMenuItem();
			this.MenuStorageLoad = new System.Windows.Forms.ToolStripMenuItem();
			this.StorageLoad = new System.Windows.Forms.OpenFileDialog();
			this.StorageSave = new System.Windows.Forms.SaveFileDialog();
			this.TableMain.SuspendLayout();
			this.TableValues.SuspendLayout();
			this.TableSubProperties.SuspendLayout();
			this.TableProperties.SuspendLayout();
			this.TableObjects.SuspendLayout();
			this.Menu.SuspendLayout();
			this.SuspendLayout();
			// 
			// TableMain
			// 
			this.TableMain.ColumnCount = 4;
			this.TableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.TableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.TableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.TableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
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
			this.TableMain.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Global_MouseUp);
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
			this.TableValues.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Global_MouseUp);
			// 
			// LabelValues
			// 
			this.LabelValues.AutoSize = true;
			this.LabelValues.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelValues.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.LabelValues.ForeColor = System.Drawing.Color.White;
			this.LabelValues.Location = new System.Drawing.Point(3, 0);
			this.LabelValues.Name = "LabelValues";
			this.LabelValues.Size = new System.Drawing.Size(184, 27);
			this.LabelValues.TabIndex = 0;
			this.LabelValues.Text = "Values";
			this.LabelValues.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.LabelValues.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Global_MouseUp);
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
			this.ListValues.TabStop = false;
			this.ListValues.Tag = "Value";
			this.ListValues.MouseDown += new System.Windows.Forms.MouseEventHandler(this.List_MouseDown);
			this.ListValues.MouseEnter += new System.EventHandler(this.List_MouseEnter);
			this.ListValues.MouseUp += new System.Windows.Forms.MouseEventHandler(this.List_MouseUp);
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
			this.TableSubProperties.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Global_MouseUp);
			// 
			// LabelSubProperties
			// 
			this.LabelSubProperties.AutoSize = true;
			this.LabelSubProperties.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelSubProperties.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.LabelSubProperties.ForeColor = System.Drawing.Color.White;
			this.LabelSubProperties.Location = new System.Drawing.Point(3, 0);
			this.LabelSubProperties.Name = "LabelSubProperties";
			this.LabelSubProperties.Size = new System.Drawing.Size(184, 27);
			this.LabelSubProperties.TabIndex = 0;
			this.LabelSubProperties.Text = "Sub Properties";
			this.LabelSubProperties.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.LabelSubProperties.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Global_MouseUp);
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
			this.ListSubProperties.TabStop = false;
			this.ListSubProperties.Tag = "Sub Property";
			this.ListSubProperties.SelectedIndexChanged += new System.EventHandler(this.ListSubProperties_SelectedIndexChanged);
			this.ListSubProperties.MouseDown += new System.Windows.Forms.MouseEventHandler(this.List_MouseDown);
			this.ListSubProperties.MouseEnter += new System.EventHandler(this.List_MouseEnter);
			this.ListSubProperties.MouseUp += new System.Windows.Forms.MouseEventHandler(this.List_MouseUp);
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
			this.TableProperties.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Global_MouseUp);
			// 
			// LabelProperties
			// 
			this.LabelProperties.AutoSize = true;
			this.LabelProperties.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelProperties.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.LabelProperties.ForeColor = System.Drawing.Color.White;
			this.LabelProperties.Location = new System.Drawing.Point(3, 0);
			this.LabelProperties.Name = "LabelProperties";
			this.LabelProperties.Size = new System.Drawing.Size(184, 27);
			this.LabelProperties.TabIndex = 0;
			this.LabelProperties.Text = "Properties";
			this.LabelProperties.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.LabelProperties.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Global_MouseUp);
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
			this.ListProperties.TabStop = false;
			this.ListProperties.Tag = "Property";
			this.ListProperties.SelectedIndexChanged += new System.EventHandler(this.ListProperties_SelectedIndexChanged);
			this.ListProperties.MouseDown += new System.Windows.Forms.MouseEventHandler(this.List_MouseDown);
			this.ListProperties.MouseEnter += new System.EventHandler(this.List_MouseEnter);
			this.ListProperties.MouseUp += new System.Windows.Forms.MouseEventHandler(this.List_MouseUp);
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
			this.TableObjects.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Global_MouseUp);
			// 
			// LabelObjects
			// 
			this.LabelObjects.AutoSize = true;
			this.LabelObjects.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelObjects.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.LabelObjects.ForeColor = System.Drawing.Color.White;
			this.LabelObjects.Location = new System.Drawing.Point(3, 0);
			this.LabelObjects.Name = "LabelObjects";
			this.LabelObjects.Size = new System.Drawing.Size(184, 27);
			this.LabelObjects.TabIndex = 0;
			this.LabelObjects.Text = "Objects";
			this.LabelObjects.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.LabelObjects.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Global_MouseUp);
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
			this.ListObjects.TabStop = false;
			this.ListObjects.Tag = "Object";
			this.ListObjects.SelectedIndexChanged += new System.EventHandler(this.ListObjects_SelectedIndexChanged);
			this.ListObjects.MouseDown += new System.Windows.Forms.MouseEventHandler(this.List_MouseDown);
			this.ListObjects.MouseEnter += new System.EventHandler(this.List_MouseEnter);
			this.ListObjects.MouseUp += new System.Windows.Forms.MouseEventHandler(this.List_MouseUp);
			// 
			// Menu
			// 
			this.Menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuEdit,
            this.MenuDuplicate,
            this.MenuCopyText,
            this.MenuRemove,
            this.MenuSeparator1,
            this.MenuCreate,
            this.MenuSeparator2,
            this.MenuStorage});
			this.Menu.Name = "contextMenuStrip1";
			this.Menu.Size = new System.Drawing.Size(180, 148);
			// 
			// MenuEdit
			// 
			this.MenuEdit.Name = "MenuEdit";
			this.MenuEdit.Size = new System.Drawing.Size(179, 22);
			this.MenuEdit.Text = "Edit [E]";
			this.MenuEdit.Click += new System.EventHandler(this.MenuEdit_Click);
			// 
			// MenuDuplicate
			// 
			this.MenuDuplicate.Name = "MenuDuplicate";
			this.MenuDuplicate.Size = new System.Drawing.Size(179, 22);
			this.MenuDuplicate.Text = "Duplicate [Ctrl + D]";
			this.MenuDuplicate.Click += new System.EventHandler(this.MenuDuplicate_Click);
			// 
			// MenuCopyText
			// 
			this.MenuCopyText.Name = "MenuCopyText";
			this.MenuCopyText.Size = new System.Drawing.Size(179, 22);
			this.MenuCopyText.Text = "Copy Text [Ctrl + C]";
			this.MenuCopyText.Click += new System.EventHandler(this.MenuCopyText_Click);
			// 
			// MenuRemove
			// 
			this.MenuRemove.Name = "MenuRemove";
			this.MenuRemove.Size = new System.Drawing.Size(179, 22);
			this.MenuRemove.Text = "Remove [Delete / R]";
			this.MenuRemove.Click += new System.EventHandler(this.MenuRemove_Click);
			// 
			// MenuSeparator1
			// 
			this.MenuSeparator1.Name = "MenuSeparator1";
			this.MenuSeparator1.Size = new System.Drawing.Size(176, 6);
			// 
			// MenuCreate
			// 
			this.MenuCreate.Name = "MenuCreate";
			this.MenuCreate.Size = new System.Drawing.Size(179, 22);
			this.MenuCreate.Text = "Create [C]";
			this.MenuCreate.Click += new System.EventHandler(this.MenuCreate_Click);
			// 
			// MenuSeparator2
			// 
			this.MenuSeparator2.Name = "MenuSeparator2";
			this.MenuSeparator2.Size = new System.Drawing.Size(176, 6);
			// 
			// MenuStorage
			// 
			this.MenuStorage.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuStorageSave,
            this.MenuStorageLoad});
			this.MenuStorage.Name = "MenuStorage";
			this.MenuStorage.Size = new System.Drawing.Size(179, 22);
			this.MenuStorage.Text = "Storage";
			// 
			// MenuStorageSave
			// 
			this.MenuStorageSave.Name = "MenuStorageSave";
			this.MenuStorageSave.Size = new System.Drawing.Size(150, 22);
			this.MenuStorageSave.Text = "Save [Ctrl + S]";
			this.MenuStorageSave.Click += new System.EventHandler(this.MenuSave_Click);
			// 
			// MenuStorageLoad
			// 
			this.MenuStorageLoad.Name = "MenuStorageLoad";
			this.MenuStorageLoad.Size = new System.Drawing.Size(150, 22);
			this.MenuStorageLoad.Text = "Load [Ctrl + L]";
			this.MenuStorageLoad.Click += new System.EventHandler(this.MenuLoad_Click);
			// 
			// StorageLoad
			// 
			this.StorageLoad.AddExtension = false;
			this.StorageLoad.Title = "Load Storage";
			// 
			// StorageSave
			// 
			this.StorageSave.Title = "Save Storage";
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
			this.Text = "Pure - Storage Editor";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Global_KeyDown);
			this.TableMain.ResumeLayout(false);
			this.TableValues.ResumeLayout(false);
			this.TableValues.PerformLayout();
			this.TableSubProperties.ResumeLayout(false);
			this.TableSubProperties.PerformLayout();
			this.TableProperties.ResumeLayout(false);
			this.TableProperties.PerformLayout();
			this.TableObjects.ResumeLayout(false);
			this.TableObjects.PerformLayout();
			this.Menu.ResumeLayout(false);
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
		private ContextMenuStrip Menu;
		private ToolStripMenuItem MenuCreate;
		private ToolStripMenuItem MenuRemove;
		private ToolStripSeparator MenuSeparator1;
		private ToolStripSeparator MenuSeparator2;
		private ToolStripMenuItem MenuEdit;
		private OpenFileDialog StorageLoad;
		private ToolStripMenuItem MenuStorage;
		private ToolStripMenuItem MenuStorageSave;
		private ToolStripMenuItem MenuStorageLoad;
		private SaveFileDialog StorageSave;
		private ToolStripMenuItem MenuCopyText;
		private ToolStripMenuItem MenuDuplicate;
	}
}