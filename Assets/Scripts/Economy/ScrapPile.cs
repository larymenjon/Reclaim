using System.Collections.Generic;
using UnityEngine;

namespace Reclaim.Economy
{
    /// <summary>
    /// Monte de sucata ou escombros espalhado pelo mapa. Um catador reserva o monte,
    /// vai até ele, recolhe e leva a sucata para a oficina.
    /// </summary>
    public class ScrapPile : MonoBehaviour
    {
        public static readonly List<ScrapPile> All = new List<ScrapPile>();

        [SerializeField, Min(1)] private int amount = 8;
        [SerializeField, Min(0.1f)] private float collectDurationSeconds = 4f;

        private Object _reservedBy;

        public int Amount => amount;
        public float CollectDurationSeconds => collectDurationSeconds;
        public bool IsAvailable => _reservedBy == null && amount > 0;

        /// <summary>Define a quantidade e o visual. Prefab nulo = monte simples gerado por código.</summary>
        public void Configure(int scrapAmount, float collectSeconds, GameObject visualPrefab, bool isRubble)
        {
            amount = Mathf.Max(1, scrapAmount);
            collectDurationSeconds = Mathf.Max(0.1f, collectSeconds);

            if (visualPrefab != null)
            {
                GameObject visual = Instantiate(visualPrefab, transform);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localRotation = Quaternion.identity;
                return;
            }

            BuildPlaceholder(isRubble);
        }

        public bool TryReserve(Object by)
        {
            if (!IsAvailable || by == null)
            {
                return false;
            }

            _reservedBy = by;
            return true;
        }

        public void Release(Object by)
        {
            if (_reservedBy == by)
            {
                _reservedBy = null;
            }
        }

        /// <summary>Recolhe tudo e some do mapa. Devolve a sucata levada.</summary>
        public int Collect()
        {
            int taken = amount;
            amount = 0;
            Destroy(gameObject);
            return taken;
        }

        private void OnEnable()
        {
            All.Add(this);
        }

        private void OnDisable()
        {
            All.Remove(this);
        }

        private void BuildPlaceholder(bool isRubble)
        {
            Color metal = new Color(0.42f, 0.44f, 0.46f);
            Color rust = new Color(0.45f, 0.27f, 0.17f);
            Color stone = new Color(0.5f, 0.48f, 0.44f);

            int pieces = Random.Range(4, 8);
            for (int i = 0; i < pieces; i++)
            {
                GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(piece.GetComponent<Collider>());
                piece.transform.SetParent(transform, false);
                piece.transform.localPosition = new Vector3(Random.Range(-1.2f, 1.2f), Random.Range(0.1f, 0.4f), Random.Range(-1.2f, 1.2f));
                piece.transform.localRotation = Random.rotation;
                piece.transform.localScale = new Vector3(Random.Range(0.3f, 0.9f), Random.Range(0.2f, 0.6f), Random.Range(0.3f, 0.9f));
                piece.GetComponent<Renderer>().material.color = isRubble ? stone : (i % 2 == 0 ? metal : rust);
            }
        }
    }
}
