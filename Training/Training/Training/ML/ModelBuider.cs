using System;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Transforms.Text;
using ToxicCommentClassifier.Models;

namespace ToxicCommentClassifier.ML
{
    public class ModelBuilder
    {
        private readonly MLContext _mlContext;

        public ModelBuilder()
        {
            _mlContext = new MLContext(seed: 42);
        }

        public void TrainAndSaveModel()
        {
            Console.WriteLine("1. Đang nạp bộ Dataset (5000 dòng)...");
            IDataView dataView = _mlContext.Data.LoadFromTextFile<ModelInput>(
                path: AppConstants.DatasetPath,
                hasHeader: true,
                separatorChar: ',',
                allowQuoting: true);

            Console.WriteLine("2. Đang xây dựng NLP Pipeline...");

            // TỐI ƯU 1: BỘ LỌC ĐẶC TRƯNG CHUẨN XÁC
            var textOptions = new TextFeaturizingEstimator.Options()
            {
                CaseMode = TextNormalizingEstimator.CaseMode.Lower,
                KeepDiacritics = true, // Giữ dấu tiếng Việt cực kỳ quan trọng
                KeepPunctuations = false, // Xóa dấu câu để giảm nhiễu

                // Mở rộng ngữ cảnh: Bắt cụm 3 từ (VD: "không hề ngu")
                WordFeatureExtractor = new WordBagEstimator.Options() { NgramLength = 3, UseAllLengths = true },

                // Bắt Teencode: Bắt cụm 4 ký tự (VD: "vcll", "d.k.m")
                CharFeatureExtractor = new WordBagEstimator.Options() { NgramLength = 4, UseAllLengths = true }
            };

            // TỐI ƯU 2: THUẬT TOÁN ĐƯỢC ÉP XUNG CAO NHẤT
            var pipeline = _mlContext.Transforms.Text.FeaturizeText("Features", textOptions, nameof(ModelInput.TextContent))
                .Append(_mlContext.BinaryClassification.Trainers.LbfgsLogisticRegression(
                    labelColumnName: "Label",
                    featureColumnName: "Features",

                    // L1: Ép trọng số của các "từ rác" (như: là, thì, mà) về đúng 0
                    l1Regularization: 0.1f,

                    // L2:Chống học vẹt (Overfitting), không cho mô hình quá tin vào 1 từ duy nhất
                    l2Regularization: 0.1f,

                    // Tolerance: Ép thuật toán phải học cực sâu, sai số nhỏ hơn 0.00001 mới được dừng
                    optimizationTolerance: 1e-5f,

                    // HistorySize: Tăng bộ nhớ tạm của thuật toán để giải ma trận 5000 dòng tốt hơn
                    historySize: 50
                ));

            Console.WriteLine("\n3. Đang thực hiện Đánh giá chéo K-Fold (5-Fold Cross Validation)...");
            var cvResults = _mlContext.BinaryClassification.CrossValidate(
                data: dataView,
                estimator: pipeline,
                numberOfFolds: 5,
                labelColumnName: "Label");

            // Lấy kết quả trung bình
            double avgAccuracy = cvResults.Average(r => r.Metrics.Accuracy);
            double avgF1 = cvResults.Average(r => r.Metrics.F1Score);
            double avgAuc = cvResults.Average(r => r.Metrics.AreaUnderRocCurve);

            Console.WriteLine("   =========================================");
            Console.WriteLine("   --- KẾT QUẢ ĐÁNH GIÁ THỰC TẾ (SOTA) ---");
            Console.WriteLine($"   - Accuracy (Độ chính xác): {avgAccuracy:P2}");
            Console.WriteLine($"   - F1 Score (Độ cân bằng):  {avgF1:P2}");
            Console.WriteLine($"   - AUC (Độ phân biệt):      {avgAuc:P2}");
            Console.WriteLine("   =========================================\n");

            Console.WriteLine("4. Đang huấn luyện mô hình Final...");
            var finalModel = pipeline.Fit(dataView);

            Console.WriteLine("5. Đang lưu mô hình...");
            _mlContext.Model.Save(finalModel, dataView.Schema, AppConstants.ModelSavePath);
            Console.WriteLine($"Hoàn tất! Mô hình đã được lưu tại: {AppConstants.ModelSavePath}\n");
        }
    }
}