using System;
using UnityEngine;

namespace Reclaim.Survival.Families
{
    [Serializable]
    public class Family
    {
        [SerializeField] private string familyId;
        [SerializeField] private string familyName;
        [SerializeField] private int membersCount;
        [SerializeField, Range(0f, 100f)] private float hunger;
        [SerializeField, Range(0f, 100f)] private float health;
        [SerializeField, Range(0f, 100f)] private float morale;
        [SerializeField] private float temperatureTolerance;
        [SerializeField] private EmploymentStatus employmentStatus;
        [SerializeField] private bool isSick;
        [SerializeField] private bool isHomeless;
        [SerializeField, Range(0f, 1f)] private float illnessVulnerability;
        [SerializeField, Range(0f, 1f)] private float growthPotential;

        public string FamilyId => familyId;
        public string FamilyName => familyName;
        public int MembersCount => membersCount;
        public float Hunger => hunger;
        public float Health => health;
        public float Morale => morale;
        public float TemperatureTolerance => temperatureTolerance;
        public EmploymentStatus EmploymentStatus => employmentStatus;
        public bool IsSick => isSick;
        public bool IsHomeless => isHomeless;
        public bool IsAlive => membersCount > 0;
        public float IllnessVulnerability => illnessVulnerability;
        public float GrowthPotential => growthPotential;

        public Family(string id, string name, int members, float initialHunger, float initialHealth, float initialMorale,
            float tolerance, EmploymentStatus employment, float illnessRisk, float growthChance)
        {
            familyId = id;
            familyName = name;
            membersCount = Mathf.Max(1, members);
            hunger = Mathf.Clamp(initialHunger, 0f, 100f);
            health = Mathf.Clamp(initialHealth, 0f, 100f);
            morale = Mathf.Clamp(initialMorale, 0f, 100f);
            temperatureTolerance = tolerance;
            employmentStatus = employment;
            illnessVulnerability = Mathf.Clamp01(illnessRisk);
            growthPotential = Mathf.Clamp01(growthChance);
        }

        public void ApplyHunger(float delta) => hunger = Mathf.Clamp(hunger + delta, 0f, 100f);
        public void ApplyHealth(float delta) => health = Mathf.Clamp(health + delta, 0f, 100f);
        public void ApplyMorale(float delta) => morale = Mathf.Clamp(morale + delta, 0f, 100f);
        public void SetSick(bool sick) => isSick = sick;
        public void SetHomeless(bool homeless) => isHomeless = homeless;
        public void SetEmployment(EmploymentStatus status) => employmentStatus = status;

        public void AddMembers(int amount)
        {
            membersCount = Mathf.Max(0, membersCount + amount);
        }

        public void LoseMembers(int amount)
        {
            membersCount = Mathf.Max(0, membersCount - Mathf.Max(0, amount));
        }
    }
}
