using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Reclaim.Economy.Forestry
{
    /// <summary>
    /// Uma árvore do mapa. De pé, é cortada por um lenhador e vira toco; o toco só volta
    /// a ser árvore quando alguém planta (muda crescendo até ficar de pé de novo).
    /// Os visuais de cada estado são criados só quando o estado aparece pela primeira vez.
    /// </summary>
    public class TreeResourceNode : MonoBehaviour
    {
        private const float SaplingScale = 0.3f;

        public static readonly List<TreeResourceNode> All = new List<TreeResourceNode>();

        [Header("Visuals")]
        [SerializeField] private GameObject standingPrefab;
        [SerializeField] private GameObject choppedPrefab;
        [SerializeField] private GameObject growingPrefab;
        [SerializeField] private GameObject standingVisual;
        [SerializeField] private GameObject choppedVisual;
        [SerializeField] private GameObject growingVisual;

        [Header("Economy")]
        [SerializeField, Min(1)] private int woodYield = 12;

        [Header("Timings")]
        [SerializeField, Min(0.1f)] private float chopDurationSeconds = 3f;
        [SerializeField, Min(0.1f)] private float plantDurationSeconds = 3f;
        [SerializeField, Min(0.1f)] private float growDurationSeconds = 60f;

        [Header("Ciclo")]
        [Tooltip("O toco só vira árvore de novo quando alguém planta. Desligado, a árvore rebrota sozinha.")]
        [SerializeField] private bool requiresReplanting = true;
        [SerializeField, Min(0.1f)] private float autoRegrowDelaySeconds = 12f;

        private TreeForestrySystem _forestrySystem;
        private Object _reservedBy;
        private float _growProgress01;

        public TreeLifecycleState State { get; private set; } = TreeLifecycleState.Standing;
        public bool IsBusy { get; private set; }
        public bool IsReserved => _reservedBy != null;
        public bool IsHarvestable => State == TreeLifecycleState.Standing && !IsBusy && !IsReserved;
        /// <summary>Toco livre, esperando alguém plantar.</summary>
        public bool NeedsReplant => requiresReplanting && State == TreeLifecycleState.Chopped && !IsBusy && !IsReserved;
        public int WoodYield => woodYield;
        public float ChopDurationSeconds => chopDurationSeconds;
        public float PlantDurationSeconds => plantDurationSeconds;

        // ---------- Configuração em tempo de execução ----------

        /// <summary>Define os modelos de cada estado. Prefab nulo = usa um modelo simples gerado por código.</summary>
        public void Configure(GameObject standing, GameObject chopped, GameObject growing, int yield, float chopSeconds, float plantSeconds, float growSeconds)
        {
            standingPrefab = standing;
            choppedPrefab = chopped;
            growingPrefab = growing;
            woodYield = Mathf.Max(1, yield);
            chopDurationSeconds = Mathf.Max(0.1f, chopSeconds);
            plantDurationSeconds = Mathf.Max(0.1f, plantSeconds);
            growDurationSeconds = Mathf.Max(0.1f, growSeconds);
            ApplyStateVisual();
        }

        // ---------- Trabalho por moradores ----------

        /// <summary>Reserva a árvore (ou o toco) para um morador, para dois não irem ao mesmo lugar.</summary>
        public bool TryReserve(Object by)
        {
            if (by == null || IsReserved || IsBusy)
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

        /// <summary>Árvore derrubada: vira toco. Devolve a madeira que o lenhador leva.</summary>
        public int CompleteChop()
        {
            if (State != TreeLifecycleState.Standing)
            {
                return 0;
            }

            StopAllCoroutines();
            State = TreeLifecycleState.Chopped;
            ApplyStateVisual();
            if (!requiresReplanting)
            {
                StartCoroutine(AutoRegrowRoutine());
            }

            return woodYield;
        }

        /// <summary>Planta uma muda no toco. Ela cresce sozinha até ficar de pé.</summary>
        public bool Plant()
        {
            if (State != TreeLifecycleState.Chopped)
            {
                return false;
            }

            StopAllCoroutines();
            StartCoroutine(GrowRoutine());
            return true;
        }

        // ---------- Corte automático antigo (sem moradores) ----------

        public bool TryHarvest(TreeForestrySystem forestrySystem)
        {
            if (!IsHarvestable || forestrySystem == null)
            {
                return false;
            }

            if (!forestrySystem.TryReserveWorker())
            {
                return false;
            }

            StartCoroutine(HarvestRoutine(forestrySystem));
            return true;
        }

        public bool TryClearForConstruction(TreeForestrySystem forestrySystem)
        {
            if (State == TreeLifecycleState.Chopped || forestrySystem == null)
            {
                return false;
            }

            bool hadBusyWorker = IsBusy;
            StopAllCoroutines();
            IsBusy = false;
            _reservedBy = null;

            if (hadBusyWorker)
            {
                forestrySystem.ReleaseWorker();
            }

            State = TreeLifecycleState.Chopped;
            ApplyStateVisual();
            forestrySystem.DepositWood(woodYield, transform.position);
            _forestrySystem = null;
            return true;
        }

        // ---------- Unity ----------

        private void OnEnable()
        {
            All.Add(this);
        }

        private void OnDisable()
        {
            All.Remove(this);
        }

        private void Awake()
        {
            ApplyStateVisual();
        }

        private void OnDestroy()
        {
            if (IsBusy && _forestrySystem != null)
            {
                _forestrySystem.ReleaseWorker();
            }
        }

        private IEnumerator HarvestRoutine(TreeForestrySystem forestrySystem)
        {
            _forestrySystem = forestrySystem;
            IsBusy = true;
            yield return new WaitForSeconds(chopDurationSeconds);

            State = TreeLifecycleState.Chopped;
            ApplyStateVisual();
            forestrySystem.DepositWood(woodYield, transform.position);
            forestrySystem.ReleaseWorker();
            IsBusy = false;
            _forestrySystem = null;

            if (!requiresReplanting)
            {
                yield return AutoRegrowRoutine();
            }
        }

        private IEnumerator AutoRegrowRoutine()
        {
            yield return new WaitForSeconds(autoRegrowDelaySeconds);
            yield return GrowRoutine();
        }

        private IEnumerator GrowRoutine()
        {
            State = TreeLifecycleState.Growing;
            _growProgress01 = 0f;
            ApplyStateVisual();

            while (_growProgress01 < 1f)
            {
                _growProgress01 = Mathf.Clamp01(_growProgress01 + UnityEngine.Time.deltaTime / growDurationSeconds);
                ScaleGrowingVisual();
                yield return null;
            }

            State = TreeLifecycleState.Standing;
            ApplyStateVisual();
        }

        // ---------- Visuais ----------

        private void ApplyStateVisual()
        {
            SetActiveIfExists(standingVisual, State == TreeLifecycleState.Standing);
            SetActiveIfExists(choppedVisual, State == TreeLifecycleState.Chopped);
            SetActiveIfExists(growingVisual, State == TreeLifecycleState.Growing);

            switch (State)
            {
                case TreeLifecycleState.Standing:
                    standingVisual = EnsureVisual(standingVisual, standingPrefab, PrimitiveKind.Standing);
                    break;
                case TreeLifecycleState.Chopped:
                    choppedVisual = EnsureVisual(choppedVisual, choppedPrefab, PrimitiveKind.Stump);
                    break;
                case TreeLifecycleState.Growing:
                    growingVisual = EnsureVisual(growingVisual, growingPrefab != null ? growingPrefab : standingPrefab, PrimitiveKind.Standing);
                    ScaleGrowingVisual();
                    break;
            }
        }

        private void ScaleGrowingVisual()
        {
            if (growingVisual == null || State != TreeLifecycleState.Growing)
            {
                return;
            }

            growingVisual.transform.localScale = Vector3.one * Mathf.Lerp(SaplingScale, 1f, _growProgress01);
        }

        private GameObject EnsureVisual(GameObject existing, GameObject prefab, PrimitiveKind fallbackKind)
        {
            if (existing != null)
            {
                existing.SetActive(true);
                return existing;
            }

            GameObject created = prefab != null ? Instantiate(prefab, transform) : BuildPrimitive(fallbackKind);
            created.transform.SetParent(transform, false);
            created.transform.localPosition = Vector3.zero;
            created.transform.localRotation = Quaternion.identity;
            return created;
        }

        private static void SetActiveIfExists(GameObject visual, bool active)
        {
            if (visual != null)
            {
                visual.SetActive(active);
            }
        }

        private enum PrimitiveKind
        {
            Standing,
            Stump
        }

        /// <summary>Modelo simples para quando não há prefab: tronco e copa, ou só o toco.</summary>
        private static GameObject BuildPrimitive(PrimitiveKind kind)
        {
            bool standing = kind == PrimitiveKind.Standing;
            GameObject root = new GameObject(standing ? "Tree_Placeholder" : "Stump_Placeholder");
            AddPart(root.transform, PrimitiveType.Cylinder, new Vector3(0f, standing ? 2f : 0.3f, 0f),
                standing ? new Vector3(0.6f, 2f, 0.6f) : new Vector3(0.7f, 0.3f, 0.7f), new Color(0.38f, 0.26f, 0.15f));

            if (standing)
            {
                AddPart(root.transform, PrimitiveType.Sphere, new Vector3(0f, 5f, 0f), new Vector3(3.2f, 3.6f, 3.2f), new Color(0.2f, 0.38f, 0.16f));
            }

            return root;
        }

        private static void AddPart(Transform parent, PrimitiveType type, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            Destroy(part.GetComponent<Collider>());
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            part.GetComponent<Renderer>().material.color = color;
        }
    }
}
