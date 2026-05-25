using UnityEngine;

namespace Reclaim.Road
{
    /// <summary>
    /// Configurações e prefabs do sistema de estradas.
    /// Crie via: Assets > Create > Reclaim > Road > Prefab Library
    /// </summary>
    [CreateAssetMenu(fileName = "RoadPrefabLibrary", menuName = "Reclaim/Road/Prefab Library")]
    public class RoadPrefabLibrary : ScriptableObject
    {
        [Header("Prefabs de Trecho")]
        [Tooltip("Trecho reto. Pivot no centro, comprimento = segmentLength no eixo Z.")]
        public GameObject straightPrefab;

        [Tooltip("Curva de ~90°. Pivot no vértice interno da curva. Entrada -Z, saída +X.")]
        public GameObject curvePrefab;

        [Tooltip("Cruzamento T. Pivot no centro.")]
        public GameObject tJunctionPrefab;

        [Tooltip("Cruzamento X (4 vias). Pivot no centro.")]
        public GameObject xJunctionPrefab;

        [Tooltip("Tampa de fim de rua. Pivot na boca de entrada.")]
        public GameObject endCapPrefab;

        [Header("Preview")]
        public Material previewValidMaterial;
        public Material previewInvalidMaterial;

        [Header("Medidas")]
        [Tooltip("Comprimento em unidades de 1 prefab reto.")]
        public float segmentLength = 4f;

        [Tooltip("Largura da estrada (LineRenderer de preview).")]
        public float roadWidth = 2f;

        [Tooltip("Raio de snap a nós existentes.")]
        public float snapRadius = 1.5f;

        [Tooltip("Distância mínima entre dois pontos colocados.")]
        public float minPointDistance = 2f;

        [Tooltip("Desvio máximo (graus) para considerar dois segmentos como linha reta.")]
        [Range(5f, 45f)]
        public float straightAngleThreshold = 20f;
    }
}
