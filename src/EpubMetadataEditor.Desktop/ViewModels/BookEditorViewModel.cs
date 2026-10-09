using System.Xml;
using CommunityToolkit.Mvvm.ComponentModel;
using EpubMetadataEditor.Core;

namespace EpubMetadataEditor.Desktop.ViewModels;

public partial class BookEditorViewModel : ObservableObject
{
    private EpubBook? book;
    [ObservableProperty] private string filePath = "No book open";
    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsDirty))] private string bookTitle = "";
    [ObservableProperty] private string metadataSummary = "Open an EPUB to read its metadata.";
    [ObservableProperty] private string status = "Development prototype: title editing and save-copy only.";
    [ObservableProperty] private string filenameTemplate = "%Creator% - %Title%.epub";
    [ObservableProperty] private string filenamePreview = "";
    [ObservableProperty, NotifyPropertyChangedFor(nameof(CanSave))] private bool isBusy;
    public bool HasBook => book is not null;
    public bool CanSave => HasBook && !IsBusy && !string.IsNullOrWhiteSpace(BookTitle);
    public bool IsDirty => book is not null && BookTitle != book.Metadata.Title;
    public BookCover? Cover { get; private set; }

    partial void OnBookTitleChanged(string value)
    {
        OnPropertyChanged(nameof(CanSave));
        UpdateFilenamePreview();
    }
    partial void OnFilenameTemplateChanged(string value) => UpdateFilenamePreview();

    public async Task<bool> OpenAsync(string path)
    {
        if (IsBusy) return false;
        IsBusy = true;
        try
        {
            var loaded = await Task.Run(() => EpubBook.Open(path));
            BookCover? cover = null;
            string? coverWarning = null;
            try { cover = loaded.GetCover(); }
            catch (InvalidDataException ex) { coverWarning = ex.Message; }
            book = loaded;
            Cover = cover;
            FilePath = loaded.SourcePath;
            BookTitle = loaded.Metadata.Title;
            MetadataSummary = $"{string.Join("; ", loaded.Metadata.Creators)}\n{loaded.Metadata.Language} · EPUB {loaded.Metadata.Version}"
                .Replace("\n", Environment.NewLine);
            OnPropertyChanged(nameof(HasBook));
            OnPropertyChanged(nameof(IsDirty));
            OnPropertyChanged(nameof(Cover));
            UpdateFilenamePreview();
            Status = coverWarning ?? "Book opened. Save a copy to keep your changes.";
            return true;
        }
        catch (Exception ex) when (IsBookError(ex)) { Status = ex.Message; return false; }
        finally { IsBusy = false; }
    }

    public async Task<bool> SaveCopyAsync(string path)
    {
        if (!CanSave || book is null) return false;
        IsBusy = true;
        var current = book;
        var title = BookTitle;
        try
        {
            await Task.Run(() => current.SaveTitleCopy(path, title));
            Status = $"Saved copy: {path}. The original book remains open.";
            return true;
        }
        catch (Exception ex) when (IsBookError(ex)) { Status = ex.Message; return false; }
        finally { IsBusy = false; }
    }

    private void UpdateFilenamePreview()
    {
        if (book is null) { FilenamePreview = ""; return; }
        try { FilenamePreview = book.PreviewFilename(FilenameTemplate, BookTitle); }
        catch (FormatException ex) { FilenamePreview = ex.Message; }
    }

    private static bool IsBookError(Exception ex) => ex is IOException or InvalidDataException
        or UnauthorizedAccessException or XmlException or ArgumentException or NotSupportedException;
}
