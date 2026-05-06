using System.Collections;
using UnityEngine;

namespace Reclaim.Resources.Forestry
{
    public class TreeResourceNode : MonoBehaviour
    {
        [Header("Visuals")]
        [SerializeField] private GameObject standingVisual;
        [SerializeField] private GameObject choppedVisual;
        [SerializeField] private GameObject growingVisual;

        [Header("Economy")]
        [SerializeField, Min(1)] private int woodYield = 12;

        [Header("Timings")]
        [SerializeField, Min(0.1f)] private float chopDurationSeconds = 3f;
        [SerializeField, Min(0.1f)] private float regrowDelaySeconds = 12f;
        [SerializeField, Min(0.1f)] private float growDurationSeconds = 8f;

        public TreeLifecycleState State { get; private set; } = TreeLifecycleState.Standing;
        public bool IsBusy { get; private set; }
        public bool IsHarvestable => State == TreeLifecycleState.Standing && !IsBusy;

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

        private void Awake()
        {
            ApplyStateVisual();
        }

        private IEnumerator HarvestRoutine(TreeForestrySystem forestrySystem)
        {
            IsBusy = true;
            yield return new WaitForSeconds(chopDurationSeconds);

            State = TreeLifecycleState.Chopped;
            ApplyStateVisual();
            forestrySystem.AddWood(woodYield);
            forestrySystem.ReleaseWorker();

            yield return new WaitForSeconds(regrowDelaySeconds);
            State = TreeLifecycleState.Growing;
            ApplyStateVisual();

            yield return new WaitForSeconds(growDurationSeconds);
            State = TreeLifecycleState.Standing;
            ApplyStateVisual();
            IsBusy = false;
        }

        private void ApplyStateVisual()
        {
            if (standingVisual != null) standingVisual.SetActive(State == TreeLifecycleState.Standing);
            if (choppedVisual != null) choppedVisual.SetActive(State == TreeLifecycleState.Chopped);
            if (growingVisual != null) growingVisual.SetActive(State == TreeLifecycleState.Growing);
        }
    }
}
