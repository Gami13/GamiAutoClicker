using GamiAutoClicker.WindowManager;
using Microsoft.UI.Xaml;

namespace GamiAutoClicker;

public sealed partial class MainWindow : Window {

	public MainWindow() {
		InitializeComponent();
		Closed += MainWindow_Closed;
	}

	private void MainWindow_Closed(object? sender, WindowEventArgs args) {
		Application.Current.Exit();
	}
}

