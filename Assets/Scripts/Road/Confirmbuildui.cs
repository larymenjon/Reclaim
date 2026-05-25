using System;
using UnityEngine;
using UnityEngine.UI;

namespace Reclaim.Road
{
    /// <summary>
    /// Botão de confirmação (martelo) em World Space que aparece perto da estrada.
    ///
    /// Setup:
    ///  1. Crie um Canvas (World Space), desative-o → referencie em confirmCanvas.
    ///  2. Adicione um Button dentro com ícone de martelo → referencie em confirmButton.
    ///  3. Adicione este script no mesmo objeto do Canvas (ou num manager).
    /// </summary>
    public class ConfirmBuildUI : MonoBehaviour
    {
        [SerializeField] private Canvas confirmCanvas;
        [SerializeField] private Button confirmButton;
        [SerializeField] private GameObject confirmRoot;

        [Tooltip("Offset vertical acima da posição alvo.")]
        [SerializeField] private Vector3 worldOffset = new(0f, 2.5f, 0f);

        [Tooltip("Escala do Canvas no mundo.")]
        [SerializeField] private float canvasScale = 0.01f;

        private Action   _onConfirm;
        private Vector3  _targetPos;
        private Camera   _cam;

        public bool IsVisible => GetVisibilityTarget() != null && GetVisibilityTarget().activeSelf;

        // ── Unity ────────────────────────────────────────────────────

        private void Awake()
        {
            _cam = Camera.main;

            if (confirmRoot == null && confirmButton != null)
            {
                confirmRoot = confirmButton.gameObject;
            }

            if (confirmCanvas != null)
            {
                confirmCanvas.worldCamera = _cam;
                confirmCanvas.transform.localScale = Vector3.one * canvasScale;
            }

            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnButtonClicked);

            Hide();
        }

        private void LateUpdate()
        {
            if (!IsVisible || _cam == null) return;

            confirmCanvas.transform.position = _targetPos + worldOffset;

            Vector3 dir = confirmCanvas.transform.position - _cam.transform.position;
            if (dir != Vector3.zero)
                confirmCanvas.transform.rotation = Quaternion.LookRotation(dir);
        }

        // ── Public API ────────────────────────────────────────────────

        public void Show(Vector3 worldPos, Action onConfirm)
        {
            _targetPos = worldPos;
            _onConfirm = onConfirm;
            SetVisible(true);
        }

        public void UpdatePosition(Vector3 worldPos) => _targetPos = worldPos;

        public void Hide()
        {
            SetVisible(false);
            _onConfirm = null;
        }

        // ── Events ───────────────────────────────────────────────────

        private void OnButtonClicked()
        {
            var cb = _onConfirm;
            Hide();   // primeiro, para evitar duplo-clique
            cb?.Invoke();
        }

        private GameObject GetVisibilityTarget()
        {
            if (confirmRoot != null)
            {
                return confirmRoot;
            }

            return confirmCanvas != null ? confirmCanvas.gameObject : null;
        }

        private void SetVisible(bool isVisible)
        {
            GameObject target = GetVisibilityTarget();
            if (target != null)
            {
                target.SetActive(isVisible);
            }
        }
    }
}
