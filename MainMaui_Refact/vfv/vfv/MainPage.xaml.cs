using vfv.ViewModels;
#if WINDOWS
using Windows.Storage;
using Windows.ApplicationModel.DataTransfer;
using WinRT;
#endif

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

		private async void OnIconButtonTapped(object sender, EventArgs e)
		{
			if (sender is Border border)
			{
				// Animate scale down and opacity
				await Task.WhenAll(
					border.ScaleTo(0.95, 50, Easing.CubicOut),
					border.FadeTo(0.8, 50, Easing.CubicOut)
				);

				// Animate back to normal
				await Task.WhenAll(
					border.ScaleTo(1.0, 100, Easing.CubicOut),
					border.FadeTo(1.0, 100, Easing.CubicOut)
				);
			}
		}

		private async void OnButtonPressed(object sender, EventArgs e)
		{
			if (sender is Button button)
			{
				await Task.WhenAll(
					button.ScaleTo(0.95, 50, Easing.CubicOut),
					button.FadeTo(0.8, 50, Easing.CubicOut)
				);
			}
		}

		private async void OnButtonReleased(object sender, EventArgs e)
		{
			if (sender is Button button)
			{
				await Task.WhenAll(
					button.ScaleTo(1.0, 100, Easing.CubicOut),
					button.FadeTo(1.0, 100, Easing.CubicOut)
				);
			}
		}

		private void OnDragOver(object sender, DragEventArgs e)
		{
#if WINDOWS
			try
			{
				// Check if we have platform-specific drag event args
				if (e.PlatformArgs?.DragEventArgs != null)
				{
					var winArgs = e.PlatformArgs.DragEventArgs.As<Microsoft.UI.Xaml.DragEventArgs>();
					if (winArgs?.DataView != null && winArgs.DataView.Contains(StandardDataFormats.StorageItems))
					{
						winArgs.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
						e.AcceptedOperation = Microsoft.Maui.Controls.DataPackageOperation.Copy;

						// Visual feedback - change border color
						if (sender is Frame frame)
						{
							frame.BorderColor = Color.FromArgb("#003399");
						}
						return;
					}
				}
			}
			catch
			{
				// Silently fail and reject the drop
			}
#endif
			e.AcceptedOperation = Microsoft.Maui.Controls.DataPackageOperation.None;
		}

		private void OnDragLeave(object sender, DragEventArgs e)
		{
			// Reset visual feedback
			if (sender is Frame frame)
			{
				frame.BorderColor = Color.FromArgb("#E5E7EB");
			}
		}

		private async void OnDrop(object sender, DropEventArgs e)
		{
			// Reset visual feedback
			if (sender is Frame frame)
			{
				frame.BorderColor = Color.FromArgb("#E5E7EB");
			}

			try
			{
#if WINDOWS
				if (e.PlatformArgs?.DragEventArgs != null)
				{
					var winArgs = e.PlatformArgs.DragEventArgs.As<Microsoft.UI.Xaml.DragEventArgs>();
					if (winArgs?.DataView != null && winArgs.DataView.Contains(StandardDataFormats.StorageItems))
					{
						var items = await winArgs.DataView.GetStorageItemsAsync();
						var files = items.OfType<IStorageFile>();

						if (files.Any())
						{
							await _viewModel.AddFilesFromDrop(files);
						}
					}
				}
#else
				_viewModel.Errors.Add("Drag and drop is only supported on Windows");
				await Task.CompletedTask;
#endif
			}
			catch (Exception ex)
			{
				_viewModel.Errors.Add($"Error dropping files: {ex.Message}");
			}
		}
	}
}
