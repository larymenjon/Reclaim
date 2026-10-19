using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Reclaim.Flow
{
    /// <summary>
    /// Ao entrar no jogo vindo do Loading, abre a câmera com um zoom de lente e um fade do preto.
    /// Adiciona-se sozinho à câmera principal quando a cena pedida carrega.
    /// </summary>
    public class GameEntryLensZoomTransition : MonoBehaviour
    {
        [SerializeField, Min(0.05f)] private float transitionDurationSeconds = 1.25f;
        [SerializeField, Range(0.1f, 1f)] private float startFovMultiplier = 0.45f;
        [SerializeField] private bool useUnscaledTime = true;

        private Camera targetCamera;
        private Image overlayImage;
        private Canvas overlayCanvas;
        private float targetFieldOfView;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void RegisterSceneHook()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!LoadingTransitionState.ConsumeLensZoomRequestForScene(scene.name))
            {
                return;
            }

            Camera camera = Camera.main;
            if (camera == null)
            {
                return;
            }

            if (camera.GetComponent<GameEntryLensZoomTransition>() == null)
            {
                camera.gameObject.AddComponent<GameEntryLensZoomTransition>();
            }
        }

        private void Awake()
        {
            targetCamera = GetComponent<Camera>();
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }
        }

        private void OnEnable()
        {
            StartCoroutine(PlayTransition());
        }

        private IEnumerator PlayTransition()
        {
            if (targetCamera == null)
            {
                Destroy(this);
                yield break;
            }

            targetFieldOfView = targetCamera.fieldOfView;
            float startFieldOfView = Mathf.Clamp(targetFieldOfView * startFovMultiplier, 12f, targetFieldOfView);

            targetCamera.fieldOfView = startFieldOfView;
            CreateOverlay();
            SetOverlayAlpha(1f);

            float elapsed = 0f;
            while (elapsed < transitionDurationSeconds)
            {
                float delta = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                elapsed += delta;
                float t = Mathf.Clamp01(elapsed / transitionDurationSeconds);
                float eased = 1f - Mathf.Pow(1f - t, 3f);

                targetCamera.fieldOfView = Mathf.Lerp(startFieldOfView, targetFieldOfView, eased);
                SetOverlayAlpha(Mathf.Lerp(1f, 0f, eased));
                yield return null;
            }

            targetCamera.fieldOfView = targetFieldOfView;
            SetOverlayAlpha(0f);

            if (overlayCanvas != null)
            {
                Destroy(overlayCanvas.gameObject);
            }

            Destroy(this);
        }

        private void CreateOverlay()
        {
            if (overlayCanvas != null)
            {
                return;
            }

            GameObject canvasObject = new GameObject("GameEntryFadeCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            overlayCanvas = canvasObject.GetComponent<Canvas>();
            overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            overlayCanvas.sortingOrder = short.MaxValue;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject imageObject = new GameObject("FadeImage", typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(canvasObject.transform, false);

            RectTransform rect = imageObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            overlayImage = imageObject.GetComponent<Image>();
            overlayImage.color = Color.black;
            overlayImage.raycastTarget = false;
        }

        private void SetOverlayAlpha(float alpha)
        {
            if (overlayImage == null)
            {
                return;
            }

            Color color = overlayImage.color;
            color.a = Mathf.Clamp01(alpha);
            overlayImage.color = color;
        }
    }
}
