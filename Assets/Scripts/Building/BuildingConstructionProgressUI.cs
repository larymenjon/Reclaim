using UnityEngine;
using UnityEngine.UI;

namespace Reclaim.Building
{
    /// <summary>
    /// Runtime world-space progress bar shown while a building is under construction.
    /// </summary>
    [DisallowMultipleComponent]
    public class BuildingConstructionProgressUI : MonoBehaviour
    {
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2.5f, 0f);
        [SerializeField, Min(0.1f)] private float canvasScale = 0.01f;
        [SerializeField] private bool hideOnComplete = true;

        private Building _building;
        private Canvas _canvas;
        private Slider _slider;
        private Image _fillImage;
        private Camera _targetCamera;

        private void Awake()
        {
            _building = GetComponent<Building>();
            if (_building == null)
            {
                enabled = false;
                return;
            }

            CreateUi();
        }

        private void OnEnable()
        {
            if (_building != null)
            {
                _building.OnConstructionProgressChanged += HandleProgressChanged;
                _building.OnConstructionCompleted += HandleCompleted;
                HandleProgressChanged(_building, _building.ConstructionProgress01);
            }
        }

        private void OnDisable()
        {
            if (_building != null)
            {
                _building.OnConstructionProgressChanged -= HandleProgressChanged;
                _building.OnConstructionCompleted -= HandleCompleted;
            }
        }

        private void LateUpdate()
        {
            if (_canvas == null)
            {
                return;
            }

            if (_targetCamera == null)
            {
                _targetCamera = Camera.main != null ? Camera.main : FindFirstObjectByType<Camera>();
            }

            _canvas.transform.position = transform.position + worldOffset;
            if (_targetCamera != null)
            {
                _canvas.transform.forward = _targetCamera.transform.forward;
            }
        }

        private void HandleProgressChanged(Building building, float progress01)
        {
            if (_slider == null)
            {
                return;
            }

            _slider.value = Mathf.Clamp01(progress01);
            UpdateFillColor(progress01);
            if (_canvas != null)
            {
                _canvas.gameObject.SetActive(!building.IsConstructionComplete || !hideOnComplete);
            }
        }

        private void HandleCompleted(Building building)
        {
            if (hideOnComplete && _canvas != null)
            {
                _canvas.gameObject.SetActive(false);
            }
        }

        private void CreateUi()
        {
            GameObject canvasObject = new GameObject("ConstructionProgressCanvas", typeof(Canvas));
            canvasObject.transform.SetParent(transform, false);
            _canvas = canvasObject.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.WorldSpace;
            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(200f, 36f);
            canvasObject.transform.localScale = Vector3.one * canvasScale;

            GameObject sliderObject = new GameObject("ProgressSlider", typeof(RectTransform), typeof(Slider));
            sliderObject.transform.SetParent(canvasObject.transform, false);
            RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0f, 0f);
            sliderRect.anchorMax = new Vector2(1f, 1f);
            sliderRect.offsetMin = new Vector2(8f, 8f);
            sliderRect.offsetMax = new Vector2(-8f, -8f);

            _slider = sliderObject.GetComponent<Slider>();
            _slider.minValue = 0f;
            _slider.maxValue = 1f;
            _slider.interactable = false;
            _slider.transition = Selectable.Transition.None;

            GameObject background = new GameObject("Background", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(sliderObject.transform, false);
            RectTransform bgRect = background.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            Image bgImage = background.GetComponent<Image>();
            bgImage.color = new Color(0f, 0f, 0f, 0.55f);
            _slider.targetGraphic = bgImage;

            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderObject.transform, false);
            RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0f, 0f);
            fillAreaRect.anchorMax = new Vector2(1f, 1f);
            fillAreaRect.offsetMin = new Vector2(2f, 2f);
            fillAreaRect.offsetMax = new Vector2(-2f, -2f);

            GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(1f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            _fillImage = fill.GetComponent<Image>();
            _fillImage.color = new Color(0.95f, 0.72f, 0.15f, 1f);

            _slider.fillRect = fillRect;
        }

        private void UpdateFillColor(float progress01)
        {
            if (_fillImage == null)
            {
                return;
            }

            if (progress01 < (1f / 3f))
            {
                _fillImage.color = new Color(0.87f, 0.45f, 0.16f, 1f);
                return;
            }

            if (progress01 < (2f / 3f))
            {
                _fillImage.color = new Color(0.95f, 0.72f, 0.15f, 1f);
                return;
            }

            _fillImage.color = new Color(0.33f, 0.82f, 0.42f, 1f);
        }
    }
}