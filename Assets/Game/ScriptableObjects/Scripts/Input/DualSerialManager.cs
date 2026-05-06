// Assets/_Game/Scripts/Input/DualSerialManager.cs
// Mengelola 2 ESP32 sekaligus di 2 COM port berbeda
// Player 1 = port pertama, Player 2 = port kedua

using System;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

namespace WizardPunk
{
    public class DualSerialManager : MonoBehaviour
    {
        public static DualSerialManager Instance { get; private set; }

        [Header("── Player 1 Serial ─────────────────────")]
        [SerializeField] private string portP1 = "COM3";
        [SerializeField] private int baudRateP1 = 115200;
        [SerializeField] private bool autoDetectP1 = true;

        [Header("── Player 2 Serial ─────────────────────")]
        [SerializeField] private string portP2 = "COM4";
        [SerializeField] private int baudRateP2 = 115200;
        [SerializeField] private bool autoDetectP2 = false;

        [Header("── Debug ───────────────────────────────")]
        [SerializeField] private bool logRawData = false;

        // Events
        public event Action<WandInputData> OnP1DataReceived;
        public event Action<WandInputData> OnP2DataReceived;

        // State
        public bool P1Connected { get; private set; }
        public bool P2Connected { get; private set; }
        public WandInputData P1LatestData { get; private set; }
        public WandInputData P2LatestData { get; private set; }

        // Internal
        private SerialPort serialP1, serialP2;
        private Thread threadP1, threadP2;
        private bool isRunning = false;

        private readonly object lockP1 = new object();
        private readonly object lockP2 = new object();
        private WandInputData pendingP1, pendingP2;
        private bool newDataP1, newDataP2;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            string[] allPorts = SerialPort.GetPortNames();
            Debug.Log($"[DualSerial] Available ports: {string.Join(", ", allPorts)}");

            if (autoDetectP1 && allPorts.Length >= 1)
                portP1 = allPorts[0];
            if (autoDetectP2 && allPorts.Length >= 2)
                portP2 = allPorts[1];

            isRunning = true;
            ConnectPort(ref serialP1, ref threadP1, portP1, baudRateP1, 1);
            ConnectPort(ref serialP2, ref threadP2, portP2, baudRateP2, 2);
        }

        void Update()
        {
            lock (lockP1)
            {
                if (newDataP1)
                {
                    P1LatestData = pendingP1;
                    newDataP1 = false;
                    OnP1DataReceived?.Invoke(pendingP1);
                }
            }
            lock (lockP2)
            {
                if (newDataP2)
                {
                    P2LatestData = pendingP2;
                    newDataP2 = false;
                    OnP2DataReceived?.Invoke(pendingP2);
                }
            }
        }

        private void ConnectPort(ref SerialPort sp, ref Thread t, string port, int baud, int player)
        {
            try
            {
                sp = new SerialPort(port, baud) { ReadTimeout = 200, DtrEnable = true };
                sp.Open();
                if (player == 1) P1Connected = true;
                else P2Connected = true;
                Debug.Log($"[DualSerial] P{player} connected: {port}");

                var capturedSP = sp;
                int capturedPlayer = player;
                t = new Thread(() => ReadLoop(capturedSP, capturedPlayer))
                { IsBackground = true };
                t.Start();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[DualSerial] P{player} failed ({port}): {e.Message}");
            }
        }

        private void ReadLoop(SerialPort sp, int player)
        {
            while (isRunning)
            {
                try
                {
                    if (!sp.IsOpen) { Thread.Sleep(100); continue; }
                    string line = sp.ReadLine().Trim();
                    if (!line.StartsWith("DATA,")) continue;
                    if (logRawData) Debug.Log($"[P{player}] {line}");

                    var data = ParseLine(line);
                    if (!data.IsValid) continue;

                    if (player == 1) lock (lockP1) { pendingP1 = data; newDataP1 = true; }
                    else lock (lockP2) { pendingP2 = data; newDataP2 = true; }
                }
                catch (TimeoutException) { }
                catch (Exception e)
                {
                    if (isRunning) Debug.LogWarning($"[DualSerial] P{player} read err: {e.Message}");
                    Thread.Sleep(100);
                }
            }
        }

        private WandInputData ParseLine(string line)
        {
            var parts = line.Split(',');
            if (parts.Length < 7) return WandInputData.Invalid;
            try
            {
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
            catch { return WandInputData.Invalid; }
        }

        public void SendP1(string cmd) => Send(serialP1, cmd);
        public void SendP2(string cmd) => Send(serialP2, cmd);

        private void Send(SerialPort sp, string cmd)
        {
            try { if (sp != null && sp.IsOpen) sp.WriteLine(cmd); }
            catch (Exception e) { Debug.LogWarning($"[DualSerial] Send err: {e.Message}"); }
        }

        void OnDestroy()
        {
            isRunning = false;
            threadP1?.Join(500); threadP2?.Join(500);
            if (serialP1?.IsOpen == true) serialP1.Close();
            if (serialP2?.IsOpen == true) serialP2.Close();
        }

        void OnApplicationQuit() => OnDestroy();
    }
}