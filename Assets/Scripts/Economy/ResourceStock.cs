using System;
using System.Collections.Generic;
using UnityEngine;

namespace Reclaim.Economy
{
    /// <summary>
    /// Fonte única de verdade dos estoques da colônia. Toda entrada e saída de recurso
    /// (obra, corte de árvore, consumo das famílias, evento) passa por aqui.
    /// </summary>
    public class ResourceStock : MonoBehaviour
    {
        private sealed class Entry
        {
            public ResourceDefinition Definition;
            public float Amount;
            public float Capacity;
        }

        [SerializeField] private List<ResourceDefinition> definitions = new List<ResourceDefinition>();

        private readonly Dictionary<ResourceType, Entry> _entries = new Dictionary<ResourceType, Entry>();

        public bool IsInitialized { get; private set; }

        /// <summary>Tipo, quantidade atual e capacidade.</summary>
        public event Action<ResourceType, float, float> OnResourceChanged;

        public void Initialize()
        {
            _entries.Clear();

            for (int i = 0; i < definitions.Count; i++)
            {
                RegisterDefinition(definitions[i]);
            }

            if (_entries.Count == 0)
            {
                Debug.LogError("[ResourceStock] Nenhuma ResourceDefinition atribuída. A colônia começa sem recursos.", this);
            }

            IsInitialized = true;
            foreach (KeyValuePair<ResourceType, Entry> pair in _entries)
            {
                Notify(pair.Key, pair.Value);
            }
        }

        public bool IsTracked(ResourceType type) => _entries.ContainsKey(type);

        public ResourceDefinition GetDefinition(ResourceType type)
        {
            return _entries.TryGetValue(type, out Entry entry) ? entry.Definition : null;
        }

        public float GetAmount(ResourceType type)
        {
            return _entries.TryGetValue(type, out Entry entry) ? entry.Amount : 0f;
        }

        public float GetCapacity(ResourceType type)
        {
            return _entries.TryGetValue(type, out Entry entry) ? entry.Capacity : 0f;
        }

        public float GetFill01(ResourceType type)
        {
            float capacity = GetCapacity(type);
            return capacity > 0f ? GetAmount(type) / capacity : 0f;
        }

        public bool CanAfford(IReadOnlyList<ResourceAmount> costs)
        {
            for (int i = 0; i < costs.Count; i++)
            {
                if (GetAmount(costs[i].ResourceType) < costs[i].Amount)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Gasta todos os custos ou nenhum.</summary>
        public bool TrySpend(IReadOnlyList<ResourceAmount> costs)
        {
            if (!CanAfford(costs))
            {
                return false;
            }

            for (int i = 0; i < costs.Count; i++)
            {
                ConsumeUpTo(costs[i].ResourceType, costs[i].Amount);
            }

            return true;
        }

        /// <summary>Adiciona até o limite do armazém e devolve quanto realmente entrou.</summary>
        public float Add(ResourceType type, float amount)
        {
            if (amount <= 0f || !_entries.TryGetValue(type, out Entry entry))
            {
                return 0f;
            }

            float added = Mathf.Min(amount, entry.Capacity - entry.Amount);
            if (added <= 0f)
            {
                return 0f;
            }

            entry.Amount += added;
            Notify(type, entry);
            return added;
        }

        public void AddAll(IReadOnlyList<ResourceAmount> amounts)
        {
            for (int i = 0; i < amounts.Count; i++)
            {
                Add(amounts[i].ResourceType, amounts[i].Amount);
            }
        }

        /// <summary>Retira o que houver, até o pedido, e devolve quanto foi retirado.</summary>
        public float ConsumeUpTo(ResourceType type, float requested)
        {
            if (requested <= 0f || !_entries.TryGetValue(type, out Entry entry))
            {
                return 0f;
            }

            float consumed = Mathf.Min(entry.Amount, requested);
            if (consumed <= 0f)
            {
                return 0f;
            }

            entry.Amount -= consumed;
            Notify(type, entry);
            return consumed;
        }

        public void ApplyDelta(ResourceType type, float delta)
        {
            if (delta >= 0f)
            {
                Add(type, delta);
            }
            else
            {
                ConsumeUpTo(type, -delta);
            }
        }

        public void ChangeCapacity(ResourceType type, float capacityDelta)
        {
            if (!_entries.TryGetValue(type, out Entry entry))
            {
                return;
            }

            entry.Capacity = Mathf.Max(1f, entry.Capacity + capacityDelta);
            entry.Amount = Mathf.Min(entry.Amount, entry.Capacity);
            Notify(type, entry);
        }

        private void RegisterDefinition(ResourceDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            if (_entries.ContainsKey(definition.ResourceType))
            {
                Debug.LogWarning($"[ResourceStock] Recurso {definition.ResourceType} definido duas vezes. Ignorando '{definition.name}'.", this);
                return;
            }

            float capacity = Mathf.Max(1f, definition.StorageCapacity);
            _entries[definition.ResourceType] = new Entry
            {
                Definition = definition,
                Capacity = capacity,
                Amount = Mathf.Clamp(definition.StartingAmount, 0f, capacity)
            };
        }

        private void Notify(ResourceType type, Entry entry)
        {
            OnResourceChanged?.Invoke(type, entry.Amount, entry.Capacity);
        }
    }
}
