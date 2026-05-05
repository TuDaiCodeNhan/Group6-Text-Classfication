using System.Globalization;
using System.Text;

namespace ToxicCommentClassifier.Logging;

public static class PredictionLogger
{
    private static readonly object SyncLock = new();
    private const string Header = "Time,TextContent,IsToxic,Confidence,Action";

    public static void Log(string? textContent, bool isToxic, float confidence, string action, string logPath = "prediction_logs.csv")
    {
        var safeText = EscapeCsv(textContent ?? string.Empty);
        var safeAction = EscapeCsv(action ?? string.Empty);
        var line = string.Join(",",
            DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
            safeText,
            isToxic ? "1" : "0",
            confidence.ToString("F4", CultureInfo.InvariantCulture),
            safeAction);

        lock (SyncLock)
        {
            var directory = Path.GetDirectoryName(logPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var fileExists = File.Exists(logPath);
            using var writer = new StreamWriter(logPath, append: true, new UTF8Encoding(true));
            if (!fileExists)
            {
                writer.WriteLine(Header);
            }
            writer.WriteLine(line);
        }
    }

    private static string EscapeCsv(string value)
    {
        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
