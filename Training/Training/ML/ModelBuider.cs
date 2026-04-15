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
            _mlContext = new MLContext(seed: 42); // Giữ seed để kết quả ổn định
        }

        public void TrainAndSaveModel()
        {
            Console.WriteLine("1. Đang nạp Dataset (1358 dòng)...");
            IDataView dataView = _mlContext.Data.LoadFromTextFile<ModelInput>(
                path: AppConstants.DatasetPath,
                hasHeader: true,
                separatorChar: ',');

            Console.WriteLine("2. Đang xây dựng Pipeline (FastTree + Char-Ngrams)...");

            // Cấu hình bộ lọc văn bản chuẩn xác nhất cho Tiếng Việt
            var textOptions = new TextFeaturizingEstimator.Options()
            {
                CaseMode = TextNormalizingEstimator.CaseMode.Lower,
                KeepDiacritics = true, // Giữ dấu tiếng việt để phân biệt từ
                KeepPunctuations = false,
                WordFeatureExtractor = new WordBagEstimator.Options() { NgramLength = 2, UseAllLengths = true },
                CharFeatureExtractor = new WordBagEstimator.Options() { NgramLength = 3, UseAllLengths = true }
            };

            // Sử dụng FastTree (Thuật toán cây quyết định mạnh mẽ hơn)
            var pipeline = _mlContext.Transforms.Text.FeaturizeText("Features", textOptions, nameof(ModelInput.TextContent))
                // SỬ DỤNG LBFGS: Thuật toán vua cho dữ liệu văn bản thưa (Sparse Text Data)
                .Append(_mlContext.BinaryClassification.Trainers.LbfgsLogisticRegression(
                    labelColumnName: "Label",
                    featureColumnName: "Features",
                    l2Regularization: 0.1f)); // Thêm L2 Regularization để chống học vẹt (Overfitting)

            Console.WriteLine("\n3. Đang thực hiện Đánh giá chéo K-Fold (5-Fold Cross Validation)...");

            // THUẬT TOÁN K-FOLD: Chia dữ liệu làm 5, huấn luyện và test 5 vòng xoay tua
            var cvResults = _mlContext.BinaryClassification.CrossValidate(
                data: dataView,
                estimator: pipeline,
                numberOfFolds: 5,
                labelColumnName: "Label");

            // Lấy điểm trung bình của 5 vòng
            double avgAccuracy = cvResults.Average(r => r.Metrics.Accuracy);
            double avgF1 = cvResults.Average(r => r.Metrics.F1Score);
            double avgAuc = cvResults.Average(r => r.Metrics.AreaUnderRocCurve);

            Console.WriteLine("   =========================================");
            Console.WriteLine("   --- KẾT QUẢ IQ THỰC TẾ CỦA MÔ HÌNH ---");
            Console.WriteLine($"   - Độ chính xác (Accuracy): {avgAccuracy:P2}");
            Console.WriteLine($"   - F1 Score (Cân bằng):     {avgF1:P2}");
            Console.WriteLine($"   - AUC (Độ phân biệt):      {avgAuc:P2}");
            Console.WriteLine("   =========================================\n");

            Console.WriteLine("4. Đang huấn luyện lại mô hình trên TOÀN BỘ dữ liệu...");
            // Vì K-Fold chỉ dùng để "đo lường", nên sau khi đo xong, ta đem 100% data đi Train để Model khôn nhất
            var finalModel = pipeline.Fit(dataView);

            Console.WriteLine("5. Đang lưu mô hình...");
            _mlContext.Model.Save(finalModel, dataView.Schema, AppConstants.ModelSavePath);
            Console.WriteLine($"Hoàn tất! Mô hình đã được lưu tại: {AppConstants.ModelSavePath}\n");
        }
    }
}