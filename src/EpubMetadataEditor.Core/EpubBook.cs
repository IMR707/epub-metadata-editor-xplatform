using System.IO.Compression;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace EpubMetadataEditor.Core;

/// <summary>
/// Milestone 0: immutable archive snapshot and a deliberately narrow title-edit operation.
/// No resources are extracted and the source is never overwritten.
/// </summary>
public sealed class EpubBook
{
    private static readonly XNamespace ContainerNs = "urn:oasis:names:tc:opendocument:xmlns:container";
    private static readonly XNamespace Opf = "http://www.idpf.org/2007/opf";
    private static readonly XNamespace Dc = "http://purl.org/dc/elements/1.1/";
    private static readonly byte[] MimeType = Encoding.ASCII.GetBytes("application/epub+zip");
    private const long MaxEntryBytes = 32 * 1024 * 1024;
    private const long MaxBookBytes = 128 * 1024 * 1024;
    private readonly List<Entry> entries;
    private readonly XDocument package;
    private readonly string packagePath;
    private sealed record Entry(string Name, byte[] Bytes, DateTimeOffset Modified, int Attributes);

    public string SourcePath { get; }
    public BookMetadata Metadata { get; }
    public string PackageXml => Encoding.UTF8.GetString(Serialize(package));

    private EpubBook(string path, List<Entry> entries, XDocument package, string packagePath)
    {
        SourcePath = path;
        this.entries = entries;
        this.package = package;
        this.packagePath = packagePath;
        var metadata = package.Root!.Element(Opf + "metadata")
            ?? throw new InvalidDataException("The package has no metadata element.");
        Metadata = new BookMetadata(
            metadata.Element(Dc + "title")?.Value ?? throw new InvalidDataException("The book has no title."),
            Array.AsReadOnly(metadata.Elements(Dc + "creator").Select(e => e.Value).ToArray()),
            metadata.Element(Dc + "language")?.Value ?? "",
            package.Root.Attribute("version")?.Value ?? "", packagePath);
        if (Metadata.Version is not ("2.0" or "3.0"))
            throw new InvalidDataException("Only EPUB 2.0 and 3.0 packages are supported by this prototype.");
    }

    public static EpubBook Open(string path, CancellationToken cancellationToken = default)
    {
        var entries = new List<Entry>();
        var names = new HashSet<string>(StringComparer.Ordinal);
        long total = 0;
        using var file = File.OpenRead(path);
        using var archive = new ZipArchive(file, ZipArchiveMode.Read);
        if (archive.Entries.Count > 4096)
            throw new InvalidDataException("This prototype supports at most 4096 archive entries.");
        foreach (var entry in archive.Entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ArchivePath.Validate(entry.FullName);
            if (!names.Add(entry.FullName))
                throw new InvalidDataException($"Duplicate archive entry: {entry.FullName}");
            total += entry.Length;
            if (entry.Length > MaxEntryBytes || total > MaxBookBytes)
                throw new InvalidDataException("This prototype supports 32 MiB per entry and 128 MiB per book (uncompressed).");
            using var input = entry.Open();
            using var output = new MemoryStream();
            var buffer = new byte[81920];
            int count;
            while ((count = input.Read(buffer)) != 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (output.Length + count > MaxEntryBytes || output.Length + count > entry.Length)
                    throw new InvalidDataException("Archive entry exceeds its declared or supported size.");
                output.Write(buffer, 0, count);
            }
            entries.Add(new Entry(entry.FullName, output.ToArray(), entry.LastWriteTime, entry.ExternalAttributes));
        }
        byte[] Get(string name) => entries.FirstOrDefault(e => e.Name == name)?.Bytes
            ?? throw new InvalidDataException($"Missing archive entry: {name}");
        if (!Get("mimetype").AsSpan().SequenceEqual(MimeType))
            throw new InvalidDataException("Invalid EPUB mimetype.");
        var container = Parse(Get("META-INF/container.xml"));
        if (container.Root?.Name != ContainerNs + "container")
            throw new InvalidDataException("Invalid EPUB container document.");
        var roots = container.Root.Element(ContainerNs + "rootfiles")?.Elements(ContainerNs + "rootfile")
            .Where(e => (string?)e.Attribute("media-type") == "application/oebps-package+xml").ToArray() ?? [];
        if (roots.Length != 1)
            throw new InvalidDataException("This prototype requires exactly one EPUB package rootfile.");
        var packagePath = Uri.UnescapeDataString((string?)roots[0].Attribute("full-path")
            ?? throw new InvalidDataException("The package path is missing."));
        ArchivePath.Validate(packagePath);
        var package = Parse(Get(packagePath));
        if (package.Root?.Name != Opf + "package")
            throw new InvalidDataException("Invalid EPUB package document.");
        return new EpubBook(Path.GetFullPath(path), entries, package, packagePath);
    }

