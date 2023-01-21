namespace ImageEditor
{
	public partial class Window : Form
	{
		public Window()
		{
			InitializeComponent();
			CenterToScreen();

			Tool.SelectedIndex = 0;
		}

		private void OnBrushColorClick(object sender, EventArgs e)
		{
			if(Colors.ShowDialog() != DialogResult.OK)
				return;

			ColorIndicator.BackColor = Colors.Color;
		}

		private void OnBackgroundColorClick(object sender, EventArgs e)
		{
			if(Colors.ShowDialog() != DialogResult.OK)
				return;

			BackgroundColorIndicator.BackColor = Colors.Color;
		}
	}
}