using System;
using UnityEngine;

namespace Reclaim.UI
{
    [CreateAssetMenu(fileName = "CharacterDatabase", menuName = "Reclaim/Character Database")]
    public class CharacterDatabase : ScriptableObject
    {
        [SerializeField] private CharacterData[] characters;

        public CharacterData[] Characters => characters;

        [Serializable]
        public class CharacterData
        {
            [Header("Informacoes Basicas")]
            [SerializeField] private string characterName;
            [SerializeField] private string description;
            [SerializeField] private string quote;
            [SerializeField] private Sprite portrait;

            [Header("Especializacao")]
            [SerializeField] private CharacterSpecialization specialization;

            [Header("Habilidades")]
            [SerializeField] private string passiveName;
            [SerializeField] private string passiveDescription;
            [SerializeField] private string activeName;
            [SerializeField] private string activeDescription;

            [Header("Buffs")]
            [SerializeField, Range(-50f, 50f)] private float constructionSpeedBonus;
            [SerializeField, Range(-50f, 50f)] private float materialCostBonus;
            [SerializeField, Range(-50f, 50f)] private float defenseBonus;
            [SerializeField, Range(-50f, 50f)] private float happinessBonus;
            [SerializeField, Range(-50f, 50f)] private float foodProductionBonus;
            [SerializeField, Range(-50f, 50f)] private float medicalEfficiencyBonus;
            [SerializeField, Range(-50f, 50f)] private float explorationSpeedBonus;
            [SerializeField, Range(-50f, 50f)] private float resourceBonus;

            [Header("Debuffs")]
            [SerializeField, Range(-50f, 50f)] private float revoltChanceBonus;
            [SerializeField, Range(-50f, 50f)] private float resourceConsumptionBonus;
            [SerializeField, Range(-50f, 50f)] private float expansionBonus;
            [SerializeField, Range(-50f, 50f)] private float securityBonus;
            [SerializeField, Range(-50f, 50f)] private float productivityBonus;
            [SerializeField, Range(-50f, 50f)] private float trustBonus;

            [Header("Evento Especial")]
            [SerializeField] private string eventName;
            [SerializeField] private string eventDescription;

            public string CharacterName => characterName;
            public string Description => description;
            public string Quote => quote;
            public Sprite Portrait => portrait;
            public CharacterSpecialization Specialization => specialization;
            public string PassiveName => passiveName;
            public string PassiveDescription => passiveDescription;
            public string ActiveName => activeName;
            public string ActiveDescription => activeDescription;
            public float ConstructionSpeedBonus => constructionSpeedBonus;
            public float MaterialCostBonus => materialCostBonus;
            public float DefenseBonus => defenseBonus;
            public float HappinessBonus => happinessBonus;
            public float FoodProductionBonus => foodProductionBonus;
            public float MedicalEfficiencyBonus => medicalEfficiencyBonus;
            public float ExplorationSpeedBonus => explorationSpeedBonus;
            public float ResourceBonus => resourceBonus;
            public float RevoltChanceBonus => revoltChanceBonus;
            public float ResourceConsumptionBonus => resourceConsumptionBonus;
            public float ExpansionBonus => expansionBonus;
            public float SecurityBonus => securityBonus;
            public float ProductivityBonus => productivityBonus;
            public float TrustBonus => trustBonus;
            public string EventName => eventName;
            public string EventDescription => eventDescription;
        }
    }
}
