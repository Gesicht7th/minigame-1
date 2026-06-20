// Assets/_Game/Scripts/Input/DebugInputManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace WizardPunk
{
    public enum InputMode
    {
        Wand,
        KeyboardDebug
    }

    public enum PlayerSide
    {
        PlayerA,
        PlayerB
    }

    public class DebugInputManager : MonoBehaviour
    {
        public static DebugInputManager Instance { get; private set; }

        private InputMode currentMode = InputMode.Wand;
        private GameObject debugOverlay;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (Instance == null)
            {
                GameObject go = new GameObject("DebugInputManager");
                Instance = go.AddComponent<DebugInputManager>();
                DontDestroyOnLoad(go);
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ApplyCursorMode();
        }

        private void Start()
        {
            UpdateOverlayVisibility();
            ApplyCursorMode();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                currentMode = currentMode == InputMode.Wand ? InputMode.KeyboardDebug : InputMode.Wand;
                Debug.Log($"[InputMode] {currentMode}");
                UpdateOverlayVisibility();
                ApplyCursorMode();
            }

            if (Input.GetKeyDown(KeyCode.F11))
            {
                PerformEmergencyReset();
            }
        }

        private void PerformEmergencyReset()
        {
            Debug.Log("[EmergencyReset] F11 pressed");

            // 1. Clear Cached Session Results (PlayerPrefs)
            PlayerPrefs.DeleteKey("G1_ScoreP1");
            PlayerPrefs.DeleteKey("G1_ScoreP2");
            PlayerPrefs.DeleteKey("G2_ScoreP1");
            PlayerPrefs.DeleteKey("G2_ScoreP2");
            PlayerPrefs.DeleteKey("G3_Winner");
            PlayerPrefs.Save();

            // 2. Clear Temporary Cross-Scene Bridge State
            GameDataBridge.Instance?.ResetAll();

            Debug.Log("[EmergencyReset] Session cleared");

            // 3. Navigate back to MainMenu
            if (SceneFlowManager.Instance != null)
            {
                Debug.Log("[EmergencyReset] Loading MainMenu");
                SceneFlowManager.Instance.GoTo("MainMenu");
            }
            else
            {
                Debug.LogWarning("[EmergencyReset] SceneFlowManager not found, executing raw LoadScene.");
                SceneManager.LoadScene("MainMenu");
            }
        }

        public static void ApplyCursorMode()
        {
            if (Instance == null) return;

            if (Instance.currentMode == InputMode.KeyboardDebug)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                
                if (GlobalVirtualCursor.Instance != null)
                {
                    GlobalVirtualCursor.Instance.SetInputEnabled(false);
                }
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;

                if (GlobalVirtualCursor.Instance != null)
                {
                    GlobalVirtualCursor.Instance.SetInputEnabled(true);
                }
            }
        }


        private void UpdateOverlayVisibility()
        {
            if (debugOverlay != null)
            {
                debugOverlay.SetActive(currentMode == InputMode.KeyboardDebug);
            }
        }

        public static InputMode GetCurrentMode() => Instance != null ? Instance.currentMode : InputMode.Wand;

        public static bool GetActionDown(PlayerSide player)
        {
            if (GetCurrentMode() == InputMode.KeyboardDebug)
            {
                if (player == PlayerSide.PlayerA) return Input.GetKeyDown(KeyCode.Space);
                if (player == PlayerSide.PlayerB) return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
                return false;
            }
            else
            {
                WandSerialReader reader = player == PlayerSide.PlayerA ? PlayerAssignment.PlayerA : PlayerAssignment.PlayerB;
                return reader != null && reader.ConsumeAction();
            }
        }

        public static bool GetActionHeld(PlayerSide player)
        {
            if (GetCurrentMode() == InputMode.KeyboardDebug)
            {
                if (player == PlayerSide.PlayerA) return Input.GetKey(KeyCode.Space);
                if (player == PlayerSide.PlayerB) return Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter);
                return false;
            }
            else
            {
                WandSerialReader reader = player == PlayerSide.PlayerA ? PlayerAssignment.PlayerA : PlayerAssignment.PlayerB;
                return reader != null && reader.IsHolding;
            }
        }

        public static string ConsumeDirection(PlayerSide player)
        {
            if (GetCurrentMode() == InputMode.KeyboardDebug)
            {
                if (player == PlayerSide.PlayerA)
                {
                    if (Input.GetKeyDown(KeyCode.W)) return "U";
                    if (Input.GetKeyDown(KeyCode.S)) return "D";
                    if (Input.GetKeyDown(KeyCode.A)) return "L";
                    if (Input.GetKeyDown(KeyCode.D)) return "R";
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.I)) return "U";
                    if (Input.GetKeyDown(KeyCode.K)) return "D";
                    if (Input.GetKeyDown(KeyCode.J)) return "L";
                    if (Input.GetKeyDown(KeyCode.L)) return "R";
                }
                return null;
            }
            else
            {
                WandSerialReader reader = player == PlayerSide.PlayerA ? PlayerAssignment.PlayerA : PlayerAssignment.PlayerB;
                return reader != null ? reader.ConsumeGesture() : null;
            }
        }

        public static bool IsConnected(PlayerSide player)
        {
            if (GetCurrentMode() == InputMode.KeyboardDebug) return true;

            WandSerialReader reader = player == PlayerSide.PlayerA ? PlayerAssignment.PlayerA : PlayerAssignment.PlayerB;
            return WandSerialReader.IsAlive(reader) && reader.IsConnected;
        }
    }
}
