using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

/// <summary>
/// AltBot3 - GreedySurvivor
///
/// STRATEGI GREEDY: Wall-Hugging Survivor
/// Heuristic: Bergerak melingkar di sepanjang tepi arena (wall-hugging),
///            tembak musuh yang melintas, utamakan BERTAHAN HIDUP
///
/// Elemen Greedy:
///   Himpunan Kandidat : Posisi di sekeliling arena (perimeter)
///   Fungsi Seleksi    : Pilih posisi perimeter yang menjauhi musuh terdekat
///   Fungsi Kelayakan  : Bot tidak bertabrakan dengan dinding
///   Fungsi Objektif   : Maksimalkan Survival Score (50 poin setiap musuh mati)
///                       + Last Survival Bonus (10 x jumlah musuh)
///
/// Perbedaan dari bot lain:
///   - MainBot : kejar musuh (agresif, mengejar kill)
///   - AltBot1 : patrol dan sniper (semi-defensif)
///   - AltBot2 : ram musuh lemah (taktis)
///   - AltBot3 : hugging dinding, hindari konflik, tunggu musuh saling bunuh
///
/// Mekanisme:
///   1. Bot berjalan melingkari arena mengikuti dinding (wall-hugging)
///   2. Radar scan terus; jika ada musuh, tembak dengan firepower sedang
///   3. Jika terkena damage besar, kabur ke sisi arena yang berbeda
///   4. Prioritas utama: jangan mati dulu (tunggu survival score menumpuk)
/// </summary>
public class AltBot3 : Bot
{
    static void Main(string[] args) { new AltBot3().Start(); }
    AltBot3() : base(BotInfo.FromFile("alt-bot-3.json")) { }

    // Arah perputaran (searah/berlawanan jarum jam)
    private bool   _clockwise      = true;
    private double _prevEnergy     = 100.0;
    private int    _turnCount      = 0;

    // Target tembak (opportunistik)
    private double _targetX    = 0;
    private double _targetY    = 0;
    private bool   _hasTarget  = false;

    public override void Run()
    {
        // Warna: Hijau Tua (survivor, kamuflase)
        BodyColor   = Color.FromArgb(0x00, 0x64, 0x00);
        TurretColor = Color.FromArgb(0x22, 0x8B, 0x22);
        RadarColor  = Color.FromArgb(0x7C, 0xFC, 0x00);
        BulletColor = Color.FromArgb(0x00, 0xFF, 0x7F);
        ScanColor   = Color.FromArgb(0xAD, 0xFF, 0x2F);
        TracksColor = Color.FromArgb(0x00, 0x3C, 0x00);
        GunColor    = Color.FromArgb(0x32, 0xCD, 0x32);

        AdjustGunForBodyTurn   = true;
        AdjustRadarForBodyTurn = true;

        // Posisikan diri ke tepi arena dulu
        MoveToPerimeter();

        while (IsRunning)
        {
            _turnCount++;

            // Deteksi apakah baru terkena damage (energi turun lebih dari tembakan)
            double energyDrop = _prevEnergy - Energy;
            if (energyDrop > 5)
            {
                // Kena damage signifikan: balik arah dan kabur
                _clockwise = !_clockwise;
                Back(60);
            }
            _prevEnergy = Energy;

            // Gerak wall-hugging: ikuti tepi arena
            WallHugMove();

            // Radar scan selama bergerak
            TurnRadarRight(45);
        }
    }

