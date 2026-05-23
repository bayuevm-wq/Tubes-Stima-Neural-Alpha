using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

/// <summary>
/// MainBot - GreedyHunter
///
/// STRATEGI GREEDY: Aggressive Target Hunter
/// Heuristic: Selalu kejar dan tembak musuh dengan ENERGI TERENDAH
///
/// Elemen Greedy:
///   Himpunan Kandidat : Semua musuh yang terdeteksi radar
///   Fungsi Seleksi    : Pilih musuh dengan energi paling rendah
///   Fungsi Kelayakan  : Musuh masih hidup dan bisa dijangkau radar
///   Fungsi Objektif   : Maksimalkan skor via Bullet Damage + Kill Bonus (20%)
///
/// Alasan pemilihan sebagai bot utama:
///   Membunuh musuh memberikan 20% bonus dari total damage ke musuh itu.
///   Dengan selalu fokus ke musuh terlemah, peluang membunuh jauh lebih tinggi
///   dibanding menyerang musuh kuat secara acak.
/// </summary>
public class MainBot : Bot
{
    static void Main(string[] args) { new MainBot().Start(); }
    MainBot() : base(BotInfo.FromFile("BotUtama.json")) { }

    // Target saat ini
    private double _targetX        = 0;
    private double _targetY        = 0;
    private double _targetEnergy   = double.MaxValue;
    private double _targetDist     = double.MaxValue;
    private bool   _hasTarget      = false;

    public override void Run()
    {
        // Warna: Merah Tua (identifikasi bot utama)
        BodyColor   = Color.FromArgb(0x8B, 0x00, 0x00);
        TurretColor = Color.FromArgb(0xFF, 0x00, 0x00);
        RadarColor  = Color.FromArgb(0xFF, 0x45, 0x00);
        BulletColor = Color.FromArgb(0xFF, 0xD7, 0x00);
        ScanColor   = Color.FromArgb(0xFF, 0x00, 0x7F);
        TracksColor = Color.FromArgb(0x5C, 0x00, 0x00);
        GunColor    = Color.FromArgb(0xDC, 0x14, 0x3C);

        AdjustGunForBodyTurn   = true;
        AdjustRadarForBodyTurn = true;

        while (IsRunning)
        {
            // Hindari dinding sebelum bergerak
            AvoidWalls();

            if (_hasTarget)
            {
                // Greedy move: maju ke target
                double bearing = CalcBearing(BearingTo(_targetX, _targetY));
                if (_targetDist > 150)
                {
                    TurnLeft(bearing);
                    Forward(Math.Min(_targetDist * 0.5, 100));
                }
                else
                {
                    // Terlalu dekat, sedikit mundur agar tidak tabrakan
                    Back(40);
                }
            }
            else
            {
                Forward(60);
            }

            // Radar terus berputar agar tidak ada musuh yang terlewat
            TurnRadarRight(36);
        }
    }

    /// <summary>
    /// Radar mendeteksi musuh.
    /// GREEDY: Perbarui target jika musuh ini lebih lemah dari target sekarang.
    /// </summary>
    public override void OnScannedBot(ScannedBotEvent evt)
    {
        double dist = DistanceTo(evt.X, evt.Y);

        // Seleksi greedy: prioritas energi terendah
        if (!_hasTarget || evt.Energy < _targetEnergy)
        {
            _targetX      = evt.X;
            _targetY      = evt.Y;
            _targetEnergy = evt.Energy;
            _targetDist   = dist;
            _hasTarget    = true;
        }

        // Arahkan gun ke target terpilih
        double gunBearing = CalcGunBearing(BearingTo(_targetX, _targetY));
        TurnGunLeft(gunBearing);

        // Tembak jika gun sudah cukup mengarah
        if (Math.Abs(gunBearing) < 8 && GunHeat == 0)
        {
            Fire(CalcFirePower(dist));
        }
    }

    /// <summary>
    /// Greedy firepower berdasarkan jarak:
    ///   Dekat  (< 200) : 3.0  -> damage besar, bonus energi besar
    ///   Sedang (< 500) : 2.0  -> seimbang antara damage dan kecepatan peluru
    ///   Jauh  (>= 500) : 1.0  -> peluru cepat, lebih mudah mengenai musuh
    /// </summary>
    private double CalcFirePower(double distance)
    {
        if (distance < 200) return 3.0;
        if (distance < 500) return 2.0;
        return 1.0;
    }

    /// <summary>
    /// Hindari dinding: arahkan ke tengah arena jika terlalu dekat ke tepi.
    /// </summary>
    private void AvoidWalls()
    {
        const double margin = 80;
        if (X < margin || X > ArenaWidth - margin ||
            Y < margin || Y > ArenaHeight - margin)
        {
            double cx = ArenaWidth / 2.0;
            double cy = ArenaHeight / 2.0;
            double bearing = CalcBearing(BearingTo(cx, cy));
            TurnLeft(bearing);
            Forward(100);
        }
    }

    /// <summary>
    /// Terkena peluru: dodge tegak lurus arah datangnya peluru.
    /// </summary>
    public override void OnHitByBullet(HitByBulletEvent evt)
    {
        double bearing = CalcBearing(evt.Bullet.Direction);
        TurnLeft(90 - bearing);
        Forward(80);
    }

    public override void OnHitWall(HitWallEvent evt)
    {
        Back(60);
        TurnRight(45);
    }

    public override void OnBotDeath(BotDeathEvent evt)
    {
        // Reset target agar cari musuh baru setelah seseorang mati
        _hasTarget    = false;
        _targetEnergy = double.MaxValue;
        _targetDist   = double.MaxValue;
    }
}
