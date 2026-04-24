using System;
using UnityEngine;
using UnityEngine.UI;

namespace Reclaim.UI
{
    /// <summary>
    /// Controls selection visuals for a manually authored Canvas layout.
    /// </summary>
    public class NewGameSetupSelectionUI : MonoBehaviour
    {
        [Serializable]
        public class LeaderSlot
        {
            public Button button;
            public Image border;
            public Image portrait;
        }

        [Serializable]
        public class MapSlot
        {
            public Button button;
            public Image border;
            public Text label;
            public string displayName;
        }

        [Header("References")]
        [SerializeField] private NewGameSetupManager manager;
        [SerializeField] private CharacterSelectionUI characterSelection;

        [Header("Leader Slots (exactly 7)")]
        [SerializeField] private LeaderSlot[] leaders = new LeaderSlot[7];
        [SerializeField] private Sprite[] leaderPortraits = new Sprite[7];

        [Header("Map Slots (exactly 3)")]
        [SerializeField] private MapSlot[] maps = new MapSlot[3];

        [Header("Selection Colors")]
        [SerializeField] private Color normalBorderColor = new Color(0.45f, 0.45f, 0.45f, 1f);
        [SerializeField] private Color selectedBorderColor = new Color(0.4f, 0.95f, 0.55f, 1f);

        private bool buttonsBound;

        private void Start()
        {
            if (manager == null)
            {
                manager = GetComponent<NewGameSetupManager>();
            }
            
            if (manager == null)
            {
                manager = GetComponentInParent<NewGameSetupManager>();
            }

            if (manager == null)
            {
                Debug.LogWarning("NewGameSetupSelectionUI: missing NewGameSetupManager reference.");
                return;
            }
            
            if (characterSelection == null)
            {
                characterSelection = GetComponent<CharacterSelectionUI>();
            }
            
            if (characterSelection == null)
            {
                characterSelection = GetComponentInParent<CharacterSelectionUI>();
            }

            ApplyPortraits();
            ApplyMapLabels();
            if (!buttonsBound)
            {
                BindButtons();
                buttonsBound = true;
            }
            RefreshSelectionVisuals();
        }

        public void SelectLeaderFromUI(int index)
        {
            if (manager == null || index < 0 || index >= leaders.Length)
            {
                return;
            }

            // If CharacterSelectionUI is present, route selection through it so text/description stay synced.
            if (characterSelection != null)
            {
                characterSelection.SelectLeader(index);
                return;
            }

            manager.SetLeader(index);
            RefreshSelectionVisuals();
        }

        public void SelectMapFromUI(int index)
        {
            if (manager == null || index < 0 || index >= maps.Length)
            {
                return;
            }

            manager.SetMap(index);
            RefreshSelectionVisuals();
        }

        public void RefreshSelectionVisuals()
        {
            if (manager == null)
            {
                return;
            }

            int selectedLeader = manager.SelectedLeaderIndex;
            for (int i = 0; i < leaders.Length; i++)
            {
                if (leaders[i]?.border != null)
                {
                    leaders[i].border.color = i == selectedLeader ? selectedBorderColor : normalBorderColor;
                }
            }

            int selectedMap = manager.SelectedMapIndex;
            for (int i = 0; i < maps.Length; i++)
            {
                if (maps[i]?.border != null)
                {
                    maps[i].border.color = i == selectedMap ? selectedBorderColor : normalBorderColor;
                }
            }
        }

        private void BindButtons()
        {
            for (int i = 0; i < leaders.Length; i++)
            {
                if (leaders[i] == null)
                {
                    continue;
                }

                int captured = i;
                Button button = leaders[i].button;

                // If no explicit button was assigned, make the portrait (or border) clickable.
                if (button == null)
                {
                    button = EnsureClickableButton(leaders[i].portrait, leaders[i].border);
                    leaders[i].button = button;
                }

                if (button != null)
                {
                    button.onClick.AddListener(() => SelectLeaderFromUI(captured));
                }
            }

            for (int i = 0; i < maps.Length; i++)
            {
                if (maps[i] == null)
                {
                    continue;
                }

                int captured = i;
                Button button = maps[i].button;

                if (button == null)
                {
                    button = EnsureClickableButton(null, maps[i].border);
                    maps[i].button = button;
                }

                if (button != null)
                {
                    button.onClick.AddListener(() => SelectMapFromUI(captured));
                }
            }
        }

        private static Button EnsureClickableButton(Image preferredTarget, Image fallbackTarget)
        {
            Image targetImage = preferredTarget != null ? preferredTarget : fallbackTarget;
            if (targetImage == null)
            {
                return null;
            }

            Button button = targetImage.GetComponent<Button>();
            if (button == null)
            {
                button = targetImage.gameObject.AddComponent<Button>();
            }

            if (button.targetGraphic == null)
            {
                button.targetGraphic = targetImage;
            }

            return button;
        }

        private void ApplyPortraits()
        {
            for (int i = 0; i < leaders.Length; i++)
            {
                if (leaders[i]?.portrait == null)
                {
                    continue;
                }

                if (leaderPortraits != null && i < leaderPortraits.Length && leaderPortraits[i] != null)
                {
                    leaders[i].portrait.sprite = leaderPortraits[i];
                    leaders[i].portrait.preserveAspect = true;
                }
            }
        }

        private void ApplyMapLabels()
        {
            for (int i = 0; i < maps.Length; i++)
            {
                if (maps[i] == null || maps[i].label == null)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(maps[i].displayName))
                {
                    maps[i].label.text = maps[i].displayName;
                }
            }
        }
    }
}
