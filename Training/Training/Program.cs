using System;
using ToxicCommentClassifier.ML;
using System.Text;

namespace ToxicCommentClassifier
{
    class Program
    {
        static void Main(string[] args)
        {
            // Thiết lập Encoding để hiển thị và gõ tiếng Việt có dấu
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            try
            {
                // 1. Huấn luyện và lưu model
                var builder = new ModelBuilder();
                builder.TrainAndSaveModel();

                // 2. Khởi tạo bộ dự đoán với model vừa lưu
                var predictor = new Predictor();

                // Chạy thử vài câu mẫu trước
                Console.WriteLine("--- KẾT QUẢ DỰ ĐOÁN MẪU ---");
                TestSentence(predictor, "Đồ án này làm chán quá, toàn lỗi!");
                TestSentence(predictor, "Mọi người chia sẻ tài liệu ôn thi nhé.");
                TestSentence(predictor, "Thằng ngu này biến đi!");

                // 3. Vòng lặp cho phép người dùng tự nhập để test liên tục
                Console.WriteLine("========================================");
                Console.WriteLine("👉 BẮT ĐẦU CHẾ ĐỘ TEST THỦ CÔNG");
                Console.WriteLine("   (Nhập 'exit' hoặc để trống rồi nhấn Enter để kết thúc)");
                Console.WriteLine("========================================");

                while (true)
                {
                    Console.Write("Nhập câu cần kiểm tra: ");
                    string userInput = Console.ReadLine();

                    // Kiểm tra điều kiện thoát vòng lặp
                    if (string.IsNullOrWhiteSpace(userInput) || userInput.Trim().ToLower() == "exit")
                    {
                        Console.WriteLine("Đã thoát chương trình test.");
                        break;
                    }

                    // Gọi hàm dự đoán cho câu người dùng vừa nhập
                    TestSentence(predictor, userInput);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Đã xảy ra lỗi: {ex.Message}");
            }
        }

        static void TestSentence(Predictor predictor, string sentence)
        {
            var result = predictor.Predict(sentence);
            Console.WriteLine($"-> Phân loại: {(result.Prediction ? "🔴 Toxic" : "🟢 Bình thường")} (Độ tin cậy: {result.Probability:P2})\n");
        }
    }
}