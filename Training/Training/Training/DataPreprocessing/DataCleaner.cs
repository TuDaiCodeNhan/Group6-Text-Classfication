using System.Text.RegularExpressions;

namespace ToxicCommentClassifier.DataPreprocessing;

public static class DataCleaner
{
    private static readonly Regex MultiSpaceRegex = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex UrlRegex = new(@"(https?:\/\/\S+|www\.\S+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex MentionRegex = new(@"@\w+", RegexOptions.Compiled);
    private static readonly Regex RepeatedCharRegex = new(@"([^\W\d_])\1{2,}", RegexOptions.Compiled);
    private static readonly Regex SpecialCharRegex = new(@"[^\p{L}\p{N}\s\.\,\!\?\-_/]", RegexOptions.Compiled);

    public static string CleanText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var cleaned = text.Trim().ToLowerInvariant();
        cleaned = UrlRegex.Replace(cleaned, " ");
        cleaned = MentionRegex.Replace(cleaned, " ");
        cleaned = RepeatedCharRegex.Replace(cleaned, "$1$1");
        cleaned = SpecialCharRegex.Replace(cleaned, " ");
        cleaned = MultiSpaceRegex.Replace(cleaned, " ").Trim();
        return cleaned;
    }
}
