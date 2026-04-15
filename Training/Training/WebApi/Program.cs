using WebApi.Models;
using Microsoft.Extensions.ML;

namespace WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 1. Đổi AddControllers → AddControllersWithViews để hỗ trợ Razor View
        builder.Services.AddControllersWithViews();

        // 2. AI Prediction Engine Pool (giữ nguyên)
        builder.Services.AddPredictionEnginePool<ModelInput, ModelOutput>()
            .FromFile(modelName: "ToxicModel",
                      filePath: "TextClassifierModel.zip",
                      watchForChanges: true);

        var app = builder.Build();

        // 3. Cho phép serve file tĩnh từ thư mục wwwroot (css, js, ảnh...)
        app.UseStaticFiles();

        // 4. Routing
        app.UseRouting();

        // 5. Map API controllers (PredictController → /api/predict)
        app.MapControllers();

        // 6. Map trang chủ UI → HomeController/Index
        app.MapDefaultControllerRoute(); // mặc định: Home/Index

        app.Run();
    }
}