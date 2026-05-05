using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.ML;
using ToxicCommentClassifier.DataPreprocessing;
using ToxicCommentClassifier.Logging;
using WebApi.Models;
namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PredictController : ControllerBase
{
    private readonly PredictionEnginePool<ModelInput, ModelOutput> _pool;

    // Cơ chế Dependency Injection: Lõi .NET sẽ tự động bơm Pool từ Program.cs vào đây
    public PredictController(PredictionEnginePool<ModelInput, ModelOutput> pool)
    {
        _pool = pool;
    }

    // Tạo API endpoint dạng POST
    [HttpPost]
    public IActionResult PredictToxic([FromBody] ModelInput input)
    {
        if (input is null)
        {
            return BadRequest(new { Error = "Payload không hợp lệ." });
        }

        if (string.IsNullOrWhiteSpace(input.TextContent))
        {
            return BadRequest(new { Error = "Nội dung bình luận không được để trống." });
        }

        if (input.TextContent.Length > 500)
        {
            return BadRequest(new { Error = "Nội dung vượt quá 500 ký tự." });
        }

        var normalizedText = DataCleaner.CleanText(input.TextContent);
        if (string.IsNullOrWhiteSpace(normalizedText))
        {
            return BadRequest(new { Error = "Nội dung không hợp lệ sau khi chuẩn hóa." });
        }

        if (normalizedText.Count(char.IsLetterOrDigit) < 2)
        {
            return BadRequest(new { Error = "Nội dung chứa quá nhiều ký tự bất thường." });
        }

        // Gọi AI dự đoán
        var modelInput = new ModelInput { TextContent = normalizedText };
        var result = _pool.Predict(modelName: "ToxicModel", example: modelInput);

        // Logic tính toán ngưỡng
        float threshold = 0.75f;
        bool isToxic = result.Probability >= threshold;
        var action = isToxic ? "Block & Review" : "Allow";

        PredictionLogger.Log(normalizedText, isToxic, result.Probability, action);

        // Trả về JSON (Hàm Ok() tự động bọc thành HTTP Status 200)
        return Ok(new
        {
            Message = normalizedText,
            IsToxic = isToxic,
            ConfidenceScore = Math.Round(result.Probability * 100, 2) + "%",
            RecommendedAction = action
        });
    }
}
