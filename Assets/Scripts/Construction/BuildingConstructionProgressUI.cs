using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reclaim.Construction
{
    /// <summary>
    /// Barra de progresso sobre a obra com os segundos que faltam.
    /// Tem tamanho constante na tela, em qualquer zoom, e some quando a construção termina.
    /// </summary>
    [DisallowMultipleComponent]
    public class BuildingConstructionProgressUI : MonoBehaviour
    {
        private static readonly Vector2 CanvasSize = new Vector2(220f, 44f);

        [SerializeField, Min(0f)] private float heightMargin = 0.8f;
        [SerializeField, Min(0f)] private float minimumHeight = 2f;
        [Tooltip("Largura aproximada da barra na tela, em pixels.")]
        [SerializeField, Min(20f)] private float widthOnScreen = 150f;

        private Building _building;
        private Canvas _canvas;
        private Image _fillImage;
        private RectTransform _fillRect;
        private TMP_Text _label;
        private Camera _camera;

        private void Awake()
        {
            _building = GetComponent<Building>();
            if (_building == null)
            {
                enabled = false;
                return;
            }

            CreateUi();
            Refresh(_building.ConstructionProgress01);
        }

        private void OnEnable()
        {
            if (_building != null)
            {
                _building.OnConstructionProgressChanged += HandleProgressChanged;
            }
        }

        private void OnDisable()
        {
            if (_building != null)
            {
                _building.OnConstructionProgressChanged -= HandleProgressChanged;
            }
        }

        private void LateUpdate()
        {
            if (_canvas == null || !_canvas.gameObject.activeSelf)
            {
                return;
            }

            if (_camera == null)
            {
                _camera = Camera.main;
                if (_camera == null)
                {
                    return;
                }
            }

            Transform canvasTransform = _canvas.transform;
            float height = Mathf.Max(minimumHeight, _building.VisualHeight + heightMargin);
            canvasTransform.position = transform.position + Vector3.up * height;
            canvasTransform.rotation = _camera.transform.rotation;
            canvasTransform.localScale = Vector3.one * ScaleForConstantScreenSize();
        }

        private void HandleProgressChanged(Building building, float progress01)
        {
            Refresh(progress01);
        }

        private void Refresh(float progress01)
        {
            if (_canvas == null)
            {
                return;
            }

            bool inProgress = progress01 < 1f && !_building.IsConstructionComplete;
            _canvas.gameObject.SetActive(inProgress);
            if (!inProgress)
            {
                return;
            }

            _fillRect.anchorMax = new Vector2(Mathf.Clamp01(progress01), 1f);
            _fillImage.color = ColorFor(progress01);
            _label.text = $"{Mathf.CeilToInt(_building.ConstructionSecondsRemaining)} s";
        }

        /// <summary>Escala do canvas para a barra ocupar sempre ~widthOnScreen pixels, perto ou longe.</summary>
        private float ScaleForConstantScreenSize()
        {
            float worldPerPixel;
            if (_camera.orthographic)
            {
                worldPerPixel = 2f * _camera.orthographicSize / Screen.height;
            }
            else
            {
                float distance = Vector3.Dot(_canvas.transform.position - _camera.transform.position, _camera.transform.forward);
                worldPerPixel = 2f * Mathf.Max(0.1f, distance) * Mathf.Tan(_camera.fieldOfView * 0.5f * Mathf.Deg2Rad) / Screen.height;
            }

            return worldPerPixel * widthOnScreen / CanvasSize.x;
        }

        private void CreateUi()
        {
            GameObject canvasObject = new GameObject("ConstructionProgressCanvas", typeof(Canvas));
            canvasObject.transform.SetParent(transform, false);
            _canvas = canvasObject.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.WorldSpace;
            canvasObject.GetComponent<RectTransform>().sizeDelta = CanvasSize;

            CreateImage("Background", canvasObject.transform, new Color(0.05f, 0.05f, 0.05f, 0.75f));
            RectTransform fillArea = CreateImage("FillArea", canvasObject.transform, Color.clear).rectTransform;
            fillArea.offsetMin = new Vector2(4f, 4f);
            fillArea.offsetMax = new Vector2(-4f, -4f);

            _fillImage = CreateImage("Fill", fillArea, Color.white);
            _fillRect = _fillImage.rectTransform;

            _label = CreateLabel(canvasObject.transform);
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static TMP_Text CreateLabel(Transform parent)
        {
            GameObject go = new GameObject("Seconds", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            TMP_Text label = go.GetComponent<TMP_Text>();
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 28f;
            label.fontStyle = FontStyles.Bold;
            label.color = Color.white;
            label.raycastTarget = false;
            return label;
        }

        private static Color ColorFor(float progress01)
        {
            if (progress01 < 0.5f)
            {
                return new Color(0.87f, 0.45f, 0.16f, 1f);
            }

            return progress01 < 1f ? new Color(0.95f, 0.72f, 0.15f, 1f) : new Color(0.33f, 0.82f, 0.42f, 1f);
        }
    }
}
