using System;
using UnityEngine;

namespace Reclaim.UI
{
    [Serializable]
    public class CharacterData
    {
        [Header("Informações Básicas")]
        public string characterName;
        public string description;
        public string quote;
        public Sprite portrait;
        
        [Header("Especialização")]
        public CharacterSpecialization specialization;
        
        [Header("Habilidades")]
        public string passiveName;
        public string passiveDescription;
        public string activeName;
        public string activeDescription;
        
        [Header("Buffs")]
        [Range(-50f, 50f)] public float constructionSpeedBonus;
        [Range(-50f, 50f)] public float materialCostBonus;
        [Range(-50f, 50f)] public float defenseBonus;
        [Range(-50f, 50f)] public float happinessBonus;
        [Range(-50f, 50f)] public float foodProductionBonus;
        [Range(-50f, 50f)] public float medicalEfficiencyBonus;
        [Range(-50f, 50f)] public float explorationSpeedBonus;
        [Range(-50f, 50f)] public float resourceBonus;
        
        [Header("Debuffs")]
        [Range(-50f, 50f)] public float revoltChanceBonus;
        [Range(-50f, 50f)] public float resourceConsumptionBonus;
        [Range(-50f, 50f)] public float expansionBonus;
        [Range(-50f, 50f)] public float securityBonus;
        [Range(-50f, 50f)] public float productivityBonus;
        [Range(-50f, 50f)] public float trustBonus;
        
        [Header("Evento Especial")]
        public string eventName;
        public string eventDescription;
    }

    public enum CharacterSpecialization
    {
        Construction,
        Defense,
        Food,
        Health,
        Exploration,
        Resources,
        Morale
    }
}