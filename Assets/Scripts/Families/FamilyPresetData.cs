using UnityEngine;

namespace Reclaim.Survival.Families
{
    [CreateAssetMenu(fileName = "FamilyPreset", menuName = "Reclaim/Survival/Family Preset")]
    public class FamilyPresetData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string presetId = "default_family";
        [SerializeField] private string displayName = "Settler Family";

        [Header("Population")]
        [SerializeField] private int minMembers = 2;
        [SerializeField] private int maxMembers = 5;

        [Header("Initial Needs")]
        [SerializeField, Range(0f, 100f)] private float initialHunger = 10f;
        [SerializeField, Range(0f, 100f)] private float initialHealth = 80f;
        [SerializeField, Range(0f, 100f)] private float initialMorale = 70f;
        [SerializeField] private float temperatureTolerance = -10f;

        [Header("Behavior")]
        [SerializeField] private EmploymentStatus initialEmploymentStatus = EmploymentStatus.Unemployed;
        [SerializeField, Range(0f, 1f)] private float illnessVulnerability = 0.25f;
        [SerializeField, Range(0f, 1f)] private float growthPotential = 0.35f;

        public string PresetId => presetId;
        public string DisplayName => displayName;
        public int MinMembers => minMembers;
        public int MaxMembers => maxMembers;
        public float InitialHunger => initialHunger;
        public float InitialHealth => initialHealth;
        public float InitialMorale => initialMorale;
        public float TemperatureTolerance => temperatureTolerance;
        public EmploymentStatus InitialEmploymentStatus => initialEmploymentStatus;
        public float IllnessVulnerability => illnessVulnerability;
        public float GrowthPotential => growthPotential;
    }
}
