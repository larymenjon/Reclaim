using Reclaim.Calendar;
using UnityEngine;

namespace Reclaim.Audio
{
    /// <summary>Troca a trilha entre dia e noite conforme o DayNightCycle.</summary>
    public class MusicLayer : MonoBehaviour
    {
        [SerializeField] private DayNightCycle dayNightCycle;
        [SerializeField] private AudioClip dayClip;
        [SerializeField] private AudioClip nightClip;
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.75f;

        private void OnEnable()
        {
            if (dayNightCycle != null)
            {
                dayNightCycle.OnPhaseChanged += PlayForPhase;
            }
        }

        private void Start()
        {
            if (dayNightCycle == null)
            {
                Debug.LogError("[MusicLayer] DayNightCycle não configurado.", this);
                return;
            }

            PlayForPhase(dayNightCycle.CurrentPhase);
        }

        private void OnDisable()
        {
            if (dayNightCycle != null)
            {
                dayNightCycle.OnPhaseChanged -= PlayForPhase;
            }
        }

        private void PlayForPhase(DayPhase phase)
        {
            AudioClip clip = phase == DayPhase.Night ? nightClip : dayClip;
            if (clip != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMusic(clip, musicVolume);
            }
        }
    }
}
