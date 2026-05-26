using UnityEngine;

namespace Reclaim.Road
{
    /// <summary>
    /// Thin facade used by UI/GameManager to control the road builder workflow.
    /// </summary>
    public class RoadBuilderManager : MonoBehaviour
    {
        [SerializeField] private RoadBuilderSystem _roadBuilderSystem;
        [SerializeField] private Reclaim.GameManager _gameManager;

        private bool _initialized;

        public bool IsBuilding => _roadBuilderSystem != null && _roadBuilderSystem.IsBuilding;

        private void Awake()
        {
            ResolveReferences();
            InitializeSystemIfNeeded();
        }

        public void StartBuildMode()
        {
            ResolveReferences();
            InitializeSystemIfNeeded();

            if (_roadBuilderSystem == null)
            {
                Debug.LogWarning("[RoadBuilderManager] Missing RoadBuilderSystem reference.", this);
                return;
            }

            _roadBuilderSystem.EnterBuildMode();
        }

        public void ExitBuildMode()
        {
            if (_roadBuilderSystem == null)
            {
                return;
            }

            _roadBuilderSystem.ExitBuildMode();
        }

        private void InitializeSystemIfNeeded()
        {
            if (_initialized || _roadBuilderSystem == null || _gameManager == null)
            {
                return;
            }

            _roadBuilderSystem.Initialize(_gameManager);
            _initialized = true;
        }

        private void ResolveReferences()
        {
            if (_roadBuilderSystem == null)
            {
                _roadBuilderSystem = FindFirstObjectByType<RoadBuilderSystem>();
            }

            if (_gameManager == null)
            {
                _gameManager = FindFirstObjectByType<Reclaim.GameManager>();
            }
        }
    }
}
