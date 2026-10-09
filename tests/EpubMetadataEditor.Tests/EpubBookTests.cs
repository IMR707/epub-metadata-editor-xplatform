using System.IO.Compression;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using EpubMetadataEditor.Core;

namespace EpubMetadataEditor.Tests;

public sealed class EpubBookTests
{
    [Theory]
    [InlineData("epub2-nested.epub", "2.0")]
    [InlineData("epub3-unicode.epub", "3.0")]
    public void ReadsContainerMetadataAndCover(string name, string version)
    {
        var book = EpubBook.Open(TestFiles.Fixture(name), TestContext.Current.CancellationToken);
        Assert.Equal("The Sample Book", book.Metadata.Title);
        Assert.Equal(new[] { "Zoë Chen", "陈 Example" }, book.Metadata.Creators);
        Assert.Equal(version, book.Metadata.Version);
        Assert.StartsWith("Books/EPUB", book.Metadata.PackagePath);
        var cover = Assert.IsType<BookCover>(book.GetCover());
        Assert.Equal("image/png", cover.MediaType);
        Assert.Equal(new byte[] { 137, 80, 78, 71 }, cover.Data.Take(4));
    }

    [Theory]
    [InlineData("epub2-nested.epub")]
    [InlineData("epub3-unicode.epub")]
    public void SavesTitleWithoutChangingSourceOrOtherResources(string name)
    {
        using var temp = new TestFiles();
        var source = TestFiles.Fixture(name);
        var sourceBytes = File.ReadAllBytes(source);
        var original = EpubBook.Open(source, TestContext.Current.CancellationToken);
        var destination = temp.NewPath("edited 陈.epub");
        original.SaveTitleCopy(destination, "Changed & <escaped> — 陈", TestContext.Current.CancellationToken);
        var edited = EpubBook.Open(destination, TestContext.Current.CancellationToken);
        Assert.Equal("Changed & <escaped> — 陈", edited.Metadata.Title);
        Assert.Equal(sourceBytes, File.ReadAllBytes(source));
        Assert.Equal("The Sample Book", original.Metadata.Title);
        var before = TestFiles.ReadEntries(source);
        var after = TestFiles.ReadEntries(destination);
        Assert.Equal(before.Keys.Order(), after.Keys.Order());
        foreach (var (path, bytes) in before)
            if (path != original.Metadata.PackagePath) Assert.Equal(bytes, after[path]);
        var beforeXml = XDocument.Parse(Encoding.UTF8.GetString(before[original.Metadata.PackagePath]), LoadOptions.PreserveWhitespace);
        var afterXml = XDocument.Parse(Encoding.UTF8.GetString(after[original.Metadata.PackagePath]), LoadOptions.PreserveWhitespace);
        XNamespace dc = "http://purl.org/dc/elements/1.1/";
        beforeXml.Descendants(dc + "title").First().Remove();
        afterXml.Descendants(dc + "title").First().Remove();
        Assert.True(XNode.DeepEquals(beforeXml.Root, afterXml.Root), "All XML other than the edited title must remain equivalent.");
        Assert.Single(Directory.GetFiles(temp.DirectoryPath));
    }

    [Fact]
    public void WrittenMimetypeIsFirstStoredAndHasNoExtraHeader()
    {
        using var temp = new TestFiles();
        EpubBook.Open(TestFiles.Fixture(), TestContext.Current.CancellationToken).SaveTitleCopy(temp.NewPath(), "New title", TestContext.Current.CancellationToken);
        using var reader = new BinaryReader(File.OpenRead(temp.NewPath()));
        Assert.Equal(0x04034b50u, reader.ReadUInt32());
        reader.BaseStream.Position = 8;
        Assert.Equal((ushort)0, reader.ReadUInt16());
        reader.BaseStream.Position = 26;
        var nameLength = reader.ReadUInt16();
        Assert.Equal((ushort)0, reader.ReadUInt16());
        Assert.Equal("mimetype", Encoding.ASCII.GetString(reader.ReadBytes(nameLength)));
        Assert.Equal("application/epub+zip", Encoding.ASCII.GetString(reader.ReadBytes(20)));
    }

    [Fact]
    public void RefusesToOverwriteAnyExistingDestination()
    {
        using var temp = new TestFiles();
        var book = EpubBook.Open(TestFiles.Fixture(), TestContext.Current.CancellationToken);
        var original = File.ReadAllBytes(book.SourcePath);
        File.WriteAllText(temp.NewPath(), "Keep this file");
        Assert.Throws<IOException>(() => book.SaveTitleCopy(temp.NewPath(), "New title", TestContext.Current.CancellationToken));
        Assert.Equal("Keep this file", File.ReadAllText(temp.NewPath()));
        Assert.Throws<IOException>(() => book.SaveTitleCopy(book.SourcePath, "New title", TestContext.Current.CancellationToken));
        Assert.Equal(original, File.ReadAllBytes(book.SourcePath));
    }

    [Fact]
    public void CancellationLeavesNoOutputOrTemporaryFile()
    {
        using var temp = new TestFiles();
        var book = EpubBook.Open(TestFiles.Fixture(), TestContext.Current.CancellationToken);
        Assert.Throws<OperationCanceledException>(() => book.SaveTitleCopy(temp.NewPath(), "New", new CancellationToken(true)));
        Assert.Empty(Directory.GetFiles(temp.DirectoryPath));
        Assert.Equal("The Sample Book", EpubBook.Open(book.SourcePath, TestContext.Current.CancellationToken).Metadata.Title);
    }

