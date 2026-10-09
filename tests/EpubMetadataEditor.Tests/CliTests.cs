using System.Text.Json;
using EpubMetadataEditor.Cli;
using EpubMetadataEditor.Core;

namespace EpubMetadataEditor.Tests;

public sealed class CliTests
{
    [Fact]
    public void InspectProducesMachineReadableMetadata()
    {
        using var output = new StringWriter(); using var error = new StringWriter();
        Assert.Equal(0, CliApplication.Run(["inspect", TestFiles.Fixture()], output, error));
        using var json = JsonDocument.Parse(output.ToString());
        Assert.Equal("The Sample Book", json.RootElement.GetProperty("Title").GetString());
        Assert.Equal("", error.ToString());
    }
    [Fact]
    public void SetTitleWritesCopyAndInvalidInputReturnsNonzero()
    {
        using var temp = new TestFiles();
        using var output = new StringWriter(); using var error = new StringWriter();
        Assert.Equal(0, CliApplication.Run(["set-title", TestFiles.Fixture(), temp.NewPath(), "CLI title"], output, error));
        Assert.Equal("CLI title", EpubBook.Open(temp.NewPath(), TestContext.Current.CancellationToken).Metadata.Title);
        Assert.Equal(1, CliApplication.Run(["inspect", temp.NewPath("missing.epub")], output, error));
        Assert.Equal(2, CliApplication.Run(["unknown"], output, error));
    }
    [Fact]
    public void CorruptArchiveReportsErrorInsteadOfCrashing()
    {
        using var temp = new TestFiles();
        File.WriteAllText(temp.NewPath(), "not a zip");
        using var output = new StringWriter(); using var error = new StringWriter();
        Assert.Equal(1, CliApplication.Run(["inspect", temp.NewPath()], output, error));
        Assert.NotEmpty(error.ToString());
    }
}
