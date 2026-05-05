using System.Globalization;
using System.Text;

namespace ToxicCommentClassifier.DataPreprocessing;

public class DatasetProcessor
{
    public sealed record ProcessingStats(
        int TotalRows,
        int RemovedRows,
        int ToxicCount,
        int SafeCount
    );

    public ProcessingStats ProcessCsv(string inputPath, string outputPath = "cleaned_toxic_dataset.csv")
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

        var uniqueRows = new HashSet<string>(StringComparer.Ordinal);
        var outputRows = new List<(string Message, bool IsToxic)>();
        int totalRows = 0;
        int removedRows = 0;
        int toxicCount = 0;
        int safeCount = 0;

        for (int i = 1; i < lines.Length; i++)
        {
            var rawLine = lines[i];
            if (string.IsNullOrWhiteSpace(rawLine))
            {
                removedRows++;
                continue;
            }

            totalRows++;
            var columns = SplitCsvLine(rawLine);
            if (columns.Count <= Math.Max(messageIndex, labelIndex))
            {
                removedRows++;
                continue;
            }

            var cleanedText = DataCleaner.CleanText(columns[messageIndex]);
            if (string.IsNullOrWhiteSpace(cleanedText))
            {
                removedRows++;
                continue;
            }

            if (!TryParseLabel(columns[labelIndex], out var isToxic))
            {
                removedRows++;
                continue;
            }

            var key = $"{cleanedText}|{isToxic}";
            if (!uniqueRows.Add(key))
            {
                removedRows++;
                continue;
            }

            outputRows.Add((cleanedText, isToxic));
            if (isToxic) toxicCount++;
            else safeCount++;
        }

        WriteCsv(outputPath, outputRows);
        PrintSummary(totalRows, removedRows, toxicCount, safeCount);
        return new ProcessingStats(totalRows, removedRows, toxicCount, safeCount);
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
            writer.WriteLine($"\"{escaped}\",{(row.IsToxic ? 1 : 0).ToString(CultureInfo.InvariantCulture)}");
        }
    }

    private static void PrintSummary(int totalRows, int removedRows, int toxicCount, int safeCount)
    {
        var keptRows = toxicCount + safeCount;
        var toxicPercent = keptRows == 0 ? 0 : toxicCount * 100.0 / keptRows;
        var safePercent = keptRows == 0 ? 0 : safeCount * 100.0 / keptRows;
        Console.WriteLine($"Tong so dong: {totalRows}");
        Console.WriteLine($"So dong bi loai: {removedRows}");
        Console.WriteLine($"Toxic: {toxicCount} ({toxicPercent:F2}%)");
        Console.WriteLine($"Safe: {safeCount} ({safePercent:F2}%)");
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
