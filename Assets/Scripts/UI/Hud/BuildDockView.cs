using System.Collections.Generic;
using Reclaim.Construction;
using Reclaim.Economy;
using Reclaim.Gameplay;
using Reclaim.Simulation;
using UnityEngine;

namespace Reclaim.UI.Hud
{
    /// <summary>
    /// Doca de construção com os cartões do Figma. Cria um cartão por BuildingData a partir do modelo
    /// e, se configurado, um cartão de estrada. Clicar no cartão já selecionado volta ao modo livre.
    /// </summary>
    public class BuildDockView : MonoBehaviour
    {
        [Header("Fonte de dados")]
        [SerializeField] private ColonySimulation simulation;
        [SerializeField] private BuildingSystem buildingSystem;
        [SerializeField] private ToolModeController toolModeController;
        [SerializeField] private BuildHudController buildHudController;

        [Header("Cartões")]
        [Tooltip("Cartão modelo, desligado na cena. Cada construção ganha uma cópia.")]
        [SerializeField] private BuildCardView cardTemplate;
        [SerializeField] private Transform cardsParent;
        [SerializeField] private List<BuildingData> buildings = new List<BuildingData>();
        [Tooltip("Arte usada quando a BuildingData não tem ícone.")]
        [SerializeField] private Sprite fallbackArt;

        [Header("Estrada")]
        [SerializeField] private bool includeRoad = true;
        [SerializeField] private string roadName = "Estrada";
        [SerializeField] private Sprite roadArt;

        private readonly List<BuildCardView> _buildingCards = new List<BuildCardView>();
        private BuildCardView _roadCard;
        private bool _isDirty = true;

        private ResourceStock Stock => simulation.Resources;

        private void Awake()
        {
            if (simulation == null || buildingSystem == null || toolModeController == null || buildHudController == null || cardTemplate == null)
            {
                Debug.LogError("[BuildDockView] Referências incompletas. Rode Reclaim > Criar HUD do redesign de novo.", this);
                enabled = false;
                return;
            }

            cardTemplate.gameObject.SetActive(false);
        }

        private void Start()
        {
            CreateCards();
            _isDirty = true;
        }

        private void OnEnable()
        {
            Stock.OnResourceChanged += HandleResourceChanged;
            buildingSystem.OnSelectedBuildingChanged += HandleSelectionChanged;
            toolModeController.OnModeChanged += HandleModeChanged;
            _isDirty = true;
        }

        private void OnDisable()
        {
            Stock.OnResourceChanged -= HandleResourceChanged;
            buildingSystem.OnSelectedBuildingChanged -= HandleSelectionChanged;
            toolModeController.OnModeChanged -= HandleModeChanged;
        }

        private void LateUpdate()
        {
            if (!_isDirty || !Stock.IsInitialized)
            {
                return;
            }

            _isDirty = false;
            RefreshStates();
        }

        private void HandleResourceChanged(ResourceType type, float amount, float capacity) => _isDirty = true;

        private void HandleSelectionChanged(BuildingData data) => _isDirty = true;

        private void HandleModeChanged(GameMode mode) => _isDirty = true;

        private void CreateCards()
        {
            for (int i = 0; i < buildings.Count; i++)
            {
                BuildingData data = buildings[i];
                if (data == null)
                {
                    continue;
                }

                BuildCardView card = Instantiate(cardTemplate, cardsParent);
                card.name = $"Cartão {data.DisplayName}";
                bool hasIcon = data.Icon != null;
                card.Bind(data.DisplayName, hasIcon ? data.Icon : fallbackArt, !hasIcon, data.Size, data.Costs, Stock, () => ToggleBuilding(data));
                card.gameObject.SetActive(true);
                _buildingCards.Add(card);
            }

            if (includeRoad)
            {
                _roadCard = Instantiate(cardTemplate, cardsParent);
                _roadCard.name = "Cartão Estrada";
                _roadCard.Bind(roadName, roadArt, true, Vector2Int.zero, null, Stock, ToggleRoad);
                _roadCard.gameObject.SetActive(true);
            }
        }

        private void RefreshStates()
        {
            bool inBuildMode = toolModeController.CurrentMode == GameMode.Build;
            int cardIndex = 0;
            for (int i = 0; i < buildings.Count; i++)
            {
                BuildingData data = buildings[i];
                if (data == null)
                {
                    continue;
                }

                BuildCardState state = inBuildMode && buildingSystem.SelectedBuilding == data ? BuildCardState.Selected
                    : buildingSystem.CanAfford(data) ? BuildCardState.Available
                    : BuildCardState.Insufficient;
                _buildingCards[cardIndex++].SetState(state, Stock);
            }

            if (_roadCard != null)
            {
                bool roadSelected = toolModeController.CurrentMode == GameMode.Road;
                _roadCard.SetState(roadSelected ? BuildCardState.Selected : BuildCardState.Available, Stock);
            }
        }

        private void ToggleBuilding(BuildingData data)
        {
            bool alreadySelected = toolModeController.CurrentMode == GameMode.Build && buildingSystem.SelectedBuilding == data;
            if (alreadySelected)
            {
                buildHudController.SelectIdleTool();
            }
            else
            {
                buildHudController.SelectBuildingTool(data);
            }
        }

        private void ToggleRoad()
        {
            if (toolModeController.CurrentMode == GameMode.Road)
            {
                buildHudController.SelectIdleTool();
            }
            else
            {
                buildHudController.SelectRoadTool();
            }
        }
    }
}
