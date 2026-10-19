using UnityEngine;

namespace Reclaim.Settlement
{
    /// <summary>
    /// Modelos simples (primitivas) usados quando o prefab real ainda não foi arrastado
    /// para o SettlementSettings. Servem só para o jogo rodar e ser testado.
    /// </summary>
    public static class PlaceholderFactory
    {
        public static GameObject CreatePerson(CitizenKind kind)
        {
            GameObject root = new GameObject(kind + "_Placeholder");
            float height = kind == CitizenKind.Child ? 1.1f : (kind == CitizenKind.Man ? 1.85f : 1.7f);
            Color color = kind == CitizenKind.Man ? new Color(0.25f, 0.42f, 0.7f)
                : kind == CitizenKind.Woman ? new Color(0.75f, 0.3f, 0.35f)
                : new Color(0.95f, 0.8f, 0.3f);

            AddPart(root.transform, PrimitiveType.Capsule, new Vector3(0f, height * 0.5f, 0f), new Vector3(0.55f, height * 0.5f, 0.55f), color);
            return root;
        }

        public static GameObject CreateDog()
        {
            GameObject root = new GameObject("Dog_Placeholder");
            Color fur = new Color(0.5f, 0.34f, 0.2f);
            AddPart(root.transform, PrimitiveType.Cube, new Vector3(0f, 0.45f, 0f), new Vector3(0.35f, 0.4f, 0.85f), fur);
            AddPart(root.transform, PrimitiveType.Cube, new Vector3(0f, 0.7f, 0.5f), new Vector3(0.3f, 0.3f, 0.3f), fur);
            return root;
        }

        /// <summary>Barraca em A: duas abas inclinadas e o fundo.</summary>
        public static GameObject CreateTent()
        {
            GameObject root = new GameObject("Tent_Placeholder");
            Color canvas = new Color(0.72f, 0.64f, 0.45f);
            const float tilt = 48f;

            AddPart(root.transform, PrimitiveType.Cube, new Vector3(-0.9f, 1.05f, 0f), new Vector3(0.1f, 2.6f, 3.4f), canvas)
                .localRotation = Quaternion.Euler(0f, 0f, -tilt);
            AddPart(root.transform, PrimitiveType.Cube, new Vector3(0.9f, 1.05f, 0f), new Vector3(0.1f, 2.6f, 3.4f), canvas)
                .localRotation = Quaternion.Euler(0f, 0f, tilt);
            AddPart(root.transform, PrimitiveType.Cube, new Vector3(0f, 0.05f, 0f), new Vector3(2.4f, 0.1f, 3.4f), new Color(0.3f, 0.26f, 0.2f));
            return root;
        }

        private static Transform AddPart(Transform parent, PrimitiveType type, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            Object.Destroy(part.GetComponent<Collider>());
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            part.GetComponent<Renderer>().material.color = color;
            return part.transform;
        }
    }
}
