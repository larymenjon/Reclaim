using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Reclaim.UI.Bottom
{
    /// <summary>
    /// Handles a bottom HUD category button that toggles one submenu panel.
    /// </summary>
    public class BottomCategoryMenuController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Button categoryButton;
        [SerializeField] private GameObject submenuRoot;
        [SerializeField] private bool startOpened;
        [SerializeField] private bool closeWhenClickedAgain = true;
        [SerializeField] private BottomCategoryMenuController[] siblingMenus;

        public bool IsOpen => submenuRoot != null && submenuRoot.activeSelf;

        private void Awake()
        {
            if (categoryButton == null)
            {
                categoryButton = GetComponent<Button>();
            }

            if (categoryButton != null)
            {
                categoryButton.onClick.AddListener(ToggleMenu);
            }

            SetOpen(startOpened);
        }

        public void ToggleMenu()
        {
            if (submenuRoot == null)
            {
                return;
            }

            if (IsOpen && closeWhenClickedAgain)
            {
                SetOpen(false);
                return;
            }

            CloseSiblings();
            SetOpen(true);
        }

        public void SetOpen(bool value)
        {
            if (submenuRoot != null)
            {
                submenuRoot.SetActive(value);
            }
        }

        public void CloseSiblings()
        {
            if (siblingMenus == null)
            {
                return;
            }

            for (int i = 0; i < siblingMenus.Length; i++)
            {
                BottomCategoryMenuController sibling = siblingMenus[i];
                if (sibling == null || sibling == this)
                {
                    continue;
                }

                sibling.SetOpen(false);
            }
        }

        public void SetSiblings(List<BottomCategoryMenuController> menus)
        {
            if (menus == null || menus.Count == 0)
            {
                siblingMenus = null;
                return;
            }

            siblingMenus = menus.ToArray();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // Reserved for future visual state hooks.
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // Reserved for future visual state hooks.
        }
    }
}
