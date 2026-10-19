using System;
using UnityEngine;

namespace Reclaim.Road
{
    /// <summary>
    /// Ponto de entrada da ferramenta de estrada para o ToolModeController e para botões da HUD.
    /// Mantido separado do RoadBuilderSystem para que a cena possa trocar a implementação
    /// sem mexer nas referências da UI.
    /// </summary>
    public class RoadBuilderManager : MonoBehaviour
    {
        [SerializeField] private RoadBuilderSystem _roadBuilderSystem;

        public bool IsBuilding => _roadBuilderSystem != null && _roadBuilderSystem.IsBuilding;

        /// <summary>A ferramenta terminou (estrada confirmada ou cancelada).</summary>
        public event Action OnFinished;

        private void Awake()
        {
            if (_roadBuilderSystem == null)
            {
                Debug.LogError("[RoadBuilderManager] RoadBuilderSystem não configurado.", this);
                enabled = false;
                return;
            }

            _roadBuilderSystem.OnFinished += HandleFinished;
        }

        private void OnDestroy()
        {
            if (_roadBuilderSystem != null)
            {
                _roadBuilderSystem.OnFinished -= HandleFinished;
            }
        }

        public void SetActive(bool active)
        {
            if (_roadBuilderSystem == null)
            {
                return;
            }

            if (active)
            {
                _roadBuilderSystem.Enable();
            }
            else
            {
                _roadBuilderSystem.Disable();
            }
        }

        private void HandleFinished()
        {
            OnFinished?.Invoke();
        }
    }
}
