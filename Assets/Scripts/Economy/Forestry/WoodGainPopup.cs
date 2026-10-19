using TMPro;
using UnityEngine;

namespace Reclaim.Economy.Forestry
{
    /// <summary>Texto "+N madeira" que sobe, encara a câmera e some.</summary>
    public class WoodGainPopup : MonoBehaviour
    {
        [SerializeField] private float riseDistance = 1.25f;

        private Camera _camera;
        private TMP_Text _text;
        private float _duration = 1f;
        private Color _baseColor = Color.white;
        private float _elapsed;
        private Vector3 _startPosition;

        public void Initialize(Camera viewCamera, float durationSeconds, Color baseColor)
        {
            _camera = viewCamera;
            _text = GetComponent<TMP_Text>();
            _duration = Mathf.Max(0.1f, durationSeconds);
            _baseColor = baseColor;
            _startPosition = transform.position;
        }

        private void Update()
        {
            _elapsed += UnityEngine.Time.deltaTime;
            float t = Mathf.Clamp01(_elapsed / _duration);

            transform.position = _startPosition + Vector3.up * (riseDistance * t);
            if (_camera != null)
            {
                transform.rotation = _camera.transform.rotation;
            }

            if (_text != null)
            {
                Color color = _baseColor;
                color.a = 1f - t;
                _text.color = color;
            }

            if (t >= 1f)
            {
                Destroy(gameObject);
            }
        }
    }
}
