using Gami;
using GamiAutoClicker;
using Microsoft.UI.Reactor;
AppState.Initialize();
ReactorApp.Run<MainWindow>(EasyWindows.GetWindowSpec(GamiAutoClicker.WindowKey.Main));
