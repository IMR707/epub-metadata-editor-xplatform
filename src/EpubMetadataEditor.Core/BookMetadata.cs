namespace EpubMetadataEditor.Core;

/// <summary>A read-only projection; the complete package XML remains in EpubBook.</summary>
public sealed record BookMetadata(string Title, IReadOnlyList<string> Creators, string Language,
    string Version, string PackagePath);

public sealed record BookCover(string MediaType, byte[] Data);
