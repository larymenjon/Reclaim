using Reclaim.Calendar;
using Reclaim.Construction;
using Reclaim.Economy;
using Reclaim.Flow;
using Reclaim.Simulation;
using TMPro;
using UnityEngine;

namespace Reclaim.UI.Hud
{
    /// <summary>
    /// "Barra superior" do Figma: pessoas à esquerda, materiais à direita e a placa com o nome da cidade.
    /// Consumíveis com consumo diário aparecem em dias de reserva; materiais em unidades com o saldo do último dia.
    /// Só exibe dados; nenhuma regra de jogo mora aqui.
    /// </summary>
    public class TopBarView : MonoBehaviour
    {
        [Header("Fonte de dados")]
        [SerializeField] private ColonySimulation simulation;
        [SerializeField] private BuildingSystem buildingSystem;
        [SerializeField] private HudTheme theme;

        [Header("Placa do nome")]
        [SerializeField] private TMP_Text cityNameText;

        [Header("Esquerda")]
        [SerializeField] private ResourceCounterView houses;
        [SerializeField] private ResourceCounterView population;
        [SerializeField] private ResourceCounterView food;
        [SerializeField] private ResourceCounterView water;
        [SerializeField] private ResourceCounterView medicine;

        [Header("Direita")]
        [SerializeField] private ResourceCounterView wood;
        [SerializeField] private ResourceCounterView scrap;
        [SerializeField] private ResourceCounterView fuel;

        private static readonly int ResourceTypeCount = System.Enum.GetValues(typeof(ResourceType)).Length;

        private readonly float[] _amountAtDayStart = new float[ResourceTypeCount];
        private readonly float[] _lastDayBalance = new float[ResourceTypeCount];
        private bool _hasDayStart;
        private bool _hasBalance;
        private bool _isDirty = true;

        private void Awake()
        {
            if (simulation == null || theme == null)
            {
                Debug.LogError("[TopBarView] ColonySimulation ou HudTheme não configurados.", this);
                enabled = false;
            }
        }

        private void Start()
        {
            cityNameText.text = NewGameSettings.CityName;
        }

        private void OnEnable()
        {
            simulation.Resources.OnResourceChanged += HandleResourceChanged;
            simulation.Families.OnPopulationChanged += HandlePopulationChanged;
            simulation.Clock.OnDayStarted += HandleDayStarted;
            if (buildingSystem != null)
            {
                buildingSystem.OnBuildingCompleted += HandleBuildingCompleted;
            }

            _isDirty = true;
        }

        private void OnDisable()
        {
            simulation.Resources.OnResourceChanged -= HandleResourceChanged;
            simulation.Families.OnPopulationChanged -= HandlePopulationChanged;
            simulation.Clock.OnDayStarted -= HandleDayStarted;
            if (buildingSystem != null)
            {
                buildingSystem.OnBuildingCompleted -= HandleBuildingCompleted;
            }
        }

        // Recursos mudam várias vezes por tick; basta redesenhar uma vez por frame.
        private void LateUpdate()
        {
            if (!_isDirty || !simulation.Resources.IsInitialized)
            {
                return;
            }

            _isDirty = false;
            Refresh();
        }

        private void HandleResourceChanged(ResourceType type, float amount, float capacity) => _isDirty = true;

        private void HandlePopulationChanged(int total) => _isDirty = true;

        private void HandleBuildingCompleted(Building building) => _isDirty = true;

        private void HandleDayStarted(CalendarDate date)
        {
            ResourceStock stock = simulation.Resources;
            if (!stock.IsInitialized)
            {
                return;
            }

            for (int i = 0; i < ResourceTypeCount; i++)
            {
                float amount = stock.GetAmount((ResourceType)i);
                if (_hasDayStart)
                {
                    _lastDayBalance[i] = amount - _amountAtDayStart[i];
                }

                _amountAtDayStart[i] = amount;
            }

            _hasBalance = _hasDayStart;
            _hasDayStart = true;
            _isDirty = true;
        }

        private void Refresh()
        {
            int completedHouses = buildingSystem != null ? buildingSystem.CompletedHouses : 0;
            houses.Show(completedHouses.ToString(), string.Empty, CounterState.Normal);
            population.Show(simulation.Families.TotalPopulation.ToString(), $"{simulation.Families.FamilyCount} fam.", CounterState.Normal);

            ShowStock(food, ResourceType.Food);
            ShowStock(water, ResourceType.Water);
            ShowStock(medicine, ResourceType.Medicine);
            ShowStock(wood, ResourceType.Wood);
            ShowStock(scrap, ResourceType.Scrap);
            ShowStock(fuel, ResourceType.Fuel);
        }

        /// <summary>Com consumo diário mostra dias de reserva; sem consumo mostra unidades e o saldo do último dia.</summary>
        private void ShowStock(ResourceCounterView counter, ResourceType type)
        {
            ResourceStock stock = simulation.Resources;
            bool tracked = stock.IsTracked(type);
            counter.SetVisible(tracked);
            if (!tracked)
            {
                return;
            }

            float amount = stock.GetAmount(type);
            int days = HudFormat.ReserveDays(amount, simulation.Needs.GetDailyDemand(type));
            if (days >= 0)
            {
                CounterState state = days < theme.LowReserveDays ? CounterState.Low : CounterState.Normal;
                counter.Show(days.ToString(), "dias", state);
                return;
            }

            string value = Mathf.FloorToInt(amount).ToString();
            float balance = _lastDayBalance[(int)type];
            if (_hasBalance && Mathf.Abs(balance) >= 0.5f)
            {
                counter.Show(value, HudFormat.SignedPerDay(balance), balance > 0f ? CounterState.Positive : CounterState.Normal);
            }
            else
            {
                counter.Show(value, "un.", CounterState.Normal);
            }
        }
    }
}
