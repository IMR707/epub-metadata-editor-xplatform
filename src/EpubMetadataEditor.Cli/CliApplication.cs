using System.Text.Json;
using System.Xml;
using EpubMetadataEditor.Core;

namespace EpubMetadataEditor.Cli;

public static class CliApplication
{
    public static int Run(string[] args, TextWriter output, TextWriter error)
    {
        if (args is [] or ["--help"] or ["-h"])
        {
            output.WriteLine("Development commands (legacy console compatibility is not implemented yet):");
            output.WriteLine("  inspect <book.epub>");
            output.WriteLine("  set-title <book.epub> <new-copy.epub> <title>");
            output.WriteLine("  preview-name <book.epub> <template>");
            return 0;
        }
        try
        {
            switch (args)
            {
                case ["inspect", var path]:
                    output.WriteLine(JsonSerializer.Serialize(EpubBook.Open(path).Metadata,
                        new JsonSerializerOptions { WriteIndented = true }));
                    return 0;
                case ["set-title", var path, var destination, var title]:
                    EpubBook.Open(path).SaveTitleCopy(destination, title);
                    output.WriteLine(Path.GetFullPath(destination));
                    return 0;
                case ["preview-name", var path, var template]:
                    output.WriteLine(EpubBook.Open(path).PreviewFilename(template));
                    return 0;
                default:
                    error.WriteLine("Invalid arguments. Run with --help for usage.");
                    return 2;
            }
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or UnauthorizedAccessException or XmlException or ArgumentException or FormatException or NotSupportedException)
        {
            error.WriteLine(ex.Message);
            return 1;
        }
    }
}
