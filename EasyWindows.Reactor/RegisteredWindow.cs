using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using static Microsoft.UI.Reactor.Factories;
namespace Gami;

// Retains the native window and typed registration state across content reloads.
public class RegisteredWindow(object key) : Component {
	public override Element Render() {
		ReactorWindow window = UseWindow()!;
		var (version, setVersion) = UseState(0);
		var (_, setTitleVersion) = UseState(0);
		var (active, setActive) = UseState(true);
		UseEffect(() => {
			EasyWindows.Attach(window, key);
			void Activated(object sender, Microsoft.UI.Xaml.WindowActivatedEventArgs args) => setActive(args.WindowActivationState != Microsoft.UI.Xaml.WindowActivationState.Deactivated);
			window.NativeWindow.Activated += Activated;
			int generation = 0;
			void Reload() => setVersion(++generation);
			int titleGeneration = 0;
			void Retitle() => setTitleVersion(++titleGeneration);
			EasyWindows.ContentInvalidated += Reload;
			EasyWindows.TitlesChanged += Retitle;
			return () => { EasyWindows.ContentInvalidated -= Reload; EasyWindows.TitlesChanged -= Retitle; window.NativeWindow.Activated -= Activated; };
		}, []);
		Element content = EasyWindows.GetWindowOptions(key).CreateContent() with { Key = version.ToString() };
		return Grid([GridSize.Star()], [GridSize.Auto, GridSize.Star()],
			WindowTitleBar.Create(window, key, active), content.Grid(1, 0));
	}
}
