using UnityEngine;

namespace Reclaim.Core
{
    /// <summary>
    /// Handles ghost preview object for placement feedback.
    /// </summary>
    public class PreviewSystem : MonoBehaviour
    {
        [Header("Preview Colors")]
        [SerializeField] private Color validColor = new Color(0f, 1f, 0f, 0.35f);
        [SerializeField] private Color invalidColor = new Color(1f, 0f, 0f, 0.35f);

        private GameObject _previewInstance;
        private Renderer[] _renderers;
        private bool _isValid = true;

        public void SetPreviewPrefab(GameObject prefab)
        {
            ClearPreview();

            if (prefab == null)
            {
                return;
            }

            _previewInstance = Instantiate(prefab);
            _previewInstance.name = $"{prefab.name}_Preview";
            _renderers = _previewInstance.GetComponentsInChildren<Renderer>(true);

            Collider[] colliders = _previewInstance.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }

            ApplyPreviewColor();
        }

        public void SetTransform(Vector3 worldPosition, Quaternion worldRotation)
        {
            if (_previewInstance == null)
            {
                return;
            }

            _previewInstance.transform.SetPositionAndRotation(worldPosition, worldRotation);
        }

        public void SetValidity(bool isValid)
        {
            if (_isValid == isValid)
            {
                return;
            }

            _isValid = isValid;
            ApplyPreviewColor();
        }

        public void SetVisible(bool visible)
        {
            if (_previewInstance == null)
            {
                return;
            }

            _previewInstance.SetActive(visible);
        }

        public void ClearPreview()
        {
            if (_previewInstance != null)
            {
                Destroy(_previewInstance);
            }

            _previewInstance = null;
            _renderers = null;
        }

        private void ApplyPreviewColor()
        {
            if (_renderers == null)
            {
                return;
            }

            Color color = _isValid ? validColor : invalidColor;
            MaterialPropertyBlock block = new MaterialPropertyBlock();

            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].GetPropertyBlock(block);

                if (_renderers[i].sharedMaterial != null && _renderers[i].sharedMaterial.HasProperty("_BaseColor"))
                {
                    block.SetColor("_BaseColor", color);
                }

                if (_renderers[i].sharedMaterial != null && _renderers[i].sharedMaterial.HasProperty("_Color"))
                {
                    block.SetColor("_Color", color);
                }

                _renderers[i].SetPropertyBlock(block);
            }
        }
    }
}
