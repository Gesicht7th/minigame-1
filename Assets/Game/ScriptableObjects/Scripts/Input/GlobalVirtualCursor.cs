using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WizardPunk
{
    public class GlobalVirtualCursor : MonoBehaviour
    {
        public static GlobalVirtualCursor Instance { get; private set; }

        [Header("Wand Input")]
        [SerializeField] private WandSerialReader wandReader;

        [Header("Cursor Visual")]
        [SerializeField] private RectTransform cursorRect;

        [Header("Gyro Settings")]
        [SerializeField] private float gyroDeadzone = 4.08f;
        [SerializeField] private float gyroSensitivity = 0.05f;
        [SerializeField] private float aimSmoothing = 12f;
        [SerializeField] private float borderPadding = 0.03f;

        [Header("Fallback")]
        [SerializeField] private bool mouseFallback = false;

        private readonly List<RaycastResult> raycastResults = new List<RaycastResult>();
        private Vector2 normalized = new Vector2(0.5f, 0.5f);
        private Vector2 targetNormalized = new Vector2(0.5f, 0.5f);
        private GameObject hovered;
        private bool visible = true;
        private bool wandMode;
        private string wandReaderPortHint;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (wandReader != null)
                wandReaderPortHint = wandReader.serialPort;

            RefreshInputMode();
            ApplyVisibility();
            UpdateCursorRect();
        }

        private void Update()
        {
            RefreshInputMode();

            if (visible)
            {
                if (wandMode)
                {
                    UpdateWandPosition();
                }
                else if (mouseFallback)
                {
                    UpdateMouseFallback();
                }

                normalized = Vector2.Lerp(
                    normalized,
                    targetNormalized,
                    Time.deltaTime * aimSmoothing);

                UpdateCursorRect();
                HandleUIInteraction();
            }
            else
            {
                ClearHover();
            }
        }

        public void Show()
        {
            SetVisible(true);
        }

        public void Hide()
        {
            SetVisible(false);
            Debug.Log("[VirtualCursor] Input blocked because hidden");
        }

        public void SetVisible(bool show)
        {
            if (visible == show)
            {
                if (show)
                    ApplyVisibility();

                return;
            }

            visible = show;
            ApplyVisibility();

            if (!visible)
                ClearHover();
        }

        public void Recenter()
        {
            targetNormalized = new Vector2(0.5f, 0.5f);
            normalized = new Vector2(0.5f, 0.5f);
            UpdateCursorRect();
        }

        private void RefreshInputMode()
        {
            if (wandReader != null && wandReader.IsConnected)
                wandReaderPortHint = wandReader.serialPort;

            if (wandReader == null || !wandReader.IsConnected)
                ReacquireWandReader();

            wandMode = DebugInputManager.IsConnected(PlayerSide.PlayerA);

            Cursor.visible = !visible || !wandMode;
            Cursor.lockState = visible && wandMode
                ? CursorLockMode.Locked
                : CursorLockMode.None;
        }

        private void ReacquireWandReader()
        {
            if (!WandSerialReader.IsAlive(wandReader))
            {
                wandReader = PlayerAssignment.PlayerA;
                if (wandReader != null)
                {
                    wandReaderPortHint = wandReader.serialPort;
                    Debug.Log("[ReaderResolve] Recovered PlayerA for GlobalVirtualCursor");
                }
            }
        }

        private void ApplyVisibility()
        {
            if (cursorRect != null)
                cursorRect.gameObject.SetActive(visible);

            RefreshInputMode();
        }

        private float Deadzone(float value)
        {
            if (value > gyroDeadzone) return value - gyroDeadzone;
            if (value < -gyroDeadzone) return value + gyroDeadzone;
            return 0f;
        }

        private void UpdateWandPosition()
        {
            if (wandReader == null)
                return;

            Vector3 gyro = wandReader.GyroVelocity;
            float inputX = Deadzone(gyro.z);
            float inputY = Deadzone(gyro.x);

            float deltaX = inputX * -gyroSensitivity * Time.deltaTime;
            float deltaY = inputY * gyroSensitivity * Time.deltaTime;

            float border = borderPadding;
            targetNormalized.x = Mathf.Clamp(targetNormalized.x + deltaX, border, 1f - border);
            targetNormalized.y = Mathf.Clamp(targetNormalized.y + deltaY, border, 1f - border);

            if (wandReader.ConsumeZeroed())
                Recenter();
        }

        private void UpdateMouseFallback()
        {
            float border = borderPadding;
            targetNormalized.x = Mathf.Clamp(Input.mousePosition.x / Screen.width, border, 1f - border);
            targetNormalized.y = Mathf.Clamp(Input.mousePosition.y / Screen.height, border, 1f - border);
        }

        private void UpdateCursorRect()
        {
            if (cursorRect == null)
                return;

            cursorRect.position = new Vector3(
                normalized.x * Screen.width,
                normalized.y * Screen.height,
                0f);
        }

        private void HandleUIInteraction()
        {
            if (EventSystem.current == null)
                return;

            PointerEventData pointer = new PointerEventData(EventSystem.current)
            {
                position = new Vector2(
                    normalized.x * Screen.width,
                    normalized.y * Screen.height)
            };

            GameObject hit = RaycastSelectable(pointer);
            if (hit != hovered)
            {
                ClearHover(pointer);

                if (hit != null)
                    ExecuteEvents.Execute(hit, pointer, ExecuteEvents.pointerEnterHandler);

                hovered = hit;
            }

            if (wandMode && wandReader != null && wandReader.ConsumeAction())
            {
                ExecuteClick(pointer);
            }
            else if (!wandMode && mouseFallback && Input.GetMouseButtonDown(0))
            {
                ExecuteClick(pointer);
            }
        }

        private GameObject RaycastSelectable(PointerEventData pointer)
        {
            GraphicRaycaster[] raycasters = FindObjectsOfType<GraphicRaycaster>();
            foreach (GraphicRaycaster raycaster in raycasters)
            {
                if (raycaster == null || !raycaster.isActiveAndEnabled)
                    continue;

                raycastResults.Clear();
                raycaster.Raycast(pointer, raycastResults);

                foreach (RaycastResult result in raycastResults)
                {
                    GameObject selectable = FindSelectable(result.gameObject);
                    if (selectable != null)
                        return selectable;
                }
            }

            return null;
        }

        private void ExecuteClick(PointerEventData pointer)
        {
            if (hovered == null)
                return;

            ExecuteEvents.Execute(hovered, pointer, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(hovered, pointer, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(hovered, pointer, ExecuteEvents.pointerClickHandler);
            Debug.Log($"[GlobalVirtualCursor] Clicked: {hovered.name}");
        }

        private GameObject FindSelectable(GameObject obj)
        {
            Transform current = obj != null ? obj.transform : null;
            while (current != null)
            {
                if (current.GetComponent<Selectable>() != null)
                    return current.gameObject;

                current = current.parent;
            }

            return null;
        }

        private void ClearHover()
        {
            if (EventSystem.current == null)
            {
                hovered = null;
                return;
            }

            PointerEventData pointer = new PointerEventData(EventSystem.current)
            {
                position = new Vector2(
                    normalized.x * Screen.width,
                    normalized.y * Screen.height)
            };

            ClearHover(pointer);
        }

        private void ClearHover(PointerEventData pointer)
        {
            if (hovered == null)
                return;

            ExecuteEvents.Execute(hovered, pointer, ExecuteEvents.pointerExitHandler);
            hovered = null;
        }

        private void OnDisable()
        {
            ClearHover();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
