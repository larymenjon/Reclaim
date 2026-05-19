using UnityEngine;

namespace Reclaim.Resources.Forestry
{
    public class WoodGainPopup : MonoBehaviour
    {
        [SerializeField] private float riseDistance = 1.25f;

        private UnityEngine.Camera _camera;
        private float _duration;
        private Color _baseColor;
        private float _elapsed;
        private Vector3 _startPosition;

        public void Initialize(UnityEngine.Camera camera, float durationSeconds, Color baseColor)
        {
            _camera = camera;
            _duration = Mathf.Max(0.1f, durationSeconds);
            _baseColor = baseColor;
            _startPosition = transform.position;
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsed / _duration);

            transform.position = _startPosition + Vector3.up * (riseDistance * t);

            if (_camera != null)
            {
                transform.forward = _camera.transform.forward;
            }

            SetAlpha(1f - t);

            if (t >= 1f)
            {
                Destroy(gameObject);
            }
        }

        private void SetAlpha(float alpha)
        {
            UnityEngine.UI.Text text = GetComponentInChildren<UnityEngine.UI.Text>();
            if (text == null)
            {
                return;
            }

            Color color = _baseColor;
            color.a = Mathf.Clamp01(alpha);
            text.color = color;
        }
    }
}


