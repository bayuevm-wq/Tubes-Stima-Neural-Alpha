using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

/// <summary>
/// AltBot1 - GreedySniper
///
/// STRATEGI GREEDY: Energy-Conservative Sniper
/// Heuristic: Selalu tembak musuh TERDEKAT dengan peluru paling hemat yang masih
///            menghasilkan damage cukup, sambil menjaga energi tetap tinggi.
///
/// Elemen Greedy:
///   Himpunan Kandidat : Semua musuh dalam jangkauan radar (< 1200px)
///   Fungsi Seleksi    : Pilih musuh dengan JARAK TERDEKAT
///   Fungsi Kelayakan  : Energi bot sendiri > 30 untuk terus menembak
///   Fungsi Objektif   : Bertahan hidup lebih lama (Survival Score 50 poin/kematian musuh)
///
/// Perbedaan dari MainBot:
///   - MainBot: kejar musuh terlemah (agresif)
///   - AltBot1 : diam di tempat/berputar perlahan, tembak musuh terdekat (konservatif)
///               Hemat energi dengan tidak berlari, fokus survival score
/// </summary>
public class AltBot1 : Bot
{
    static void Main(string[] args) { new AltBot1().Start(); }
    AltBot1() : base(BotInfo.FromFile("alt-bot-1.json")) { }

    // Target terdekat
    private double _targetX    = 0;
    private double _targetY    = 0;
    private double _targetDist = double.MaxValue;
    private bool   _hasTarget  = false;

    // Arah gerakan berputar (perimeter patrol)
    private bool _movingForward = true;
    private int  _moveCount     = 0;

    public override void Run()
    {
        // Warna: Biru Tua (sniper dingin)
        BodyColor   = Color.FromArgb(0x00, 0x00, 0x8B);
        TurretColor = Color.FromArgb(0x00, 0x80, 0xFF);
        RadarColor  = Color.FromArgb(0x00, 0xFF, 0xFF);
        BulletColor = Color.FromArgb(0xAD, 0xD8, 0xE6);
        ScanColor   = Color.FromArgb(0x87, 0xCE, 0xEB);
        TracksColor = Color.FromArgb(0x00, 0x00, 0x5C);
        GunColor    = Color.FromArgb(0x40, 0x80, 0xFF);

        AdjustGunForBodyTurn   = true;
        AdjustRadarForBodyTurn = true;

        while (IsRunning)
        {
            // Patrol: bergerak bolak-balik di posisi aman (hindari dinding)
            PatrolMove();

            // Radar terus scan
            TurnRadarRight(45);
        }
    }

    /// <summary>
    /// Perimeter patrol: gerak bolak-balik, hindari dinding.
    /// Tujuan: tidak diam di tempat agar tidak mudah ditembak,
    /// tapi tidak terlalu jauh dari posisi strategis.
    /// </summary>
    private void PatrolMove()
    {
        const double margin = 100;
        bool nearWall = X < margin || X > ArenaWidth - margin ||
                        Y < margin || Y > ArenaHeight - margin;

        if (nearWall)
        {
            // Arahkan ke tengah arena
            double cx = ArenaWidth / 2.0;
            double cy = ArenaHeight / 2.0;
            TurnLeft(CalcBearing(BearingTo(cx, cy)));
            _movingForward = true;
        }

        _moveCount++;
        if (_moveCount > 15)
        {
            // Sedikit belok agar tidak jalan lurus terus (mudah ditebak)
            TurnRight(30);
            _moveCount = 0;
        }

        if (_movingForward)
            Forward(50);
        else
            Back(50);
    }

    /// <summary>
    /// Radar mendeteksi musuh.
    /// GREEDY: Update target ke musuh yang PALING DEKAT.
    /// Logika: musuh terdekat paling mudah ditembak (tidak perlu prediksi jauh)
    /// </summary>
    public override void OnScannedBot(ScannedBotEvent evt)
    {
        double dist = DistanceTo(evt.X, evt.Y);

        // Seleksi greedy: prioritas jarak terpendek
        if (!_hasTarget || dist < _targetDist)
        {
            _targetX    = evt.X;
            _targetY    = evt.Y;
            _targetDist = dist;
            _hasTarget  = true;
        }

        // Arahkan gun ke target terdekat
        double gunBearing = CalcGunBearing(BearingTo(_targetX, _targetY));
        TurnGunLeft(gunBearing);

        // Tembak jika gun sudah mengarah tepat
        if (Math.Abs(gunBearing) < 6 && GunHeat == 0)
        {
            // Greedy firepower: hemat energi jika energi rendah
            double fp = SniperFirePower(dist);
            if (Energy > 30 || fp <= 1.5)
                Fire(fp);
        }
    }

    /// <summary>
    /// Sniper firepower: lebih konservatif, menjaga energi bot tetap tinggi.
    ///   Sangat dekat (< 150) : 3.0
    ///   Dekat        (< 300) : 2.0
    ///   Sedang       (< 600) : 1.5
    ///   Jauh        (>= 600) : 1.0
    /// </summary>
    private double SniperFirePower(double distance)
    {
        if (distance < 150) return 3.0;
        if (distance < 300) return 2.0;
        if (distance < 600) return 1.5;
        return 1.0;
    }

    public override void OnHitByBullet(HitByBulletEvent evt)
    {
        // Dodge: bergerak tegak lurus arah peluru datang
        double bearing = CalcBearing(evt.Bullet.Direction);
        TurnLeft(90 - bearing);
        Forward(60);
    }

    public override void OnHitWall(HitWallEvent evt)
    {
        Back(40);
        TurnRight(60);
        _movingForward = !_movingForward;
    }

    public override void OnBotDeath(BotDeathEvent evt)
    {
        _hasTarget  = false;
        _targetDist = double.MaxValue;
    }
}
