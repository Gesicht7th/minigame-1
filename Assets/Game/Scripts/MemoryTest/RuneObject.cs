// Assets/_Game/Scripts/MemoryTest/RuneObject.cs
using System.Collections;
using UnityEngine;
using TMPro;

namespace WizardPunk.MemoryTest
{
    public class RuneObject : MonoBehaviour
    {
        [SerializeField] private Renderer boxRenderer;
        [SerializeField] private TextMeshPro arrowLabel;

        [Header("Colors")]
        public Color colorIdle = Color.gray;
        public Color colorShow = Color.cyan;
        public Color colorCorrect = Color.green;
        public Color colorWrong = Color.red;

        public WandDirection AssignedDirection { get; private set; }
        private Material mat;

        void Awake()
        {
            if (boxRenderer != null) mat = boxRenderer.material;
            SetIdle();
        }

        public void Initialize(WandDirection dir)
        {
            AssignedDirection = dir;
            SetIdle();
        }

        public void SetIdle()
        {
            if (arrowLabel != null) arrowLabel.enabled = false;
            if (mat != null) mat.color = colorIdle;
            transform.localRotation = Quaternion.identity;
        }

        public void ShowArrow()
        {
            if (arrowLabel != null)
            {
                arrowLabel.text = GetArrowText(AssignedDirection);
                arrowLabel.enabled = true;
            }
            if (mat != null) mat.color = colorShow;
        }

        public void HideArrow()
        {
            if (arrowLabel != null) arrowLabel.enabled = false;
            if (mat != null) mat.color = colorIdle;
            StartCoroutine(FlipAnimation());
        }

        public void ShowResult(bool correct)
        {
            if (arrowLabel != null)
            {
                arrowLabel.text = correct ? "O" : "X";
                arrowLabel.enabled = true;
            }
            if (mat != null) mat.color = correct ? colorCorrect : colorWrong;
        }

        private IEnumerator FlipAnimation()
        {
            float elapsed = 0f;
            float duration = 0.3f;
            Quaternion start = transform.localRotation;
            Quaternion target = Quaternion.Euler(0f, 180f, 0f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.localRotation = Quaternion.Lerp(start, target, elapsed / duration);
                yield return null;
            }
        }

        private string GetArrowText(WandDirection dir) => dir switch
        {
            WandDirection.Up => "↑",
            WandDirection.Down => "↓",
            WandDirection.Left => "←",
            WandDirection.Right => "→",
            _ => "?"
        };
    }
}