    [Theory]
    [InlineData("../escape.txt")]
    [InlineData("/absolute.txt")]
    [InlineData("C:/windows.txt")]
    [InlineData("folder\\escape.txt")]
    public void RejectsUnsafeArchiveNames(string name)
    {
        using var temp = new TestFiles();
        var path = temp.ChangedFixture(zip => TestFiles.Replace(zip, name, "bad"));
        Assert.Throws<InvalidDataException>(() => EpubBook.Open(path, TestContext.Current.CancellationToken));
    }

    [Fact]
    public void RejectsDuplicateEntries()
    {
        using var temp = new TestFiles();
        var path = temp.ChangedFixture(zip => zip.CreateEntry("mimetype"));
        Assert.Throws<InvalidDataException>(() => EpubBook.Open(path, TestContext.Current.CancellationToken));
    }

    [Fact]
    public void RejectsMalformedPackageAndDtd()
    {
        using var temp = new TestFiles();
        var malformed = temp.ChangedFixture(zip => TestFiles.Replace(zip, "Books/EPUB3/package.opf", "<broken>"));
        Assert.Throws<XmlException>(() => EpubBook.Open(malformed, TestContext.Current.CancellationToken));
        var dtd = temp.ChangedFixture(zip => TestFiles.Replace(zip, "META-INF/container.xml", "<!DOCTYPE container [<!ENTITY x SYSTEM 'file:///etc/passwd'>]><container>&x;</container>"));
        Assert.Throws<XmlException>(() => EpubBook.Open(dtd, TestContext.Current.CancellationToken));
    }

    [Fact]
    public void UsesContainerPathRatherThanFirstOpfInArchive()
    {
        using var temp = new TestFiles();
        var path = temp.ChangedFixture(zip => TestFiles.Replace(zip, "decoy.opf", "<not-a-package/>"));
        Assert.Equal("The Sample Book", EpubBook.Open(path, TestContext.Current.CancellationToken).Metadata.Title);
    }

    [Fact]
    public void PreservesUnreferencedFilesAndCaseSensitiveNames()
    {
        using var temp = new TestFiles();
        var path = temp.ChangedFixture(zip =>
        {
            TestFiles.Replace(zip, "extras/A.txt", "upper");
            TestFiles.Replace(zip, "extras/a.txt", "lower");
        });
        EpubBook.Open(path, TestContext.Current.CancellationToken).SaveTitleCopy(temp.NewPath(), "Title", TestContext.Current.CancellationToken);
        var data = TestFiles.ReadEntries(temp.NewPath());
        Assert.Equal("upper", Encoding.UTF8.GetString(data["extras/A.txt"]));
        Assert.Equal("lower", Encoding.UTF8.GetString(data["extras/a.txt"]));
    }

    [Fact]
    public void CoverLookupDecodesRelativeUrlsFromEncodedPackagePaths()
    {
        using var temp = new TestFiles();
        var path = temp.ChangedFixture(zip =>
        {
            using var reader = new StreamReader(zip.GetEntry("Books/EPUB3/package.opf")!.Open());
            var xml = reader.ReadToEnd().Replace("Images/cover.png", "Images/co%20ver.png");
            reader.Dispose();
            TestFiles.Replace(zip, "Books/EPUB3/package%#.opf", xml);
            using var container = new StreamReader(zip.GetEntry("META-INF/container.xml")!.Open());
            var containerXml = container.ReadToEnd().Replace("package.opf", "package%25%23.opf");
            container.Dispose();
            TestFiles.Replace(zip, "META-INF/container.xml", containerXml);
            using var source = zip.GetEntry("Books/EPUB3/Images/cover.png")!.Open();
            using var data = new MemoryStream(); source.CopyTo(data); source.Dispose();
            using var target = zip.CreateEntry("Books/EPUB3/Images/co ver.png").Open();
            target.Write(data.ToArray());
        });
        Assert.NotNull(EpubBook.Open(path, TestContext.Current.CancellationToken).GetCover());
    }

    [Fact]
    public void MissingCoverDoesNotPreventMetadataLoading()
    {
        using var temp = new TestFiles();
        var path = temp.ChangedFixture(zip =>
        {
            using var reader = new StreamReader(zip.GetEntry("Books/EPUB3/package.opf")!.Open());
            var xml = reader.ReadToEnd().Replace(" properties=\"cover-image\"", "");
            reader.Dispose();
            TestFiles.Replace(zip, "Books/EPUB3/package.opf", xml);
        });
        var book = EpubBook.Open(path, TestContext.Current.CancellationToken);
        Assert.Null(book.GetCover());
        Assert.Equal("The Sample Book", book.Metadata.Title);
    }

    [Fact]
    public void NamespacePrefixesDoNotChangeMetadataInterpretation()
    {
        using var temp = new TestFiles();
        var path = temp.ChangedFixture(zip =>
        {
            using var reader = new StreamReader(zip.GetEntry("Books/EPUB3/package.opf")!.Open());
            var xml = reader.ReadToEnd().Replace("xmlns:dc=", "xmlns:bookdata=").Replace("dc:", "bookdata:");
            reader.Dispose();
            TestFiles.Replace(zip, "Books/EPUB3/package.opf", xml);
        });
        var book = EpubBook.Open(path, TestContext.Current.CancellationToken);
        book.SaveTitleCopy(temp.NewPath(), "Changed", TestContext.Current.CancellationToken);
        Assert.Equal("Changed", EpubBook.Open(temp.NewPath(), TestContext.Current.CancellationToken).Metadata.Title);
    }

}
