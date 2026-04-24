using UnityEngine;

namespace Reclaim.Data
{
    [CreateAssetMenu(fileName = "New Leader", menuName = "Reclaim/Leader Data")]
    public class LeaderData : ScriptableObject
    {
        public string leaderName;
        [TextArea(3, 5)] public string description;
        public string catchphrase; // Frase_Avatar
        public string subDescription; // Ex: "Especialista em Logística"
        public Sprite portrait;
        
        [Header("Buffs / Attributes")]
        public float foodEfficiency = 1.0f;
        public float explorationSpeed = 1.0f;
        public int startingScrap = 100;
    }
}
