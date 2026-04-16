using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reclaim.UI
{
    public class CharacterSelectionUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform characterContainer;
        [SerializeField] private GameObject characterButtonPrefab;
        [SerializeField] private GameObject characterInfoPanel;
        
        [Header("Info Panel References")]
        [SerializeField] private TextMeshProUGUI characterNameText;
        [SerializeField] private TextMeshProUGUI characterDescriptionText;
        [SerializeField] private TextMeshProUGUI characterQuoteText;
        [SerializeField] private TextMeshProUGUI specializationText;
        [SerializeField] private TextMeshProUGUI passiveNameText;
        [SerializeField] private TextMeshProUGUI passiveDescriptionText;
        [SerializeField] private TextMeshProUGUI activeNameText;
        [SerializeField] private TextMeshProUGUI activeDescriptionText;
        [SerializeField] private TextMeshProUGUI eventNameText;
        [SerializeField] private TextMeshProUGUI eventDescriptionText;
        [SerializeField] private Image characterPortrait;
        
        [Header("Stats Display")]
        [SerializeField] private Transform statsContainer;
        [SerializeField] private GameObject statItemPrefab;
        
        [Header("Manager References")]
        [SerializeField] private CharacterManager characterManager;
        [SerializeField] private NewGameSetupManager setupManager;

        private int selectedCharacterIndex = -1;

        private void Start()
        {
            InitializeCharacterSelection();
        }

        private void InitializeCharacterSelection()
        {
            if (characterManager == null)
            {
                characterManager = FindObjectOfType<CharacterManager>();
            }
            
            if (setupManager == null)
            {
                setupManager = FindObjectOfType<NewGameSetupManager>();
            }

            if (characterManager != null)
            {
                CreateCharacterButtons();
                SelectCharacter(0); // Seleciona o primeiro por padrão
            }
        }

        private void CreateCharacterButtons()
        {
            // Limpa botões existentes
            foreach (Transform child in characterContainer)
            {
                Destroy(child.gameObject);
            }

            int characterCount = characterManager.GetCharacterCount();
            
            for (int i = 0; i < characterCount; i++)
            {
                var characterData = characterManager.GetCharacter(i);
                if (characterData != null)
                {
                    CreateCharacterButton(characterData, i);
                }
            }
        }

        private void CreateCharacterButton(CharacterManager.CharacterData characterData, int index)
        {
            var buttonGO = Instantiate(characterButtonPrefab, characterContainer);
            var button = buttonGO.GetComponent<Button>();
            var image = buttonGO.GetComponent<Image>();
            
            if (characterData.portrait != null && image != null)
            {
                image.sprite = characterData.portrait;
            }

            button.onClick.AddListener(() => SelectCharacter(index));
        }

        public void SelectCharacter(int index)
        {
            selectedCharacterIndex = index;
            var characterData = characterManager.GetCharacter(index);
            
            if (characterData != null)
            {
                UpdateCharacterInfo(characterData);
                UpdateStatsDisplay(characterData);
                
                // Salva a escolha no setup manager (usando o índice como "leader index")
                if (setupManager != null)
                {
                    setupManager.SetLeader(index);
                }
            }
        }

        private void UpdateCharacterInfo(CharacterManager.CharacterData characterData)
        {
            if (characterInfoPanel != null) characterInfoPanel.SetActive(true);
            
            if (characterNameText != null) characterNameText.text = characterData.characterName;
            if (characterDescriptionText != null) characterDescriptionText.text = characterData.description;
            if (characterQuoteText != null) characterQuoteText.text = $"\"{characterData.quote}\"";
            if (specializationText != null) specializationText.text = GetSpecializationText(characterData.specialization);
            if (passiveNameText != null) passiveNameText.text = characterData.passiveName;
            if (passiveDescriptionText != null) passiveDescriptionText.text = characterData.passiveDescription;
            if (activeNameText != null) activeNameText.text = characterData.activeName;
            if (activeDescriptionText != null) activeDescriptionText.text = characterData.activeDescription;
            if (eventNameText != null) eventNameText.text = characterData.eventName;
            if (eventDescriptionText != null) eventDescriptionText.text = characterData.eventDescription;
            if (characterPortrait != null && characterData.portrait != null) characterPortrait.sprite = characterData.portrait;
        }

        private void UpdateStatsDisplay(CharacterManager.CharacterData characterData)
        {
            // Limpa stats existentes
            foreach (Transform child in statsContainer)
            {
                Destroy(child.gameObject);
            }

            // Cria itens de stats baseados nos buffs e debuffs
            AddStatItem("Velocidade de Construção", characterData.constructionSpeedBonus, "🏗");
            AddStatItem("Custo de Materiais", characterData.materialCostBonus, "💰");
            AddStatItem("Defesa", characterData.defenseBonus, "🛡");
            AddStatItem("Felicidade", characterData.happinessBonus, "😊");
            AddStatItem("Produção de Comida", characterData.foodProductionBonus, "🌾");
            AddStatItem("Eficiência Médica", characterData.medicalEfficiencyBonus, "🩺");
            AddStatItem("Velocidade de Exploração", characterData.explorationSpeedBonus, "🧭");
            AddStatItem("Recursos", characterData.resourceBonus, "🔧");
            AddStatItem("Chance de Revolta", characterData.revoltChanceBonus, "⚠️");
            AddStatItem("Consumo de Recursos", characterData.resourceConsumptionBonus, "🔥");
            AddStatItem("Expansão", characterData.expansionBonus, "🏗");
            AddStatItem("Segurança", characterData.securityBonus, "🔒");
            AddStatItem("Produtividade", characterData.productivityBonus, "⚡");
            AddStatItem("Confiança", characterData.trustBonus, "🤝");
        }

        private void AddStatItem(string statName, float value, string icon)
        {
            if (Mathf.Approximately(value, 0f)) return; // Não mostra stats neutros

            var statItem = Instantiate(statItemPrefab, statsContainer);
            var text = statItem.GetComponent<TextMeshProUGUI>();
            
            if (text != null)
            {
                string sign = value > 0 ? "+" : "";
                string color = value > 0 ? "#4CAF50" : "#F44336"; // Verde para buffs, vermelho para debuffs
                text.text = $"{icon} {statName}: <color={color}>{sign}{value}%</color>";
            }
        }

        private string GetSpecializationText(CharacterManager.CharacterSpecialization specialization)
        {
            switch (specialization)
            {
                case CharacterManager.CharacterSpecialization.Construction: return "🏗 Construção";
                case CharacterManager.CharacterSpecialization.Defense: return "🛡 Defesa";
                case CharacterManager.CharacterSpecialization.Food: return "🌾 Comida";
                case CharacterManager.CharacterSpecialization.Health: return "🩺 Saúde";
                case CharacterManager.CharacterSpecialization.Exploration: return "🧭 Exploração";
                case CharacterManager.CharacterSpecialization.Resources: return "🔧 Recursos";
                case CharacterManager.CharacterSpecialization.Morale: return "❤ Moral";
                default: return "❓ Desconhecido";
            }
        }

        public int GetSelectedCharacterIndex()
        {
            return selectedCharacterIndex;
        }

        public CharacterManager.CharacterData GetSelectedCharacterData()
        {
            return characterManager?.GetCharacter(selectedCharacterIndex);
        }
    }
}