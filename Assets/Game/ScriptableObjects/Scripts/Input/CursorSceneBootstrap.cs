using UnityEngine;

namespace WizardPunk
{
    public class CursorSceneBootstrap : MonoBehaviour
    {
        [SerializeField] private bool showCursorOnStart = true;
        [SerializeField] private bool recenterOnStart = false;

        private void Start()
        {
            if (GlobalVirtualCursor.Instance == null)
                return;

            if (recenterOnStart)
                GlobalVirtualCursor.Instance.Recenter();

            GlobalVirtualCursor.Instance.SetVisible(showCursorOnStart);
        }
    }
}
