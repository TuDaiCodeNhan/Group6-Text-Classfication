## 1. Build Result

- **dotnet restore**: PASS  
  - Chạy: `dotnet restore WebApi/WebApi.csproj`
- **dotnet build**: PASS (0 warnings / 0 errors)  
  - Chạy: `dotnet build WebApi/WebApi.csproj -c Release`

## 2. Lỗi đã phát hiện

- **Runtime lỗi hardcode path dataset**: `Training` console crash vì dataset đang trỏ tới đường dẫn tuyệt đối máy khác (`D:\Classroom\...Toxicdataset.csv`).
- **Nullable warnings**:
  - `ModelInput.TextContent` non-nullable nhưng chưa init (CS8618).
  - `Console.ReadLine()` có thể trả về null (CS8600).
- **Rủi ro data-engineer (CSV parsing)**:
  - `DatasetProcessor` parse CSV chưa xử lý tốt escaped quotes `""` và chưa map theo header.
  - `DataAugmentor` đọc CSV bằng `Split(',', 2)` có thể vỡ khi text có dấu phẩy trong quotes.

## 3. Lỗi đã sửa

- `**Training/AppConstants.cs`, `AppConstants.cs`**
  - **Sửa gì**: bỏ hardcode đường dẫn tuyệt đối; chuyển sang `DatasetPath` resolve theo thư mục hiện tại + các thư mục cha (tìm các file dataset phổ biến như `premium_toxic_dataset.csv`, `Toxicdataset.csv`, `toxic_dataset_1000.csv`).
  - **Vì sao**: để demo chạy được trên mọi máy, không phụ thuộc ổ đĩa/đường dẫn cá nhân; đúng nguyên tắc “không hardcode path tuyệt đối”.
- `**Training/Model/ModelInput.cs`, `WebApi/Model/ModelInput.cs`**
  - **Sửa gì**: init `TextContent` mặc định `string.Empty`.
  - **Vì sao**: hết warning CS8618, tránh null gây crash khi object được tạo bởi deserializer/ML.NET.
- `**Training/Program.cs`**
  - **Sửa gì**: `Console.ReadLine() ?? string.Empty`.
  - **Vì sao**: hết warning CS8600, tránh null khi input stream đóng.
- `**Training/DataPreprocessing/DatasetProcessor.cs`**
  - **Sửa gì**:
    - CSV split xử lý escaped quotes `""`.
    - Map index cột theo header (Message/Text/TextContent… và Label/IsToxic…); fallback hợp lý.
  - **Vì sao**: đảm bảo “không vỡ khi text có dấu phẩy / ngoặc kép”, đọc CSV đúng header.
- `**Training/DataPreprocessing/DataAugmentor.cs`**
  - **Sửa gì**:
    - Đọc CSV bằng parser có quotes/escaped quotes.
    - Parse label bằng `TryParseLabel` (không dùng `StartsWith("1")`).
  - **Vì sao**: không đổi nhãn, không vỡ khi message có dấu phẩy/quotes.
- **Thêm `DevTools/` (project phụ để test nhanh)**
  - **Sửa gì**: tạo `DevTools` console tham chiếu `Training` để chạy smoke-test cho Cleaner/Processor/Augmentor/Logger/Analyzer.
  - **Vì sao**: test nhanh theo checklist mà không phá API và không cần unit-test framework.

## 4. Những phần đã kiểm tra

- **DataCleaner**
  - Null/empty không crash (trả string rỗng).
  - Không làm mất dấu tiếng Việt (giữ `\p{L}` và không normalize bỏ dấu).
  - Xóa link/mention/space dư và giảm ký tự lặp (đã smoke-test).
- **DatasetProcessor**
  - Đọc CSV theo header; không vỡ khi message có dấu phẩy + quotes.
  - Bỏ dòng rỗng; loại trùng theo key `cleanedText|label`.
  - Xuất `cleaned_toxic_dataset.csv` (default) hoặc path tùy chọn.
  - Thống kê toxic/safe theo label parse.
- **DataAugmentor**
  - Không đổi nhãn; unique theo `cleaned|label`.
  - Tăng dataset tối thiểu theo `minIncreaseRatio` nếu có thể sinh biến thể.
- **PredictionLogger**
  - Tự tạo file nếu chưa có, có header.
  - Escape CSV bằng quotes + double quotes chuẩn.
  - Thread-safe bằng `lock`.
- **LogAnalyzer**
  - Không crash nếu file chưa tồn tại.
  - Parse confidence invariant; bỏ qua dòng lỗi format; thống kê và top low-confidence.
- **PredictController**
  - Validate null / empty / >500 ký tự.
  - Gọi `PredictionLogger.Log(...)` sau khi predict.
  - Response JSON giữ format hiện có: `Message`, `IsToxic`, `ConfidenceScore`, `RecommendedAction`.

## 5. Những rủi ro còn lại

- **Chưa có unit tests tự động** (CI). Hiện mới có smoke-test qua `DevTools`.
- **CSV reader tự viết**: chưa hỗ trợ trường hợp “newline nằm trong quotes” (đa số dataset không có, nhưng vẫn là edge-case).
- **Augmentation đơn giản**: có thể không tăng đủ đa dạng nếu dataset quá ngắn/đã sạch mạnh; cần cân nhắc thêm rule nếu muốn tăng chất lượng.

## 6. Hướng dẫn tôi demo khi thi

### Build

- Build API:
  - `dotnet restore WebApi/WebApi.csproj`
  - `dotnet build WebApi/WebApi.csproj -c Release`

### Chạy console training (train + predict mẫu + manual)

- `dotnet run --project Training/Training.csproj -c Release`
- Dataset sẽ tự resolve trong repo (không còn hardcode path).
- Khi vào chế độ manual, nhập câu và xem output; nhập `exit` để thoát.

### Chạy Web API

- `dotnet run --project WebApi/WebApi.csproj -c Release`
- Gọi thử endpoint (PowerShell):
  - `Invoke-RestMethod -Method Post -Uri http://localhost:5xxx/api/predict -ContentType application/json -Body '{\"TextContent\":\"Thằng ngu này biến đi!\"}'`
  - (Port cụ thể xem trong console khi chạy API.)
- Log dự đoán:
  - File `prediction_logs.csv` sẽ được tạo ở thư mục chạy ứng dụng (content root của WebApi).

### Show phần “tự kiểm tra lỗi”

- Nói rõ bạn đã:
  - Build pass (0 warnings/0 errors).
  - Fix lỗi hardcode path dataset để chạy được mọi máy.
  - Gia cố CSV parsing (quotes/commas) cho Processor/Augmentor.
  - Smoke-test bằng `DevTools` để chứng minh Cleaner/Logger/Analyzer hoạt động với tiếng Việt, dấu phẩy và ngoặc kép.