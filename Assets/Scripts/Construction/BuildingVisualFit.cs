using UnityEngine;

namespace Reclaim.Construction
{
    /// <summary>
    /// Posiciona e dimensiona os modelos de uma construção. Os modelos vêm em escalas e pivôs diferentes
    /// (a fundação usa escala 36, a casa pronta 3,6), então o visual é sempre centralizado no plano,
    /// apoiado no chão e, quando preciso, escalado para casar com um tamanho de referência.
    /// </summary>
    public static class BuildingVisualFit
    {
        /// <summary>Escala o modelo (uniforme) para caber em targetSize (x, z) e o apoia no chão. Devolve a altura final.</summary>
        public static float Fit(Transform instance, Transform space, Vector2 targetSize)
        {
            if (!TryGetLocalBounds(space, instance.gameObject, out Bounds bounds) || bounds.size.x < 0.0001f || bounds.size.z < 0.0001f)
            {
                return 0f;
            }

            instance.localScale *= Mathf.Min(targetSize.x / bounds.size.x, targetSize.y / bounds.size.z);
            return Ground(instance, space);
        }

        /// <summary>Centraliza o modelo no plano do espaço e apoia a base em y = 0. Devolve a altura, ou 0 sem malha.</summary>
        public static float Ground(Transform instance, Transform space)
        {
            if (!TryGetLocalBounds(space, instance.gameObject, out Bounds bounds))
            {
                return 0f;
            }

            Vector3 shift = new Vector3(-bounds.center.x, -bounds.min.y, -bounds.center.z);
            instance.position += space.TransformVector(shift);
            return bounds.size.y;
        }

        /// <summary>Caixa que envolve todas as malhas do modelo, medida no espaço local de "space".</summary>
        public static bool TryGetLocalBounds(Transform space, GameObject root, out Bounds bounds)
        {
            bounds = default;
            bool hasBounds = false;

            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>())
            {
                if (renderer is ParticleSystemRenderer || renderer is LineRenderer || renderer is TrailRenderer)
                {
                    continue;
                }

                Bounds world = renderer.bounds;
                for (int corner = 0; corner < 8; corner++)
                {
                    Vector3 offset = new Vector3(
                        (corner & 1) == 0 ? -world.extents.x : world.extents.x,
                        (corner & 2) == 0 ? -world.extents.y : world.extents.y,
                        (corner & 4) == 0 ? -world.extents.z : world.extents.z);
                    Vector3 local = space.InverseTransformPoint(world.center + offset);

                    if (!hasBounds)
                    {
                        bounds = new Bounds(local, Vector3.zero);
                        hasBounds = true;
                    }
                    else
                    {
                        bounds.Encapsulate(local);
                    }
                }
            }

            return hasBounds;
        }
    }
}
