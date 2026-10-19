using UnityEngine;

namespace Reclaim.Leaders
{
    /// <summary>
    /// Líder escolhido no Novo Jogo: identidade, retrato, especialização e habilidades.
    /// Os blocos de habilidade seguem o componente "Habilidade" do Figma (Passiva, Ativa, Dilema).
    /// </summary>
    [CreateAssetMenu(fileName = "Lider", menuName = "Reclaim/Líderes/Líder")]
    public class LeaderData : ScriptableObject
    {
        [Header("Identidade")]
        [SerializeField] private string leaderName;
        [SerializeField] private string subDescription;
        [SerializeField, TextArea(3, 5)] private string description;
        [SerializeField] private string catchphrase;
        [SerializeField] private Sprite portrait;
        [SerializeField] private LeaderSpecialization specialization;

        [Header("Habilidades (texto exibido na seleção)")]
        [SerializeField] private string passiveName;
        [SerializeField, TextArea(2, 3)] private string passiveDescription;
        [SerializeField] private string activeName;
        [SerializeField, TextArea(2, 3)] private string activeDescription;
        [SerializeField, Min(0f)] private float activeCooldownDays = 10f;
        [SerializeField] private string dilemmaName;
        [SerializeField, TextArea(2, 3)] private string dilemmaDescription;

        [Header("Modificadores (ainda não aplicados na simulação do alpha)")]
        [SerializeField] private float foodEfficiency = 1f;
        [SerializeField] private float explorationSpeed = 1f;
        [SerializeField] private int startingScrap = 100;

        public string LeaderName => leaderName;
        public string SubDescription => subDescription;
        public string Description => description;
        public string Catchphrase => catchphrase;
        public Sprite Portrait => portrait;
        public LeaderSpecialization Specialization => specialization;
        public string PassiveName => passiveName;
        public string PassiveDescription => passiveDescription;
        public string ActiveName => activeName;
        public string ActiveDescription => activeDescription;
        public float ActiveCooldownDays => activeCooldownDays;
        public string DilemmaName => dilemmaName;
        public string DilemmaDescription => dilemmaDescription;
        public float FoodEfficiency => foodEfficiency;
        public float ExplorationSpeed => explorationSpeed;
        public int StartingScrap => startingScrap;
    }
}
