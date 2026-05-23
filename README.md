# 🤖 Tubes1 Stima — Neural Alpha (Robocode Tank Royale)

Repositori ini berisi implementasi algoritma **Greedy** pada agen cerdas (bot) untuk permainan **Robocode Tank Royale**. Proyek ini dibuat untuk memenuhi **Tugas Besar 1 mata kuliah IF2211 Strategi Algoritma**.

---

## 📖 Penjelasan Singkat Algoritma Greedy

**Algoritma Greedy** adalah paradigma algoritma yang membangun solusi secara bertahap dengan membuat keputusan **optimal secara lokal** pada setiap langkah, dengan harapan menghasilkan solusi yang mendekati optimal secara global. Pada setiap tahap, greedy memilih opsi terbaik yang tersedia saat itu (*locally optimal choice*) tanpa mempertimbangkan konsekuensi jangka panjang.

### Elemen-elemen Greedy pada Bot

Setiap bot mengimplementasikan elemen greedy berikut:

| Elemen Greedy | Penjelasan |
|---|---|
| **Himpunan Kandidat** | Semua musuh yang terdeteksi oleh radar bot |
| **Fungsi Seleksi** | Kriteria pemilihan target (berbeda per bot — lihat tabel di bawah) |
| **Fungsi Kelayakan** | Musuh masih hidup, energi bot cukup, dan target dalam jangkauan |
| **Fungsi Objektif** | Fungsi skor yang dimaksimalkan (Bullet Damage, Kill Bonus, Survival Score, dll.) |

### Variasi Strategi Greedy yang Diimplementasikan

Proyek ini mengimplementasikan **4 variasi strategi greedy** dengan heuristik berbeda:

| Bot | Strategi | Fungsi Seleksi (Heuristik Greedy) | Fungsi Objektif |
|---|---|---|---|
| **MainBot** — GreedyHunter | Aggressive Target Hunter | Pilih musuh dengan **energi terendah** | Maksimalkan *Bullet Damage* + *Kill Bonus* (20%) |
| **AltBot1** — GreedySniper | Energy-Conservative Sniper | Pilih musuh **terdekat** | Maksimalkan *Survival Score* (hemat energi) |
| **AltBot2** — GreedyRammer | Ram & Destroy | Pilih musuh dengan **energi < 30** untuk ditabrak | Maksimalkan *Ram Damage* (2×) + *Ram Kill Bonus* (30%) |
| **AltBot3** — GreedySurvivor | Wall-Hugging Survivor | Pilih posisi **perimeter** yang menjauhi musuh | Maksimalkan *Survival Score* (50 poin/kematian musuh) + *Last Survival Bonus* |

---

### 1. MainBot — GreedyHunter ⚔️ *(Bot Utama)*

- **Strategi:** Aggressive Target Hunter
- **Heuristik Greedy:** Selalu mengejar dan menembak musuh dengan **ENERGI TERENDAH**.
- **Alasan:** Membunuh musuh memberikan **20% bonus** dari total damage ke musuh tersebut. Dengan selalu fokus ke musuh terlemah, peluang mendapatkan kill (dan bonus) jauh lebih tinggi.
- **Cara kerja:**
  1. Radar berputar terus-menerus, mencatat semua musuh.
  2. Fungsi seleksi greedy memilih musuh dengan energi paling rendah sebagai target.
  3. Bot bergerak mendekati target dan menembak dengan firepower adaptif (3.0 jarak dekat, 2.0 sedang, 1.0 jauh).
  4. Jika target mati, reset dan cari musuh baru.

### 2. AltBot1 — GreedySniper 🎯 *(Bot Alternatif 1)*

- **Strategi:** Energy-Conservative Sniper
- **Heuristik Greedy:** Selalu menembak musuh **TERDEKAT** (jarak terpendek).
- **Alasan:** Musuh terdekat paling mudah ditembak tanpa perlu prediksi jauh, menghemat energi peluru.
- **Cara kerja:**
  1. Bot melakukan patrol (bergerak bolak-balik) tanpa mengejar musuh secara agresif.
  2. Fungsi seleksi greedy memilih musuh dengan jarak terpendek.
  3. Tembakan konservatif — hanya menembak jika energi > 30 atau firepower rendah.
  4. Menjaga energi tetap tinggi untuk bertahan hidup lebih lama.

### 3. AltBot2 — GreedyRammer 🐏 *(Bot Alternatif 2)*

- **Strategi:** Ram & Destroy
- **Heuristik Greedy:** Menabrak (*ram*) musuh yang memiliki **ENERGI < 30** untuk mendapatkan **Ram Damage Bonus**.
- **Alasan:** Ram damage menghasilkan 2× damage normal + 30% bonus skor, strategi yang sangat menguntungkan terhadap musuh lemah.
- **Cara kerja:**
  1. Scan semua musuh, cek energi masing-masing.
  2. Jika ada musuh dengan energi di bawah threshold (30) → maju menerjang (*ram mode*).
  3. Setelah berhasil ram, mundur sejenak untuk menghindari *counter-damage*.
  4. Jika tidak ada target ram, fallback ke menembak musuh terdekat.

### 4. AltBot3 — GreedySurvivor 🛡️ *(Bot Alternatif 3)*

