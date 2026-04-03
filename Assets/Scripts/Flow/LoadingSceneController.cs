using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Reclaim.Flow
{
    /// <summary>
    /// Handles transition from loading scene to intro scene.
    /// </summary>
    public class LoadingSceneController : MonoBehaviour
    {
        [Header("Flow")]
        [SerializeField] private string nextSceneName = "Intro";
        [SerializeField, Min(0f)] private float minimumLoadingDuration = 1.5f;

        [Header("Skip")]
        [SerializeField] private bool allowSkip = true;
        [SerializeField] private bool useUnscaledTime = true;

        private bool isTransitioning;

        private void Start()
        {
            StartCoroutine(LoadAfterDelay());
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

        private IEnumerator LoadAfterDelay()
        {
            if (minimumLoadingDuration > 0f)
            {
                if (useUnscaledTime)
                {
                    yield return new WaitForSecondsRealtime(minimumLoadingDuration);
                }
                else
                {
                    yield return new WaitForSeconds(minimumLoadingDuration);
                }
            }

            LoadNextScene();
        }

        private void LoadNextScene()
        {
            if (isTransitioning || string.IsNullOrWhiteSpace(nextSceneName))
            {
                return;
            }

            isTransitioning = true;
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
