using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.ML;
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
        // Validate sơ bộ: Chặn luôn nếu gửi chuỗi rỗng
        if (string.IsNullOrWhiteSpace(input.TextContent))
        {
            return BadRequest(new { Error = "Nội dung bình luận không được để trống." });
        }

        // Gọi AI dự đoán
        var result = _pool.Predict(modelName: "ToxicModel", example: input);

        // Logic tính toán ngưỡng
        float threshold = 0.75f;
        bool isToxic = result.Probability >= threshold;

        // Trả về JSON (Hàm Ok() tự động bọc thành HTTP Status 200)
        return Ok(new
        {
            Message = input.TextContent,
            IsToxic = isToxic,
            ConfidenceScore = Math.Round(result.Probability * 100, 2) + "%",
            RecommendedAction = isToxic ? "Block & Review" : "Allow"
        });
    }
}
