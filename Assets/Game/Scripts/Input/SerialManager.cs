// Assets/_Game/Scripts/Input/SerialManager.cs
// Mengelola komunikasi serial dengan ESP32
// PENTING: Memerlukan Api Compatibility Level = .NET Framework

using System;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

namespace WizardPunk
{
    public class SerialManager : MonoBehaviour
    {
        #region Singleton
        public static SerialManager Instance { get; private set; }
        #endregion

        #region Inspector Fields
        [Header("── Serial Configuration ──────────────")]
        [Tooltip("Nama port COM. Contoh: COM3 (Windows) atau /dev/ttyUSB0 (Linux)")]
        [SerializeField] private string portName = "COM3";
        [SerializeField] private int baudRate = 115200;

        [Header("── Auto Detection ────────────────────")]
        [Tooltip("Coba detect port ESP32 otomatis")]
        [SerializeField] private bool autoDetectPort = true;
        [Tooltip("Interval retry koneksi (detik)")]
        [SerializeField] private float reconnectInterval = 3f;

        [Header("── Debug ──────────────────────────────")]
        [SerializeField] private bool logRawData = false;
        [SerializeField] private bool logParseErrors = false;
        #endregion

        #region Public Properties
        public bool IsConnected => isConnected;
        public string CurrentPort => portName;
        public WandInputData LatestData { get; private set; }
        #endregion

        #region Events
        public event Action<WandInputData> OnDataReceived;
        public event Action<bool> OnConnectionChanged;
        public event Action<string> OnMessageReceived; // Untuk PONG, READY, dll
        #endregion

        #region Private Fields
        private SerialPort serialPort;
        private Thread readThread;
        private bool isRunning = false;
        private bool isConnected = false;

        // Thread-safe data exchange
        private readonly object dataLock = new object();
        private WandInputData pendingData;
        private bool hasNewData = false;
        private string pendingMessage = null;

        // Auto-reconnect
        private float reconnectTimer = 0f;
        private bool shouldReconnect = false;
        #endregion

        #region Unity Lifecycle
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            TryConnect();
        }

        void Update()
        {
            // Dispatch data ke main thread (Unity tidak thread-safe)
            DispatchPendingData();

            // Auto-reconnect logic
            if (shouldReconnect)
            {
                reconnectTimer -= Time.deltaTime;
                if (reconnectTimer <= 0f)
                {
                    shouldReconnect = false;
                    TryConnect();
                }
            }
        }

        void OnDestroy() => Shutdown();
        void OnApplicationQuit() => Shutdown();
        #endregion

        #region Connection
        private void TryConnect()
        {
            string[] availablePorts = SerialPort.GetPortNames();

            if (autoDetectPort && availablePorts.Length > 0)
            {
                // Coba port terakhir terdeteksi (biasanya ESP32 yang baru disambung)
                portName = availablePorts[availablePorts.Length - 1];
                Debug.Log($"[Serial] Auto-detected port: {portName}");
                Debug.Log($"[Serial] All available ports: {string.Join(", ", availablePorts)}");
            }

            if (availablePorts.Length == 0)
            {
                Debug.LogWarning("[Serial] Tidak ada port COM yang ditemukan. Coba keyboard mode.");
                return;
            }

            try
            {
                serialPort = new SerialPort(portName, baudRate)
                {
                    ReadTimeout = 500,
                    WriteTimeout = 500,
                    DtrEnable = true,
                    RtsEnable = true
                };
                serialPort.Open();

                isConnected = true;
                isRunning = true;

                readThread = new Thread(ReadLoop)
                {
                    IsBackground = true,
                    Name = "SerialReadThread"
                };
                readThread.Start();

                Debug.Log($"[Serial] ✅ Connected to {portName} @ {baudRate} baud");
                OnConnectionChanged?.Invoke(true);

                // Ping ESP32
                SendCommand("PING");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Serial] ❌ Gagal connect ke {portName}: {ex.Message}");
                isConnected = false;
                ScheduleReconnect();
            }
        }

        private void ScheduleReconnect()
        {
            shouldReconnect = true;
            reconnectTimer = reconnectInterval;
        }

        public void SendCommand(string cmd)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                try { serialPort.WriteLine(cmd); }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[Serial] Gagal kirim command '{cmd}': {ex.Message}");
                }
            }
        }

        public void Calibrate() => SendCommand("CALIBRATE");

        public string[] GetAvailablePorts() => SerialPort.GetPortNames();

        public void ChangePort(string newPort)
        {
            portName = newPort;
            Shutdown();
            System.Threading.Thread.Sleep(200);
            TryConnect();
        }
        #endregion

        #region Read Thread
        private void ReadLoop()
        {
            while (isRunning)
            {
                try
                {
                    if (serialPort == null || !serialPort.IsOpen)
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    string line = serialPort.ReadLine().Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    if (logRawData) Debug.Log($"[Serial RAW] {line}");

                    if (line.StartsWith("DATA,"))
                    {
                        WandInputData parsed = ParseDataLine(line);
                        if (parsed.IsValid)
                        {
                            lock (dataLock)
                            {
                                pendingData = parsed;
                                hasNewData = true;
                            }
                        }
                    }
                    else
                    {
                        // READY, PONG, CAL_DONE, ERROR, dll
                        lock (dataLock) { pendingMessage = line; }
                    }
                }
                catch (TimeoutException) { /* Normal saat tidak ada data */ }
                catch (Exception ex)
                {
                    if (isRunning)
                    {
                        Debug.LogWarning($"[Serial] Read error: {ex.Message}");
                        Thread.Sleep(200);
                    }
                }
            }
        }

        private WandInputData ParseDataLine(string line)
        {
            // Format: "DATA,ax,ay,az,gx,gy,gz"
            try
            {
                string[] parts = line.Split(',');
                if (parts.Length < 7) return WandInputData.Invalid;

                return new WandInputData
                {
                    ax = float.Parse(parts[1]),
                    ay = float.Parse(parts[2]),
                    az = float.Parse(parts[3]),
                    gx = float.Parse(parts[4]),
                    gy = float.Parse(parts[5]),
                    gz = float.Parse(parts[6]),
                    IsValid = true
                };
            }
            catch (Exception ex)
            {
                if (logParseErrors) Debug.LogWarning($"[Serial] Parse error: {ex.Message} | Line: {line}");
                return WandInputData.Invalid;
            }
        }
        #endregion

        #region Main Thread Dispatch
        private void DispatchPendingData()
        {
            WandInputData data = default;
            string msg = null;
            bool hasData = false;

            lock (dataLock)
            {
                if (hasNewData)
                {
                    data = pendingData;
                    hasData = true;
                    hasNewData = false;
                    LatestData = data;
                }
                if (pendingMessage != null)
                {
                    msg = pendingMessage;
                    pendingMessage = null;
                }
            }

            if (hasData) OnDataReceived?.Invoke(data);
            if (msg != null) OnMessageReceived?.Invoke(msg);
        }
        #endregion

        #region Shutdown
        private void Shutdown()
        {
            isRunning = false;
            isConnected = false;

            if (readThread != null && readThread.IsAlive)
            {
                readThread.Join(1000);
                readThread = null;
            }

            if (serialPort != null)
            {
                if (serialPort.IsOpen) serialPort.Close();
                serialPort.Dispose();
                serialPort = null;
            }
        }
        #endregion
    }
}