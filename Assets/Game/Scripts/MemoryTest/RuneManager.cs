// Assets/_Game/Scripts/MemoryTest/RuneManager.cs
using System.Collections;
using UnityEngine;

namespace WizardPunk.MemoryTest
{
    public class RuneManager : MonoBehaviour
    {
        public static RuneManager Instance { get; private set; }

        [Header("── Rune Player 1 (Kiri) ──")]
        // Hapus batasan angka, biarkan Unity Inspector yang menentukan jumlahnya
        [SerializeField] private RuneObject[] runesP1;

        [Header("── Rune Player 2 (Kanan) ──")]
        [SerializeField] private RuneObject[] runesP2;

        public RuneObject[] RunesP1 => runesP1;
        public RuneObject[] RunesP2 => runesP2;

        // Arah TETAP 4 (Atas, Bawah, Kiri, Kanan)
        private static readonly WandDirection[] ALL_DIRS = {
            WandDirection.Up, WandDirection.Down, WandDirection.Left, WandDirection.Right
        };

        void Awake() => Instance = this;

        public void SetupRound()
        {
            // Random.Range(0, 4) JANGAN diubah karena ini untuk mengambil 4 arah di atas
            foreach (var r in runesP1) if (r != null) r.Initialize(ALL_DIRS[Random.Range(0, 4)]);
            foreach (var r in runesP2) if (r != null) r.Initialize(ALL_DIRS[Random.Range(0, 4)]);
        }

        public IEnumerator PlaySequence(float delay, float memoTime)
        {
            // Ambil jumlah rune terbanyak (sekarang 5)
            int maxRunes = Mathf.Max(runesP1.Length, runesP2.Length);

            for (int i = 0; i < maxRunes; i++)
            {
                if (i < runesP1.Length && runesP1[i] != null) runesP1[i].ShowArrow();
                if (i < runesP2.Length && runesP2[i] != null) runesP2[i].ShowArrow();
                yield return new WaitForSeconds(delay);
            }

            yield return new WaitForSeconds(memoTime);

            // Sembunyikan semua arrow
            foreach (var r in runesP1) if (r != null) r.HideArrow();
            foreach (var r in runesP2) if (r != null) r.HideArrow();
        }

        public void ResetAll()
        {
            foreach (var r in runesP1) if (r != null) r.SetIdle();
            foreach (var r in runesP2) if (r != null) r.SetIdle();
        }
    }
}