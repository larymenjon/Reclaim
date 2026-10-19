using System.Collections.Generic;
using Reclaim.Economy;
using Reclaim.Work;
using UnityEngine;
using UnityEngine.Serialization;

namespace Reclaim.Construction
{
    /// <summary>Definição de uma construção: visual, tamanho no grid, custo e tempo de obra.</summary>
    [CreateAssetMenu(fileName = "BuildingData", menuName = "Reclaim/Construção/Definição de Construção")]
    public class BuildingData : ScriptableObject
    {
        [Header("Identidade")]
        [SerializeField] private string id = "building";
        [SerializeField] private string displayName = "Nova construção";
        [SerializeField, TextArea(2, 5)] private string description = "";
        [SerializeField] private Sprite icon;

        [Header("Posicionamento")]
        [SerializeField] private GameObject prefab;
        [SerializeField] private Vector2Int size = Vector2Int.one;
        [SerializeField] private bool canRotate = true;
        [Tooltip("Multiplica a escala original do modelo. 1 = como foi modelado.")]
        [SerializeField, Min(0.1f)] private float visualScale = 1f;
        [Tooltip("Ignora a escala do modelo e o ajusta para caber na pegada definida em Size.")]
        [SerializeField] private bool fitToFootprint;
        [Tooltip("Quanto da pegada o modelo ocupa. 1 = encosta nas bordas das células.")]
        [SerializeField, Range(0.5f, 1f)] private float footprintFill = 0.9f;
        [Tooltip("Conta no total de casas exibido na HUD quando a obra termina.")]
        [SerializeField] private bool countsAsHouse;

        [Header("Custo")]
        [FormerlySerializedAs("cost")]
        [SerializeField, Min(0)] private int woodCost = 10;
        [SerializeField, Min(0)] private int scrapCost;
        [SerializeField, Min(0f)] private float constructionDurationSeconds = 10f;

        [Header("Trabalho")]
        [Tooltip("Ofícios que o jogador pode designar quando a obra termina. Vazio = não é local de trabalho.")]
        [SerializeField] private List<JobSlot> jobSlots = new List<JobSlot>();

        [Header("Estágios visuais da obra (opcionais)")]
        [SerializeField] private GameObject foundationStagePrefab;
        [SerializeField] private GameObject midStagePrefab;
        [SerializeField] private GameObject completedStagePrefab;

        private readonly List<ResourceAmount> _costs = new List<ResourceAmount>(2);

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public GameObject Prefab => prefab;
        public Vector2Int Size => size;
        public bool CanRotate => canRotate;
        public float VisualScale => visualScale;
        public bool FitToFootprint => fitToFootprint;
        public float FootprintFill => footprintFill;
        public bool CountsAsHouse => countsAsHouse;
        public int WoodCost => woodCost;
        public int ScrapCost => scrapCost;
        public float ConstructionDurationSeconds => constructionDurationSeconds;
        public IReadOnlyList<JobSlot> JobSlots => jobSlots;
        public GameObject FoundationStagePrefab => foundationStagePrefab;
        public GameObject MidStagePrefab => midStagePrefab;
        public GameObject CompletedStagePrefab => completedStagePrefab;

        public IReadOnlyList<ResourceAmount> Costs
        {
            get
            {
                _costs.Clear();
                if (woodCost > 0)
                {
                    _costs.Add(new ResourceAmount(ResourceType.Wood, woodCost));
                }

                if (scrapCost > 0)
                {
                    _costs.Add(new ResourceAmount(ResourceType.Scrap, scrapCost));
                }

                return _costs;
            }
        }
    }
}
