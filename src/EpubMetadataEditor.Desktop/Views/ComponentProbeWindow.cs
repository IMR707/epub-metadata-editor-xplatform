using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using AvaloniaEdit;

namespace EpubMetadataEditor.Desktop.Views;

/// <summary>Isolates native component feasibility from the book editing workflow.</summary>
public sealed class ComponentProbeWindow : Window
{
    private readonly TextBlock status = new() { Text = "Loading the local HTML sample…", TextWrapping = Avalonia.Media.TextWrapping.Wrap };
    private readonly NativeWebView webView = new();
    private readonly bool autoCheck;
    private readonly DispatcherTimer timeout = new() { Interval = TimeSpan.FromSeconds(25) };
    private readonly Bitmap bitmap;
    private bool finished;

    public ComponentProbeWindow(bool autoCheck = false)
    {
        this.autoCheck = autoCheck;
        Title = "XPlatform component checks";
        Width = 1000; Height = 680;
        var assets = Path.Combine(AppContext.BaseDirectory, "Assets", "Preview");
        var editor = new TextEditor { ShowLineNumbers = true, Text = "<metadata>\n  <title>Edit this sample text</title>\n</metadata>".Replace("\n", Environment.NewLine) };
        bitmap = new Bitmap(Path.Combine(assets, "cover.png"));
        var image = new Image { Source = bitmap, Width = 90, Height = 120 };
        var open = new Button { Content = "Test file dialog…" };
        open.Click += async (_, _) =>
        {
            var files = await StorageProvider.OpenFilePickerAsync(new() { AllowMultiple = false, Title = "File dialog check" });
            status.Text = files.Count == 0 ? "Dialog opened and was cancelled." : $"Selected: {files[0].Name}";
        };
        var grid = new Grid { RowDefinitions = new RowDefinitions("Auto,*,Auto"), ColumnDefinitions = new ColumnDefinitions("*,*"), Margin = new Thickness(20), ColumnSpacing = 20, RowSpacing = 16 };
        var intro = new StackPanel { Spacing = 8, Children =
        {
            new TextBlock { Text = "Component feasibility checks", FontSize = 22, FontWeight = Avalonia.Media.FontWeight.SemiBold },
            new TextBlock { Text = "Editable sample XML · raster image · local HTML with relative CSS and image resources" }
        }};
        Grid.SetColumnSpan(intro, 2); grid.Children.Add(intro);
        Grid.SetRow(editor, 1); grid.Children.Add(editor);
        Grid.SetRow(webView, 1); Grid.SetColumn(webView, 1); grid.Children.Add(webView);
        var footer = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 20, Children = { image, open, status } };
        Grid.SetRow(footer, 2); Grid.SetColumnSpan(footer, 2); grid.Children.Add(footer);
        Content = grid;
        webView.NavigationCompleted += async (_, args) =>
        {
            if (finished) return;
            if (!args.IsSuccess) { Finish(false, "Local HTML navigation failed."); return; }
            try
            {
                var result = await webView.InvokeScript("JSON.stringify({image: document.querySelector('img').naturalWidth > 0, css: getComputedStyle(document.body).backgroundColor === 'rgb(238, 246, 248)', text: document.querySelector('h1').textContent === 'Preview ready'})");
                var decoded = result?.Replace("\\\"", "\"");
                var passed = decoded is not null && decoded.Contains("\"image\":true") && decoded.Contains("\"css\":true") && decoded.Contains("\"text\":true");
                Finish(passed, passed ? "Local HTML, CSS, and image loaded successfully." : $"Preview resource check failed: {result}");
            }
            catch (Exception ex) { Finish(false, "Browser scripting check failed: " + ex.Message); }
        };
        Opened += (_, _) =>
        {
            timeout.Start();
            webView.Source = new Uri(Path.Combine(assets, "index.html"));
        };
        timeout.Tick += (_, _) => Finish(false, "Browser did not finish loading within 25 seconds; check native browser dependencies.");
        Closed += (_, _) => { timeout.Stop(); bitmap.Dispose(); };
    }

    private void Finish(bool passed, string message)
    {
        finished = true;
        timeout.Stop();
        status.Text = message;
        Console.WriteLine(message);
        if (autoCheck && Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.Shutdown(passed ? 0 : 1);
    }
}
