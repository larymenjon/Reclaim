using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Reclaim.Flow
{
    /// <summary>
    /// Plays studio logo objects followed by an optional intro video.
    /// </summary>
    public class IntroSequenceController : MonoBehaviour
    {
        [Header("Flow")]
        [SerializeField] private string menuSceneName = "Menu";

        [Header("Studios")]
        [SerializeField] private List<GameObject> studioLogos = new();
        [SerializeField, Min(0f)] private float logoDurationSeconds = 1.5f;
        [SerializeField, Min(0f)] private float holdAfterLogosSeconds = 0.25f;

        [Header("Video")]
        [SerializeField] private VideoPlayer introVideoPlayer;
        [SerializeField, Min(0f)] private float fallbackVideoDurationSeconds = 8f;
        [SerializeField] private bool autoConfigureVideoOutput = true;

        [Header("Skip")]
        [SerializeField] private bool allowSkip = true;
        [SerializeField] private bool useUnscaledTime = true;

        private bool isTransitioning;
        private bool videoEnded;

        private void Start()
        {
            ResolveVideoPlayer();
            SetLogosActive(false);
            StartCoroutine(PlaySequence());
        }

        private void Update()
        {
            if (!allowSkip || isTransitioning)
            {
                return;
            }

            if (HasSkipInput())
            {
                LoadMenu();
            }
        }

        private IEnumerator PlaySequence()
        {
            for (int i = 0; i < studioLogos.Count; i++)
            {
                SetLogosActive(false);

                if (studioLogos[i] != null)
                {
                    studioLogos[i].SetActive(true);
                    yield return WaitForSecondsSafe(logoDurationSeconds);
                }
            }

            SetLogosActive(false);

            if (holdAfterLogosSeconds > 0f)
            {
                yield return WaitForSecondsSafe(holdAfterLogosSeconds);
            }

            if (introVideoPlayer != null)
            {
                yield return PlayVideo();
            }
            else if (fallbackVideoDurationSeconds > 0f)
            {
                yield return WaitForSecondsSafe(fallbackVideoDurationSeconds);
            }

            LoadMenu();
        }

        private IEnumerator PlayVideo()
        {
            videoEnded = false;
            bool playbackStarted = false;
            introVideoPlayer.loopPointReached += HandleVideoEnded;
            introVideoPlayer.Prepare();

            while (!introVideoPlayer.isPrepared && !isTransitioning)
            {
                yield return null;
            }

            if (isTransitioning)
            {
                introVideoPlayer.loopPointReached -= HandleVideoEnded;
                yield break;
            }

            introVideoPlayer.Play();

            while (!videoEnded && !isTransitioning)
            {
                if (introVideoPlayer.isPlaying)
                {
                    playbackStarted = true;
                }
                else if (playbackStarted)
                {
                    break;
                }

                yield return null;
            }

            if (!videoEnded && fallbackVideoDurationSeconds > 0f && !isTransitioning)
            {
                yield return WaitForSecondsSafe(fallbackVideoDurationSeconds);
            }

            introVideoPlayer.loopPointReached -= HandleVideoEnded;
        }

        private void ResolveVideoPlayer()
        {
            if (introVideoPlayer == null)
            {
                introVideoPlayer = FindFirstObjectByType<VideoPlayer>();
            }

            if (introVideoPlayer == null)
            {
                Debug.LogWarning("IntroSequenceController: no VideoPlayer found in Intro scene.");
                return;
            }

            introVideoPlayer.playOnAwake = false;

            if (!autoConfigureVideoOutput)
            {
                return;
            }

            if (introVideoPlayer.renderMode == VideoRenderMode.APIOnly)
            {
                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    introVideoPlayer.renderMode = VideoRenderMode.CameraFarPlane;
                    introVideoPlayer.targetCamera = mainCamera;
                }
            }
        }

        private void HandleVideoEnded(VideoPlayer player)
        {
            videoEnded = true;
        }

        private void SetLogosActive(bool isActive)
        {
            for (int i = 0; i < studioLogos.Count; i++)
            {
                if (studioLogos[i] != null)
                {
                    studioLogos[i].SetActive(isActive);
                }
            }
        }

        private object WaitForSecondsSafe(float seconds)
        {
            return useUnscaledTime ? new WaitForSecondsRealtime(seconds) : new WaitForSeconds(seconds);
        }

        private void LoadMenu()
        {
            if (isTransitioning || string.IsNullOrWhiteSpace(menuSceneName))
            {
                return;
            }

            isTransitioning = true;
            SceneManager.LoadScene(menuSceneName);
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
