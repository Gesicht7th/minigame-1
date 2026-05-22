// Assets/_Game/Scripts/ReflexShowdown/CharacterVisual.cs
// Animasi karakter cylinder: idle, wand rendah, wand naik, hit

using System.Collections;
using UnityEngine;

namespace WizardPunk.Reflex
{
    public class CharacterVisual : MonoBehaviour
    {
        [Header("── Character Parts ─────────────────────")]
        [SerializeField] private Transform body;       // Cylinder badan
        [SerializeField] private Transform head;       // Sphere kepala
        [SerializeField] private Transform wandArm;   // Cylinder lengan
        [SerializeField] private Transform wandTip;    // Sphere ujung wand (emissive)
        [SerializeField] private Transform hat;        // Cylinder topi (wizard hat)

        [Header("── Wand Positions ──────────────────────")]
        [SerializeField] private Vector3 wandLowLocalRot = new Vector3(60f, 0f, 0f);
        [SerializeField] private Vector3 wandRaisedLocalRot = new Vector3(-30f, 0f, 0f);

        [Header("── Colors ──────────────────────────────")]
        [SerializeField] private Color wandTipIdleColor = new Color(0.5f, 0.2f, 0.8f);
        [SerializeField] private Color wandTipFireColor = new Color(1.0f, 0.8f, 0.0f);
        [SerializeField] private Color wandTipFalseColor = new Color(1.0f, 0.2f, 0.1f);
        [SerializeField] private Color bodyReadyColor = new Color(0.2f, 0.2f, 0.4f);
        [SerializeField] private Color bodyWinColor = new Color(0.1f, 0.8f, 0.2f);
        [SerializeField] private Color bodyLoseColor = new Color(0.6f, 0.1f, 0.1f);

        [Header("── Animation Speed ─────────────────────")]
        [SerializeField] private float wandRotateSpeed = 10f;
        [SerializeField] private float bodyBobSpeed = 1.2f;
        [SerializeField] private float bodyBobAmount = 0.04f;

        [Header("── Animator ─────────────────────────────")]
        [SerializeField] private Animator animator;
        [SerializeField] private float dieTransitionDelay = 0.4f;
        [SerializeField] private float winTransitionDelay = 0.6f;

        private Quaternion targetWandRot;
        private Material tipMat, bodyMat;
        private float bobTimer;
        private bool isDead = false;

        void Awake()
        {
            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            if (wandTip != null)
                tipMat = wandTip.GetComponent<Renderer>()?.material;
            if (body != null)
                bodyMat = body.GetComponent<Renderer>()?.material;

            targetWandRot = Quaternion.Euler(wandLowLocalRot);
        }

        void Update()
        {
            // Smooth wand rotation
            if (wandArm != null)
                wandArm.localRotation = Quaternion.Lerp(
                    wandArm.localRotation, targetWandRot,
                    Time.deltaTime * wandRotateSpeed);

            // Body idle bob
            bobTimer += Time.deltaTime * bodyBobSpeed;
            if (body != null)
            {
                Vector3 pos = body.localPosition;
                pos.y = Mathf.Sin(bobTimer) * bodyBobAmount;
                body.localPosition = pos;
            }
        }

        // ── State methods ─────────────────────────────
        public void SetWandLow()
        {
            targetWandRot = Quaternion.Euler(wandLowLocalRot);
            SetTipColor(wandTipIdleColor, 1f);
        }

        public void SetWandRaised()
        {
            targetWandRot = Quaternion.Euler(wandRaisedLocalRot);
            SetTipColor(wandTipFireColor, 3f);
        }

        public void PlayFireEffect()
        {
            StartCoroutine(FireFlash());
        }

        public void PlayFalseStartEffect()
        {
            SetTipColor(wandTipFalseColor, 2f);
            StartCoroutine(Shake(body, 0.3f));
            
            if (animator != null)
            {
                animator.SetTrigger("DoDie");
                isDead = true;
            }
        }

