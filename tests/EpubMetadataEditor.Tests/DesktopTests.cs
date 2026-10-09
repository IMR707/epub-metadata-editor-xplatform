using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using EpubMetadataEditor.Desktop;
using EpubMetadataEditor.Desktop.ViewModels;
using EpubMetadataEditor.Desktop.Views;

[assembly: AvaloniaTestApplication(typeof(EpubMetadataEditor.Tests.TestAppBuilder))]

namespace EpubMetadataEditor.Tests;

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}

public sealed class DesktopTests
{
    [AvaloniaFact]
    public async Task TitleBindingUpdatesModelAndSaveCopy()
    {
        using var temp = new TestFiles();
        var window = new MainWindow();
        var model = Assert.IsType<BookEditorViewModel>(window.DataContext);
        Assert.False(model.CanSave);
        await model.OpenAsync(TestFiles.Fixture());
        Assert.False(model.IsDirty);
        var title = window.FindControl<TextBox>("TitleInput")!;
        title.Text = "Edited through the view";
        Dispatcher.UIThread.RunJobs();
        Assert.Equal("Edited through the view", model.BookTitle);
        Assert.True(model.IsDirty);
        Assert.True(await model.SaveCopyAsync(temp.NewPath()));
        Assert.Equal("Edited through the view", Core.EpubBook.Open(temp.NewPath(), TestContext.Current.CancellationToken).Metadata.Title);
        Assert.False(model.IsBusy);
    }

    [AvaloniaFact]
    public async Task FailedOpenRetainsCurrentBookAndRecoversBusyState()
    {
        using var temp = new TestFiles();
        var model = new BookEditorViewModel();
        Assert.True(await model.OpenAsync(TestFiles.Fixture()));
        model.BookTitle = "Unsaved title";
        Assert.False(await model.OpenAsync(temp.NewPath("missing.epub")));
        Assert.Equal("Unsaved title", model.BookTitle);
        Assert.True(model.IsDirty);
        Assert.False(model.IsBusy);
        Assert.True(model.CanSave);
    }
}
