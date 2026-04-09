using UnityEngine;
using UnityEngine.EventSystems;

namespace Reclaim.UI.Bottom
{
    /// <summary>
    /// Raises a UI element while hovered, with a gentle return animation.
    /// </summary>
    public class BottomHudHoverLift : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private float hoverLift = 18f;
        [SerializeField] private float pressedOffset = -4f;
        [SerializeField] private float smoothSpeed = 16f;
        [SerializeField] private bool useUnscaledTime = true;

        private Vector2 _basePosition;
        private bool _isHovered;
        private bool _isPressed;
        private bool _initialized;

        private void Awake()
        {
            if (target == null)
            {
                target = transform as RectTransform;
            }

            if (target == null)
            {
                enabled = false;
                return;
            }

            _basePosition = target.anchoredPosition;
            _initialized = true;
        }

        private void OnEnable()
        {
            if (_initialized && target != null)
            {
                target.anchoredPosition = _basePosition;
            }
        }

        private void Update()
        {
            if (target == null)
            {
                return;
            }

            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float desiredLift = _isHovered ? hoverLift : 0f;
            if (_isPressed)
            {
                desiredLift += pressedOffset;
            }

            Vector2 desired = _basePosition + Vector2.up * desiredLift;
            target.anchoredPosition = Vector2.Lerp(target.anchoredPosition, desired, 1f - Mathf.Exp(-smoothSpeed * dt));
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            _isPressed = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                _isPressed = true;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;
        }
    }
}
