using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using Reclaim.Data;

namespace Reclaim.UI
{
    public class CharacterSelectionUI : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private LeaderData[] allLeaders; 
        [SerializeField] private NewGameSetupManager manager;
        [SerializeField] private NewGameSetupSelectionUI visualUI; // Referência ao script acima

        [Header("UI Text References")]
        [SerializeField] private TextMeshProUGUI nameText;        
        [SerializeField] private TextMeshProUGUI subDescriptionText; 
        [SerializeField] private TextMeshProUGUI descriptionText;    
        [SerializeField] private TextMeshProUGUI phraseText;        
        [SerializeField] private Image bigDisplayPortrait;

        [Header("UI Icon")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Sprite[] icon = new Sprite[7];

        private int currentIndex = 0;

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

            if (allLeaders == null || allLeaders.Length == 0)
            {
                Debug.LogWarning("CharacterSelectionUI: allLeaders is empty. Assign LeaderData assets in the Inspector.");
                return;
            }

            int startIndex = manager != null ? manager.SelectedLeaderIndex : 0;
            currentIndex = Mathf.Clamp(startIndex, 0, allLeaders.Length - 1);
            SelectLeader(currentIndex);
        }

        private void Update()
        {
            if (allLeaders == null || allLeaders.Length == 0)
            {
                return;
            }

            if (Keyboard.current != null && Keyboard.current.rightArrowKey.wasPressedThisFrame) ChangeLeader(1);
            if (Keyboard.current != null && Keyboard.current.leftArrowKey.wasPressedThisFrame) ChangeLeader(-1);

            if (manager != null && manager.SelectedLeaderIndex != currentIndex)
            {
                currentIndex = Mathf.Clamp(manager.SelectedLeaderIndex, 0, allLeaders.Length - 1);
                UpdateUI();
            }
        }

        public void ChangeLeader(int direction)
        {
            if (allLeaders == null || allLeaders.Length == 0)
            {
                return;
            }

            currentIndex += direction;
            if (currentIndex >= allLeaders.Length) currentIndex = 0;
            if (currentIndex < 0) currentIndex = allLeaders.Length - 1;

            SelectLeader(currentIndex);
        }

        public void SelectLeader(int index)
        {
            if (allLeaders == null || allLeaders.Length == 0)
            {
                return;
            }

            currentIndex = Mathf.Clamp(index, 0, allLeaders.Length - 1);
            if (manager != null) manager.SetLeader(currentIndex);
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (allLeaders == null || allLeaders.Length == 0) return;

            LeaderData selected = allLeaders[currentIndex];
            if(nameText) nameText.text = selected.leaderName;
            if(subDescriptionText) subDescriptionText.text = selected.subDescription;
            if(descriptionText) descriptionText.text = selected.description;
            if(phraseText) phraseText.text = $"\"{selected.catchphrase}\"";
            if(bigDisplayPortrait) bigDisplayPortrait.sprite = selected.portrait;
            if(iconImage)
            {
                Sprite selectedIcon = (icon != null && currentIndex < icon.Length) ? icon[currentIndex] : null;
                iconImage.sprite = selectedIcon;
                iconImage.enabled = selectedIcon != null;
            }
            
            // Chama o outro script para atualizar as bordas
            if (visualUI != null) visualUI.RefreshSelectionVisuals();
        }
    }
}
