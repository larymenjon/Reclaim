using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Reclaim.Flow
{
    /// <summary>
    /// Handles transition from loading scene to game scene with async loading and slide rotation.
    /// </summary>
    public class LoadingSceneController : MonoBehaviour
    {
        [Header("Flow")]
        [SerializeField] private string nextSceneName = "Game";
        [SerializeField, Min(0f)] private float minimumLoadingDuration = 1.5f;
        [SerializeField] private bool useAsyncSceneLoading = true;

        [Header("Slides")]
        [SerializeField] private List<GameObject> loadingSlides = new();
        [SerializeField, Min(0.05f)] private float slideDurationSeconds = 0.75f;
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
            SetSlidesActive(false);
            AdvanceSlide();

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
                    AdvanceSlide();
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
            if (loadingSlides.Count <= 1)
            {
                return false;
            }

            return slideTimer >= slideDurationSeconds;
        }

        private void AdvanceSlide()
        {
            if (loadingSlides.Count == 0)
            {
                return;
            }

            currentSlideIndex = (currentSlideIndex + 1) % loadingSlides.Count;
            slideChanges++;
            slideTimer = 0f;

            for (int i = 0; i < loadingSlides.Count; i++)
            {
                GameObject slide = loadingSlides[i];
                if (slide != null)
                {
                    slide.SetActive(i == currentSlideIndex);
                }
            }
        }

        private void SetSlidesActive(bool active)
        {
            for (int i = 0; i < loadingSlides.Count; i++)
            {
                if (loadingSlides[i] != null)
                {
                    loadingSlides[i].SetActive(active);
                }
            }
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
