using Microsoft.UI;
using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Xaml.Media;
using Windows.System;
using static Microsoft.UI.Reactor.Factories;

namespace GamiAutoClicker;

internal sealed class MainView : Component
{
    private static readonly VirtualKey[] MouseButtons =
    [
        VirtualKey.LeftButton, VirtualKey.RightButton, VirtualKey.MiddleButton,
        VirtualKey.XButton1, VirtualKey.XButton2
    ];

    public override Element Render()
    {
        ClickEngine engine = AppState.Engine;
        ReactorWindow window = UseWindow()!;
        var (delay, setDelay) = UseState(engine.IntervalMilliseconds);
        var (offset, setOffset) = UseState(engine.RandomOffsetMilliseconds);
        var (buttonIndex, setButtonIndex) = UseState(Math.Max(0, Array.IndexOf(MouseButtons, engine.MouseButton)));
        var (holdMode, setHoldMode) = UseState(engine.HoldMode);
        var (enabled, setEnabled) = UseState(engine.Enabled);
        var (failure, setFailure) = UseState<string?>(null);
        var (_, setLanguageVersion) = UseState(0);

        UseEffect(() =>
        {
            AppState.Appearance.Attach(window);
            void ToggleRequested()
            {
                engine.Enabled = !engine.Enabled;
                setEnabled(engine.Enabled);
                setFailure(null);
            }
            void ClickingFailed(string message)
            {
                setEnabled(false);
                setFailure(message);
            }
            void GeneralChanged()
            {
                engine.Enabled = false;
                engine.ToggleKey = GeneralSettings.ToggleKey;
                engine.HoldKey = GeneralSettings.HoldKey;
                setEnabled(false);
            }
            int languageVersion = 0;
            void LanguageChanged() => setLanguageVersion(++languageVersion);

            engine.ToggleRequested += ToggleRequested;
            engine.ClickingFailed += ClickingFailed;
            GeneralSettings.Changed += GeneralChanged;
            Localization.Changed += LanguageChanged;
            CancellationTokenSource cancellation = new();
            Task task = engine.RunAsync(cancellation.Token);
            if (Environment.GetEnvironmentVariable("GAMI_PERF_SCENARIO") == "settings")
                AppState.OpenSettings();
            if (Environment.GetEnvironmentVariable("GAMI_PERF_SCENARIO") == "main" &&
                Environment.GetEnvironmentVariable("GAMI_PERF_READY_FILE") is { Length: > 0 } readyFile)
                File.WriteAllText(readyFile, "main");
            return () =>
            {
                engine.ToggleRequested -= ToggleRequested;
                engine.ClickingFailed -= ClickingFailed;
                GeneralSettings.Changed -= GeneralChanged;
                Localization.Changed -= LanguageChanged;
                cancellation.Cancel();
                _ = task.ContinueWith(_ => cancellation.Dispose(), TaskScheduler.Default);
                AppState.Appearance.Detach(window);
            };
        }, []);

        void ChangeDelay(double value)
        {
            if (!double.IsFinite(value) || value < ClickEngine.MinimumIntervalMilliseconds) return;
            engine.IntervalMilliseconds = value;
            setDelay(value);
        }

        string[] mouseLabels = Enumerable.Range(1, MouseButtons.Length)
            .Select(number => Localization.Format("MouseTarget", number)).ToArray();
        var titleBar = TitleBar("Gami's AutoClicker") with
        {
            RightHeader = Button(Localization.Get("Settings"), AppState.OpenSettings)
                .AutomationName(Localization.Get("Settings")).IsDragRegion(false)
        };

        return VStack(0,
            titleBar,
            VStack(8,
                Card(VStack(6,
                    Grid([GridSize.Star(), GridSize.Px(170)], [GridSize.Auto],
                        TextBlock(Localization.Get("MainWindow_Localized1.Text")).Grid(0, 0),
                        HStack(4,
                            NumberBox(delay, ChangeDelay).Range(ClickEngine.MinimumIntervalMilliseconds, 1_000_000)
                                .Width(128).IsEnabled(!enabled).AutomationName(Localization.Get("MainWindow_Localized1.Text")),
                            TextBlock("ms")).Grid(0, 1)),
                    Grid([GridSize.Star(), GridSize.Px(170)], [GridSize.Auto],
                        TextBlock(Localization.Get("MainWindow_Localized2.Text")).Grid(0, 0),
                        HStack(4,
                            NumberBox(1000d / delay, value =>
                            {
                                if (double.IsFinite(value) && value > 0) ChangeDelay(1000d / value);
                            }).Range(0.001, ClickEngine.MaximumClicksPerSecond)
                                .Width(128).IsEnabled(!enabled).AutomationName(Localization.Get("MainWindow_Localized2.Text")),
                            TextBlock("CPS")).Grid(0, 1)),
                    Grid([GridSize.Star(), GridSize.Px(170)], [GridSize.Auto],
                        TextBlock(Localization.Get("MainWindow_RandomOffset.Text")).Grid(0, 0),
                        HStack(4,
                            NumberBox(offset, value =>
                            {
                                if (!double.IsFinite(value)) return;
                                engine.RandomOffsetMilliseconds = Math.Clamp(value, 0, 1_000_000);
                                setOffset(engine.RandomOffsetMilliseconds);
                            }).Range(0, 1_000_000).Width(128).IsEnabled(!enabled)
                                .ToolTip(Localization.Get("RandomOffsetHint"))
                                .AutomationName(Localization.Get("RandomOffset")),
                            TextBlock("ms")).Grid(0, 1))
                )),
                Card(VStack(6,
                    Grid([GridSize.Star(), GridSize.Px(120)], [GridSize.Auto],
                        TextBlock(Localization.Get("MainWindow_Localized3.Text")).Grid(0, 0),
                        ComboBox(mouseLabels, buttonIndex, index =>
                        {
                            if ((uint)index >= MouseButtons.Length) return;
                            engine.MouseButton = MouseButtons[index];
                            setButtonIndex(index);
                        }).IsEnabled(!enabled).AutomationName(Localization.Get("Clicktarget")).Grid(0, 1)),
                    Grid([GridSize.Star(), GridSize.Px(120)], [GridSize.Auto],
                        TextBlock(Localization.Get("MainWindow_Localized4.Text"))
                            .ToolTip(Localization.Format("HoldHint", GeneralSettings.KeyLabel(engine.HoldKey))).Grid(0, 0),
                        ToggleSwitch(holdMode, value =>
                        {
                            engine.HoldMode = value;
                            setHoldMode(value);
                        }).IsEnabled(!enabled).AutomationName(Localization.Get("Holdmode")).Grid(0, 1))
                )),
                Card(Grid([GridSize.Star(), GridSize.Auto], [GridSize.Auto],
                    HStack(6,
                        TextBlock(Localization.Get("MainWindow_Localized5.Text")),
                        ToggleSwitch(enabled, value =>
                        {
                            engine.Enabled = value;
                            setEnabled(value);
                            setFailure(null);
                        }).ToolTip(Localization.Format("ToggleHint", GeneralSettings.KeyLabel(engine.ToggleKey)))
                            .AutomationName(Localization.Get("Enableclicking"))).Grid(0, 0),
                    HStack(6,
                        Ellipse().Width(8).Height(8).Fill(new SolidColorBrush(enabled ? Colors.Green : Colors.Red)),
                        TextBlock(failure is null ? Localization.Get(enabled ? "Enabled" : "Disabled") : Localization.Get("Blocked"))
                            .ToolTip(failure ?? string.Empty)).Grid(0, 1)))
            ).Padding(12)
        );
    }
}
