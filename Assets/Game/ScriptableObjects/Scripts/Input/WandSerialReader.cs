// ============================================================
//  WandSerialReader.cs
//  Baca data dari ESP32 via Serial Port
//  Pasang script ini pada GameObject kosong "SerialManager"
// ============================================================
//  Setup:
//  1. Edit SERIAL_PORT sesuai COM port kamu (lihat di Arduino IDE)
//  2. Pastikan Build Settings → Player Settings → API Compatibility
//     di set ke ".NET 4.x" atau ".NET Standard 2.1"
// ============================================================

using System;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

public class WandSerialReader : MonoBehaviour
{

    //public static WandSerialReader Instance { get; private set; }

    //void Awake()
    //{
    //    if (Instance != null && Instance != this)
    //    {
    //        Destroy(gameObject);
    //        return;
    //    }
    //    Instance = this;
    //    DontDestroyOnLoad(gameObject);
    //}
    //// -------------------------------

    [Header("Serial Config")]
    [Tooltip("Contoh: COM3 (Windows) atau /dev/ttyUSB0 (Linux/Mac)")]
    public string serialPort = "COM6";
    public int baudRate = 115200;

    private volatile bool _isHolding = false;
    public bool IsHolding => _isHolding;

    // ── Data yang dibaca thread, dikonsumsi di main thread ──
    // Volatile supaya aman dibaca lintas thread
    private volatile float _pitch = 0f;
    private volatile float _roll = 0f;
    private volatile float _yaw = 0f;

    // Gesture buffer — pakai lock karena string bukan primitive
    private readonly object _gestureLock = new object();
    private string _pendingGesture = null;

    private volatile bool _zeroed = false;

    // Gyro velocity (dari "V:gx,gy,gz")
    private volatile float _gx = 0f;
    private volatile float _gy = 0f;
    private volatile float _gz = 0f;

    // Tambahkan variabel ini di bawah deklarasi variabel yang sudah ada
    // Thread-safe: pakai DateTime.UtcNow.Ticks, bukan Time.realtimeSinceStartup
    private long _lastActionTimeTicks = 0L;  // Diakses via Interlocked, tidak perlu volatile
    public float ActionHoldTimeout = 0.15f;

    public bool IsActionHeld
    {
        get
        {
            long lastTicks = Interlocked.Read(ref _lastActionTimeTicks);
            double elapsed = (DateTime.UtcNow.Ticks - lastTicks) / 10_000_000.0;
            return elapsed < ActionHoldTimeout;
        }
    }

    // Action button
    private volatile bool _actionPressed = false;

    // ── Public accessor (dipanggil WandController) ──
    public Vector3 EulerAngles => new Vector3(_pitch, _yaw, _roll);

    public Vector3 GyroVelocity => new Vector3(_gx, _gy, _gz);

