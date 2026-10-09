using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using EpubMetadataEditor.Desktop.ViewModels;

namespace EpubMetadataEditor.Desktop.Views;

public partial class MainWindow : Window
{
    private readonly BookEditorViewModel model = new();
    private Bitmap? coverBitmap;
    private bool closingApproved;
    private bool askingToClose;
    public MainWindow()
    {
        InitializeComponent();
        DataContext = model;
        Closing += async (_, e) =>
        {
            if (closingApproved) return;
            if (model.IsBusy) { e.Cancel = true; return; }
            if (!model.IsDirty) return;
            e.Cancel = true;
            if (askingToClose) return;
            askingToClose = true;
            try
            {
                if (await ConfirmDiscardAsync()) { closingApproved = true; Close(); }
            }
            finally { askingToClose = false; }
        };
        Closed += (_, _) => coverBitmap?.Dispose();
    }

    public async Task OpenBookAsync(string path)
    {
        if (model.IsBusy || (model.IsDirty && !await ConfirmDiscardAsync())) return;
        if (!await model.OpenAsync(path)) return;
        CoverImage.Source = null;
        coverBitmap?.Dispose();
        coverBitmap = null;
        CoverStatus.Text = "No supported raster cover found.";
        if (model.Cover is { MediaType: "image/png" or "image/jpeg" } cover)
        {
            try
            {
                using var stream = new MemoryStream(cover.Data);
                coverBitmap = Bitmap.DecodeToWidth(stream, 360);
                CoverImage.Source = coverBitmap;
                CoverStatus.Text = "Book cover";
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or NotSupportedException)
            { CoverStatus.Text = "This cover could not be decoded."; }
        }
    }

    private async void Open_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open EPUB", AllowMultiple = false,
                FileTypeFilter = [new FilePickerFileType("EPUB books") { Patterns = ["*.epub", "*.EPUB"] }]
            });
            if (files.FirstOrDefault()?.TryGetLocalPath() is { } path) await OpenBookAsync(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        { model.Status = ex.Message; }
    }

    private async void Save_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var target = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save a new EPUB copy", SuggestedFileName = Path.GetFileNameWithoutExtension(model.FilePath) + "-edited.epub",
                DefaultExtension = "epub", ShowOverwritePrompt = true,
                FileTypeChoices = [new FilePickerFileType("EPUB books") { Patterns = ["*.epub"] }]
            });
            if (target?.TryGetLocalPath() is { } path) await model.SaveCopyAsync(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        { model.Status = ex.Message; }
    }

    private void Components_Click(object? sender, RoutedEventArgs e) => new ComponentProbeWindow().Show(this);

    private Task<bool> ConfirmDiscardAsync()
    {
        var dialog = new Window { Title = "Unsaved title change", Width = 420, SizeToContent = SizeToContent.Height,
            CanResize = false, WindowStartupLocation = WindowStartupLocation.CenterOwner };
        var keep = new Button { Content = "Keep editing" };
        var discard = new Button { Content = "Discard change" };
        keep.Click += (_, _) => dialog.Close(false);
        discard.Click += (_, _) => dialog.Close(true);
        dialog.Content = new StackPanel { Margin = new Thickness(20), Spacing = 16, Children =
        {
            new TextBlock { Text = "Save a copy first if you want to keep the edited title.", TextWrapping = Avalonia.Media.TextWrapping.Wrap },
            new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right, Spacing = 12, Children = { keep, discard } }
        }};
        return dialog.ShowDialog<bool>(this);
    }
}
