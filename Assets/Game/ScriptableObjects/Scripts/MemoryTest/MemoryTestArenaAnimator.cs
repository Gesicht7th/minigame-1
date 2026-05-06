// Assets/_Game/Scripts/MemoryTest/MemoryTestArenaAnimator.cs
// Animasi objek 3D di arena (rune stones berputar, lampu berkedip)

using System.Collections;
using UnityEngine;

namespace WizardPunk.MemoryTest
{
    public class MemoryTestArenaAnimator : MonoBehaviour
    {
        [Header("── Rune Stones ─────────────────────────")]
        [SerializeField] private Transform[] runeStones;
        [SerializeField] private float idleRotateSpeed = 20f;
        [SerializeField] private float activeRotateSpeed = 60f;

        [Header("── Floating Objects ────────────────────")]
        [SerializeField] private Transform[] floatingObjects;
        [SerializeField] private float floatSpeed = 0.8f;
        [SerializeField] private float floatRange = 0.2f;

        [Header("── Ambient Light ────────────────────────")]
        [SerializeField] private Light arenaLight;
        [SerializeField] private Color idleColor = new Color(0.1f, 0.05f, 0.2f);
        [SerializeField] private Color activeColor = new Color(0.25f, 0.1f, 0.5f);

        private float currentRotateSpeed;
        private bool isActive;

        void Start()
        {
            currentRotateSpeed = idleRotateSpeed;
        }

        void Update()
        {
            // Putar rune stones
            foreach (var s in runeStones)
                if (s != null) s.Rotate(Vector3.up, currentRotateSpeed * Time.deltaTime);

            // Float objects
            for (int i = 0; i < floatingObjects.Length; i++)
            {
                if (floatingObjects[i] == null) continue;
                float y = Mathf.Sin(Time.time * floatSpeed + i * 1.3f) * floatRange;
                Vector3 p = floatingObjects[i].localPosition;
                p.y = y;
                floatingObjects[i].localPosition = p;
            }
        }

        public void SetActiveState()
        {
            isActive = true;
            currentRotateSpeed = activeRotateSpeed;
            StartCoroutine(LerpLight(activeColor, 0.5f));
        }

        public void SetIdleState()
        {
            isActive = false;
            currentRotateSpeed = idleRotateSpeed;
            StartCoroutine(LerpLight(idleColor, 1f));
        }

        private IEnumerator LerpLight(Color target, float dur)
        {
            if (arenaLight == null) yield break;
            Color start = arenaLight.color;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / dur;
                arenaLight.color = Color.Lerp(start, target, t);
                yield return null;
            }
        }
    }
}