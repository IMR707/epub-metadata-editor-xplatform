using System.IO.Compression;
using System.Text;

namespace EpubMetadataEditor.Tests;

internal sealed class TestFiles : IDisposable
{
    public string DirectoryPath { get; } = Path.Combine(Path.GetTempPath(), "epub-tests-" + Guid.NewGuid().ToString("N"));
    public TestFiles() => Directory.CreateDirectory(DirectoryPath);
    public string NewPath(string name = "output.epub") => Path.Combine(DirectoryPath, name);
    public static string Fixture(string name = "epub3-unicode.epub") => Path.Combine(AppContext.BaseDirectory, "Fixtures", name);
    public string ChangedFixture(Action<ZipArchive> mutate)
    {
        var path = NewPath(Guid.NewGuid() + ".epub");
        File.Copy(Fixture(), path);
        using var archive = ZipFile.Open(path, ZipArchiveMode.Update);
        mutate(archive);
        return path;
    }
    public static void Replace(ZipArchive zip, string name, string text)
    {
        zip.GetEntry(name)?.Delete();
        using var writer = new StreamWriter(zip.CreateEntry(name).Open(), new UTF8Encoding(false));
        writer.Write(text);
    }
    public static Dictionary<string, byte[]> ReadEntries(string path)
    {
        using var zip = ZipFile.OpenRead(path);
        return zip.Entries.ToDictionary(e => e.FullName, e =>
        {
            using var input = e.Open(); using var result = new MemoryStream();
            input.CopyTo(result); return result.ToArray();
        });
    }
    public void Dispose() => Directory.Delete(DirectoryPath, recursive: true);
}
