using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace thirdpartynetwork.Application.Helpers;

public partial class SlugGenerator
{
    public static string GenerateSlug(string title)
    {
        if (string.IsNullOrWhiteSpace(title)) 
            return string.Empty;

        var str = title.ToLowerInvariant();

        // "café" -> "cafe"
        str = RemoveDiacritics(str);

        str = RegexNonSimple().Replace(str, "");
        str = RegexWhitespace().Replace(str, "-");

        str = str.Trim('-');
        return str;
    }

    private static string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    [GeneratedRegex(@"[^a-z0-9\s-]")]
    private static partial Regex RegexNonSimple();
    [GeneratedRegex(@"[\s-]+")]
    private static partial Regex RegexWhitespace();
}