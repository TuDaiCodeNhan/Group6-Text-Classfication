using System.Globalization;

namespace ToxicCommentClassifier.Logging;

public class LogAnalyzer
{
    public void Analyze(string logPath = "prediction_logs.csv", int lowConfidenceTop = 5)
    {
        if (!File.Exists(logPath))
        {
            Console.WriteLine($"Khong tim thay file log: {logPath}");
            return;
        }

        var lines = File.ReadAllLines(logPath);
        if (lines.Length <= 1)
        {
            Console.WriteLine("File log chua co du lieu.");
            return;
        }

        int toxicCount = 0;
        int safeCount = 0;
        double confidenceSum = 0;
        int confidenceCount = 0;
        var lowConfidence = new List<(double Confidence, string Text)>();

        foreach (var line in lines.Skip(1))
        {
            var cols = SplitCsvLine(line);
            if (cols.Count < 5)
            {
                continue;
            }

            var isToxic = cols[2].Trim() == "1";
            if (isToxic) toxicCount++;
            else safeCount++;

            if (double.TryParse(cols[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var confidence))
            {
                confidenceSum += confidence;
                confidenceCount++;
                lowConfidence.Add((confidence, cols[1]));
            }
        }

        var avgConfidence = confidenceCount == 0 ? 0 : confidenceSum / confidenceCount;
        Console.WriteLine($"So toxic: {toxicCount}");
        Console.WriteLine($"So safe: {safeCount}");
        Console.WriteLine($"Confidence trung binh: {avgConfidence:F4}");
        Console.WriteLine("Top cau co confidence thap:");

        foreach (var item in lowConfidence.OrderBy(x => x.Confidence).Take(Math.Max(1, lowConfidenceTop)))
        {
            Console.WriteLine($"- [{item.Confidence:F4}] {item.Text}");
        }
    }

    private static List<string> SplitCsvLine(string line)
    {
        var result = new List<string>();
        var current = new List<char>();
        var inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (ch == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Add('"');
                    i++;
                    continue;
                }

                inQuotes = !inQuotes;
                continue;
            }

            if (ch == ',' && !inQuotes)
            {
                result.Add(new string(current.ToArray()));
                current.Clear();
                continue;
            }

            current.Add(ch);
        }

        result.Add(new string(current.ToArray()));
        return result;
    }
}
