using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Reclaim.Flow
{
    /// <summary>
    /// Handles transition from loading scene to game scene with async loading and rotating tips/slides.
    /// </summary>
    public class LoadingSceneController : MonoBehaviour
    {
        [Header("Flow")]
        [SerializeField] private string nextSceneName = "Game";
        [SerializeField, Min(0f)] private float minimumLoadingDuration = 1.5f;
        [SerializeField] private bool useAsyncSceneLoading = true;

        [Header("Slides")]
        [SerializeField] private Image slideImage;
        [SerializeField] private List<Sprite> loadingSprites = new();
        [SerializeField] private List<string> slideCaptions = new();
        [SerializeField] private Text captionText;
        [SerializeField] private Font captionFont;
        [SerializeField] private Image darkFadeOverlay;
        [SerializeField, Min(0.1f)] private float slideDurationSeconds = 5f;
        [SerializeField, Min(0.05f)] private float fadeDurationSeconds = 1.2f;
        [SerializeField, Min(0)] private int minimumSlideChangesBeforeFinish = 4;

        [Header("Skip")]
        [SerializeField] private bool allowSkip = true;
        [SerializeField] private bool useUnscaledTime = true;

        private bool isTransitioning;
        private int currentSlideIndex = -1;
        private int slideChanges;
        private float slideTimer;
        private AsyncOperation asyncLoadOperation;

        private void Start()
        {
            StartCoroutine(LoadRoutine());
        }

        private void Update()
        {
            if (!allowSkip || isTransitioning)
            {
                return;
            }

            if (HasSkipInput())
            {
                LoadNextScene();
            }
        }

        private IEnumerator LoadRoutine()
        {
            ResolveUiReferences();
            ShowFirstSlide();

            asyncLoadOperation = null;
            if (useAsyncSceneLoading && !string.IsNullOrWhiteSpace(nextSceneName))
            {
                asyncLoadOperation = SceneManager.LoadSceneAsync(nextSceneName);
                if (asyncLoadOperation != null)
                {
                    asyncLoadOperation.allowSceneActivation = false;
                }
            }

            float elapsed = 0f;
            while (!isTransitioning)
            {
                float delta = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                elapsed += delta;
                slideTimer += delta;

                if (ShouldAdvanceSlide())
                {
                    yield return PlaySlideTransition();
                }

                bool minimumReached = elapsed >= minimumLoadingDuration;
                bool slidesReached = slideChanges >= minimumSlideChangesBeforeFinish;
                bool asyncReady = asyncLoadOperation == null || asyncLoadOperation.progress >= 0.9f;

                if (minimumReached && slidesReached && asyncReady)
                {
                    break;
                }

                yield return null;
            }

            if (isTransitioning)
            {
                yield break;
            }

            if (asyncLoadOperation != null)
            {
                isTransitioning = true;
                asyncLoadOperation.allowSceneActivation = true;
                yield break;
            }

            LoadNextScene();
        }

        private bool ShouldAdvanceSlide()
        {
            if (loadingSprites.Count <= 1)
            {
                return false;
            }

            return slideTimer >= slideDurationSeconds;
        }

        private void ShowFirstSlide()
        {
            if (loadingSprites.Count == 0)
            {
                return;
            }

            currentSlideIndex = 0;
            ApplySlide(currentSlideIndex);
            slideChanges = 1;
            slideTimer = 0f;
            SetOverlayAlpha(0f);
        }

        private IEnumerator PlaySlideTransition()
        {
            if (loadingSprites.Count <= 1)
            {
                yield break;
            }

            float halfFadeDuration = Mathf.Max(0.01f, fadeDurationSeconds * 0.5f);

            yield return FadeOverlay(0f, 1f, halfFadeDuration);

            currentSlideIndex = (currentSlideIndex + 1) % loadingSprites.Count;
            ApplySlide(currentSlideIndex);
            slideChanges++;
            slideTimer = 0f;

            yield return FadeOverlay(1f, 0f, halfFadeDuration);
        }

        private IEnumerator FadeOverlay(float from, float to, float duration)
        {
            if (darkFadeOverlay == null)
            {
                yield break;
            }

            if (duration <= 0f)
            {
                SetOverlayAlpha(to);
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                float delta = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                elapsed += delta;
                float t = Mathf.Clamp01(elapsed / duration);
                SetOverlayAlpha(Mathf.Lerp(from, to, t));
                yield return null;
            }

            SetOverlayAlpha(to);
        }

        private void LoadNextScene()
        {
            if (isTransitioning || string.IsNullOrWhiteSpace(nextSceneName))
            {
                return;
            }

            isTransitioning = true;

            if (asyncLoadOperation != null)
            {
                asyncLoadOperation.allowSceneActivation = true;
                return;
            }

            SceneManager.LoadScene(nextSceneName);
        }

        private void ResolveUiReferences()
        {
            if (slideImage == null)
            {
                slideImage = FindFirstObjectByType<Image>();
            }

            if (captionText == null)
            {
                captionText = FindFirstObjectByType<Text>();
            }

            if (captionText == null)
            {
                captionText = CreateCaptionText();
            }

            if (darkFadeOverlay == null)
            {
                darkFadeOverlay = CreateFadeOverlay();
            }
        }

        private void ApplySlide(int index)
        {
            if (slideImage != null && index >= 0 && index < loadingSprites.Count)
            {
                slideImage.sprite = loadingSprites[index];
            }

            if (captionText != null)
            {
                captionText.text = GetCaptionForSlide(index);
            }
        }

        private string GetCaptionForSlide(int index)
        {
            if (index >= 0 && index < slideCaptions.Count && !string.IsNullOrWhiteSpace(slideCaptions[index]))
            {
                return slideCaptions[index];
            }

            string[] fallbackCaptions =
            {
                "Supere os desafios.",
                "Reconstrua a cidade.",
                "Cuide com os suprimentos fora do armazem.",
                "Cada escolha define o futuro da comunidade."
            };

            if (index >= 0 && index < fallbackCaptions.Length)
            {
                return fallbackCaptions[index];
            }

            return string.Empty;
        }

        private void SetOverlayAlpha(float alpha)
        {
            if (darkFadeOverlay == null)
            {
                return;
            }

            Color color = darkFadeOverlay.color;
            color.a = Mathf.Clamp01(alpha);
            darkFadeOverlay.color = color;
        }

        private Text CreateCaptionText()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                return null;
            }

            GameObject textObject = new GameObject("LoadingCaption", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(canvas.transform, false);

            RectTransform rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0f);
            rectTransform.anchorMax = new Vector2(0.5f, 0f);
            rectTransform.pivot = new Vector2(0.5f, 0f);
            rectTransform.anchoredPosition = new Vector2(0f, 40f);
            rectTransform.sizeDelta = new Vector2(1000f, 120f);

            Text text = textObject.GetComponent<Text>();
            Font runtimeFont = captionFont;
            if (runtimeFont == null)
            {
                runtimeFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            if (runtimeFont == null)
            {
                runtimeFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            text.font = runtimeFont;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 36;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private Image CreateFadeOverlay()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                return null;
            }

            GameObject overlayObject = new GameObject("DarkFadeOverlay", typeof(RectTransform), typeof(Image));
            overlayObject.transform.SetParent(canvas.transform, false);
            overlayObject.transform.SetAsLastSibling();

            RectTransform rectTransform = overlayObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            Image overlay = overlayObject.GetComponent<Image>();
            overlay.color = new Color(0f, 0f, 0f, 0f);
            overlay.raycastTarget = false;
            return overlay;
        }

        private static bool HasSkipInput()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            {
                return true;
            }

            if (Mouse.current != null)
            {
                return Mouse.current.leftButton.wasPressedThisFrame ||
                       Mouse.current.rightButton.wasPressedThisFrame;
            }
#endif

            return UnityEngine.Input.anyKeyDown;
        }
    }
}
