using System;
using Reclaim.Construction;
using Reclaim.Grid;
using Reclaim.Input;
using Reclaim.Road;
using UnityEngine;
using UnityEngine.Serialization;

namespace Reclaim.Gameplay
{
    /// <summary>
    /// Controla qual ferramenta do jogador está ativa (nenhuma, construir, estrada).
    /// As ferramentas não se conhecem: este componente liga uma e desliga as outras.
    /// Métodos públicos podem ser usados em OnClick de botões.
    /// </summary>
    public class ToolModeController : MonoBehaviour
    {
        [Header("Sistemas")]
        [FormerlySerializedAs("_gridManager")]
        [SerializeField] private GridManager gridManager;
        [FormerlySerializedAs("_previewSystem")]
        [SerializeField] private PreviewSystem previewSystem;
        [FormerlySerializedAs("_inputHandler")]
        [SerializeField] private InputHandler inputHandler;
        [FormerlySerializedAs("_buildingSystem")]
        [SerializeField] private BuildingSystem buildingSystem;
        [FormerlySerializedAs("_placementHistory")]
        [SerializeField] private PlacementHistory placementHistory;
        [FormerlySerializedAs("_roadBuilderManager")]
        [SerializeField] private RoadBuilderManager roadBuilderManager;

        [Header("Início")]
        [FormerlySerializedAs("_startMode")]
        [SerializeField] private GameMode startMode = GameMode.None;
        [FormerlySerializedAs("_defaultBuilding")]
        [SerializeField] private BuildingData defaultBuilding;
        [FormerlySerializedAs("_selectDefaultBuildingOnStart")]
        [SerializeField] private bool selectDefaultBuildingOnStart;

        public GameMode CurrentMode { get; private set; } = GameMode.None;

        public event Action<GameMode> OnModeChanged;

        private void Awake()
        {
            if (gridManager == null || previewSystem == null || inputHandler == null
                || buildingSystem == null || placementHistory == null)
            {
                Debug.LogError("[ToolModeController] Referências de construção incompletas. Configure no Inspector.", this);
                enabled = false;
                return;
            }

            inputHandler.SetGridManager(gridManager);
            inputHandler.OnUndoPressed += HandleUndoPressed;
            buildingSystem.Initialize(gridManager, previewSystem, placementHistory, inputHandler);
            buildingSystem.OnCancelRequested += EnterIdleMode;

            if (roadBuilderManager != null)
            {
                roadBuilderManager.OnFinished += EnterIdleMode;
            }
        }

        private void OnDestroy()
        {
            if (inputHandler != null)
            {
                inputHandler.OnUndoPressed -= HandleUndoPressed;
            }

            if (buildingSystem != null)
            {
                buildingSystem.OnCancelRequested -= EnterIdleMode;
            }

            if (roadBuilderManager != null)
            {
                roadBuilderManager.OnFinished -= EnterIdleMode;
            }
        }

        private void Start()
        {
            SetMode(startMode);

            if (selectDefaultBuildingOnStart && defaultBuilding != null)
            {
                EnterBuildMode(defaultBuilding);
            }
        }

        public void SetMode(GameMode mode)
        {
            CurrentMode = mode;
            buildingSystem.SetActive(mode == GameMode.Build);

            if (roadBuilderManager != null)
            {
                roadBuilderManager.SetActive(mode == GameMode.Road);
            }

            OnModeChanged?.Invoke(CurrentMode);
        }

        public void EnterBuildMode(BuildingData buildingData)
        {
            buildingSystem.SelectBuilding(buildingData);
            SetMode(GameMode.Build);
        }

        /// <summary>Sobrecarga para OnClick configurado com argumento Object.</summary>
        public void EnterBuildMode(UnityEngine.Object buildingAsset)
        {
            EnterBuildMode(buildingAsset as BuildingData);
        }

        public void EnterRoadMode()
        {
            if (roadBuilderManager == null)
            {
                Debug.LogWarning("[ToolModeController] RoadBuilderManager não configurado.", this);
                return;
            }

            SetMode(GameMode.Road);
        }

        public void EnterIdleMode()
        {
            SetMode(GameMode.None);
        }

        private void HandleUndoPressed()
        {
            if (CurrentMode != GameMode.Road)
            {
                placementHistory.UndoLast();
            }
        }
    }
}
