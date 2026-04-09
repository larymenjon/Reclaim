using System.Collections.Generic;
using UnityEngine;

namespace Reclaim.UI.Bottom
{
    /// <summary>
    /// Optional helper that wires sibling category menus automatically.
    /// </summary>
    public class BottomCategoryMenuGroup : MonoBehaviour
    {
        [SerializeField] private List<BottomCategoryMenuController> menus = new List<BottomCategoryMenuController>();

        private void Awake()
        {
            if (menus.Count == 0)
            {
                GetComponentsInChildren(true, menus);
            }

            for (int i = 0; i < menus.Count; i++)
            {
                if (menus[i] == null)
                {
                    continue;
                }

                menus[i].SetSiblings(menus);
            }
        }
    }
}
