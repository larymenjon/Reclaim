using System;
using System.Collections.Generic;
using Reclaim.Survival.Core;
using UnityEngine;

namespace Reclaim.Survival.Resources
{
    public class ResourceManager : MonoBehaviour
    {
        [Serializable]
        private struct ResourceSetup
        {
            [SerializeField] public ResourceDefinition definition;
            [SerializeField] public float overrideStartingAmount;
            [SerializeField] public bool useOverrideStart;
            [SerializeField] public float overrideCapacity;
            [SerializeField] public bool useOverrideCapacity;
            [SerializeField] public float passiveProductionPerDay;
            [SerializeField] public float passiveConsumptionPerDay;
        }

        private struct ResourceRuntime
        {
            public float Amount;
            public float Capacity;
            public float PassiveProductionPerDay;
            public float PassiveConsumptionPerDay;
        }

        [SerializeField] private List<ResourceSetup> resourceSetups = new List<ResourceSetup>();

        private readonly Dictionary<ResourceType, ResourceRuntime> _resources = new Dictionary<ResourceType, ResourceRuntime>();

        public event Action<ResourceType, float, float> OnResourceChanged;

        public void Initialize()
        {
            _resources.Clear();

            for (int i = 0; i < resourceSetups.Count; i++)
            {
                ResourceSetup setup = resourceSetups[i];
                if (setup.definition == null)
                {
                    continue;
                }

                float startAmount = setup.useOverrideStart ? setup.overrideStartingAmount : setup.definition.DefaultStartingAmount;
                float capacity = setup.useOverrideCapacity ? setup.overrideCapacity : setup.definition.DefaultStorageCapacity;

                ResourceRuntime runtime = new ResourceRuntime
                {
                    Amount = Mathf.Clamp(startAmount, 0f, capacity),
                    Capacity = Mathf.Max(1f, capacity),
                    PassiveProductionPerDay = setup.passiveProductionPerDay,
                    PassiveConsumptionPerDay = setup.passiveConsumptionPerDay
                };

                _resources[setup.definition.ResourceType] = runtime;
            }

            if (_resources.Count == 0)
            {
                CreateFallbackRuntimeResources();
            }
        }

        public void ProcessTick(TickContext context)
        {
            List<ResourceType> keys = new List<ResourceType>(_resources.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                ResourceType type = keys[i];
                ResourceRuntime runtime = _resources[type];

                float production = runtime.PassiveProductionPerDay * context.DeltaDays;
                float consumption = runtime.PassiveConsumptionPerDay * context.DeltaDays;
                runtime.Amount = Mathf.Clamp(runtime.Amount + production - consumption, 0f, runtime.Capacity);
                _resources[type] = runtime;

                OnResourceChanged?.Invoke(type, runtime.Amount, runtime.Capacity);
            }
        }

        public float GetAmount(ResourceType type)
        {
            return _resources.TryGetValue(type, out ResourceRuntime runtime) ? runtime.Amount : 0f;
        }

        public float GetCapacity(ResourceType type)
        {
            return _resources.TryGetValue(type, out ResourceRuntime runtime) ? runtime.Capacity : 0f;
        }

        public float GetFill01(ResourceType type)
        {
            if (!_resources.TryGetValue(type, out ResourceRuntime runtime) || runtime.Capacity <= 0f)
            {
                return 0f;
            }

            return runtime.Amount / runtime.Capacity;
        }

        public bool HasEnough(ResourceType type, float amount)
        {
            return GetAmount(type) >= Mathf.Max(0f, amount);
        }

        public float ConsumeUpTo(ResourceType type, float requestedAmount)
        {
            requestedAmount = Mathf.Max(0f, requestedAmount);
            if (!_resources.TryGetValue(type, out ResourceRuntime runtime))
            {
                return 0f;
            }

            float consumed = Mathf.Min(runtime.Amount, requestedAmount);
            runtime.Amount -= consumed;
            _resources[type] = runtime;
            OnResourceChanged?.Invoke(type, runtime.Amount, runtime.Capacity);
            return consumed;
        }

        public bool TryConsume(ResourceType type, float amount)
        {
            if (!HasEnough(type, amount))
            {
                return false;
            }

            ConsumeUpTo(type, amount);
            return true;
        }

        public void AddResource(ResourceType type, float amount)
        {
            if (!_resources.TryGetValue(type, out ResourceRuntime runtime))
            {
                return;
            }

            runtime.Amount = Mathf.Clamp(runtime.Amount + amount, 0f, runtime.Capacity);
            _resources[type] = runtime;
            OnResourceChanged?.Invoke(type, runtime.Amount, runtime.Capacity);
        }

        public void ApplyDelta(ResourceType type, float amount)
        {
            if (amount >= 0f)
            {
                AddResource(type, amount);
            }
            else
            {
                ConsumeUpTo(type, -amount);
            }
        }

        public void ModifyStorage(ResourceType type, float capacityDelta)
        {
            if (!_resources.TryGetValue(type, out ResourceRuntime runtime))
            {
                return;
            }

            runtime.Capacity = Mathf.Max(1f, runtime.Capacity + capacityDelta);
            runtime.Amount = Mathf.Clamp(runtime.Amount, 0f, runtime.Capacity);
            _resources[type] = runtime;
            OnResourceChanged?.Invoke(type, runtime.Amount, runtime.Capacity);
        }

        private void CreateFallbackRuntimeResources()
        {
            AddFallback(ResourceType.Food, 150f, 300f, 18f, 0f);
            AddFallback(ResourceType.Water, 180f, 320f, 16f, 0f);
            AddFallback(ResourceType.Materials, 90f, 250f, 8f, 0f);
            AddFallback(ResourceType.Energy, 120f, 220f, 12f, 0f);
        }

        private void AddFallback(ResourceType type, float amount, float capacity, float productionPerDay, float consumptionPerDay)
        {
            ResourceRuntime runtime = new ResourceRuntime
            {
                Amount = Mathf.Clamp(amount, 0f, capacity),
                Capacity = Mathf.Max(1f, capacity),
                PassiveProductionPerDay = productionPerDay,
                PassiveConsumptionPerDay = consumptionPerDay
            };

            _resources[type] = runtime;
            OnResourceChanged?.Invoke(type, runtime.Amount, runtime.Capacity);
        }
    }
}
