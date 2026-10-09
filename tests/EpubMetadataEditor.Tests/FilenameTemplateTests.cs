using EpubMetadataEditor.Core;

namespace EpubMetadataEditor.Tests;

public sealed class FilenameTemplateTests
{
    // Examples traced from legacy Form4.UpdateFilename; not yet runtime-compared on Windows.
    [Theory]
    [InlineData("%Creator% - %Title%.epub", "Zoë Chen - The Sample Book.epub")]
    [InlineData("%CREATOR% - %TITLE%.epub", "ZOË CHEN - THE SAMPLE BOOK.epub")]
    [InlineData("100%% %Title%", "100% The Sample Book")]
    [InlineData("%Language% - %Creator2%", "en - 陈 Example")]
    public void ExpandsDocumentedLegacyTokens(string template, string expected)
        => Assert.Equal(expected, EpubBook.Open(TestFiles.Fixture(), TestContext.Current.CancellationToken).PreviewFilename(template));

    [Theory]
    [InlineData("%Missing%")]
    [InlineData("%title%")]
    [InlineData("%Title")]
    public void ReportsUnsupportedOrMalformedTokens(string template)
        => Assert.Throws<FormatException>(() => EpubBook.Open(TestFiles.Fixture(), TestContext.Current.CancellationToken).PreviewFilename(template));

    [Theory]
    [InlineData("a/b:c?d", "a-b-c-d")]
    [InlineData("CON", "_CON")]
    [InlineData("lpt1.epub", "_lpt1.epub")]
    [InlineData(" title. ", "title")]
    public void ProducesPortableFlatFilename(string title, string expected)
        => Assert.Equal(expected, EpubBook.Open(TestFiles.Fixture(), TestContext.Current.CancellationToken).PreviewFilename("%Title%", title));
}
