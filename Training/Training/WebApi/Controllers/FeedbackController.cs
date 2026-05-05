using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using ToxicCommentClassifier.DataPreprocessing;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FeedbackController : ControllerBase
{
    private static readonly object SyncLock = new();
    private const string DefaultLogPath = "feedback_logs.csv";
    private const string Header = "Time,TextContent,PredictedIsToxic,CorrectedIsToxic,ConfidenceScore";

    public sealed record FeedbackRequest(
        string? TextContent,
        bool PredictedIsToxic,
        bool CorrectedIsToxic,
        double ConfidenceScore
    );

    [HttpPost]
    public IActionResult Post([FromBody] FeedbackRequest request)
    {
        if (request is null)
        {
            return BadRequest(new { Error = "Payload không hợp lệ." });
        }

        if (string.IsNullOrWhiteSpace(request.TextContent))
        {
            return BadRequest(new { Error = "Nội dung bình luận không được để trống." });
        }

        if (request.TextContent.Length > 500)
        {
            return BadRequest(new { Error = "Nội dung vượt quá 500 ký tự." });
        }

        var normalizedText = DataCleaner.CleanText(request.TextContent);
        if (string.IsNullOrWhiteSpace(normalizedText))
        {
            return BadRequest(new { Error = "Nội dung không hợp lệ sau khi chuẩn hóa." });
        }

        WriteFeedbackLog(DefaultLogPath, normalizedText, request.PredictedIsToxic, request.CorrectedIsToxic, request.ConfidenceScore);
        return Ok(new { Success = true });
    }

    private static void WriteFeedbackLog(string logPath, string textContent, bool predictedIsToxic, bool correctedIsToxic, double confidenceScore)
    {
        var line = string.Join(",",
            DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
            EscapeCsv(textContent),
            predictedIsToxic ? "1" : "0",
            correctedIsToxic ? "1" : "0",
            confidenceScore.ToString("F4", CultureInfo.InvariantCulture));

        lock (SyncLock)
        {
            var directory = Path.GetDirectoryName(logPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var fileExists = System.IO.File.Exists(logPath);
            using var writer = new StreamWriter(logPath, append: true, new UTF8Encoding(true));
            if (!fileExists)
            {
                writer.WriteLine(Header);
            }
            writer.WriteLine(line);
        }
    }

    private static string EscapeCsv(string value) => $"\"{value.Replace("\"", "\"\"")}\"";
}

