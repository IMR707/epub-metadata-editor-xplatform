using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EpubMetadataEditor.Desktop.Views;

namespace EpubMetadataEditor.Desktop;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (Program.Arguments.Contains("--component-probe"))
                desktop.MainWindow = new ComponentProbeWindow(Program.Arguments.Contains("--auto-check"));
            else
            {
                var main = new MainWindow();
                desktop.MainWindow = main;
                if (Program.Arguments is [var path] && !path.StartsWith('-'))
                    main.Opened += async (_, _) => await main.OpenBookAsync(path);
            }
        }
        base.OnFrameworkInitializationCompleted();
    }
}
