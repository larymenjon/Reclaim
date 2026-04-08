using System;
using System.Collections.Generic;
using Reclaim.Survival.Core;
using UnityEngine;

namespace Reclaim.Survival.Families
{
    public class FamilyManager : MonoBehaviour
    {
        [Serializable]
        public struct StartingFamilyGroup
        {
            public FamilyPresetData preset;
            public int count;
        }

        [SerializeField] private List<StartingFamilyGroup> startingFamilies = new List<StartingFamilyGroup>();
        [SerializeField] private int randomSeed = 0;
        [Header("Fallback Setup")]
        [SerializeField] private bool useFallbackStartingFamilies = true;
        [SerializeField] private int fallbackFamilyCount = 7;
        [SerializeField] private int fallbackMinMembers = 2;
        [SerializeField] private int fallbackMaxMembers = 5;
        [SerializeField, Range(0f, 100f)] private float fallbackInitialHunger = 14f;
        [SerializeField, Range(0f, 100f)] private float fallbackInitialHealth = 78f;
        [SerializeField, Range(0f, 100f)] private float fallbackInitialMorale = 64f;
        [SerializeField] private float fallbackTemperatureTolerance = -8f;

        private readonly List<Family> _families = new List<Family>();
        private System.Random _rng;

        public event Action<Family> OnFamilyAdded;
        public event Action<Family> OnFamilyRemoved;
        public event Action<int> OnPopulationChanged;

        public IReadOnlyList<Family> Families => _families;
        public int FamilyCount => _families.Count;
        public int TotalPopulation { get; private set; }
        public int HomelessFamilies { get; private set; }

        private void Awake()
        {
            _rng = randomSeed == 0 ? new System.Random() : new System.Random(randomSeed);
        }

        public void Initialize()
        {
            _families.Clear();
            SpawnStartingFamilies();
            RecalculatePopulation();
        }

        public Family AddFamily(FamilyPresetData preset, string forcedId = null)
        {
            if (preset == null)
            {
                return null;
            }

            int members = _rng.Next(preset.MinMembers, preset.MaxMembers + 1);
            string familyId = string.IsNullOrWhiteSpace(forcedId)
                ? $"{preset.PresetId}_{Guid.NewGuid():N}"
                : forcedId;

            Family family = new Family(
                familyId,
                preset.DisplayName,
                members,
                preset.InitialHunger,
                preset.InitialHealth,
                preset.InitialMorale,
                preset.TemperatureTolerance,
                preset.InitialEmploymentStatus,
                preset.IllnessVulnerability,
                preset.GrowthPotential
            );

            _families.Add(family);
            RecalculatePopulation();
            OnFamilyAdded?.Invoke(family);
            return family;
        }

        public bool RemoveFamily(Family family)
        {
            if (family == null)
            {
                return false;
            }

            bool removed = _families.Remove(family);
            if (removed)
            {
                RecalculatePopulation();
                OnFamilyRemoved?.Invoke(family);
            }

            return removed;
        }

        public Family GetRandomFamily(Func<Family, bool> predicate = null)
        {
            if (_families.Count == 0)
            {
                return null;
            }

            if (predicate == null)
            {
                return _families[_rng.Next(0, _families.Count)];
            }

            List<Family> filtered = new List<Family>();
            for (int i = 0; i < _families.Count; i++)
            {
                if (predicate(_families[i]))
                {
                    filtered.Add(_families[i]);
                }
            }

            if (filtered.Count == 0)
            {
                return null;
            }

            return filtered[_rng.Next(0, filtered.Count)];
        }

        public void ProcessTick(TickContext _)
        {
            for (int i = _families.Count - 1; i >= 0; i--)
            {
                if (!_families[i].IsAlive)
                {
                    Family removed = _families[i];
                    _families.RemoveAt(i);
                    OnFamilyRemoved?.Invoke(removed);
                }
            }

            RecalculatePopulation();
        }

        public void AddMemberToFamily(Family family, int amount = 1)
        {
            if (family == null || amount <= 0)
            {
                return;
            }

            family.AddMembers(amount);
            RecalculatePopulation();
        }

        public void RemoveMembersFromFamily(Family family, int amount = 1)
        {
            if (family == null || amount <= 0)
            {
                return;
            }

            family.LoseMembers(amount);
            RecalculatePopulation();
        }

        public float GetAverageMorale()
        {
            if (_families.Count == 0)
            {
                return 0f;
            }

            float total = 0f;
            for (int i = 0; i < _families.Count; i++)
            {
                total += _families[i].Morale;
            }

            return total / _families.Count;
        }

        public float GetAverageHealth()
        {
            if (_families.Count == 0)
            {
                return 0f;
            }

            float total = 0f;
            for (int i = 0; i < _families.Count; i++)
            {
                total += _families[i].Health;
            }

            return total / _families.Count;
        }

        private void SpawnStartingFamilies()
        {
            int familiesBefore = _families.Count;
            for (int i = 0; i < startingFamilies.Count; i++)
            {
                StartingFamilyGroup group = startingFamilies[i];
                if (group.preset == null || group.count <= 0)
                {
                    continue;
                }

                for (int j = 0; j < group.count; j++)
                {
                    AddFamily(group.preset);
                }
            }

            if (_families.Count == familiesBefore && useFallbackStartingFamilies)
            {
                SpawnFallbackFamilies();
            }
        }

        private void SpawnFallbackFamilies()
        {
            int targetCount = Mathf.Max(0, fallbackFamilyCount);
            for (int i = 0; i < targetCount; i++)
            {
                int members = _rng.Next(Mathf.Max(1, fallbackMinMembers), Mathf.Max(fallbackMinMembers + 1, fallbackMaxMembers + 1));
                Family family = new Family(
                    $"fallback_family_{Guid.NewGuid():N}",
                    "Settler Family",
                    members,
                    fallbackInitialHunger,
                    fallbackInitialHealth,
                    fallbackInitialMorale,
                    fallbackTemperatureTolerance,
                    EmploymentStatus.Unemployed,
                    0.25f,
                    0.3f
                );

                _families.Add(family);
                OnFamilyAdded?.Invoke(family);
            }
        }

        private void RecalculatePopulation()
        {
            int oldPopulation = TotalPopulation;

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

            if (oldPopulation != TotalPopulation)
            {
                OnPopulationChanged?.Invoke(TotalPopulation);
            }
        }
    }
}
