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

        [Header("Leader Slots (exactly 7)")]
        [SerializeField] private LeaderSlot[] leaders = new LeaderSlot[7];
        [SerializeField] private Sprite[] leaderPortraits = new Sprite[7];

        [Header("Map Slots (exactly 3)")]
        [SerializeField] private MapSlot[] maps = new MapSlot[3];

        [Header("Selection Colors")]
        [SerializeField] private Color normalBorderColor = new(0.45f, 0.45f, 0.45f, 1f);
        [SerializeField] private Color selectedBorderColor = new(0.4f, 0.95f, 0.55f, 1f);

        private void Start()
        {
            if (manager == null)
            {
                manager = GetComponent<NewGameSetupManager>();
            }

            if (manager == null)
            {
                Debug.LogWarning("NewGameSetupSelectionUI: missing NewGameSetupManager reference.");
                return;
            }

            ApplyPortraits();
            ApplyMapLabels();
            BindButtons();
            RefreshSelectionVisuals();
        }

        public void SelectLeaderFromUI(int index)
        {
            manager.SetLeader(index);
            RefreshSelectionVisuals();
        }

        public void SelectMapFromUI(int index)
        {
            manager.SetMap(index);
            RefreshSelectionVisuals();
        }

        public void RefreshSelectionVisuals()
        {
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
                if (leaders[i]?.button == null)
                {
                    continue;
                }

                int captured = i;
                leaders[i].button.onClick.AddListener(() => SelectLeaderFromUI(captured));
            }

            for (int i = 0; i < maps.Length; i++)
            {
                if (maps[i]?.button == null)
                {
                    continue;
                }

                int captured = i;
                maps[i].button.onClick.AddListener(() => SelectMapFromUI(captured));
            }
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
