# 🛡️Social Comment Classifier using ML.NET

Hệ thống phân loại bình luận độc hại (Toxic Comment) ứng dụng Trí tuệ nhân tạo (Machine Learning). Một dự án nghiên cứu chuyên sâu được phát triển qua 3 giai đoạn Lab (Xây dựng - Tinh chỉnh - Triển khai), sử dụng **ML.NET** và **ASP.NET Core Web API** để bảo vệ không gian mạng.

---

## 👥 Team Members & Responsibilities

| Thành viên | Vai trò | Nội dung phụ trách | Chi tiết nhiệm vụ |
| :--- | :--- | :--- | :--- |
| **Nguyễn Tuấn Việt** | ** Data Engineer** | Xử lý dữ liệu đầu vào, pipeline tiền xử lý và hỗ trợ giám sát hệ thống |Thu thập, mở rộng và làm sạch bộ dữ liệu toxic / non-toxic để đảm bảo chất lượng đầu vào cho mô hình.Xây dựng pipeline tiền xử lý dữ liệu:chuẩn hóa văn bản,xử lý teencode / từ viết tắt / ký tự nhiễu,loại bỏ dữ liệu trùng lặp,
xuất dữ liệu sạch phục vụ huấn luyện.Tạo và bổ sung các trường hợp dữ liệu khó (hard negatives) để giảm tỷ lệ dự đoán nhầm.Phối hợp kiểm thử dữ liệu đầu vào thực tế, phát hiện các trường hợp mô hình dễ sai để cải thiện tập train.Phát triển module logging và giám sát kết quả dự đoán của hệ thống:ghi nhận comment người dùng gửi vào,lưu kết quả phân loại toxic / safe,theo dõi độ tin cậy của mô hình,hỗ trợ debug lỗi và thống kê hiệu năng hệ thống khi chạy thực tế. |
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
