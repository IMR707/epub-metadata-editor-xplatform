using System.Globalization;
using System.Text;

namespace EpubMetadataEditor.Core;

/// <summary>
/// Initial port of Form4.UpdateFilename token expansion, independent of WinForms.
/// Only a flat filename preview is supported; folder templates are a later milestone.
/// </summary>
public static class FilenameTemplate
{
    private static readonly string[] Supported = ["Title", "Creator", "Creator2", "Language"];
    private const string Invalid = "/\\:*?<>|\"";

    public static string Expand(string template, IReadOnlyDictionary<string, string> values)
    {
        ArgumentNullException.ThrowIfNull(template);
        var result = new StringBuilder();
        for (var i = 0; i < template.Length; i++)
        {
            if (template[i] != '%')
            {
                result.Append(template[i]);
                continue;
            }
            if (i + 1 < template.Length && template[i + 1] == '%')
            {
                result.Append('%');
                i++;
                continue;
            }
            var end = template.IndexOf('%', i + 1);
            if (end < 0)
                throw new FormatException("Close each template field with %, or use %% for a literal percent sign.");
            var token = template[(i + 1)..end];
            var key = Supported.FirstOrDefault(s => token == s || token == s.ToUpperInvariant())
                ?? throw new FormatException($"Unsupported prototype field: %{token}%.");
            var value = values.GetValueOrDefault(key, "");
            result.Append(token == key.ToUpperInvariant() ? value.ToUpper(CultureInfo.InvariantCulture) : value);
            i = end;
        }
        var filename = new string(result.ToString().Select(c => Invalid.Contains(c) || char.IsControl(c) ? '-' : c).ToArray())
            .Trim().TrimEnd('.');
        if (string.IsNullOrWhiteSpace(filename) || filename is "." or "..")
            throw new FormatException("The template produces an empty filename.");
        var stem = filename.Split('.')[0].ToUpperInvariant();
        if (stem is "CON" or "PRN" or "AUX" or "NUL" ||
            (stem.Length == 4 && (stem.StartsWith("COM") || stem.StartsWith("LPT")) && stem[3] is >= '1' and <= '9'))
            filename = "_" + filename;
        return filename;
    }
}
