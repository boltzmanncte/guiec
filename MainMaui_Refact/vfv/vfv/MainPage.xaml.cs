using vfv.ViewModels;

namespace vfv
{
	public partial class MainPage : ContentPage
	{
		private readonly MainViewModel _viewModel;

		public MainPage(MainViewModel viewModel)
		{
			InitializeComponent();
			_viewModel = viewModel;
			BindingContext = _viewModel;
		}

		private async void OnToolsClicked(object sender, EventArgs e)
		{
			var action = await DisplayActionSheet("Tools", "Cancel", null, "Job Editor", "Graph", "Open Log");

			switch (action)
			{
				case "Job Editor":
					await _viewModel.OpenJobEditorCommand.ExecuteAsync(null);
					break;
				case "Graph":
					await _viewModel.OpenGraphCommand.ExecuteAsync(null);
					break;
				case "Open Log":
					await _viewModel.OpenLogCommand.ExecuteAsync(null);
					break;
			}
		}

		private async void OnHelpClicked(object sender, EventArgs e)
		{
#if WINDOWS
			var action = await DisplayActionSheet("Help", "Cancel", null, "User Manual", "Release Notes", "Report issue via code.europa.eu", "About Vecto");
#endif
			switch (action)
			{
				case "User Manual":
					await _viewModel.OpenUserManualCommand.ExecuteAsync(null);
					break;
				case "Release Notes":
					await _viewModel.OpenReleaseNotesCommand.ExecuteAsync(null);
					break;
				case "Report issue via code.europa.eu":
					await _viewModel.ReportIssueCommand.ExecuteAsync(null);
					break;
				case "About Vecto":
					await _viewModel.ShowAboutCommand.ExecuteAsync(null);
					break;
			}
		}
	}
}
