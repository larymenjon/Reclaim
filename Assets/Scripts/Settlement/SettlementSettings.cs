using Reclaim.Construction;
using UnityEngine;

namespace Reclaim.Settlement
{
    /// <summary>
    /// Configuração do assentamento inicial e do mundo: prefabs de personagens, barraca, árvores,
    /// sucata e horizonte. Fica em Assets/Resources/SettlementSettings. Campo de prefab vazio
    /// usa um modelo simples gerado por código, então o jogo roda mesmo sem nenhum prefab.
    /// </summary>
    [CreateAssetMenu(fileName = "SettlementSettings", menuName = "Reclaim/Assentamento/Configuração do assentamento")]
    public class SettlementSettings : ScriptableObject
    {
        public const string ResourcePath = "SettlementSettings";

        [Header("Prefabs de personagens (arraste aqui)")]
        [SerializeField] private GameObject manPrefab;
        [SerializeField] private GameObject womanPrefab;
        [SerializeField] private GameObject childPrefab;
        [SerializeField] private GameObject dogPrefab;

        [Header("Acampamento inicial")]
        [Tooltip("Barraca de cada família, dispostas em círculo no meio do mapa.")]
        [SerializeField] private GameObject tentPrefab;
        [SerializeField, Min(3f)] private float campRadius = 11f;
        [SerializeField, Min(0)] private int dogsPerTwoFamilies = 1;

        [Header("Movimento dos moradores")]
        [SerializeField, Min(0.1f)] private float adultSpeed = 2.2f;
        [SerializeField, Min(0.1f)] private float childSpeed = 1.8f;
        [SerializeField, Min(0.1f)] private float dogSpeed = 3.2f;
        [Tooltip("Horário em que quem tem ofício sai para trabalhar e em que volta.")]
        [SerializeField, Range(0f, 24f)] private float workStartHour = 7f;
        [SerializeField, Range(0f, 24f)] private float workEndHour = 17f;
        [Tooltip("Depois deste horário e antes do início do trabalho, todos dormem em casa.")]
        [SerializeField, Range(0f, 24f)] private float sleepStartHour = 21f;

        [Header("Árvores (espalhadas ao iniciar)")]
        [Tooltip("Modelos de cada estado da árvore. Vazios usam os prefabs do projeto ou um modelo simples.")]
        [SerializeField] private GameObject treeStandingPrefab;
        [SerializeField] private GameObject treeStumpPrefab;
        [SerializeField] private GameObject treeSaplingPrefab;
        [SerializeField, Min(0)] private int treeCount = 700;
        [SerializeField, Min(1)] private int forestClusters = 16;
        [Tooltip("Distância máxima do acampamento em que o mapa é povoado.")]
        [SerializeField, Min(50f)] private float populateRadius = 700f;
        [SerializeField, Min(0f)] private float treeClearRadiusAroundCamp = 30f;
        [SerializeField, Min(0.5f)] private float minTreeSpacing = 3f;
        [SerializeField, Range(0f, 60f)] private float maxSlopeDegrees = 28f;
        [SerializeField, Min(1)] private int woodPerTree = 12;
        [SerializeField, Min(0.5f)] private float chopSeconds = 6f;
        [SerializeField, Min(0.5f)] private float plantSeconds = 4f;
        [Tooltip("Tempo (em segundos de jogo) para a muda virar árvore.")]
        [SerializeField, Min(1f)] private float growSeconds = 120f;

        [Header("Sucata e escombros (espalhados ao iniciar)")]
        [SerializeField] private GameObject scrapPilePrefab;
        [SerializeField, Min(0)] private int scrapPileCount = 90;
        [SerializeField, Min(1)] private int scrapPerPile = 8;
        [SerializeField, Min(0.5f)] private float collectSeconds = 5f;

        [Header("Horizonte (o mapa tem limite, mas não parece ter)")]
        [SerializeField] private bool createHorizon = true;
        [SerializeField, Min(100f)] private float horizonWidth = 3000f;
        [SerializeField, Min(10f)] private float horizonMountainHeight = 260f;
        [SerializeField] private bool useFog = true;
        [SerializeField] private Color fogDayColor = new Color(0.62f, 0.70f, 0.78f);
        [SerializeField] private Color fogNightColor = new Color(0.05f, 0.07f, 0.11f);
        [SerializeField, Min(100f)] private float fogStartDistance = 500f;
        [SerializeField, Min(200f)] private float fogEndDistance = 3200f;

        [Header("Pendências iniciais")]
        [Tooltip("Construções que o jogador precisa erguer para a colônia funcionar. Casas: uma por família.")]
        [SerializeField] private BuildingData storageBuilding;
        [SerializeField] private BuildingData industryBuilding;

        public GameObject ManPrefab => manPrefab;
        public GameObject WomanPrefab => womanPrefab;
        public GameObject ChildPrefab => childPrefab;
        public GameObject DogPrefab => dogPrefab;
        public GameObject TentPrefab => tentPrefab;
        public float CampRadius => campRadius;
        public int DogsPerTwoFamilies => dogsPerTwoFamilies;
        public float AdultSpeed => adultSpeed;
        public float ChildSpeed => childSpeed;
        public float DogSpeed => dogSpeed;
        public float WorkStartHour => workStartHour;
        public float WorkEndHour => workEndHour;
        public float SleepStartHour => sleepStartHour;
        public GameObject TreeStandingPrefab => treeStandingPrefab;
        public GameObject TreeStumpPrefab => treeStumpPrefab;
        public GameObject TreeSaplingPrefab => treeSaplingPrefab;
        public int TreeCount => treeCount;
        public int ForestClusters => forestClusters;
        public float PopulateRadius => populateRadius;
        public float TreeClearRadiusAroundCamp => treeClearRadiusAroundCamp;
        public float MinTreeSpacing => minTreeSpacing;
        public float MaxSlopeDegrees => maxSlopeDegrees;
        public int WoodPerTree => woodPerTree;
        public float ChopSeconds => chopSeconds;
        public float PlantSeconds => plantSeconds;
        public float GrowSeconds => growSeconds;
        public GameObject ScrapPilePrefab => scrapPilePrefab;
        public int ScrapPileCount => scrapPileCount;
        public int ScrapPerPile => scrapPerPile;
        public float CollectSeconds => collectSeconds;
        public bool CreateHorizon => createHorizon;
        public float HorizonWidth => horizonWidth;
        public float HorizonMountainHeight => horizonMountainHeight;
        public bool UseFog => useFog;
        public Color FogDayColor => fogDayColor;
        public Color FogNightColor => fogNightColor;
        public float FogStartDistance => fogStartDistance;
        public float FogEndDistance => fogEndDistance;
        public BuildingData StorageBuilding => storageBuilding;
        public BuildingData IndustryBuilding => industryBuilding;

        /// <summary>Carrega o asset de Resources; sem ele, usa os valores padrão e modelos simples.</summary>
        public static SettlementSettings LoadOrDefault()
        {
            SettlementSettings loaded = Resources.Load<SettlementSettings>(ResourcePath);
            if (loaded != null)
            {
                return loaded;
            }

            Debug.LogWarning("[SettlementSettings] Asset não encontrado em Resources; usando padrões. Use o menu Reclaim > Criar configuração do assentamento.");
            return CreateInstance<SettlementSettings>();
        }
    }
}
