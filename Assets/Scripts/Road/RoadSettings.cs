using UnityEngine;

namespace Reclaim.Road
{
    /// <summary>
    /// Configurações do sistema de estrada de terra procedural.
    /// Crie via: Assets > Create > Reclaim > Estrada > Configurações de Estrada
    /// </summary>
    [CreateAssetMenu(fileName = "RoadSettings", menuName = "Reclaim/Estrada/Configurações de Estrada")]
    public class RoadSettings : ScriptableObject
    {
        [Header("Visual")]
        [Tooltip("Material de terra aplicado na mesh da estrada.")]
        [SerializeField] private Material roadMaterial;

        [Tooltip("Largura da estrada em unidades do mundo.")]
        [SerializeField] private float roadWidth = 3f;

        [Tooltip("Quantos passos de spline por metro (mais = mais suave, mais pesado).")]
        [Range(1f, 8f)]
        [SerializeField] private float resolutionPerMeter = 3f;

        [Tooltip("Offset vertical para evitar z-fighting com o terreno.")]
        [SerializeField] private float verticalOffset = 0.02f;

        [Header("Forma")]
        [Tooltip("Número de vértices na seção transversal (mínimo 3). Mais vértices = bordas mais suaves.")]
        [Range(3, 10)]
        [SerializeField] private int crossSectionVerts = 5;

        [Tooltip("A estrada afunda levemente no centro para parecer desgastada pelo uso.")]
        [Range(0f, 0.3f)]
        [SerializeField] private float centerDepression = 0.08f;

        [Header("Entrada")]
        [Tooltip("Distância mínima entre dois pontos colocados.")]
        [SerializeField] private float minPointDistance = 2f;

        [Header("Preview LineRenderer")]
        [SerializeField] private Material previewMaterial;
        [SerializeField] private float previewWidth = 3f;

        [Header("Terreno")]
        [Tooltip("Layer(s) do terreno para Raycast de altura.")]
        [SerializeField] private LayerMask terrainLayer;

        [Tooltip("Altura máxima de busca acima do ponto ao amostrar o terreno.")]
        [SerializeField] private float terrainSampleHeight = 50f;

        public Material RoadMaterial => roadMaterial;
        public float RoadWidth => roadWidth;
        public float ResolutionPerMeter => resolutionPerMeter;
        public float VerticalOffset => verticalOffset;
        public int CrossSectionVerts => crossSectionVerts;
        public float CenterDepression => centerDepression;
        public float MinPointDistance => minPointDistance;
        public Material PreviewMaterial => previewMaterial;
        public float PreviewWidth => previewWidth;
        public LayerMask TerrainLayer => terrainLayer;
        public float TerrainSampleHeight => terrainSampleHeight;
    }
}