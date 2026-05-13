// Assets/_Game/Scripts/MemoryTest/RuneManager.cs
using System.Collections;
using UnityEngine;

namespace WizardPunk.MemoryTest
{
    public class RuneManager : MonoBehaviour
    {
        public static RuneManager Instance { get; private set; }

        [Header("── Rune Player 1 (Kiri) ──")]
        [SerializeField] private RuneObject[] runesP1;

        [Header("── Rune Player 2 (Kanan) ──")]
        [SerializeField] private RuneObject[] runesP2;

        public RuneObject[] RunesP1 => runesP1;
        public RuneObject[] RunesP2 => runesP2;

        private int currentActiveRunes = 3; // Default

        private static readonly WandDirection[] ALL_DIRS = {
            WandDirection.Up, WandDirection.Down, WandDirection.Left, WandDirection.Right
        };

        void Awake() => Instance = this;

        // Modifikasi untuk menyembunyikan/menampilkan rune sesuai difficulty
        public void SetupRound(int activeCount)
        {
            currentActiveRunes = activeCount;

            for (int i = 0; i < runesP1.Length; i++)
            {
                if (runesP1[i] != null)
                {
                    bool isActive = i < currentActiveRunes;
                    runesP1[i].gameObject.SetActive(isActive);
                    if (isActive) runesP1[i].Initialize(ALL_DIRS[Random.Range(0, 4)]);
                }
            }

            for (int i = 0; i < runesP2.Length; i++)
            {
                if (runesP2[i] != null)
                {
                    bool isActive = i < currentActiveRunes;
                    runesP2[i].gameObject.SetActive(isActive);
                    if (isActive) runesP2[i].Initialize(ALL_DIRS[Random.Range(0, 4)]);
                }
            }
        }

        public IEnumerator PlaySequence(float delay, float memoTime)
        {
            for (int i = 0; i < currentActiveRunes; i++)
            {
                if (i < runesP1.Length && runesP1[i] != null) runesP1[i].ShowArrow();
                if (i < runesP2.Length && runesP2[i] != null) runesP2[i].ShowArrow();
                yield return new WaitForSeconds(delay);
            }

            yield return new WaitForSeconds(memoTime);

            for (int i = 0; i < currentActiveRunes; i++)
            {
                if (i < runesP1.Length && runesP1[i] != null) runesP1[i].HideArrow();
                if (i < runesP2.Length && runesP2[i] != null) runesP2[i].HideArrow();
            }
        }

        public void ResetAll()
        {
            for (int i = 0; i < currentActiveRunes; i++)
            {
                if (i < runesP1.Length && runesP1[i] != null) runesP1[i].SetIdle();
                if (i < runesP2.Length && runesP2[i] != null) runesP2[i].SetIdle();
            }
        }
    }
}