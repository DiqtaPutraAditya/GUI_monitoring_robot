# GUI Monitoring Robot

Aplikasi **GUI Monitoring Robot** berbasis **C# WinForms** menggunakan Visual Studio.  
Dibuat untuk memonitor data robot secara real-time melalui antarmuka grafis, dengan integrasi sensor (imu MPU6050, Sensor Lidar, Ultrasonic) yang terhubung ke mikrokontroler STM32.

---

## 🎯 Fitur

- Tampilan GUI berbasis **Windows Forms (WinForms)**  
- Monitoring real-time data robot  
- Komunikasi serial dengan mikrokontroler (misalnya Arduino)  
- Visualisasi data sensor seperti:
  - Suhu  
  - Kelembaban  
  - Intensitas cahaya  
  - Kondisi hujan / tidak hujan  
- (Opsional) Log / penyimpanan data  

---

## 🛠️ Teknologi

- **C# .NET Framework**  
- **WinForms (Windows Forms Application)**  
- **Visual Studio** sebagai IDE  
- **SerialPort** untuk komunikasi dengan perangkat  

---

## 📁 Struktur Project
    GUI_monitoring_robot/

     ├── GUI/                          # Kode utama aplikasi WinForms
     ├── GUI EILERO 0.3/              # Versi lain/draft GUI
     ├── INSTALLER GUI EILERO/        # File installer project
     ├── My_GUI_EILERO/Release/       # Build release
     ├── packages/                    # Dependency NuGet
     ├── GUI.sln                     # Solution file Visual Studio
     └── README.md                   # Dokumentasi project

---

## 🚀 Cara Menjalankan

1. Clone repo ini:
   ```bash
   git clone https://github.com/DiqtaPutraAditya/GUI_monitoring_robot.git
2. Buka file solution GUI.sln di Visual Studio

3. Pastikan dependency NuGet terpasang (restore packages jika diminta Visual Studio)

4. Sambungkan mikrokontroler (misalnya Arduino) via Serial (COM Port)

5. Jalankan project dengan klik Start / F5

---

   📸 Screenshot

Tambahkan screenshot GUI kamu di sini (contoh di bawah masih dummy):

![Tampilan GUI](https://github.com/DiqtaPutraAditya/GUI_monitoring_robot/blob/main/Screenshot%202025-09-19%20090430.png)

---

⚠️ Catatan

Pastikan COM port yang dipilih sesuai dengan mikrokontroler

Jika muncul error "Access to port denied", coba jalankan Visual Studio sebagai administrator

Aplikasi hanya berjalan di Windows (WinForms)

---

🤝 Kontak

📧 Email: diqtaputraaditya2019@gmail.com

🐙 Github: DiqtaPutraAditya


---

