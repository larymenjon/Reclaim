using System;
using Reclaim.Calendar;
using Reclaim.Climate;
using Reclaim.Economy;
using Reclaim.Events;
using Reclaim.Morale;
using Reclaim.Population;
using UnityEngine;

namespace Reclaim.Simulation
{
    /// <summary>
    /// Raiz de composição da simulação da colônia. Inicializa os sistemas na ordem certa
    /// e define a sequência de cada tick: clima, necessidades, população, moral, eventos.
    /// Também decide derrota e pausa o jogo em eventos críticos.
    /// </summary>
    public class ColonySimulation : MonoBehaviour
    {
        [Header("Sistemas")]
        [SerializeField] private SimulationClock clock;
        [SerializeField] private ClimateSystem climate;
        [SerializeField] private ResourceStock resourceStock;
        [SerializeField] private FamilyManager familyManager;
        [SerializeField] private NeedsSystem needsSystem;
        [SerializeField] private MoraleSystem moraleSystem;
        [SerializeField] private EventManager eventManager;

        [Header("Condições de derrota")]
        [SerializeField] private bool failOnZeroPopulation = true;
        [SerializeField, Range(0f, 100f)] private float lowMoraleThreshold = 5f;
        [Tooltip("Dias seguidos com moral média abaixo do limite até a derrota. 0 desativa.")]
        [SerializeField, Min(0)] private int consecutiveLowMoraleDaysToFail = 3;

        private int _lowMoraleDayStreak;
        private bool _pausedByEvent;

        public ColonySnapshot CurrentSnapshot { get; private set; }
        public bool IsGameOver { get; private set; }
        public SimulationClock Clock => clock;
        public ResourceStock Resources => resourceStock;
        public FamilyManager Families => familyManager;
        public NeedsSystem Needs => needsSystem;
        public MoraleSystem Morale => moraleSystem;
        public EventManager Events => eventManager;
        public ClimateSystem Climate => climate;

        public event Action<ColonySnapshot> OnStateUpdated;
        public event Action OnGameOver;

        private void Awake()
        {
            if (!HasAllReferences())
            {
                enabled = false;
                return;
            }

            InitializeSystems();
        }

        private void OnEnable()
        {
            if (!HasAllReferences())
            {
                return;
            }

            clock.OnTick += HandleTick;
            clock.OnDayStarted += HandleDayStarted;
            eventManager.OnEventTriggered += HandleEventTriggered;
            eventManager.OnEventResolved += HandleEventResolved;
        }

        private void OnDisable()
        {
            if (!HasAllReferences())
            {
                return;
            }

            clock.OnTick -= HandleTick;
            clock.OnDayStarted -= HandleDayStarted;
            eventManager.OnEventTriggered -= HandleEventTriggered;
            eventManager.OnEventResolved -= HandleEventResolved;
        }

        private void InitializeSystems()
        {
            resourceStock.Initialize();
            familyManager.Initialize();
            needsSystem.Initialize(familyManager, resourceStock);
            moraleSystem.Initialize(familyManager);
            eventManager.Initialize(resourceStock, familyManager, moraleSystem);
            climate.Initialize(clock.CurrentDate);

            CurrentSnapshot = BuildSnapshot(clock.CurrentTick, clock.CurrentDate.DayIndex);
        }

        private void HandleTick(int tickIndex, CalendarDate date, float deltaDays)
        {
            if (IsGameOver)
            {
                return;
            }

            float temperature = climate.Refresh(date);
            TickContext context = new TickContext(tickIndex, date, deltaDays, temperature);

            NeedsTickReport needsReport = needsSystem.ProcessTick(context);
            familyManager.RemoveExtinctFamilies();
            moraleSystem.ProcessTick(context, needsReport, BuildSnapshot(tickIndex, date.DayIndex));

            CurrentSnapshot = BuildSnapshot(tickIndex, date.DayIndex);
            OnStateUpdated?.Invoke(CurrentSnapshot);

            eventManager.ProcessTick(context, CurrentSnapshot);

            if (failOnZeroPopulation && familyManager.TotalPopulation <= 0)
            {
                TriggerGameOver("A colônia ficou sem moradores.");
            }
        }

        private void HandleDayStarted(CalendarDate date)
        {
            if (IsGameOver || consecutiveLowMoraleDaysToFail <= 0)
            {
                return;
            }

            bool lowMorale = familyManager.GetAverageMorale() <= lowMoraleThreshold;
            _lowMoraleDayStreak = lowMorale ? _lowMoraleDayStreak + 1 : 0;

            if (_lowMoraleDayStreak >= consecutiveLowMoraleDaysToFail)
            {
                TriggerGameOver($"Moral abaixo de {lowMoraleThreshold:0} por {_lowMoraleDayStreak} dias.");
            }
        }

        private void HandleEventTriggered(ActiveEvent activeEvent)
        {
            if (!activeEvent.Data.IsCritical || clock.IsPaused)
            {
                return;
            }

            clock.Pause();
            _pausedByEvent = true;
        }

        private void HandleEventResolved(ActiveEvent activeEvent, EventChoice choice)
        {
            if (!_pausedByEvent || IsGameOver)
            {
                return;
            }

            _pausedByEvent = false;
            clock.Resume();
        }

        private void TriggerGameOver(string reason)
        {
            IsGameOver = true;
            clock.Pause();
            Debug.Log($"[ColonySimulation] Fim de jogo: {reason}");
            OnGameOver?.Invoke();
        }

        private ColonySnapshot BuildSnapshot(int tickIndex, int dayIndex)
        {
            return new ColonySnapshot(
                tickIndex,
                dayIndex,
                familyManager.TotalPopulation,
                familyManager.GetAverageMorale(),
                familyManager.GetAverageHealth(),
                moraleSystem.GlobalHope,
                resourceStock.GetFill01(ResourceType.Food),
                resourceStock.GetFill01(ResourceType.Water),
                climate.CurrentTemperatureCelsius,
                familyManager.HomelessFamilies);
        }

        private bool HasAllReferences()
        {
            bool valid = clock != null && climate != null && resourceStock != null && familyManager != null
                         && needsSystem != null && moraleSystem != null && eventManager != null;

            if (!valid && enabled)
            {
                Debug.LogError(
                    "[ColonySimulation] Referências incompletas. Use o menu Reclaim > Configurar cena de jogo para criar e ligar os sistemas.",
                    this);
            }

            return valid;
        }
    }
}
