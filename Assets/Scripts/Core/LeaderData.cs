using UnityEngine;

namespace Reclaim.Data
{
    [CreateAssetMenu(fileName = "New Leader", menuName = "Reclaim/Leader Data")]
    public class LeaderData : ScriptableObject
    {
        [SerializeField] private string leaderName;
        [SerializeField, TextArea(3, 5)] private string description;
        [SerializeField] private string catchphrase;
        [SerializeField] private string subDescription;
        [SerializeField] private Sprite portrait;

        [Header("Buffs / Attributes")]
        [SerializeField] private float foodEfficiency = 1.0f;
        [SerializeField] private float explorationSpeed = 1.0f;
        [SerializeField] private int startingScrap = 100;

        public string LeaderName => leaderName;
        public string Description => description;
        public string Catchphrase => catchphrase;
        public string SubDescription => subDescription;
        public Sprite Portrait => portrait;
        public float FoodEfficiency => foodEfficiency;
        public float ExplorationSpeed => explorationSpeed;
        public int StartingScrap => startingScrap;
    }
}
