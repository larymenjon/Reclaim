using UnityEngine;
using Reclaim.UI.Bottom;

namespace Reclaim.UI
{
    /// <summary>
    /// Auto-disables all BottomHudHoverLift components in the scene.
    /// Attach this to any GameObject - it will remove hover animation from ALL buttons.
    /// </summary>
    public class DisableAllHoverLift : MonoBehaviour
    {
        private void Awake()
        {
            BottomHudHoverLift[] allHoverLifts =
                FindObjectsByType<BottomHudHoverLift>(FindObjectsSortMode.None);

            int disabledCount = 0;
            foreach (BottomHudHoverLift hoverLift in allHoverLifts)
            {
                hoverLift.enabled = false;
                disabledCount++;
            }

            Debug.Log($"Disabled {disabledCount} BottomHudHoverLift components.");
            Destroy(this);
        }
    }
}
