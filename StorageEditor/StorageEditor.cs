namespace StorageEditor
{
	public partial class Window : Form
	{
		private readonly Dictionary<string, Dictionary<string, string>> data = new();
		private ListBox? clickedList;

		public Window()
		{
			InitializeComponent();
		}

		private void ParseValue(string instanceName, string prop)
		{
			var values = prop.Split("~|", StringSplitOptions.RemoveEmptyEntries);
			var valueStr = "";

			for(int k = 1; k < values.Length; k++)
				valueStr += "~|" + values[k];

			data[instanceName][values[0]] = DecryptText(valueStr);
		}
		private void ParseStruct(string instanceName, string prop)
		{
			var structSplit = prop.Split("~&", StringSplitOptions.RemoveEmptyEntries);
			var structProps = "";

			for(int i = 1; i < structSplit.Length; i++)
				structProps += $"~&" + structSplit[i];

			data[instanceName][structSplit[0]] = structProps;
		}
		private static string Trim(string text)
		{
			return text
				.Replace(" ", "")
				.Replace("\t", "")
				.Replace(Environment.NewLine, "");
		}
		private static string DecryptText(string text)
		{
			return text
				.Replace("~_", " ")
				.Replace("~__", "\t")
				.Replace("~/", Environment.NewLine);
		}
		private static string GetText(string title, string text)
		{
			var window = new Form()
			{
				Width = 500,
				Height = 150,
				FormBorderStyle = FormBorderStyle.FixedDialog,
				Text = title,
				StartPosition = FormStartPosition.CenterScreen
			};
			var textBox = new TextBox()
			{
				Text = text,
				Dock = DockStyle.Fill,
				Multiline = true,
				AcceptsReturn = true,
				AcceptsTab = true,
				BackColor = Color.Black,
				ForeColor = Color.Wheat
			};

			window.Controls.Add(textBox);
			window.ShowDialog();

			return textBox.Text;
		}

		private void Global_Click(object sender, MouseEventArgs e)
		{
			if(e.Button != MouseButtons.Right)
				return;

			MenuGlobal.Show(MousePosition);
		}
		private void List_Click(object sender, MouseEventArgs e)
		{
			if(e.Button != MouseButtons.Right)
				return;

			var listbox = (ListBox)sender;
			MenuEditEdit.Enabled = listbox.SelectedIndex != -1;
			MenuEditRemove.Enabled = listbox.SelectedIndex != -1;

			MenuEdit.Show(MousePosition);
			MenuGlobal.Hide();

			clickedList = listbox;

			MenuEditEdit.Text = MenuEditEdit.Enabled ?
				$"Edit [{listbox.SelectedItem}]" : $"Edit {listbox.Tag}";
			MenuEditRemove.Text = MenuEditEdit.Enabled ?
				$"Remove [{listbox.SelectedItem}]" : $"Remove {listbox.Tag}";
			MenuEditCreate.Text = $"Create {listbox.Tag}";

			if(listbox == ListObjects)
				MenuEditCreate.Enabled = true;
			else if(listbox == ListProperties)
				MenuEditCreate.Enabled = ListObjects.Items.Count > 0;
			else if(listbox == ListSubProperties)
				MenuEditCreate.Enabled = ListObjects.Items.Count > 0 && ListProperties.Items.Count > 0;
			else if(listbox == ListValues)
			{
				MenuEditCreate.Enabled = ListObjects.Items.Count > 0 &&
					ListProperties.Items.Count > 0 && ListSubProperties.Items.Count == 0;
				MenuEditRemove.Enabled = ListValues.Items.Count > 1;
			}
		}
		private void Load_Click(object sender, EventArgs e)
		{
			if(Load.ShowDialog() != DialogResult.OK)
				return;

			data.Clear();
			ListObjects.Items.Clear();
			ListProperties.Items.Clear();
			ListSubProperties.Items.Clear();
			ListValues.Items.Clear();

			var file = File.ReadAllText(Load.FileName);
			var instances = file.Split("~@", StringSplitOptions.RemoveEmptyEntries);

			for(int i = 0; i < instances.Length; i++)
			{
				var instance = Trim(instances[i]);
				if(instance.Contains("~~") == false)
					continue;

				var props = instance.Split("~~", StringSplitOptions.RemoveEmptyEntries);
				var instanceName = props[0];

				if(data.ContainsKey(instanceName) == false)
					data[instanceName] = new();

				ListObjects.Items.Add(Trim(instanceName));

				for(int j = 1; j < props.Length; j++)
				{
					var prop = Trim(props[j]);

					if(prop.Contains("~&"))
					{
						ParseStruct(instanceName, prop);
						continue;
					}
					ParseValue(instanceName, prop);
				}
			}

			if(ListProperties.SelectedIndex != 0)
				ListObjects.SelectedIndex = 0;
		}

		private void ListObjects_SelectedIndexChanged(object sender, EventArgs e)
		{
			var instance = (string)ListObjects.SelectedItem;
			var props = data[instance];

			ListProperties.Items.Clear();
			ListSubProperties.Items.Clear();
			ListValues.Items.Clear();

			foreach(var kvp in props)
			{
				ListProperties.Items.Add(kvp.Key);

				if(ListProperties.SelectedIndex != 0)
					ListProperties.SelectedIndex = 0;
			}
		}
		private void ListProperties_SelectedIndexChanged(object sender, EventArgs e)
		{
			var instance = (string)ListObjects.SelectedItem;
			var prop = (string)ListProperties.SelectedItem;
			var values = data[instance][prop];

			ListSubProperties.Items.Clear();
			ListValues.Items.Clear();

			if(values.Contains("~&"))
			{
				var structs = values.Split("~&", StringSplitOptions.RemoveEmptyEntries);
				var structProp = structs[0].Split("~=", StringSplitOptions.RemoveEmptyEntries);

				for(int j = 0; j < structProp.Length; j++)
				{
					var structValues = structProp[j]
						.Split("~|", StringSplitOptions.RemoveEmptyEntries);

					ListSubProperties.Items.Add(structValues[0]);
					ListSubProperties.SelectedIndex = 0;
				}

				return;
			}

			var valuesSplit = values.Split("~|", StringSplitOptions.RemoveEmptyEntries);
			for(int i = 0; i < valuesSplit.Length; i++)
			{
				ListValues.Items.Add(valuesSplit[i]);

				if(ListValues.SelectedIndex != 0)
					ListValues.SelectedIndex = 0;
			}
		}
		private void ListSubProperties_SelectedIndexChanged(object sender, EventArgs e)
		{
			var instance = (string)ListObjects.SelectedItem;
			var prop = (string)ListProperties.SelectedItem;
			var structs = data[instance][prop].Split("~&", StringSplitOptions.RemoveEmptyEntries);
			var propIndex = ListSubProperties.SelectedIndex;

			ListValues.Items.Clear();

			if(structs.Length > 1)
			{
				for(int i = 0; i < structs.Length; i++)
				{
					var props = structs[i].Split("~=", StringSplitOptions.RemoveEmptyEntries);
					var values = props[propIndex].Split("~|", StringSplitOptions.RemoveEmptyEntries);
					ListValues.Items.Add(DecryptText(values[1]));
				}

				ListValues.SelectedIndex = 0;
				return;
			}

			var property = structs[0].Split("~=", StringSplitOptions.RemoveEmptyEntries);
			var value = property[ListSubProperties.SelectedIndex]
				.Split("~|", StringSplitOptions.RemoveEmptyEntries);
			ListValues.Items.Add(DecryptText(value[1]));
			ListValues.SelectedIndex = 0;
		}

		private void MenuEditCreate_Click(object sender, EventArgs e)
		{
			if(clickedList == null)
				return;

			if(clickedList == ListObjects)
			{
				var i = 1;
				var key = "Object";
				while(data.ContainsKey($"{key}{i}"))
					i++;

				var result = $"{key}{i}";
				data[result] = new();
				ListObjects.Items.Add(result);

				if(ListObjects.SelectedIndex == -1)
					ListObjects.SelectedIndex = 0;
			}
			else if(clickedList == ListProperties)
			{
				var selectedObj = (string)ListObjects.SelectedItem;
				var i = 1;
				var key = "Property";
				while(data[selectedObj].ContainsKey($"{key}{i}"))
					i++;

				var result = $"{key}{i}";
				data[selectedObj][result] = "Value";
				ListProperties.Items.Add(result);

				if(ListProperties.SelectedIndex == -1)
					ListProperties.SelectedIndex = 0;
			}
			else if(clickedList == ListSubProperties)
			{
				var selectedObj = (string)ListObjects.SelectedItem;
				var selectedProp = (string)ListProperties.SelectedItem;
				if(data[selectedObj][selectedProp].Contains("~&") == false)
				{
					var val = $"~&SubProperty1~|{ListValues.SelectedItem}";
					data[selectedObj][selectedProp] = val;
					ListSubProperties.Items.Add("SubProperty1");
					ListSubProperties.SelectedIndex = 0;
					return;
				}

				var i = 1;
				var key = "SubProperty";
				var structs = data[selectedObj][selectedProp]
					.Split("~&", StringSplitOptions.RemoveEmptyEntries);
				var props = structs[0].Split("~=", StringSplitOptions.RemoveEmptyEntries);
				var subPropNames = new List<string>();

				for(int j = 0; j < props.Length; j++)
				{
					var values = props[j].Split("~|", StringSplitOptions.RemoveEmptyEntries);
					subPropNames.Add(values[0]);
				}

				while(subPropNames.Contains($"{key}{i}"))
					i++;

				for(int j = 0; j < structs.Length; j++)
					structs[j] = $"~&{structs[j]}~={key}{i}~|Value";

				var result = "";
				for(int j = 0; j < structs.Length; j++)
					result += structs[j];

				data[selectedObj][selectedProp] = result;
				ListSubProperties.Items.Add($"{key}{i}");

				if(ListSubProperties.SelectedIndex == -1)
					ListSubProperties.SelectedIndex = 0;
			}
			else if(clickedList == ListValues)
			{
				var selectedObj = (string)ListObjects.SelectedItem;
				var selectedProp = (string)ListProperties.SelectedItem;

				data[selectedObj][selectedProp] = data[selectedObj][selectedProp] + "~|Value";
				ListValues.Items.Add("Value");

				if(ListValues.SelectedIndex == -1)
					ListValues.SelectedItem = 0;
			}
		}
	}
}