using GamiAutoClicker;
using Microsoft.UI.Reactor;

AppState.Initialize();
ReactorApp.Run<MainView>(new WindowSpec
{
    Key = new WindowKey("main"),
    Title = "Gami's AutoClicker",
    Width = 370,
    Height = 370,
    MinWidth = 370,
    MaxWidth = 370,
    MinHeight = 370,
    MaxHeight = 370,
    ResizeMode = WindowResizeMode.NoResize,
    IsMaximizable = false,
    IsMinimizable = false
});
