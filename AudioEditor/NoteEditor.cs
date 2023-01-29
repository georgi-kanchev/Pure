using Pure.Audio;

namespace AudioEditor
{
	public partial class Window : Form
	{
		private enum Hear { Note, All }

		public Window()
		{
			InitializeComponent();
			CenterToScreen();

			Note.SelectedIndex = 0;
			Octave.SelectedIndex = 4;
			Wave.SelectedIndex = 1;
		}

		private string GetValue()
		{
			var value = Note.Text + (IsSharp.Checked ? "#" : "") + Octave.Text;

			if(Repeat.Value > 1)
				value += "~" + Repeat.Value.ToString();

			return value;
		}
		private string GetPause()
		{
			var value = ".";

			if(Repeat.Value > 1)
				value += Repeat.Value.ToString();

			return value;
		}

		private void OnPlayNote(object sender, EventArgs e)
		{
			Notes<Hear>.Generate(Hear.Note, GetValue(), (int)Tempo.Value, (Wave)Wave.SelectedIndex);

			if(Notes<Hear>.HasID(Hear.Note))
				Notes<Hear>.Play(Hear.Note, 0.3f, false);
		}
		private void OnPlayFromCursor(object sender, EventArgs e)
		{
			var cursorIndex = Notes.SelectionStart;
			if(cursorIndex == Notes.Text.Length)
				cursorIndex = 0;

			var notes = Notes.Text[cursorIndex..^0];
			Notes<Hear>.Generate(Hear.All, notes, (int)Tempo.Value, (Wave)Wave.SelectedIndex);

			if(Notes<Hear>.HasID(Hear.All))
				Notes<Hear>.Play(Hear.All, 0.3f, false);
		}
		private void OnStop(object sender, EventArgs e)
		{
			Notes<Hear>.Stop();
		}

		private void OnAddNote(object sender, EventArgs e)
		{
			Notes.Text += GetValue() + " ";
		}
		private void OnAddPause(object sender, EventArgs e)
		{
			Notes.Text += GetPause() + " ";
		}

		private void OnSave(object sender, EventArgs e)
		{
			if(SaveNotes.ShowDialog() != DialogResult.OK)
				return;

			File.WriteAllText(SaveNotes.FileName, Notes.Text);
		}
		private void OnLoad(object sender, EventArgs e)
		{
			if(LoadNotes.ShowDialog() != DialogResult.OK)
				return;

			Notes.Text = File.ReadAllText(LoadNotes.FileName);
		}
	}
}