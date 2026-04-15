# 🛡️Social Comment Classifier using ML.NET

Hệ thống phân loại bình luận độc hại (Toxic Comment) ứng dụng Trí tuệ nhân tạo (Machine Learning). Một dự án nghiên cứu chuyên sâu được phát triển qua 3 giai đoạn Lab (Xây dựng - Tinh chỉnh - Triển khai), sử dụng **ML.NET** và **ASP.NET Core Web API** để bảo vệ không gian mạng.

---

## 👥 Team Members & Responsibilities

| Thành viên | Vai trò | Nội dung phụ trách | Chi tiết nhiệm vụ |
| :--- | :--- | :--- | :--- |
| **Nguyễn Tuấn Việt** | **Input Processing Engineer** | Dữ liệu & Tiền xử lý | Xử lý đầu vào trực tiếp trong app	Xây dựng module tiền xử lý văn bản trong Web API: chuẩn hóa chữ thường, loại ký tự nhiễu, chuẩn hóa khoảng trắng, chuyển teencode/lách luật về dạng chuẩn trước khi đưa vào mô hình. Bổ sung log hoặc lưu câu trước/sau xử lý để dễ kiểm thử và giải thích kết quả dự đoán. |
| **Đàm Xuân Hòa** | **ML Researcher** | Huấn luyện & Tối ưu AI | Huấn luyện mô hình **LBFGS Logistic Regression**, cấu hình **N-Grams (3-grams)**, tinh chỉnh hệ số **L1/L2 Regularization** và đánh giá qua **5-Fold Cross Validation**. |
| **Giáp Đức Anh** | **Backend Lead** | Kiến trúc API & Hệ thống | Xây dựng **Controller-based Web API**, triển khai **PredictionEnginePool (Thread-safe)**, cấu hình **CORS Policy** và thiết lập **ngrok Tunneling** để live demo từ xa. |
| **Nguyễn Văn Đức** | **FrontEnd & ConnectAPI Tester** | Kiểm thử & Trải nghiệm | Thực hiện Integration Test qua **Postman**, xác định ngưỡng **Threshold (75%)**, thiết kế cấu trúc JSON phản hồi (DTO) và xây dựng kịch bản kiểm thử thực tế. Demo 1 app đơn giản test. |

---

## 🚀 Quick Access & Live Demo (via ngrok)

Để sử dụng API từ xa mà không cần cài đặt môi trường phức tạp, nhóm sử dụng giải pháp **ngrok Tunneling** để mở cổng giao tiếp công khai từ Local Server.

> **⚠️ Lưu ý:** Vì sử dụng bản ngrok miễn phí, URL có thể thay đổi sau mỗi lần restart server.

* **API Endpoint:** `https://<your-ngrok-id>.ngrok-free.app/api/predict`
* **Postman Header bắt buộc:**
    * `Content-Type: application/json`
    * `ngrok-skip-browser-warning: true` (Để bỏ qua trang chào của ngrok)

### Mẫu Request (JSON Body):
```json
{
  "textContent": "Nội dung bình luận cần kiểm tra..."
}
