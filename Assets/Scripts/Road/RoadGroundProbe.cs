using UnityEngine;

namespace Reclaim.Road
{
    /// <summary>
    /// Raycast que ignora paredes e encostas íngremes: devolve o acerto mais próximo
    /// cuja normal aponta para cima. Compartilhado pelo cursor e pela malha da estrada.
    /// </summary>
    public static class RoadGroundProbe
    {
        private const float MinUpDot = 0.55f;
        private static readonly RaycastHit[] Hits = new RaycastHit[32];

        public static bool TryRaycast(Ray ray, float maxDistance, LayerMask mask, out RaycastHit bestHit)
        {
            int count = Physics.RaycastNonAlloc(ray, Hits, maxDistance, mask, QueryTriggerInteraction.Ignore);
            int bestIndex = -1;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                if (Vector3.Dot(Hits[i].normal, Vector3.up) < MinUpDot || Hits[i].distance >= bestDistance)
                {
                    continue;
                }

                bestDistance = Hits[i].distance;
                bestIndex = i;
            }

            bestHit = bestIndex >= 0 ? Hits[bestIndex] : default;
            return bestIndex >= 0;
        }
    }
}
