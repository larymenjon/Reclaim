using System;
using Reclaim.Survival.Core;
using Reclaim.Survival.Events;
using Reclaim.Survival.Families;
using Reclaim.Survival.Resources;
using Reclaim.Survival.Systems;
using UnityEngine;

namespace Reclaim.Survival.Managers
{
    public class GameManager : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private TimeSystem timeSystem;
        [SerializeField] private FamilyManager familyManager;
        [SerializeField] private ResourceManager resourceManager;
        [SerializeField] private NeedsSystem needsSystem;
        [SerializeField] private MoraleSystem moraleSystem;
        [SerializeField] private EventManager eventManager;

        [Header("Global State")]
        [SerializeField] private bool failOnZeroPopulation = true;
        [SerializeField] private bool failOnZeroHope;

        public event Action<GameStateSnapshot> OnStateUpdated;
        public event Action OnGameOver;

        public GameStateSnapshot CurrentSnapshot { get; private set; }
        public bool IsGameOver { get; private set; }

        private void Awake()
        {
            AutoResolveReferences();
            InitializeSystems();
        }

        private void OnDestroy()
        {
            if (timeSystem != null)
            {
                timeSystem.OnTick -= HandleTick;
            }
        }

        private void InitializeSystems()
        {
            familyManager.Initialize();
            resourceManager.Initialize();
            moraleSystem.Initialize(familyManager);
            needsSystem.Initialize(familyManager, resourceManager);
            eventManager.Initialize(resourceManager, familyManager, moraleSystem);

            timeSystem.OnTick += HandleTick;
        }

        private void HandleTick(TickContext context)
        {
            if (IsGameOver)
            {
                return;
            }

            resourceManager.ProcessTick(context);
            NeedsTickReport needsReport = needsSystem.ProcessTick(context);
            familyManager.ProcessTick(context);

            GameStateSnapshot preMoraleSnapshot = BuildSnapshot(context.TickIndex, context.TemperatureCelsius);
            moraleSystem.ProcessTick(context, needsReport, preMoraleSnapshot);

            CurrentSnapshot = BuildSnapshot(context.TickIndex, context.TemperatureCelsius);
            OnStateUpdated?.Invoke(CurrentSnapshot);

            eventManager.ProcessTick(context, CurrentSnapshot);

            EvaluateFailState();
        }

        private GameStateSnapshot BuildSnapshot(int tickIndex, float temperatureCelsius)
        {
            return new GameStateSnapshot(
                tickIndex,
                familyManager.TotalPopulation,
                familyManager.GetAverageMorale(),
                familyManager.GetAverageHealth(),
                moraleSystem.GlobalHope,
                resourceManager.GetFill01(ResourceType.Food),
                resourceManager.GetFill01(ResourceType.Water),
                temperatureCelsius,
                familyManager.HomelessFamilies
            );
        }

        private void EvaluateFailState()
        {
            bool failedPopulation = failOnZeroPopulation && familyManager.TotalPopulation <= 0;
            bool failedHope = failOnZeroHope && moraleSystem.GlobalHope <= 0f;
            if (!failedPopulation && !failedHope)
            {
                return;
            }

            IsGameOver = true;
            timeSystem.SetRunning(false);
            OnGameOver?.Invoke();
        }

        private void AutoResolveReferences()
        {
            if (timeSystem == null) timeSystem = FindFirstObjectByType<TimeSystem>();
            if (familyManager == null) familyManager = FindFirstObjectByType<FamilyManager>();
            if (resourceManager == null) resourceManager = FindFirstObjectByType<ResourceManager>();
            if (needsSystem == null) needsSystem = FindFirstObjectByType<NeedsSystem>();
            if (moraleSystem == null) moraleSystem = FindFirstObjectByType<MoraleSystem>();
            if (eventManager == null) eventManager = FindFirstObjectByType<EventManager>();
        }
    }
}
