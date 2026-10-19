using UnityEngine;

namespace Reclaim.Construction
{
    /// <summary>
    /// Fantasma de pré-visualização da construção, verde quando válido e vermelho quando não.
    /// </summary>
    public class PreviewSystem : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        [Header("Preview Colors")]
        [SerializeField] private Color validColor = new Color(0f, 1f, 0f, 0.35f);
        [SerializeField] private Color invalidColor = new Color(1f, 0f, 0f, 0.35f);

        private MaterialPropertyBlock _propertyBlock;
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
            _propertyBlock ??= new MaterialPropertyBlock();

            for (int i = 0; i < _renderers.Length; i++)
            {
                Material material = _renderers[i].sharedMaterial;
                if (material == null)
                {
                    continue;
                }

                _renderers[i].GetPropertyBlock(_propertyBlock);
                if (material.HasProperty(BaseColorId))
                {
                    _propertyBlock.SetColor(BaseColorId, color);
                }

                if (material.HasProperty(ColorId))
                {
                    _propertyBlock.SetColor(ColorId, color);
                }

                _renderers[i].SetPropertyBlock(_propertyBlock);
            }
        }
    }
}
