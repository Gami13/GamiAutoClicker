using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace GamiAutoClicker.WindowManager;

public abstract class ThemedWindow : Window {
	private bool _initialized;

	public abstract WindowKey WindowKey { get; }
	public WindowController? Controller { get; private set; }

	public new object? Content {
		get => base.Content;
		set {
			if (_initialized || value == null) {
				base.Content = (UIElement?)value;
				return;
			}
			_initialized = true;

			var rootGrid = new Grid();
			rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

			if (value is FrameworkElement content) {
				Grid.SetRow(content, 1);
				rootGrid.Children.Add(content);
			}

			base.Content = rootGrid;
			Controller = new WindowController(this, WindowKey);
		}
	}
}
