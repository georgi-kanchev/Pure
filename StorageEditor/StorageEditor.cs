namespace StorageEditor
{
	public partial class Window : Form
	{
		#region Fields
		private const string INSTANCE = "~@", INSTANCE_PROPERTY = "~~", VALUE = "~|",
			STRUCT = "~&", STRUCT_PROPERTY = "~=",
			SPACE = "~_", TAB = "~__", NEW_LINE = "~/";
		private const string FILE_HEADER = @"Purity - Storage file
--------------------------
| Map of symbols
|
|	~ Global separator
|
|	@ Object
|	~ Property
|	& Property containing sub properties
|	- Sub property
|	| Value
|
|	/ String new line
|	_ String space
|	__ String tab
--------------------------
";

		private readonly Dictionary<(bool, Keys), Action> hotkeys = new();
		private readonly Dictionary<string, Dictionary<string, string>> data = new();
		private ListBox? hoveredList;
		#endregion

		public Window()
		{
			InitializeComponent();
			Focus();

			hotkeys.Add((false, Keys.Delete), TryRemoveLists);
			hotkeys.Add((false, Keys.R), TryRemoveLists);
			hotkeys.Add((false, Keys.C), TryCreate);
			hotkeys.Add((false, Keys.E), TryEditLists);

			hotkeys.Add((true, Keys.C), TryCopy);
			hotkeys.Add((true, Keys.D), TryDuplicate);

			hotkeys.Add((true, Keys.S), TrySave);
			hotkeys.Add((true, Keys.L), TryLoad);
		}

		#region System
		private void UpdateAllLists()
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
				prevPropIndex = ListProperties.Items.Count - 1;
			ListProperties.SelectedIndex = prevPropIndex;

			if(prevSubPropIndex >= ListSubProperties.Items.Count)
				prevSubPropIndex = ListSubProperties.Items.Count - 1;
			ListSubProperties.SelectedIndex = prevSubPropIndex;

			if(prevValueIndex >= ListValues.Items.Count)
				prevValueIndex = ListValues.Items.Count - 1;
			ListValues.SelectedIndex = prevValueIndex;

			if(ListProperties.SelectedIndex == -1 && ListProperties.Items.Count > 0)
				ListProperties.SelectedIndex = 0;
			if(ListSubProperties.SelectedIndex == -1 && ListSubProperties.Items.Count > 0)
				ListSubProperties.SelectedIndex = 0;
			if(ListValues.SelectedIndex == -1 && ListValues.Items.Count > 0)
				ListValues.SelectedIndex = 0;
		}
		private void UpdateEditMenu()
		{
			if(hoveredList == null)
				return;

			MenuEdit.Enabled = hoveredList.SelectedIndex != -1;
			MenuRemove.Enabled = hoveredList.SelectedIndex != -1;

			if(hoveredList == ListObjects)
				MenuCreate.Enabled = true;
			else if(hoveredList == ListProperties)
				MenuCreate.Enabled = ListObjects.Items.Count > 0;
			else if(hoveredList == ListSubProperties)
			{
				var objName = (string)ListObjects.SelectedItem;
				var propName = (string)ListProperties.SelectedItem;
				var isArray = ListValues.Items.Count > 1 &&
					data[objName][propName].Contains(STRUCT) == false;

				MenuCreate.Enabled = ListObjects.Items.Count > 0 &&
					ListProperties.Items.Count > 0 && isArray == false;
			}
			else if(hoveredList == ListValues)
			{
				MenuCreate.Enabled = ListObjects.Items.Count > 0 &&
					ListProperties.Items.Count > 0;
				MenuRemove.Enabled = ListObjects.Items.Count > 0 &&
					ListValues.Items.Count > 1;
			}

			MenuDuplicate.Enabled = MenuEdit.Enabled;
			MenuCopyText.Enabled = MenuEdit.Enabled;
		}

		private void ParseValue(string instanceName, string prop)
		{
			var values = prop.Split(VALUE, StringSplitOptions.RemoveEmptyEntries);
			var valueStr = "";

			for(int k = 1; k < values.Length; k++)
				valueStr += VALUE + values[k];

			data[instanceName][values[0]] = DecryptText(valueStr);
		}
		private void ParseStruct(string instanceName, string prop)
		{
			var structSplit = prop.Split(STRUCT, StringSplitOptions.RemoveEmptyEntries);
			var structProps = "";

			for(int i = 1; i < structSplit.Length; i++)
				structProps += STRUCT + structSplit[i];

			data[instanceName][structSplit[0]] = structProps;
		}
		private static string ParseSelectedIndexItem(ListBox list)
		{
			var value = (string)list.SelectedItem;
			if(list.Items.Count > 1)
			{
				var split = value.Split("] ", StringSplitOptions.RemoveEmptyEntries);
				value = "";
				for(int i = 1; i < split.Length; i++)
					value += split[i];
			}
			return value;
		}

		private void TryHotkeys(bool control, Keys key)
		{
			foreach(var kvp in hotkeys)
			{
				var ctrl = kvp.Key.Item1 == control;
				var curKey = kvp.Key.Item2;

				if(ctrl && key == curKey)
					kvp.Value.Invoke();
			}
		}
		private void TryDuplicate()
		{
			if(MenuDuplicate.Enabled == false)
				return;

			if(hoveredList == ListObjects)
			{
				var rawData = data[(string)ListObjects.SelectedItem];
				var newData = new Dictionary<string, string>();
				foreach(var kvp in rawData)
					newData[kvp.Key] = kvp.Value;

				TryCreate();
				data[(string)ListObjects.SelectedItem] = newData;
			}
			else if(hoveredList == ListProperties)
			{
				var rawData = data[(string)ListObjects.SelectedItem]
					[(string)ListProperties.SelectedItem];

				TryCreate();
				data[(string)ListObjects.SelectedItem][(string)ListProperties.SelectedItem] = rawData;
			}

			UpdateAllLists();
		}
		private void TryCopy()
		{
			if(MenuCopyText.Enabled == false || hoveredList == null)
				return;

			Clipboard.SetText(ParseSelectedIndexItem(hoveredList));
		}
		private void TrySave()
		{
			if(StorageSave.ShowDialog() != DialogResult.OK)
				return;

			var isDataFormatted = true;
			var result = isDataFormatted ? FILE_HEADER + Environment.NewLine : "";
			var newLine = isDataFormatted ? Environment.NewLine : "";
			var space = isDataFormatted ? " " : "";
			var tab = isDataFormatted ? new string(' ', 4) : "";

			foreach(var kvp in data)
			{
				result += INSTANCE + space + kvp.Key;

				foreach(var kvp2 in kvp.Value)
				{
					var value = kvp2.Value;

					result += $"{newLine}{tab}{INSTANCE_PROPERTY}{space}{kvp2.Key}" +
						$"{space}{EncryptText(kvp2.Value)}";
				}
			}

			File.WriteAllText(StorageSave.FileName, result);
		}
		private void TryLoad()
		{
			if(StorageLoad.ShowDialog() != DialogResult.OK)
				return;

			data.Clear();
			ListObjects.Items.Clear();
			ListProperties.Items.Clear();
			ListSubProperties.Items.Clear();
			ListValues.Items.Clear();

			var file = File.ReadAllText(StorageLoad.FileName);
			var instances = file.Split(INSTANCE, StringSplitOptions.RemoveEmptyEntries);

			for(int i = 0; i < instances.Length; i++)
			{
				var instance = Trim(instances[i]);
				if(instance.Contains(INSTANCE_PROPERTY) == false)
					continue;

				var props = instance.Split(INSTANCE_PROPERTY, StringSplitOptions.RemoveEmptyEntries);
				var instanceName = props[0];

				if(data.ContainsKey(instanceName) == false)
					data[instanceName] = new();

				ListObjects.Items.Add(Trim(instanceName));

				for(int j = 1; j < props.Length; j++)
				{
					var prop = Trim(props[j]);

					if(prop.Contains(STRUCT))
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
		private void TryCreate()
		{
			if(hoveredList == null || MenuCreate.Enabled == false)
				return;

			if(hoveredList == ListObjects)
			{
				var i = 1;
				var key = "Object";
				while(data.ContainsKey($"{key}{i}"))
					i++;

				var result = $"{key}{i}";
				data[result] = new();

				ListObjects.Items.Add(result);
			}
			else if(hoveredList == ListProperties)
			{
				var selectedObj = (string)ListObjects.SelectedItem;
				var i = 1;
				var key = "Property";
				while(data[selectedObj].ContainsKey($"{key}{i}"))
					i++;

				var result = $"{key}{i}";
				data[selectedObj][result] = "Value";

				UpdateAllLists();
			}
			else if(hoveredList == ListSubProperties)
			{
				var selectedObj = (string)ListObjects.SelectedItem;
				var selectedProp = (string)ListProperties.SelectedItem;

				if(selectedObj == null || selectedProp == null)
					return;

				if(data[selectedObj][selectedProp].Contains(STRUCT) == false)
				{
					var val = $"{STRUCT}SubProperty1{VALUE}{ListValues.SelectedItem}";
					data[selectedObj][selectedProp] = val;
					ListSubProperties.Items.Add("SubProperty1");
					return;
				}

				var i = 1;
				var key = "SubProperty";
				var structs = data[selectedObj][selectedProp]
					.Split(STRUCT, StringSplitOptions.RemoveEmptyEntries);
				var props = structs[0].Split(STRUCT_PROPERTY, StringSplitOptions.RemoveEmptyEntries);
				var subPropNames = new List<string>();

				for(int j = 0; j < props.Length; j++)
				{
					var values = props[j].Split(VALUE, StringSplitOptions.RemoveEmptyEntries);
					subPropNames.Add(values[0]);
				}

				while(subPropNames.Contains($"{key}{i}"))
					i++;

				for(int j = 0; j < structs.Length; j++)
					structs[j] = $"{STRUCT}{structs[j]}{STRUCT_PROPERTY}{key}{i}{VALUE}Value";

				var result = "";
				for(int j = 0; j < structs.Length; j++)
					result += structs[j];

				data[selectedObj][selectedProp] = result;
			}
			else if(hoveredList == ListValues)
			{
				var selectedObj = (string)ListObjects.SelectedItem;
				var selectedProp = (string)ListProperties.SelectedItem;

				var raw = data[selectedObj][selectedProp];
				if(raw.Contains(STRUCT) == false)
					data[selectedObj][selectedProp] += $"{VALUE}Value";
				else
				{
					var result = STRUCT;
					var structs = raw.Split(STRUCT, StringSplitOptions.RemoveEmptyEntries);
					if(structs.Length == 0)
						return;

					var props = structs[0].Split(STRUCT_PROPERTY, StringSplitOptions.RemoveEmptyEntries);

					for(int i = 0; i < props.Length; i++)
					{
						var values = props[i].Split(VALUE, StringSplitOptions.RemoveEmptyEntries);
						result += $"{STRUCT_PROPERTY}{values[0]}{VALUE}Value";
					}

					data[selectedObj][selectedProp] += result;
				}
			}

			UpdateAllLists();
			hoveredList.SelectedIndex = hoveredList.Items.Count - 1;
		}
		private void TryRemoveLists()
		{
			TryEditLists(true);
		}
		private void TryEditLists()
		{
			TryEditLists(false);
		}
		private void TryEditLists(bool remove)
		{
			if(hoveredList == null ||
				(MenuRemove.Enabled == false && remove) ||
				(MenuEdit.Enabled == false && remove == false))
				return;

			if(hoveredList == ListObjects)
				EditObject(remove);
			else if(hoveredList == ListProperties)
				EditProperties(remove);
			else if(hoveredList == ListSubProperties)
				EditSubProperties(remove);
			else if(hoveredList == ListValues)
				EditValues(remove);

			UpdateAllLists();
		}
		private static string TryGetEdit(string title, string text)
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

			var t = textBox.Text;
			if(t.Contains(STRUCT) || t.Contains(STRUCT_PROPERTY) ||
				t.Contains(INSTANCE) || t.Contains(INSTANCE_PROPERTY) ||
				t.Contains(VALUE) || t.Contains(SPACE) || t.Contains(TAB) ||
				t.Contains(NEW_LINE))
			{
				var space = "       ";
				MessageBox.Show($"The provided edit cannot contain special separators:{Environment.NewLine}" +
					$"{Environment.NewLine}{INSTANCE}{space}{INSTANCE_PROPERTY}{space}{STRUCT}{space}" +
					$"{STRUCT_PROPERTY}{space}{VALUE}{space}{SPACE}{space}{TAB}{space}{NEW_LINE}",
					"Invalid Edit");
				return text;
			}

			return t;
		}

		private void EditObject(bool remove)
		{
			var objName = (string)ListObjects.SelectedItem;
			if(objName == null)
				return;

			var prevSelectedIndex = ListObjects.SelectedIndex;

			if(remove)
			{
				data.Remove(objName);
				ListObjects.Items.Remove(objName);

				if(ListObjects.Items.Count == 0)
				{
					ListProperties.Items.Clear();
					ListSubProperties.Items.Clear();
					ListValues.Items.Clear();
					return;
				}

				if(prevSelectedIndex >= ListObjects.Items.Count)
					prevSelectedIndex = ListObjects.Items.Count - 1;
				ListObjects.SelectedIndex = prevSelectedIndex;
				return;
			}

			var value = data[objName];
			var input = Trim(TryGetEdit($"Edit Object '{objName}'", objName));

			if(objName != input && data.ContainsKey(input))
			{
				MessageBox.Show($"An Object '{input}' already exists.", $"Edit Object '{objName}'");
				return;
			}

			data.Remove(objName);
			data[input] = value;

			ListObjects.Items.Remove(objName);
			ListObjects.Items.Insert(prevSelectedIndex, input);
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
			var input = Trim(TryGetEdit($"Edit Property '{prop}'", prop));

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

			if(prop == null || subProp == null)
				return;

			var structs = data[objName][prop]
				.Split(STRUCT, StringSplitOptions.RemoveEmptyEntries);
			var input = remove ? "" : Trim(TryGetEdit($"Edit Sub Property '{subProp}'", subProp));
			var result = "";

			for(int j = 0; j < structs.Length; j++)
			{
				var subProps = structs[j]
					.Split(STRUCT_PROPERTY, StringSplitOptions.RemoveEmptyEntries);

				result += STRUCT;

				for(int i = 0; i < subProps.Length; i++)
				{
					var value = subProps[i].Split(VALUE, StringSplitOptions.RemoveEmptyEntries);
					if(i == ListSubProperties.SelectedIndex)
					{
						if(remove)
						{
							if(ListSubProperties.Items.Count == 1)
							{
								var valueIndex = ListValues.SelectedIndex;
								var remainingValue = structs[valueIndex]
									.Split(STRUCT_PROPERTY, StringSplitOptions.RemoveEmptyEntries)[0]
									.Split(VALUE, StringSplitOptions.RemoveEmptyEntries)[1];
								data[objName][prop] = $"{VALUE}{remainingValue}";
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

					result += $"{STRUCT_PROPERTY}{value[0]}{VALUE}{value[1]}";
				}
			}
			data[objName][prop] = result;
		}
		private void EditValues(bool remove)
		{
			var objName = (string)ListObjects.SelectedItem;
			var prop = (string)ListProperties.SelectedItem;
			var value = ParseSelectedIndexItem(ListValues);

			var rawValue = data[objName][prop];
			var input = remove ?
				"" : TryGetEdit($"Edit Value [{ListValues.SelectedIndex}] '{value}'", value);
			var result = "";

			if(rawValue.Contains(STRUCT))
			{
				var structs = data[objName][prop]
					.Split(STRUCT, StringSplitOptions.RemoveEmptyEntries);

				for(int j = 0; j < structs.Length; j++)
				{
					var subProps = structs[j]
						.Split(STRUCT_PROPERTY, StringSplitOptions.RemoveEmptyEntries);

					result += STRUCT;

					for(int i = 0; i < subProps.Length; i++)
					{
						var v = subProps[i].Split(VALUE, StringSplitOptions.RemoveEmptyEntries);
						if(v.Length < 2)
							continue;

						if(j == ListValues.SelectedIndex)
						{
							if(remove)
							{
								if(result.Length >= 2 && result[^2..^0] == STRUCT)
									result = result[0..^2];
								continue;
							}

							if(i == ListSubProperties.SelectedIndex)
								v[1] = input;
						}

						result += $"{STRUCT_PROPERTY}{v[0]}{VALUE}{v[1]}";
					}
				}
			}
			else
			{
				var values = data[objName][prop].Split(VALUE, StringSplitOptions.RemoveEmptyEntries);
				for(int i = 0; i < values.Length; i++)
				{
					if(i == ListValues.SelectedIndex)
						values[i] = input;

					result += $"{VALUE}{values[i]}";
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
				.Replace(SPACE, " ")
				.Replace(TAB, "\t")
				.Replace(NEW_LINE, Environment.NewLine);
		}
		private static string EncryptText(string text)
		{
			return text
				.Replace(" ", SPACE)
				.Replace("\t", TAB)
				.Replace(Environment.NewLine, NEW_LINE);
		}
		#endregion
		#region Global
		private void Global_MouseUp(object sender, MouseEventArgs e)
		{
			if(e.Button != MouseButtons.Right)
				return;

			Menu.Show(MousePosition);
			MenuEdit.Enabled = false;
			MenuCreate.Enabled = false;
			MenuRemove.Enabled = false;
		}
		private void Global_KeyDown(object sender, KeyEventArgs e)
		{
			UpdateEditMenu();
			Focus();
			TryHotkeys(e.Control, e.KeyCode);
		}
		#endregion
		#region List
		private void List_MouseEnter(object sender, EventArgs e)
		{
			ListObjects.BackColor = Color.Black;
			ListProperties.BackColor = Color.Black;
			ListSubProperties.BackColor = Color.Black;
			ListValues.BackColor = Color.Black;

			hoveredList = (ListBox)sender;
			hoveredList.BackColor = Color.FromArgb(255, 50, 50, 50);

			UpdateEditMenu();
		}
		private void List_MouseDown(object sender, MouseEventArgs e)
		{
			ActiveControl = null;
		}
		private void List_MouseUp(object sender, MouseEventArgs e)
		{
			Focus();
			UpdateEditMenu();

			if(e.Button != MouseButtons.Right)
				return;

			var listbox = (ListBox)sender;

			var rightClickedIndex = listbox.IndexFromPoint(e.X, e.Y);
			if(rightClickedIndex != -1)
				listbox.SelectedIndex = rightClickedIndex;

			Menu.Show(MousePosition);
		}

		private void ListObjects_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateAllLists();
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

			if(values.Contains(STRUCT))
			{
				var structs = values.Split(STRUCT, StringSplitOptions.RemoveEmptyEntries);
				if(structs.Length == 0)
					return;
				var structProp = structs[0].Split(STRUCT_PROPERTY, StringSplitOptions.RemoveEmptyEntries);

				for(int j = 0; j < structProp.Length; j++)
				{
					var structValues = structProp[j]
						.Split(VALUE, StringSplitOptions.RemoveEmptyEntries);

					ListSubProperties.Items.Add(structValues[0]);
					ListSubProperties.SelectedIndex = 0;
				}

				return;
			}

			var valuesSplit = values.Split(VALUE, StringSplitOptions.RemoveEmptyEntries);
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
			if(instance == null || prop == null || ListSubProperties.SelectedIndex == -1)
				return;

			var structs = data[instance][prop].Split(STRUCT, StringSplitOptions.RemoveEmptyEntries);
			var propIndex = ListSubProperties.SelectedIndex;

			ListValues.Items.Clear();

			if(structs.Length > 1)
			{
				for(int i = 0; i < structs.Length; i++)
				{
					var props = structs[i].Split(STRUCT_PROPERTY, StringSplitOptions.RemoveEmptyEntries);
					var values = props[propIndex].Split(VALUE, StringSplitOptions.RemoveEmptyEntries);
					if(values.Length < 2)
						continue;

					ListValues.Items.Add($"[{i}] {DecryptText(values[1])}");
				}

				ListValues.SelectedIndex = 0;
				return;
			}

			var property = structs[0].Split(STRUCT_PROPERTY, StringSplitOptions.RemoveEmptyEntries);
			if(ListSubProperties.SelectedIndex == -1)
				return;

			var value = property[ListSubProperties.SelectedIndex]
				.Split(VALUE, StringSplitOptions.RemoveEmptyEntries);
			ListValues.Items.Add(DecryptText(value[1]));

			if(ListValues.SelectedIndex == -1)
				ListValues.SelectedIndex = 0;
		}
		#endregion
		#region Menu
		private void MenuCreate_Click(object sender, EventArgs e)
		{
			TryCreate();
		}
		private void MenuEdit_Click(object sender, EventArgs e)
		{
			TryEditLists(false);
		}
		private void MenuRemove_Click(object sender, EventArgs e)
		{
			TryEditLists(true);
		}
		private void MenuSave_Click(object sender, EventArgs e)
		{
			TrySave();
		}
		private void MenuLoad_Click(object sender, EventArgs e)
		{
			TryLoad();
		}
		private void MenuDuplicate_Click(object sender, EventArgs e)
		{
			TryDuplicate();
		}
		private void MenuCopyText_Click(object sender, EventArgs e)
		{
			TryCopy();
		}
		#endregion
	}
}