    /// <summary>
    /// Bergerak ke tepi arena di awal ronde.
    /// </summary>
    private void MoveToPerimeter()
    {
        const double margin = 60;
        // Pilih dinding terdekat dan gerak ke sana
        double distToLeft   = X;
        double distToRight  = ArenaWidth - X;
        double distToBottom = Y;
        double distToTop    = ArenaHeight - Y;

        double minDist = Math.Min(Math.Min(distToLeft, distToRight),
                                  Math.Min(distToBottom, distToTop));

        double tx = X, ty = Y;
        if (Math.Abs(minDist - distToLeft)   < 1) tx = margin;
        else if (Math.Abs(minDist - distToRight)  < 1) tx = ArenaWidth - margin;
        else if (Math.Abs(minDist - distToBottom) < 1) ty = margin;
        else ty = ArenaHeight - margin;

        double bearing = CalcBearing(BearingTo(tx, ty));
        TurnLeft(bearing);
        Forward(DistanceTo(tx, ty));
    }

    /// <summary>
    /// Wall-hugging movement: bergerak sejajar dinding arena.
    /// Greedy: selalu pilih arah yang menjaga posisi di tepi arena.
    /// </summary>
    private void WallHugMove()
    {
        const double margin = 50;
        const double target_margin = 60;

        double x = X, y = Y;
        double w = ArenaWidth, h = ArenaHeight;

        // Tentukan di sisi mana bot berada sekarang
        bool onLeft   = x < margin * 2;
        bool onRight  = x > w - margin * 2;
        bool onBottom = y < margin * 2;
        bool onTop    = y > h - margin * 2;

        // Arah target tergantung sisi dan arah putaran
        double targetAngle;
        if (onLeft)
            targetAngle = _clockwise ? 0 : 180;   // atas atau bawah
        else if (onRight)
            targetAngle = _clockwise ? 180 : 0;
        else if (onBottom)
            targetAngle = _clockwise ? 90 : 270;  // kanan atau kiri (0=utara, 90=timur)
        else if (onTop)
            targetAngle = _clockwise ? 270 : 90;
        else
        {
            // Tidak di tepi: menuju dinding terdekat
            MoveToPerimeter();
            return;
        }

        // Putar ke arah tersebut
        double bearing = CalcBearing(targetAngle - Direction);
        TurnLeft(bearing);
        Forward(80);
    }

    /// <summary>
    /// Tembak oportunistik: jika ada musuh yang terdeteksi, tembak.
    /// Greedy: tidak buang energi mengejar, cukup tembak saat melintas.
    /// </summary>
    public override void OnScannedBot(ScannedBotEvent evt)
    {
        double dist = DistanceTo(evt.X, evt.Y);
        _targetX   = evt.X;
        _targetY   = evt.Y;
        _hasTarget = true;

        // Arahkan gun ke musuh
        double gunBearing = CalcGunBearing(BearingTo(evt.X, evt.Y));
        TurnGunLeft(gunBearing);

        // Tembak hanya jika energi cukup dan gun sudah mengarah
        if (Math.Abs(gunBearing) < 12 && GunHeat == 0 && Energy > 20)
        {
            // Greedy: tembak medium power - tidak terlalu boros energi
            Fire(CalcFirePower(dist));
        }
    }

    /// <summary>
    /// Firepower konservatif untuk survivor:
    ///   Sangat dekat (< 200): 2.0  (tidak perlu 3.0, jaga energi)
    ///   Dekat-sedang (< 500): 1.5
    ///   Jauh        (>= 500): 1.0
    /// </summary>
    private double CalcFirePower(double distance)
    {
        if (distance < 200) return 2.0;
        if (distance < 500) return 1.5;
        return 1.0;
    }

    /// <summary>
    /// Kena peluru: balik arah dan kabur ke sisi lain
    /// </summary>
    public override void OnHitByBullet(HitByBulletEvent evt)
    {
        _clockwise = !_clockwise;
        double bearing = CalcBearing(evt.Bullet.Direction);
        TurnLeft(90 - bearing);
        Forward(100);
    }

    public override void OnHitWall(HitWallEvent evt)
    {
        Back(30);
        TurnRight(_clockwise ? 90 : -90);
    }

    public override void OnBotDeath(BotDeathEvent evt)
    {
        _hasTarget = false;
    }
}
