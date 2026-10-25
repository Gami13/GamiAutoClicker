using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
namespace GamiToolkit.Components;

public abstract class ResponsiveComponent : Component {
	private Grid? _root;
	private Action<double>? _setWidth;
	protected double UseContentWidth() {
		var (width, setWidth) = UseState(0d);
		_setWidth = setWidth;
		// Reactor flushes component effects before the native subtree is mounted.
		// Attach size handlers when Set receives the actual control instead.
		UseEffect(() => () => DetachRoot(), []);
		return width;
	}
	protected void CaptureRoot(Grid grid) {
		if (ReferenceEquals(_root, grid)) return;
		DetachRoot();
		_root = grid;
		grid.SizeChanged += Resize;
		grid.Loaded += Loaded;
		if (grid.ActualWidth > 0) _setWidth?.Invoke(grid.ActualWidth);
	}
	private void Resize(object sender, SizeChangedEventArgs args) => _setWidth?.Invoke(args.NewSize.Width);
	private void Loaded(object sender, RoutedEventArgs args) => _setWidth?.Invoke(_root!.ActualWidth);
	private void DetachRoot() {
		if (_root is null) return;
		_root.SizeChanged -= Resize; _root.Loaded -= Loaded; _root = null;
	}
}
