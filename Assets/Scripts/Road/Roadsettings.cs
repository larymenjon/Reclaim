using UnityEngine;

namespace Reclaim.Road
{
    /// <summary>
    /// Configurações do sistema de estrada de terra procedural.
    /// Crie via: Assets > Create > Reclaim > Road > Road Settings
    /// </summary>
    [CreateAssetMenu(fileName = "RoadSettings", menuName = "Reclaim/Road/Road Settings")]
    public class RoadSettings : ScriptableObject
    {
        [Header("Visual")]
        [Tooltip("Material de terra aplicado na mesh da estrada.")]
        public Material roadMaterial;

        [Tooltip("Largura da estrada em unidades do mundo.")]
        public float roadWidth = 3f;

        [Tooltip("Quantos passos de spline por metro (mais = mais suave, mais pesado).")]
        [Range(1f, 8f)]
        public float resolutionPerMeter = 3f;

        [Tooltip("Offset vertical para evitar z-fighting com o terreno.")]
        public float verticalOffset = 0.02f;

        [Header("Forma")]
        [Tooltip("Número de vértices na seção transversal (mínimo 3). Mais vértices = bordas mais suaves.")]
        [Range(3, 10)]
        public int crossSectionVerts = 5;

        [Tooltip("A estrada afunda levemente no centro para parecer desgastada pelo uso.")]
        [Range(0f, 0.3f)]
        public float centerDepression = 0.08f;

        [Header("Snap / Input")]
        [Tooltip("Raio de snap a nós existentes.")]
        public float snapRadius = 1.5f;

        [Tooltip("Distância mínima entre dois pontos colocados.")]
        public float minPointDistance = 2f;

        [Header("Preview LineRenderer")]
        public Material previewMaterial;
        public float previewWidth = 3f;

        [Header("Terreno")]
        [Tooltip("Layer(s) do terreno para Raycast de altura.")]
        public LayerMask terrainLayer;

        [Tooltip("Altura máxima de busca acima do ponto ao amostrar o terreno.")]
        public float terrainSampleHeight = 50f;
    }
}