        public void SetWinPose(RpsType action = RpsType.None)
        {
            targetWandRot = Quaternion.Euler(wandRaisedLocalRot);
            SetBodyColor(bodyWinColor);
            SetTipColor(wandTipFireColor, 4f);

            if (animator != null)
            {
                if (action != RpsType.None)
                {
                    TriggerActionAnim(action);
                    StartCoroutine(TriggerAfterDelay("DoWin", winTransitionDelay));
                }
                else
                {
                    animator.SetTrigger("DoWin");
                }
            }
        }

        public void SetLosePose(RpsType action = RpsType.None)
        {
            targetWandRot = Quaternion.Euler(wandLowLocalRot);
            SetBodyColor(bodyLoseColor);
            SetTipColor(wandTipIdleColor, 0.5f);
            isDead = true;

            if (animator != null)
            {
                if (action != RpsType.None)
                {
                    TriggerActionAnim(action);
                    StartCoroutine(TriggerAfterDelay("DoDie", dieTransitionDelay));
                }
                else
                {
                    animator.SetTrigger("DoDie");
                }
            }
        }

        public void SetDrawPose(RpsType action)
        {
            if (animator != null && action != RpsType.None)
            {
                TriggerActionAnim(action);
            }
        }

        public void ResetPose()
        {
            // Hentikan semua coroutine (seperti delay trigger Win/Die) dari ronde sebelumnya agar tidak bocor
            StopAllCoroutines();

            SetWandLow();
            SetBodyColor(bodyReadyColor);

            if (animator != null)
            {
                // Wajib reset SEMUA trigger. Jika tidak, trigger yang tidak terpakai di ronde sebelumnya
                // (misal DoBack pada karakter yang menang) akan tersangkut dan merusak animasi di ronde berikutnya.
                animator.ResetTrigger("DoWin");
                animator.ResetTrigger("DoDie");
                animator.ResetTrigger("DoAttack");
                animator.ResetTrigger("DoBlock");
                animator.ResetTrigger("DoParry");
                animator.ResetTrigger("DoBack");

                if (isDead)
                {
                    // Mainkan animasi Back hanya untuk player yang kalah di ronde sebelumnya
                    animator.SetTrigger("DoBack");
                    isDead = false;
                }
                // Untuk player yang menang, kita asumsikan animasinya sudah otomatis kembali ke Idle 
                // dari state Win berkat transisi Has Exit Time di Animator Controller Anda.
            }
        }

        // ── Helpers ───────────────────────────────────
        private void TriggerActionAnim(RpsType action)
        {
            if (animator == null) return;
            switch (action)
            {
                case RpsType.Rock:
                    animator.SetTrigger("DoBlock"); // Asumsi Rock = Block
                    break;
                case RpsType.Paper:
                    animator.SetTrigger("DoParry"); // Asumsi Paper = Parry
                    break;
                case RpsType.Scissors:
                    animator.SetTrigger("DoAttack"); // Asumsi Scissors = Attack
                    break;
            }
        }

        private IEnumerator TriggerAfterDelay(string triggerName, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (animator != null)
            {
                animator.SetTrigger(triggerName);
            }
        }

        private void SetTipColor(Color c, float intensity)
        {
            if (tipMat == null) return;
            tipMat.color = c;
            if (tipMat.HasProperty("_EmissionColor"))
            {
                tipMat.EnableKeyword("_EMISSION");
                tipMat.SetColor("_EmissionColor", c * intensity);
            }
        }

        private void SetBodyColor(Color c)
        {
            if (bodyMat == null) return;
            bodyMat.color = c;
        }

        private IEnumerator FireFlash()
        {
            SetTipColor(Color.white, 5f);
            yield return new WaitForSeconds(0.1f);
            SetTipColor(wandTipFireColor, 3f);
        }

        private IEnumerator Shake(Transform t, float duration)
        {
            if (t == null) yield break;
            Vector3 origin = t.localPosition;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                t.localPosition = origin + (Vector3)UnityEngine.Random.insideUnitCircle * 0.05f;
                yield return null;
            }
            t.localPosition = origin;
        }
    }
}