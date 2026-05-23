using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

/// <summary>
/// AltBot2 - GreedyRammer
///
/// STRATEGI GREEDY: Ram & Destroy
/// Heuristic: Tabrak (ram) musuh yang HP-nya rendah untuk mendapatkan
///            RAM DAMAGE BONUS sebesar 30% dari damage yang dibuat + Ram Damage 2x
///
/// Elemen Greedy:
///   Himpunan Kandidat : Semua musuh yang terdeteksi radar
///   Fungsi Seleksi    : Pilih musuh dengan energi < threshold (lemah, siap di-ram)
///   Fungsi Kelayakan  : Energi bot sendiri cukup untuk tanggung damage tabrakan
///   Fungsi Objektif   : Maksimalkan Ram Damage (2x) + Ram Kill Bonus (30%)
///
/// Perbedaan dari bot lain:
///   - MainBot : tembak musuh terlemah
///   - AltBot1 : tembak musuh terdekat (survival)
///   - AltBot2 : TABRAK musuh lemah (ram) untuk skor bonus berbeda
///   - AltBot3 : wall-hugger dengan tembakan oportunistik
///
/// Mekanisme:
///   1. Scan semua musuh, simpan posisi & energi
///   2. Jika ada musuh dengan energi < RAM_THRESHOLD -> maju langsung menabrak
///   3. Jika tidak ada target ram -> tembak musuh terdekat seperti biasa
///   4. Setelah ram, segera mundur untuk hindari counter-damage
/// </summary>
public class AltBot2 : Bot
{
    static void Main(string[] args) { new AltBot2().Start(); }
    AltBot2() : base(BotInfo.FromFile("alt-bot-2.json")) { }

    // Threshold energi: musuh di bawah ini jadi target ram
    private const double RAM_THRESHOLD = 30.0;

    private double _ramTargetX      = 0;
    private double _ramTargetY      = 0;
    private double _ramTargetEnergy = double.MaxValue;
    private bool   _hasRamTarget    = false;

    private double _shootTargetX    = 0;
    private double _shootTargetY    = 0;
    private double _shootTargetDist = double.MaxValue;
    private bool   _hasShootTarget  = false;

    public override void Run()
    {
        // Warna: Oranye Tua (ram aggressor)
        BodyColor   = Color.FromArgb(0xFF, 0x45, 0x00);
        TurretColor = Color.FromArgb(0xFF, 0x8C, 0x00);
        RadarColor  = Color.FromArgb(0xFF, 0xD7, 0x00);
        BulletColor = Color.FromArgb(0xFF, 0xFF, 0x00);
        ScanColor   = Color.FromArgb(0xFF, 0xA5, 0x00);
        TracksColor = Color.FromArgb(0x8B, 0x25, 0x00);
        GunColor    = Color.FromArgb(0xFF, 0x6E, 0x00);

        AdjustGunForBodyTurn   = true;
        AdjustRadarForBodyTurn = true;

        while (IsRunning)
        {
            if (_hasRamTarget)
            {
                // MODE RAM: t突進 (charge) ke target yang lemah
                double bearing = CalcBearing(BearingTo(_ramTargetX, _ramTargetY));
                TurnLeft(bearing);
                // Maju penuh untuk ram
                Forward(500);
            }
            else if (_hasShootTarget)
            {
                // MODE TEMBAK: tidak ada target ram, tembak musuh terdekat
                double bearing = CalcBearing(BearingTo(_shootTargetX, _shootTargetY));
                TurnLeft(bearing);
                Forward(60);
            }
            else
            {
                // Tidak ada target: jelajahi arena
                Forward(80);
                TurnRight(20);
            }

            // Radar terus scan
            TurnRadarRight(36);
        }
    }

    /// <summary>
    /// Scan musuh.
    /// GREEDY: 
    ///   - Jika energi musuh < RAM_THRESHOLD -> jadikan target ram (prioritas)
    ///   - Sisanya -> simpan sebagai target tembak (ambil yang terdekat)
    /// </summary>
    public override void OnScannedBot(ScannedBotEvent evt)
    {
        double dist = DistanceTo(evt.X, evt.Y);

        if (evt.Energy < RAM_THRESHOLD)
        {
            // Greedy: pilih target ram dengan energi paling rendah
            if (!_hasRamTarget || evt.Energy < _ramTargetEnergy)
            {
                _ramTargetX      = evt.X;
                _ramTargetY      = evt.Y;
                _ramTargetEnergy = evt.Energy;
                _hasRamTarget    = true;
            }
        }
        else
        {
            // Target tembak: pilih yang terdekat
            if (!_hasShootTarget || dist < _shootTargetDist)
            {
                _shootTargetX    = evt.X;
                _shootTargetY    = evt.Y;
                _shootTargetDist = dist;
                _hasShootTarget  = true;
            }
        }

        // Selagi bergerak ke target, tembak juga jika ada kesempatan
        if (!_hasRamTarget)
        {
            double gunBearing = CalcGunBearing(BearingTo(evt.X, evt.Y));
            TurnGunLeft(gunBearing);
            if (Math.Abs(gunBearing) < 10 && GunHeat == 0)
                Fire(CalcFirePower(dist));
        }
    }

    /// <summary>
    /// Setelah berhasil ram, segera mundur untuk hindari counter-damage.
    /// </summary>
    public override void OnHitBot(HitBotEvent evt)
    {
        if (evt.IsRammed)
        {
            // Kita yang menabrak -> ini yang kita inginkan!
            // Mundur sebentar lalu cari target baru
            Back(60);
            _hasRamTarget    = false;
            _ramTargetEnergy = double.MaxValue;
        }
        else
        {
            // Kita yang ditabrak -> mundur dan balik
            Back(80);
            TurnRight(90);
        }
    }

    private double CalcFirePower(double distance)
    {
        if (distance < 200) return 3.0;
        if (distance < 500) return 2.0;
        return 1.0;
    }

    public override void OnHitByBullet(HitByBulletEvent evt)
    {
        double bearing = CalcBearing(evt.Bullet.Direction);
        TurnLeft(90 - bearing);
        Forward(60);
    }

    public override void OnHitWall(HitWallEvent evt)
    {
        Back(50);
        TurnRight(45);
        _hasRamTarget = false;
    }

    public override void OnBotDeath(BotDeathEvent evt)
    {
        _hasRamTarget    = false;
        _hasShootTarget  = false;
        _ramTargetEnergy = double.MaxValue;
        _shootTargetDist = double.MaxValue;
    }
}
