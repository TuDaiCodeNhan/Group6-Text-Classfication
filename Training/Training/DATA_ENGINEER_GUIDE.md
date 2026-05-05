# DATA ENGINEER GUIDE - Toxic Comment Detection (C# + ML.NET)

### 1. Tong quan bai toan
Muc tieu la phan loai binh luan `Toxic` hoac `Safe` trong boi canh tieng Viet, bao gom ca teencode, viet tat, va cau co nhieu nhieu ky tu. Data Engineer dam nhiem chat luong dau vao cho model: lam sach, tao pipeline, mo rong du lieu, va giam sat prediction sau khi deploy.

### 2. Data Cleaning (co vi du before/after)
Data cleaning duoc dong goi trong `DataPreprocessing/DataCleaner.cs` qua ham `CleanText(string text)`.

Xu ly chinh:
- Trim + lowercase
- Xoa link (`http`, `www`)
- Xoa mention (`@user`)
- Rut gon ky tu lap (`nguoooo` -> `nguoo`)
- Xoa ky tu dac biet khong can thiet
- Chuan hoa khoang trang
- Null-safe

Vi du:
- Before: `  @Nam oi xem cai nay nheeee!!! http://abc.com  `
- After: `oi xem cai nay nhee!!!`

- Before: `VCLlll, san pham nhu *** gi vay????`
- After: `vcll, san pham nhu gi vay????`

### 3. Dataset Pipeline (flow raw -> clean -> train)
Pipeline du lieu:
1. Doc CSV goc (`Message,IsToxic`)
2. Clean tung dong bang `DataCleaner`
3. Loai dong rong, loi format, va trung lap
4. Xuat file `cleaned_toxic_dataset.csv`
5. Dung file cleaned de train model

`DatasetProcessor` co thong ke tai console:
- Tong so dong
- So dong bi loai
- So luong Toxic/Safe
- Ti le %

### 4. Data Augmentation (tai sao can)
Du lieu comment doc hai thuong co bien the rat da dang, neu khong augmentation model se de overfit vao mot so mau cu the.

`DataPreprocessing/DataAugmentor.cs` sinh bien the:
- Them ky tu lap co chu dich (`ngu` -> `nguu`)
- Chuyen doi teencode (`vãi` -> `vl`, `khong` -> `ko`)
- Bien the viet thuong / viet tat

Nhan (`IsToxic`) duoc giu nguyen.
Ty le tang du lieu toi thieu: 20%.

### 5. Hard Cases (giam loi model)
File `Data/hard_cases.csv` chua cac nhom cau kho:
- Toxic ro rang
- Khong toxic nhung co tu nhay cam
- Mia mai
- Phan nan san pham

Cot `Note` giai thich ly do de review va bo sung train set nhanh hon.

### 6. Logging (production)
`Logging/PredictionLogger.cs` ghi du doan ra `prediction_logs.csv` theo format:
`Time,TextContent,IsToxic,Confidence,Action`

Diem can thiet:
- Thread-safe bang `lock`
- Tu tao header neu file chua ton tai
- De trich xuat du lieu van hanh that sau deploy

### 7. Monitoring (log analysis)
`Logging/LogAnalyzer.cs` doc `prediction_logs.csv` va thong ke:
- So toxic
- So safe
- Confidence trung binh
- Top cau confidence thap (de sai)

Nhom confidence thap la uu tien dau tien cho chu ky cai thien du lieu tiep theo.

### 8. API Validation
`WebApi/Controllers/PredictController.cs` da bo sung:
- Chan payload null
- Chan text rong
- Chan text > 500 ky tu
- Clean va chuan hoa text truoc predict
- Chan input co qua nhieu ky tu bat thuong
- Ghi log ngay sau prediction

Validation giup giam tan cong input rac va giu du lieu log on dinh.

### 9. Tong ket vai tro Data Engineer
Data Engineer quyet dinh "chat luong tri tue" cua model thong qua:
- Chat luong va su can bang du lieu
- Pipeline clean/augment co lap lai duoc
- Bo hard cases sat thuc te
- Logging + monitoring lien tuc

Model tot khong chi nhot o training, ma phai duoc nuoi boi du lieu dung trong production.

### 10. Script noi khi di thi (1-2 phut, de hoc thuoc)
"Trong de tai toxic comment detection bang C# va ML.NET, em dong vai tro Data Engineer de dam bao model hoc dung va van hanh on dinh.

Buoc 1, em xay dung module Data Cleaning de chuan hoa text tieng Viet: xoa link, mention, ky tu rac, xu ly ky tu lap, nhung van giu dau tieng Viet.

Buoc 2, em tao Dataset Pipeline: doc CSV goc, clean tung dong, loai dong rong va trung lap, sau do xuat file cleaned de train. He thong in thong ke toxic/safe va ti le de kiem soat imbalance.

Buoc 3, em thuc hien Data Augmentation de mo rong toi thieu 20%, tao bien the teencode, viet tat, va loi go ky tu lap nham giup model ben vung hon voi du lieu that.

Buoc 4, em tao bo Hard Cases gom toxic ro, cau mia mai, cau phan nan san pham va cau co tu nhay cam nhung khong doc hai de giam false positive/false negative.

Buoc 5, o production em them Prediction Logging va Monitoring. Moi prediction duoc ghi lai voi confidence; sau do analyzer tim cac cau confidence thap de dua nguoc vao quy trinh cai thien du lieu.

Cuoi cung, em bo sung input validation o API de chan du lieu xau truoc khi vao model. Tong the, phan Data Engineer giup model khong chi dat diem test cao ma con dung duoc on dinh ngoai thuc te." 