- **Strategi:** Wall-Hugging Survivor
- **Heuristik Greedy:** Bergerak melingkari tepi arena (*wall-hugging*), menjauhi musuh, tembak secara **oportunistik**.
- **Alasan:** Survival score (50 poin setiap musuh mati) + last survival bonus (10 × jumlah musuh) bisa sangat besar di akhir ronde.
- **Cara kerja:**
  1. Bot langsung menuju tepi arena di awal ronde.
  2. Berpatroli mengikuti dinding (wall-hugging) dengan gerakan melingkar.
  3. Tembakan konservatif (firepower rendah–sedang) saat musuh melintas.
  4. Jika terkena damage signifikan, balik arah dan kabur.

---

## 🛠️ Requirement Program & Instalasi

### Prasyarat Sistem

| Requirement | Versi Minimum | Keterangan |
|---|---|---|
| **Java (JRE/JDK)** | 11+ | Untuk menjalankan Robocode Tank Royale GUI/Server |
| **.NET SDK** | 6.0+ | Untuk melakukan *build* dan menjalankan bot C# |

### Instalasi

1. **Install Java JDK 11+**
   - Download dari [Oracle JDK](https://www.oracle.com/java/technologies/downloads/) atau [OpenJDK](https://adoptium.net/)
   - Pastikan `java` tersedia di PATH:
     ```bash
     java -version
     ```

2. **Install .NET SDK 6.0+**
   - Download dari [dotnet.microsoft.com](https://dotnet.microsoft.com/download)
   - Pastikan `dotnet` tersedia di PATH:
     ```bash
     dotnet --version
     ```

3. **Clone repositori ini**
   ```bash
   git clone https://github.com/bayuevm/Tubes1_NeuralAlpha.git
   cd Tubes1_NeuralAlpha
   ```

---

## 🚀 Cara Compile, Build, dan Menjalankan Program

### Langkah 1 — Jalankan Server Robocode Tank Royale

Klik dua kali pada file `robocode-tankroyale-gui-0.30.0.jar`, atau jalankan melalui terminal:

```bash
java -jar robocode-tankroyale-gui-0.30.0.jar
```

### Langkah 2 — Build & Jalankan Bot

Buka terminal baru untuk **setiap bot** yang ingin dijalankan:

**MainBot (GreedyHunter):**
```bash
cd src/main-bot/BotUtama
dotnet run
```

**AltBot1 (GreedySniper):**
```bash
cd src/alternative-bots/alt-bot-1
dotnet run
```

**AltBot2 (GreedyRammer):**
```bash
cd src/alternative-bots/alt-bot-2
dotnet run
```

**AltBot3 (GreedySurvivor):**
```bash
cd src/alternative-bots/alt-bot-3
dotnet run
```

> **Catatan:** Perintah `dotnet run` akan otomatis melakukan *restore*, *build*, dan *run* dalam satu langkah. Jika hanya ingin *build* tanpa menjalankan, gunakan `dotnet build`.

### Langkah 3 — Mulai Pertarungan

1. Di aplikasi **Robocode Tank Royale GUI**, pastikan server sudah berjalan (status *Running*).
2. Pada menu **Bot**, pilih bot-bot yang telah berhasil *connect*.
3. Klik **Start Battle** untuk memulai pertarungan.

---

## 📂 Struktur Direktori

```text
📦 Tubes1_NeuralAlpha
 ┣ 📂 src
 ┃ ┣ 📂 main-bot
 ┃ ┃ ┗ 📂 BotUtama              ← Source code MainBot (GreedyHunter)
 ┃ ┃   ┣ 📜 BotUtama.cs          ← Logika utama bot
 ┃ ┃   ┣ 📜 BotUtama.csproj      ← Project file .NET
 ┃ ┃   ┣ 📜 BotUtama.json        ← Konfigurasi metadata bot
 ┃ ┃   ┣ 📜 BotUtama.cmd         ← Script runner (Windows)
 ┃ ┃   ┗ 📜 BotUtama.sh          ← Script runner (Linux/Mac)
 ┃ ┗ 📂 alternative-bots
 ┃   ┣ 📂 alt-bot-1              ← Source code AltBot1 (GreedySniper)
 ┃   ┣ 📂 alt-bot-2              ← Source code AltBot2 (GreedyRammer)
 ┃   ┗ 📂 alt-bot-3              ← Source code AltBot3 (GreedySurvivor)
 ┣ 📜 robocode-tankroyale-gui-0.30.0.jar  ← Aplikasi Robocode
 ┣ 📜 config.properties          ← Konfigurasi GUI
 ┣ 📜 games.properties           ← Konfigurasi permainan
 ┣ 📜 server.properties          ← Konfigurasi Server
 ┣ 📜 .gitignore
 ┗ 📜 README.md
```

---

## 👥 Author

| Nama | NIM |
|---|---|
| Bayu Setiawan | 124140177 |
| Rakha Daffa Tama Truski | 124140196 |

> **Kelompok:** Neural Alpha
>
> **Kelas:** IF2211 Strategi Algoritma
>
> **Institusi:** Institut Teknologi Sumatera (ITERA)

---

*Dibuat untuk Tugas Besar 1 IF2211 Strategi Algoritma — Semester 4 2025/2026*
