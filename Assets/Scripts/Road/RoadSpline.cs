using System.Collections.Generic;
using UnityEngine;

namespace Reclaim.Road
{
    /// <summary>
    /// Ponto de controle da estrada. Sem direção manual, a tangente é automática (Catmull-Rom).
    /// Com direção manual (jogador arrastou ao colocar o ponto), a estrada passa por ele nessa direção.
    /// </summary>
    public struct RoadNode
    {
        public Vector3 Position;
        /// <summary>Direção de passagem escolhida pelo jogador. Zero = automática.</summary>
        public Vector3 ManualDirection;

        public bool HasManualDirection => ManualDirection.sqrMagnitude > 0.0001f;

        public RoadNode(Vector3 position)
        {
            Position = position;
            ManualDirection = Vector3.zero;
        }
    }

    /// <summary>
    /// Curvas de estrada: segmentos Hermite entre os nós. Sem direções manuais o resultado é
    /// idêntico a uma Catmull-Rom; com direções manuais o jogador controla cada curva.
    /// </summary>
    public static class RoadSpline
    {
        /// <summary>Spline suave por pontos simples, com tangentes automáticas.</summary>
        public static List<Vector3> BuildSpline(IReadOnlyList<Vector3> controlPoints, float resolutionPerMeter)
        {
            List<RoadNode> nodes = new List<RoadNode>(controlPoints.Count);
            for (int i = 0; i < controlPoints.Count; i++)
            {
                nodes.Add(new RoadNode(controlPoints[i]));
            }

            return BuildSpline(nodes, resolutionPerMeter);
        }

        /// <summary>Spline pelos nós, respeitando a direção manual de cada um.</summary>
        public static List<Vector3> BuildSpline(IReadOnlyList<RoadNode> nodes, float resolutionPerMeter)
        {
            List<Vector3> result = new List<Vector3>();
            if (nodes.Count < 2)
            {
                return result;
            }

            for (int i = 0; i < nodes.Count - 1; i++)
            {
                Vector3 p0 = nodes[i].Position;
                Vector3 p1 = nodes[i + 1].Position;
                float length = Vector3.Distance(p0, p1);
                Vector3 m0 = TangentAt(nodes, i, length);
                Vector3 m1 = TangentAt(nodes, i + 1, length);

                int steps = Mathf.Max(2, Mathf.RoundToInt(length * resolutionPerMeter));
                for (int s = 0; s < steps; s++)
                {
                    result.Add(Hermite(p0, m0, p1, m1, (float)s / steps));
                }
            }

            result.Add(nodes[nodes.Count - 1].Position);
            return result;
        }

        /// <summary>Direção (tangente) num ponto da spline.</summary>
        public static Vector3 GetTangent(IReadOnlyList<Vector3> splinePoints, int index)
        {
            if (splinePoints.Count < 2) return Vector3.forward;
            if (index == 0)
                return (splinePoints[1] - splinePoints[0]).normalized;
            if (index == splinePoints.Count - 1)
                return (splinePoints[^1] - splinePoints[^2]).normalized;
            return (splinePoints[index + 1] - splinePoints[index - 1]).normalized;
        }

        /// <summary>Tangente Hermite do nó, em escala do segmento que está sendo desenhado.</summary>
        private static Vector3 TangentAt(IReadOnlyList<RoadNode> nodes, int index, float segmentLength)
        {
            RoadNode node = nodes[index];
            if (node.HasManualDirection)
            {
                return node.ManualDirection.normalized * segmentLength;
            }

            if (index == 0)
            {
                return nodes[1].Position - node.Position;
            }

            if (index == nodes.Count - 1)
            {
                return node.Position - nodes[index - 1].Position;
            }

            return 0.5f * (nodes[index + 1].Position - nodes[index - 1].Position);
        }

        private static Vector3 Hermite(Vector3 p0, Vector3 m0, Vector3 p1, Vector3 m1, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            return (2f * t3 - 3f * t2 + 1f) * p0
                 + (t3 - 2f * t2 + t) * m0
                 + (-2f * t3 + 3f * t2) * p1
                 + (t3 - t2) * m1;
        }
    }
}
