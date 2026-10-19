using System;
using System.Collections.Generic;
using UnityEngine;

namespace Reclaim.Population
{
    /// <summary>
    /// Registro das famílias da colônia: criação, remoção e agregados (população, médias).
    /// As regras de fome, saúde e frio ficam no NeedsSystem.
    /// </summary>
    public class FamilyManager : MonoBehaviour
    {
        [Serializable]
        public struct StartingFamilyGroup
        {
            [SerializeField] private FamilyPresetData preset;
            [SerializeField, Min(0)] private int count;

            public FamilyPresetData Preset => preset;
            public int Count => count;
        }

        [SerializeField] private List<StartingFamilyGroup> startingFamilies = new List<StartingFamilyGroup>();
        [Tooltip("0 gera uma semente diferente a cada partida.")]
        [SerializeField] private int randomSeed;

        private readonly List<Family> _families = new List<Family>();
        private System.Random _rng;

        public IReadOnlyList<Family> Families => _families;
        public int FamilyCount => _families.Count;
        public int TotalPopulation { get; private set; }
        public int HomelessFamilies { get; private set; }

        public event Action<Family> OnFamilyAdded;
        public event Action<Family> OnFamilyRemoved;
        public event Action<int> OnPopulationChanged;

        public void Initialize()
        {
            _rng = randomSeed == 0 ? new System.Random() : new System.Random(randomSeed);
            _families.Clear();

            for (int i = 0; i < startingFamilies.Count; i++)
            {
                SpawnGroup(startingFamilies[i]);
            }

            if (_families.Count == 0)
            {
                Debug.LogError("[FamilyManager] Nenhuma família inicial configurada. Adicione um FamilyPreset em Starting Families.", this);
            }

            RecalculatePopulation();
        }

        public Family AddFamily(FamilyPresetData preset)
        {
            if (preset == null)
            {
                return null;
            }

            Family family = new Family(
                $"{preset.PresetId}_{Guid.NewGuid():N}",
                preset.DisplayName,
                _rng.Next(preset.MinMembers, preset.MaxMembers + 1),
                preset.InitialHunger,
                preset.InitialHealth,
                preset.InitialMorale,
                preset.TemperatureTolerance,
                preset.InitialEmploymentStatus,
                preset.IllnessVulnerability,
                preset.GrowthPotential);

            _families.Add(family);
            RecalculatePopulation();
            OnFamilyAdded?.Invoke(family);
            return family;
        }

        public bool RemoveFamily(Family family)
        {
            if (family == null || !_families.Remove(family))
            {
                return false;
            }

            RecalculatePopulation();
            OnFamilyRemoved?.Invoke(family);
            return true;
        }

        /// <summary>Sorteia índices distintos de famílias; usado por eventos que afetam parte da colônia.</summary>
        public void PickRandomFamilies(int count, List<Family> results)
        {
            results.Clear();
            if (count <= 0 || count >= _families.Count)
            {
                results.AddRange(_families);
                return;
            }

            List<Family> pool = new List<Family>(_families);
            for (int i = 0; i < count; i++)
            {
                int index = _rng.Next(i, pool.Count);
                (pool[i], pool[index]) = (pool[index], pool[i]);
                results.Add(pool[i]);
            }
        }

        /// <summary>Remove famílias sem membros e atualiza os totais. Chamado uma vez por tick.</summary>
        public void RemoveExtinctFamilies()
        {
            for (int i = _families.Count - 1; i >= 0; i--)
            {
                if (_families[i].IsAlive)
                {
                    continue;
                }

                Family removed = _families[i];
                _families.RemoveAt(i);
                OnFamilyRemoved?.Invoke(removed);
            }

            RecalculatePopulation();
        }

        public float GetAverageMorale() => Average(family => family.Morale);

        public float GetAverageHealth() => Average(family => family.Health);

        public float GetAverageHunger() => Average(family => family.Hunger);

        private void SpawnGroup(StartingFamilyGroup group)
        {
            if (group.Preset == null)
            {
                return;
            }

            for (int i = 0; i < group.Count; i++)
            {
                AddFamily(group.Preset);
            }
        }

        private float Average(Func<Family, float> selector)
        {
            if (_families.Count == 0)
            {
                return 0f;
            }

            float total = 0f;
            for (int i = 0; i < _families.Count; i++)
            {
                total += selector(_families[i]);
            }

            return total / _families.Count;
        }

        private void RecalculatePopulation()
        {
            int previous = TotalPopulation;
            TotalPopulation = 0;
            HomelessFamilies = 0;

            for (int i = 0; i < _families.Count; i++)
            {
                TotalPopulation += _families[i].MembersCount;
                if (_families[i].IsHomeless)
                {
                    HomelessFamilies++;
                }
            }

            if (previous != TotalPopulation)
            {
                OnPopulationChanged?.Invoke(TotalPopulation);
            }
        }
    }
}
