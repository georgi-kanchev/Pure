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

		private void UpdateAllLists()
		{
			ListObjects_SelectedIndexChanged(this, new());
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

		private void TryEditLists(bool remove)
		{
			if(clickedList == null)
				return;

			if(clickedList == ListObjects)
				EditObject(remove);
			else if(clickedList == ListProperties)
				EditProperties(remove);
			else if(clickedList == ListSubProperties)
				EditSubProperties(remove);
			else if(clickedList == ListValues)
				EditValues(remove);

			UpdateAllLists();
		}
		private void EditObject(bool remove)
		{
			var objName = (string)ListObjects.SelectedItem;
			if(remove)
			{
				data.Remove(objName);
				ListObjects.Items.Remove(objName);

				if(ListObjects.Items.Count == 0)
				{
					ListProperties.Items.Clear();
					ListSubProperties.Items.Clear();
					ListValues.Items.Clear();
				}

				return;
			}

			var value = data[objName];
			var input = Trim(GetText($"Edit Object '{objName}'", objName));

			if(objName != input && data.ContainsKey(input))
			{
				MessageBox.Show($"An Object '{input}' already exists.", $"Edit Object '{objName}'");
				return;
			}

			data.Remove(objName);
			data[input] = value;

			ListObjects.Items.Remove(objName);
			ListObjects.Items.Add(input);
			ListObjects.SelectedItem = input;
		}
		private void EditProperties(bool remove)
		{
			var objName = (string)ListObjects.SelectedItem;
			var prop = (string)ListProperties.SelectedItem;

			if(remove)
			{
				data[objName].Remove(prop);
				return;
			}

			var value = data[objName][prop];
			var input = Trim(GetText($"Edit Property '{prop}'", prop));

			if(input != prop && data[objName].ContainsKey(input))
			{
				MessageBox.Show(
					$"The Object '{objName}' already has a Property '{input}'.",
					$"Edit Property '{prop}'");
				return;
			}

			data[objName].Remove(prop);
			data[objName][input] = value;
		}
		private void EditSubProperties(bool remove)
		{
			var objName = (string)ListObjects.SelectedItem;
			var prop = (string)ListProperties.SelectedItem;
			var subProp = (string)ListSubProperties.SelectedItem;
			var structs = data[objName][prop]
				.Split("~&", StringSplitOptions.RemoveEmptyEntries);
			var input = remove ? "" : Trim(GetText($"Edit Sub Property '{subProp}'", subProp));
			var result = "";

			for(int j = 0; j < structs.Length; j++)
			{
				var subProps = structs[j]
					.Split("~=", StringSplitOptions.RemoveEmptyEntries);

				result += "~&";

				for(int i = 0; i < subProps.Length; i++)
				{
					var value = subProps[i].Split("~|", StringSplitOptions.RemoveEmptyEntries);
					if(i == ListSubProperties.SelectedIndex)
					{
						if(remove)
						{
							if(ListSubProperties.Items.Count == 1)
							{
								data[objName][prop] = "~|Value";
								return;
							}
							continue;
						}

						value[0] = input;
					}
					else if(value[0] == input)
					{
						MessageBox.Show(
							$"The Property '{prop}' of the Object '{objName}' " +
							$"already has a Sub Property '{input}'.",
							$"Edit Sub Property '{prop}'");
						return;
					}

					result += $"~={value[0]}~|{value[1]}";
				}
			}
			data[objName][prop] = result;
		}
		private void EditValues(bool remove)
		{
			var objName = (string)ListObjects.SelectedItem;
			var prop = (string)ListProperties.SelectedItem;
			var value = (string)ListValues.SelectedItem;

			if(ListValues.Items.Count > 1)
			{
				var split = value.Split("] ", StringSplitOptions.RemoveEmptyEntries);
				value = "";
				for(int i = 1; i < split.Length; i++)
					value += split[i];
			}

			var rawValue = data[objName][prop];
			var input = remove ?
				"" : GetText($"Edit Value [{ListValues.SelectedIndex}] '{value}'", value);
			var result = "";

			if(rawValue.Contains("~&"))
			{
				var structs = data[objName][prop]
					.Split("~&", StringSplitOptions.RemoveEmptyEntries);

				for(int j = 0; j < structs.Length; j++)
				{
					var subProps = structs[j]
						.Split("~=", StringSplitOptions.RemoveEmptyEntries);

					result += "~&";

					for(int i = 0; i < subProps.Length; i++)
					{
						var v = subProps[i].Split("~|", StringSplitOptions.RemoveEmptyEntries);
						if(i == ListSubProperties.SelectedIndex && j == ListValues.SelectedIndex)
						{
							if(remove)
								continue;

							v[1] = input;
						}

						result += $"~={v[0]}~|{v[1]}";
					}
				}
			}
			else
			{
				var values = data[objName][prop].Split("~|", StringSplitOptions.RemoveEmptyEntries);
				for(int i = 0; i < values.Length; i++)
				{
					if(i == ListValues.SelectedIndex)
						values[i] = input;

					result += $"~|{values[i]}";
				}
			}

			data[objName][prop] = result;
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
				$"Edit '{listbox.SelectedItem}'" : $"Edit {listbox.Tag}";
			MenuEditRemove.Text = MenuEditEdit.Enabled ?
				$"Remove '{listbox.SelectedItem}'" : $"Remove {listbox.Tag}";
			MenuEditCreate.Text = $"Create {listbox.Tag}";

			if(listbox == ListObjects)
				MenuEditCreate.Enabled = true;
			else if(listbox == ListProperties)
				MenuEditCreate.Enabled = ListObjects.Items.Count > 0;
			else if(listbox == ListSubProperties)
			{
				var objName = (string)ListObjects.SelectedItem;
				var propName = (string)ListProperties.SelectedItem;
				var isArray = ListValues.Items.Count > 1 &&
					data[objName][propName].Contains("~&") == false;

				MenuEditCreate.Enabled = ListObjects.Items.Count > 0 &&
					ListProperties.Items.Count > 0 && isArray == false;
			}
			else if(listbox == ListValues)
			{
				MenuEditCreate.Enabled = ListObjects.Items.Count > 0 &&
					ListProperties.Items.Count > 0;
				MenuEditRemove.Enabled = ListObjects.Items.Count > 0 &&
					ListValues.Items.Count > 1;

				var valueStr = MenuEditCreate.Enabled ?
					$" [{ListValues.Items.Count}] of '{ListProperties.SelectedItem}'" : "";
				MenuEditCreate.Text = $"Create Value{valueStr}";
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

			if(ListObjects.Items.Count > 0)
				ListObjects.SelectedIndex = 0;
			if(ListProperties.Items.Count > 0)
				ListProperties.SelectedIndex = 0;
		}

		private void ListObjects_SelectedIndexChanged(object sender, EventArgs e)
		{
			var instance = (string)ListObjects.SelectedItem;
			if(instance == null)
				return;

			var props = data[instance];

			var prevPropIndex = ListProperties.SelectedIndex;
			var prevSubPropIndex = ListSubProperties.SelectedIndex;
			var prevValueIndex = ListValues.SelectedIndex;

			ListProperties.Items.Clear();
			ListSubProperties.Items.Clear();
			ListValues.Items.Clear();

			foreach(var kvp in props)
				ListProperties.Items.Add(kvp.Key);

			if(prevPropIndex >= ListProperties.Items.Count)
				prevPropIndex--;
			ListProperties.SelectedIndex = prevPropIndex;

			if(prevSubPropIndex >= ListSubProperties.Items.Count)
				prevSubPropIndex--;
			ListSubProperties.SelectedIndex = prevSubPropIndex;

			if(prevValueIndex >= ListValues.Items.Count)
				prevValueIndex--;
			ListValues.SelectedIndex = prevValueIndex;
		}
		private void ListProperties_SelectedIndexChanged(object sender, EventArgs e)
		{
			var instance = (string)ListObjects.SelectedItem;
			var prop = (string)ListProperties.SelectedItem;
			if(instance == null || prop == null)
				return;

			var values = data[instance][prop];

			ListSubProperties.Items.Clear();
			ListValues.Items.Clear();

			if(values.Contains("~&"))
			{
				var structs = values.Split("~&", StringSplitOptions.RemoveEmptyEntries);
				if(structs.Length == 0)
					return;
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
				var indexStr = valuesSplit.Length > 1 ? $"[{i}] " : "";
				ListValues.Items.Add($"{indexStr}{valuesSplit[i]}");

				if(ListValues.SelectedIndex != 0)
					ListValues.SelectedIndex = 0;
			}
		}
		private void ListSubProperties_SelectedIndexChanged(object sender, EventArgs e)
		{
			var instance = (string)ListObjects.SelectedItem;
			var prop = (string)ListProperties.SelectedItem;
			if(instance == null || prop == null)
				return;

			var structs = data[instance][prop].Split("~&", StringSplitOptions.RemoveEmptyEntries);
			var propIndex = ListSubProperties.SelectedIndex;

			ListValues.Items.Clear();

			if(structs.Length > 1)
			{
				for(int i = 0; i < structs.Length; i++)
				{
					var props = structs[i].Split("~=", StringSplitOptions.RemoveEmptyEntries);
					var values = props[propIndex].Split("~|", StringSplitOptions.RemoveEmptyEntries);
					ListValues.Items.Add($"[{i}] {DecryptText(values[1])}");
				}

				ListValues.SelectedIndex = 0;
				return;
			}

			var property = structs[0].Split("~=", StringSplitOptions.RemoveEmptyEntries);
			if(ListSubProperties.SelectedIndex == -1)
				return;

			var value = property[ListSubProperties.SelectedIndex]
				.Split("~|", StringSplitOptions.RemoveEmptyEntries);
			ListValues.Items.Add(DecryptText(value[1]));

			if(ListValues.SelectedIndex == -1)
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

				if(ListObjects.Items.Count == 1)
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

				UpdateAllLists();
				if(ListProperties.Items.Count == 1)
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
			}
			else if(clickedList == ListValues)
			{
				var selectedObj = (string)ListObjects.SelectedItem;
				var selectedProp = (string)ListProperties.SelectedItem;

				var raw = data[selectedObj][selectedProp];
				if(raw.Contains("~&") == false)
					data[selectedObj][selectedProp] += "~|Value";
				else
				{
					var result = "~&";
					var structs = raw.Split("~&", StringSplitOptions.RemoveEmptyEntries);
					if(structs.Length == 0)
						return;

					var props = structs[0].Split("~=", StringSplitOptions.RemoveEmptyEntries);

					for(int i = 0; i < props.Length; i++)
					{
						var values = props[i].Split("~|", StringSplitOptions.RemoveEmptyEntries);
						result += $"~={values[0]}~|Value";
					}

					data[selectedObj][selectedProp] += result;
				}
			}

			UpdateAllLists();
		}
		private void MenuEditEdit_Click(object sender, EventArgs e)
		{
			TryEditLists(false);
		}
		private void MenuEditRemove_Click(object sender, EventArgs e)
		{
			TryEditLists(true);
		}
	}
}