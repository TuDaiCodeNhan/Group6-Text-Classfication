# Dataset Report - Toxic Comment Detection

## 1) Tong so dong
- Nguon tham chieu: `Toxicdataset.csv`
- Tong so dong du lieu: 4,656 (khong tinh header)

## 2) Ti le toxic/safe
- Toxic: 1,345 dong (28.89%)
- Safe: 3,311 dong (71.11%)

## 3) Nhan xet imbalance
- Dataset lech ve lop `Safe` (~71%).
- Muc lech chua qua nghiem trong, nhung co nguy co model uu tien du doan `Safe` de tang accuracy ao.

## 4) Van de du lieu
- Nhieu bien the teencode va viet tat (`vl`, `vcl`, `db`, `dm`) lam tang do nhieu.
- Co cau co tu nhay cam nhung khong doc hai, de gay false positive.
- Ton tai ky tu lap, ky tu dac biet, va dinh dang khong dong nhat.
- Co nhung truong hop mia mai, cong kich gian tiep ma nhan chi dua tren tu khoa.

## 5) De xuat cai thien
- Tang du lieu hard cases cho cac nhom: mia mai, phan nan san pham, tu nhay cam trung tinh.
- Day manh augmentation theo bien the teencode va typo co kiem soat.
- Theo doi confidence thap trong production va bo sung lai vao tap train theo chu ky.
- Dan nhan bo sung cho nhom cau ngan, mo ho de giam nhieu label.
