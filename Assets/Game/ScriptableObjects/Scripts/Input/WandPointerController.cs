// Assets/_Game/Scripts/UI/WandPointerController.cs
//
// GLOBAL WAND POINTER CONTROLLER
//
// Fungsi:
// - Menggerakkan virtual pointer menggunakan wand gyro
// - Hover UI
// - Click UI
// - Reusable untuk semua minigame
// - Compatible dengan CursorGlobal
//
// IMPORTANT:
// Script ini TIDAK mengatur hide/show pointer.
// Itu tugas CursorGlobal.
//
// Script ini hanya:
// - movement
// - hover
// - click
//

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WizardPunk
{
    public class WandPointerController : MonoBehaviour
    {
        // ============================================================
        // WAND INPUT
        // ============================================================

        [Header("── Wand Input ─────────────────────────")]

        [Tooltip("Wand yang digunakan sebagai pointer")]
        [SerializeField]
        private WandSerialReader wandReader;

        // ============================================================
        // POINTER VISUAL
        // ============================================================

        [Header("── Pointer Visual ─────────────────────")]

        [Tooltip("RectTransform pointer image")]
        [SerializeField]
        private RectTransform pointerRect;

        // ============================================================
        // UI RAYCAST
        // ============================================================

        [Header("── UI Raycast ─────────────────────────")]

        [SerializeField]
        private GraphicRaycaster graphicRaycaster;

        // ============================================================
        // MOVEMENT SETTINGS
        // ============================================================

        [Header("── Movement Settings ──────────────────")]

        [SerializeField]
        private float gyroDeadzone = 2.5f;

        [SerializeField]
        private float gyroSensitivity = 0.05f;

        [SerializeField]
        private float movementSmoothing = 12f;

        [SerializeField]
        private float borderPadding = 0.03f;

        // ============================================================
        // DEBUG
        // ============================================================

        [Header("── Debug ──────────────────────────────")]

        [SerializeField]
        private bool debugLogs = false;

        // ============================================================
        // STATE
        // ============================================================

        private Vector2 normalizedPosition =
            new Vector2(0.5f, 0.5f);

        private Vector2 targetPosition =
            new Vector2(0.5f, 0.5f);

        private GameObject hoveredObject;

        // ============================================================
        // UNITY
        // ============================================================

        private void Start()
        {
            // ========================================================
            // AUTO FIND WAND
            // ========================================================

            if (wandReader == null)
            {
                wandReader =
                    FindObjectOfType<WandSerialReader>();
            }

            // ========================================================
            // AUTO FIND RAYCASTER
            // ========================================================

            if (graphicRaycaster == null)
            {
                graphicRaycaster =
                    FindObjectOfType<GraphicRaycaster>();
            }

            // ========================================================
            // RECENTER
            // ========================================================

            RecenterPointer();
        }

        private void Update()
        {
            // ========================================================
            // VALIDATION
            // ========================================================

            if (wandReader == null) return;

            if (!wandReader.IsConnected) return;

            // ========================================================
            // MOVEMENT
            // ========================================================

            UpdatePointerMovement();

            // ========================================================
            // SMOOTHING
            // ========================================================

            normalizedPosition =
                Vector2.Lerp(
                    normalizedPosition,
                    targetPosition,
                    Time.deltaTime * movementSmoothing
                );

            // ========================================================
            // UPDATE VISUAL
            // ========================================================

            UpdatePointerVisual();

            // ========================================================
            // UI INTERACTION
            // ========================================================

            HandleUIInteraction();
        }

        // ============================================================
        // MOVEMENT
        // ============================================================

        private void UpdatePointerMovement()
        {
            Vector3 gyro =
                wandReader.GyroVelocity;

            // ========================================================
            // AXIS MAPPING
            // ========================================================

            float inputX =
                ApplyDeadzone(gyro.z);

            float inputY =
                ApplyDeadzone(gyro.x);

            // ========================================================
            // DELTA
            // ========================================================

            float deltaX =
                inputX *
                -gyroSensitivity *
                Time.deltaTime;

            float deltaY =
                inputY *
                gyroSensitivity *
                Time.deltaTime;

            // ========================================================
            // CLAMP
            // ========================================================

            float b = borderPadding;

            targetPosition.x =
                Mathf.Clamp(
                    targetPosition.x + deltaX,
                    b,
                    1f - b
                );

            targetPosition.y =
                Mathf.Clamp(
                    targetPosition.y + deltaY,
                    b,
                    1f - b
                );

            // ========================================================
            // RECENTER
            // ========================================================

            if (wandReader.ConsumeZeroed())
            {
                RecenterPointer();
            }
        }

        // ============================================================
        // POINTER VISUAL
        // ============================================================

        private void UpdatePointerVisual()
        {
            if (pointerRect == null) return;

            pointerRect.position =
                new Vector3(
                    normalizedPosition.x * Screen.width,
                    normalizedPosition.y * Screen.height,
                    0f
                );
        }

        // ============================================================
        // UI INTERACTION
        // ============================================================

        private void HandleUIInteraction()
        {
            if (graphicRaycaster == null) return;

            // ========================================================
            // POINTER DATA
            // ========================================================

            Vector2 screenPosition =
                new Vector2(
                    normalizedPosition.x * Screen.width,
                    normalizedPosition.y * Screen.height
                );

            PointerEventData pointerData =
                new PointerEventData(EventSystem.current)
                {
                    position = screenPosition
                };

            // ========================================================
            // RAYCAST
            // ========================================================

            List<RaycastResult> results =
                new List<RaycastResult>();

            graphicRaycaster.Raycast(
                pointerData,
                results);

            GameObject hitObject = null;

            foreach (var result in results)
            {
                GameObject selectable =
                    FindSelectable(result.gameObject);

                if (selectable != null)
                {
                    hitObject = selectable;
                    break;
                }
            }

            // ========================================================
            // HOVER
            // ========================================================

            if (hitObject != hoveredObject)
            {
                // EXIT
                if (hoveredObject != null)
                {
                    ExecuteEvents.Execute(
                        hoveredObject,
                        pointerData,
                        ExecuteEvents.pointerExitHandler
                    );
                }

                // ENTER
                if (hitObject != null)
                {
                    ExecuteEvents.Execute(
                        hitObject,
                        pointerData,
                        ExecuteEvents.pointerEnterHandler
                    );
                }

                hoveredObject = hitObject;
            }

            // ========================================================
            // CLICK
            // ========================================================

            if (wandReader.ConsumeAction())
            {
                if (hoveredObject != null)
                {
                    ExecuteEvents.Execute(
                        hoveredObject,
                        pointerData,
                        ExecuteEvents.pointerDownHandler
                    );

                    ExecuteEvents.Execute(
                        hoveredObject,
                        pointerData,
                        ExecuteEvents.pointerUpHandler
                    );

                    ExecuteEvents.Execute(
                        hoveredObject,
                        pointerData,
                        ExecuteEvents.pointerClickHandler
                    );

                    if (debugLogs)
                    {
                        Debug.Log(
                            $"[Pointer] Clicked: {hoveredObject.name}");
                    }
                }
            }
        }

        // ============================================================
        // DEADZONE
        // ============================================================

        private float ApplyDeadzone(float value)
        {
            if (value > gyroDeadzone)
            {
                return value - gyroDeadzone;
            }

            if (value < -gyroDeadzone)
            {
                return value + gyroDeadzone;
            }

            return 0f;
        }

        // ============================================================
        // FIND SELECTABLE
        // ============================================================

        private GameObject FindSelectable(GameObject obj)
        {
            Transform t = obj.transform;

            while (t != null)
            {
                if (t.GetComponent<Selectable>() != null)
                {
                    return t.gameObject;
                }

                t = t.parent;
            }

            return null;
        }

        // ============================================================
        // RECENTER
        // ============================================================

        public void RecenterPointer()
        {
            targetPosition =
                new Vector2(0.5f, 0.5f);

            normalizedPosition =
                new Vector2(0.5f, 0.5f);
        }
    }
}