    public BookCover? GetCover()
    {
        var items = package.Root!.Element(Opf + "manifest")?.Elements(Opf + "item").ToArray() ?? [];
        var coverId = package.Root.Element(Opf + "metadata")?.Elements(Opf + "meta")
            .FirstOrDefault(e => (string?)e.Attribute("name") == "cover")?.Attribute("content")?.Value;
        var cover = items.FirstOrDefault(e => ((string?)e.Attribute("properties") ?? "")
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Contains("cover-image"))
            ?? items.FirstOrDefault(e => coverId is not null && (string?)e.Attribute("id") == coverId);
        if (cover?.Attribute("href") is not { } href)
            return null;
        var name = ArchivePath.Resolve(packagePath, href.Value);
        var entry = entries.FirstOrDefault(e => e.Name == name)
            ?? throw new InvalidDataException("The cover resource is missing.");
        return new BookCover((string?)cover.Attribute("media-type") ?? "", entry.Bytes.ToArray());
    }

    public string PreviewFilename(string template, string? title = null)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Title"] = title ?? Metadata.Title,
            ["Creator"] = Metadata.Creators.FirstOrDefault() ?? "",
            ["Creator2"] = Metadata.Creators.Skip(1).FirstOrDefault() ?? "",
            ["Language"] = Metadata.Language
        };
        return FilenameTemplate.Expand(template, values);
    }

    public void SaveTitleCopy(string destination, string title, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        var outputPath = Path.GetFullPath(destination);
        if (File.Exists(outputPath) || Directory.Exists(outputPath))
            throw new IOException("Choose a new filename. Existing files are never overwritten by this prototype.");
        var edited = new XDocument(package);
        edited.Root!.Element(Opf + "metadata")!.Element(Dc + "title")!.Value = title;
        var packageBytes = Serialize(edited);
        var directory = Path.GetDirectoryName(outputPath)!;
        var temporary = Path.Combine(directory, $".epub-editor-{Guid.NewGuid():N}.tmp");
        var created = false;
        try
        {
            using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
            {
                created = true;
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
                {
                    foreach (var original in entries.OrderBy(e => e.Name == "mimetype" ? 0 : 1))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var entry = archive.CreateEntry(original.Name,
                            original.Name == "mimetype" ? CompressionLevel.NoCompression : CompressionLevel.Optimal);
                        entry.LastWriteTime = original.Modified;
                        entry.ExternalAttributes = original.Attributes;
                        using var target = entry.Open();
                        target.Write(original.Name == packagePath ? packageBytes : original.Bytes);
                    }
                }
                stream.Flush(flushToDisk: true);
            }
            var verified = Open(temporary, cancellationToken);
            if (verified.Metadata.Title != title)
                throw new InvalidDataException("The saved title failed verification.");
            cancellationToken.ThrowIfCancellationRequested();
            File.Move(temporary, outputPath, overwrite: false);
        }
        finally
        {
            if (created && File.Exists(temporary))
                File.Delete(temporary);
        }
    }

    private static XDocument Parse(byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var reader = XmlReader.Create(stream, new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = 8 * 1024 * 1024
        });
        return XDocument.Load(reader, LoadOptions.PreserveWhitespace);
    }

    private static byte[] Serialize(XDocument document)
    {
        using var stream = new MemoryStream();
        using (var writer = XmlWriter.Create(stream, new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(false), Indent = false, NewLineHandling = NewLineHandling.None,
            OmitXmlDeclaration = document.Declaration is null
        }))
            document.Save(writer);
        return stream.ToArray();
    }
}
