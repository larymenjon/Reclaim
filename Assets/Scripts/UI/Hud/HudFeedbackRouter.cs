using Reclaim.Calendar;
using Reclaim.Construction;
using Reclaim.Economy;
using Reclaim.Events;
using Reclaim.Gameplay;
using Reclaim.Simulation;
using UnityEngine;

namespace Reclaim.UI.Hud
{
    /// <summary>
    /// Traduz o que acontece na colônia em avisos rápidos e notificações.
    /// É o único lugar que decide o texto desses avisos; feed e toast só exibem.
    /// </summary>
    public class HudFeedbackRouter : MonoBehaviour
    {
        private enum ReserveLevel
        {
            Fine,
            Low,
            Critical
        }

        [Header("Fonte de dados")]
        [SerializeField] private ColonySimulation simulation;
        [SerializeField] private BuildingSystem buildingSystem;
        [SerializeField] private ToolModeController toolModeController;
        [SerializeField] private HudTheme theme;

        [Header("Saída")]
        [SerializeField] private NotificationFeedView notifications;
        [SerializeField] private ToastView toast;

        [Header("Reservas")]
        [Tooltip("Abaixo disto o aviso de reserva vira crítico. O limite de alerta vem do HudTheme.")]
        [SerializeField, Min(0f)] private float criticalReserveDays = 1f;
        [SerializeField] private ResourceType[] watchedReserves = { ResourceType.Food, ResourceType.Water };

        private ReserveLevel[] _reserveLevels;
        private int _lastPopulation = -1;

        private void Awake()
        {
            if (simulation == null || theme == null || notifications == null || toast == null)
            {
                Debug.LogError("[HudFeedbackRouter] Configure ColonySimulation, HudTheme, feed e toast.", this);
                enabled = false;
                return;
            }

            _reserveLevels = new ReserveLevel[watchedReserves.Length];
        }

        private void OnEnable()
        {
            simulation.Clock.OnDayStarted += HandleDayStarted;
            simulation.Families.OnPopulationChanged += HandlePopulationChanged;
            simulation.Events.OnEventResolved += HandleEventResolved;
            simulation.OnGameOver += HandleGameOver;

            if (buildingSystem != null)
            {
                buildingSystem.OnSelectedBuildingChanged += HandleSelectedBuildingChanged;
                buildingSystem.OnPlacementRejectedForCost += HandlePlacementRejected;
                buildingSystem.OnBuildingCompleted += HandleBuildingCompleted;
            }

            if (toolModeController != null)
            {
                toolModeController.OnModeChanged += HandleModeChanged;
            }
        }

        private void OnDisable()
        {
            simulation.Clock.OnDayStarted -= HandleDayStarted;
            simulation.Families.OnPopulationChanged -= HandlePopulationChanged;
            simulation.Events.OnEventResolved -= HandleEventResolved;
            simulation.OnGameOver -= HandleGameOver;

            if (buildingSystem != null)
            {
                buildingSystem.OnSelectedBuildingChanged -= HandleSelectedBuildingChanged;
                buildingSystem.OnPlacementRejectedForCost -= HandlePlacementRejected;
                buildingSystem.OnBuildingCompleted -= HandleBuildingCompleted;
            }

            if (toolModeController != null)
            {
                toolModeController.OnModeChanged -= HandleModeChanged;
            }
        }

        private void HandleSelectedBuildingChanged(BuildingData data)
        {
            if (data == null)
            {
                return;
            }

            string message = data.DisplayName + HudFormat.Separator + "clique no mapa";
            if (data.CanRotate)
            {
                message += HudFormat.Separator + "R gira";
            }

            toast.Show(message + HudFormat.Separator + "botão direito cancela");
        }

        private void HandleModeChanged(GameMode mode)
        {
            if (mode == GameMode.Road)
            {
                toast.Show("Estrada" + HudFormat.Separator + "clique para marcar pontos" + HudFormat.Separator + "botão direito desfaz");
            }
        }

        private void HandlePlacementRejected(BuildingData data)
        {
            toast.Show("Faltam recursos para " + data.DisplayName);
        }

        private void HandleBuildingCompleted(Building building)
        {
            if (building.Data != null)
            {
                notifications.Post(NotificationSeverity.Success, building.Data.DisplayName, "construção concluída");
            }
        }

        private void HandleEventResolved(ActiveEvent activeEvent, EventChoice choice)
        {
            if (!string.IsNullOrEmpty(choice.OutcomeText))
            {
                notifications.Post(NotificationSeverity.Radio, activeEvent.Data.Title, choice.OutcomeText);
            }
        }

        private void HandlePopulationChanged(int population)
        {
            if (_lastPopulation >= 0 && population < _lastPopulation)
            {
                int lost = _lastPopulation - population;
                notifications.Post(NotificationSeverity.Critical, "População",
                    lost == 1 ? "1 morador a menos" : $"{lost} moradores a menos");
            }

            _lastPopulation = population;
        }

        private void HandleGameOver()
        {
            notifications.Post(NotificationSeverity.Critical, "Colônia", "não restou ninguém para reconstruir");
        }

        /// <summary>Avisa só quando a reserva muda de faixa, para não repetir o mesmo alerta todo dia.</summary>
        private void HandleDayStarted(CalendarDate date)
        {
            if (!simulation.Resources.IsInitialized)
            {
                return;
            }

            for (int i = 0; i < watchedReserves.Length; i++)
            {
                ResourceType type = watchedReserves[i];
                int days = HudFormat.ReserveDays(simulation.Resources.GetAmount(type), simulation.Needs.GetDailyDemand(type));
                ReserveLevel level = days < 0 ? ReserveLevel.Fine
                    : days < criticalReserveDays ? ReserveLevel.Critical
                    : days < theme.LowReserveDays ? ReserveLevel.Low
                    : ReserveLevel.Fine;

                if (level > _reserveLevels[i])
                {
                    string title = HudFormat.ResourceName(type, simulation.Resources);
                    title = char.ToUpperInvariant(title[0]) + title.Substring(1);
                    string message = days == 1 ? "reserva para 1 dia" : $"reserva para {days} dias";
                    notifications.Post(level == ReserveLevel.Critical ? NotificationSeverity.Critical : NotificationSeverity.Alert,
                        title, message, "Aumente a produção ou racione");
                }

                _reserveLevels[i] = level;
            }
        }
    }
}
