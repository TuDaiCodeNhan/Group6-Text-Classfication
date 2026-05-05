using System.Text;

namespace ToxicCommentClassifier.DataPreprocessing;

public class DataAugmentor
{
    private static readonly Dictionary<string, string> TeencodeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["vãi"] = "vl",
        ["vailon"] = "vl",
        ["vãi cả l"] = "vcl",
        ["vãi cả loz"] = "vcl",
        ["không"] = "ko",
        ["được"] = "dc",
        ["gì"] = "j",
        ["thôi"] = "thoi"
    };

    public int AugmentCsv(string inputPath, string outputPath, double minIncreaseRatio = 0.2)
    {
        if (!File.Exists(inputPath))
        {
            throw new FileNotFoundException("Input CSV was not found.", inputPath);
        }

        var lines = File.ReadAllLines(inputPath, Encoding.UTF8);
        if (lines.Length <= 1)
        {
            throw new InvalidOperationException("Input CSV does not contain data rows.");
        }

        var headerCols = SplitCsvLine(lines[0]);
        var (messageIndex, labelIndex) = ResolveColumnIndexes(headerCols);

        var baseRows = new List<(string Message, bool IsToxic)>();
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            var columns = SplitCsvLine(lines[i]);
            if (columns.Count <= Math.Max(messageIndex, labelIndex)) continue;

            var text = columns[messageIndex];
            if (string.IsNullOrWhiteSpace(text)) continue;

            if (!TryParseLabel(columns[labelIndex], out var isToxic)) continue;

            baseRows.Add((text, isToxic));
        }

        var targetExtra = Math.Max(1, (int)Math.Ceiling(baseRows.Count * minIncreaseRatio));
        var allRows = new List<(string Message, bool IsToxic)>(baseRows);
        var unique = new HashSet<string>(baseRows.Select(r => $"{r.Message}|{r.IsToxic}"), StringComparer.Ordinal);

        foreach (var row in baseRows)
        {
            foreach (var variant in BuildVariants(row.Message))
            {
                var cleaned = DataCleaner.CleanText(variant);
                if (string.IsNullOrWhiteSpace(cleaned)) continue;

                var key = $"{cleaned}|{row.IsToxic}";
                if (unique.Add(key))
                {
                    allRows.Add((cleaned, row.IsToxic));
                    if (allRows.Count - baseRows.Count >= targetExtra)
                    {
                        WriteCsv(outputPath, allRows);
                        return allRows.Count - baseRows.Count;
                    }
                }
            }
        }

        WriteCsv(outputPath, allRows);
        return allRows.Count - baseRows.Count;
    }

    private static IEnumerable<string> BuildVariants(string source)
    {
        yield return AddRepeatedCharacters(source);
        yield return ApplyTeencode(source);
        yield return BuildAbbreviationVariant(source);
        yield return source.ToLowerInvariant();
    }

    private static string AddRepeatedCharacters(string source)
    {
        var words = source.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].Length >= 2)
            {
                words[i] = words[i] + words[i][^1];
                break;
            }
        }
        return string.Join(' ', words);
    }

    private static string ApplyTeencode(string source)
    {
        var result = source;
        foreach (var pair in TeencodeMap)
        {
            result = result.Replace(pair.Key, pair.Value, StringComparison.OrdinalIgnoreCase);
        }
        return result;
    }

    private static string BuildAbbreviationVariant(string source)
    {
        var words = source.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length < 3) return source;

        var abbreviated = words.Select((w, i) => i % 2 == 0 && w.Length > 1 ? w[0].ToString() : w);
        return string.Join(' ', abbreviated);
    }

    private static void WriteCsv(string path, IEnumerable<(string Message, bool IsToxic)> rows)
    {
        var outputDir = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        using var writer = new StreamWriter(path, false, new UTF8Encoding(true));
        writer.WriteLine("Message,IsToxic");
        foreach (var row in rows)
        {
            var escaped = row.Message.Replace("\"", "\"\"");
            writer.WriteLine($"\"{escaped}\",{(row.IsToxic ? 1 : 0)}");
        }
    }

    private static bool TryParseLabel(string value, out bool isToxic)
    {
        var normalized = value.Trim().Trim('"');
        if (normalized == "1" || normalized.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            isToxic = true;
            return true;
        }

        if (normalized == "0" || normalized.Equals("false", StringComparison.OrdinalIgnoreCase))
        {
            isToxic = false;
            return true;
        }

        isToxic = false;
        return false;
    }

    private static List<string> SplitCsvLine(string line)
    {
        var values = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (ch == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                    continue;
                }

                inQuotes = !inQuotes;
                continue;
            }

            if (ch == ',' && !inQuotes)
            {
                values.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(ch);
        }

        values.Add(current.ToString());
        return values;
    }

    private static (int messageIndex, int labelIndex) ResolveColumnIndexes(IReadOnlyList<string> headerColumns)
    {
        if (headerColumns.Count < 2)
        {
            return (0, 1);
        }

        static string Norm(string s) =>
            s.Trim().Trim('"').Replace(" ", string.Empty, StringComparison.Ordinal).ToLowerInvariant();

        int messageIndex = -1;
        int labelIndex = -1;

        for (int i = 0; i < headerColumns.Count; i++)
        {
            var col = Norm(headerColumns[i]);

            if (messageIndex < 0 && (col is "message" or "text" or "textcontent" or "comment" or "content"))
            {
                messageIndex = i;
            }

            if (labelIndex < 0 && (col is "istoxic" or "label" or "toxic"))
            {
                labelIndex = i;
            }
        }

        if (messageIndex < 0) messageIndex = 0;
        if (labelIndex < 0) labelIndex = messageIndex == 0 ? 1 : 0;

        return (messageIndex, labelIndex);
    }
}
