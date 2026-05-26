using System.Collections.Generic;
using UnityEngine;

namespace Reclaim.Road
{
    /// <summary>
    /// Utilitários de spline Catmull-Rom para suavizar caminhos de estrada.
    /// </summary>
    public static class RoadSpline
    {
        /// <summary>
        /// Converte uma lista de pontos de controle em uma spline Catmull-Rom suavizada.
        /// </summary>
        /// <param name="controlPoints">Pontos colocados pelo jogador.</param>
        /// <param name="resolutionPerMeter">Amostras por metro de distância.</param>
        /// <returns>Lista densa de pontos ao longo da curva.</returns>
        public static List<Vector3> BuildSpline(IReadOnlyList<Vector3> controlPoints, float resolutionPerMeter)
        {
            var result = new List<Vector3>();
            if (controlPoints.Count < 2) return result;

            // Para Catmull-Rom, adicionamos pontos fantasma nas extremidades
            var pts = new List<Vector3>(controlPoints.Count + 2);
            pts.Add(controlPoints[0] + (controlPoints[0] - controlPoints[1]));   // fantasma início
            pts.AddRange(controlPoints);
            pts.Add(controlPoints[^1] + (controlPoints[^1] - controlPoints[^2])); // fantasma fim

            for (int i = 1; i < pts.Count - 2; i++)
            {
                Vector3 p0 = pts[i - 1];
                Vector3 p1 = pts[i];
                Vector3 p2 = pts[i + 1];
                Vector3 p3 = pts[i + 2];

                float segmentLength = Vector3.Distance(p1, p2);
                int   steps         = Mathf.Max(2, Mathf.RoundToInt(segmentLength * resolutionPerMeter));

                for (int s = 0; s < steps; s++)
                {
                    float t = (float)s / steps;
                    result.Add(CatmullRom(p0, p1, p2, p3, t));
                }
            }

            // Adiciona o ponto final exato
            result.Add(controlPoints[^1]);
            return result;
        }

        /// <summary>
        /// Calcula a tangente (direção) num ponto da spline.
        /// </summary>
        public static Vector3 GetTangent(IReadOnlyList<Vector3> splinePoints, int index)
        {
            if (splinePoints.Count < 2) return Vector3.forward;
            if (index == 0)
                return (splinePoints[1] - splinePoints[0]).normalized;
            if (index == splinePoints.Count - 1)
                return (splinePoints[^1] - splinePoints[^2]).normalized;
            return (splinePoints[index + 1] - splinePoints[index - 1]).normalized;
        }

        // ── Fórmula Catmull-Rom ───────────────────────────────────────

        private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;

            return 0.5f * (
                2f * p1 +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3
            );
        }
    }
}