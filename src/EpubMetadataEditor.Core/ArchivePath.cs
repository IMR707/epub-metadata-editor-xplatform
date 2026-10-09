namespace EpubMetadataEditor.Core;

internal static class ArchivePath
{
    public static void Validate(string path)
    {
        if (string.IsNullOrEmpty(path) || path.StartsWith('/') || path.Contains('\\') ||
            path.Contains(':') || path.Any(char.IsControl) ||
            path.TrimEnd('/').Split('/').Any(part => part is "" or "." or ".."))
            throw new InvalidDataException($"Unsafe archive entry name: {path}");
    }

    public static string Resolve(string packagePath, string href)
    {
        if (string.IsNullOrWhiteSpace(href) || href.Contains('\\') || href.StartsWith('/') ||
            Uri.TryCreate(href, UriKind.Absolute, out _))
            throw new InvalidDataException("Expected a relative EPUB resource URL.");
        var origin = new Uri("https://epub.invalid/");
        var packageUri = new Uri(origin, string.Join("/", packagePath.Split('/').Select(Uri.EscapeDataString)));
        var resolved = new Uri(packageUri, href);
        if (resolved.Host != origin.Host || resolved.Query.Length != 0 || resolved.Fragment.Length != 0)
            throw new InvalidDataException("Unsupported EPUB resource URL.");
        var path = Uri.UnescapeDataString(resolved.AbsolutePath.TrimStart('/'));
        Validate(path);
        return path;
    }
}