    public bool ConsumeAction()
    {
        if (_actionPressed)
        {
            _actionPressed = false;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Ambil gesture terbaru. Mengembalikan null jika tidak ada.
    /// Setelah dipanggil, gesture di-clear otomatis.
    /// </summary>
    public string ConsumeGesture()
    {
        lock (_gestureLock)
        {
            string g = _pendingGesture;
            _pendingGesture = null;
            return g;
        }
    }

    /// <summary>
    /// Buang semua gesture yang tersimpan di buffer.
    /// Panggil sebelum fase input player dimulai untuk
    /// mencegah buffered input dari fase sebelumnya.
    /// </summary>
    public void FlushGesture()
    {
        lock (_gestureLock)
        {
            _pendingGesture = null;
        }
    }

    public bool ConsumeZeroed()
    {
        if (_zeroed)
        {
            _zeroed = false;
            return true;
        }
        return false;
    }

    // ── Internal ──
    private SerialPort _serial;
    private Thread _readThread;
    private volatile bool _running = false;

    // ── Status ──
    public bool IsReady { get; private set; } = false;
    public bool IsConnected => _serial != null && _serial.IsOpen;

    // ============================================================
    void Start()
    {
        OpenSerial();
    }

    void OnDestroy()
    {
        CloseSerial();
    }

    void OnApplicationQuit()
    {
        CloseSerial();
    }

    // ============================================================
    private void OpenSerial()
    {
        try
        {
            _serial = new SerialPort(serialPort, baudRate)
            {
                ReadTimeout = 1000,
                WriteTimeout = 1000,
                DtrEnable = false,   // Reset ESP32 saat connect (opsional)
            };
            _serial.Open();

            _running = true;
            _readThread = new Thread(ReadLoop) { IsBackground = true };
            _readThread.Start();

            Debug.Log($"[WandSerial] Connected to {serialPort}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[WandSerial] Failed to open {serialPort}: {e.Message}");
        }
    }

    private void CloseSerial()
    {
        _running = false;

        if (_readThread != null && _readThread.IsAlive)
            _readThread.Join(500);

        if (_serial != null && _serial.IsOpen)
        {
            _serial.Close();
            Debug.Log("[WandSerial] Port closed.");
        }
    }

    // ============================================================
    //  Thread baca — JANGAN akses Unity API di sini
    // ============================================================
    private void ReadLoop()
    {
        while (_running)
        {
            try
            {
                if (_serial == null || !_serial.IsOpen) break;

                string line = _serial.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(line)) continue;

                ParseLine(line);
            }
            catch (TimeoutException)
            {
                // Normal — lanjut loop
            }
            catch (Exception e)
            {
                if (_running)
                    Debug.LogWarning($"[WandSerial] Read error: {e.Message}");
            }
        }
    }

    // ============================================================
    private void ParseLine(string line)
    {
        // Format Euler: "E:pitch,roll,yaw"
        if (line.StartsWith("E:"))
        {
            string[] parts = line.Substring(2).Split(',');
            if (parts.Length == 3 &&
                float.TryParse(parts[0], System.Globalization.NumberStyles.Float,
                               System.Globalization.CultureInfo.InvariantCulture, out float p) &&
                float.TryParse(parts[1], System.Globalization.NumberStyles.Float,
                               System.Globalization.CultureInfo.InvariantCulture, out float r) &&
                float.TryParse(parts[2], System.Globalization.NumberStyles.Float,
                               System.Globalization.CultureInfo.InvariantCulture, out float y))
            {
                _pitch = p;
                _roll = r;
                _yaw = y;
            }
            return;
        }

        // Format Gesture: "G:X"
        if (line.StartsWith("G:") && line.Length >= 3)
        {
            string g = line.Substring(2);
            lock (_gestureLock)
            {
                _pendingGesture = g;
            }
            Debug.Log($"[WandSerial] Gesture: {g}");
            return;
        }

        // Status messages
        if (line == "RDY")
        {
            IsReady = true;
            Debug.Log("[WandSerial] ESP32 Ready!");
        }

        if (line == "ZEROED")
        {
            _zeroed = true;
            Debug.Log("[WandSerial] Zeroed received!");
        }

        // Format Velocity: "V:gx,gy,gz"
        if (line.StartsWith("V:"))
        {
            string[] parts = line.Substring(2).Split(',');
            if (parts.Length == 3 &&
                float.TryParse(parts[0], System.Globalization.NumberStyles.Float,
                               System.Globalization.CultureInfo.InvariantCulture, out float gx) &&
                float.TryParse(parts[1], System.Globalization.NumberStyles.Float,
                               System.Globalization.CultureInfo.InvariantCulture, out float gy) &&
                float.TryParse(parts[2], System.Globalization.NumberStyles.Float,
                               System.Globalization.CultureInfo.InvariantCulture, out float gz))
            {
                _gx = gx;
                _gy = gy;
                _gz = gz;
            }
            return;
        }

        // Format Action Button: "B:ACTION"
        if (line == "B:ACTION")
        {
            Interlocked.Exchange(ref _lastActionTimeTicks, DateTime.UtcNow.Ticks);
            _actionPressed = true; // Pertahankan ini jika sistem lama masih membutuhkannya
            return;
        }

        // Format HOLD (Tombol ditahan > 300ms)
        if (line == "B:HOLD")
        {
            _isHolding = true;
            return;
        }

        // Format RELEASE (Tombol dilepas setelah hold)
        if (line == "B:RELEASE")
        {
            _isHolding = false;
            return;
        }
    }
}