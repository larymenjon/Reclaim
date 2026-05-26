using Reclaim.Managers;
using UnityEngine;

namespace Reclaim.Survival.Managers
{
    public class MusicLayer : MonoBehaviour
    {
        [SerializeField] private DayNightCycle dayNightCycle;
        [SerializeField] private AudioClip dayClip;
        [SerializeField] private AudioClip nightClip;
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.75f;

        private void Awake()
        {
            if (dayNightCycle == null)
            {
                dayNightCycle = FindFirstObjectByType<DayNightCycle>();
            }
        }

        private void OnEnable()
        {
            if (dayNightCycle != null)
            {
                dayNightCycle.OnPhaseChanged += HandlePhaseChanged;
            }
        }

        private void Start()
        {
            PlayForPhase(dayNightCycle != null ? dayNightCycle.CurrentPhase : DayPhase.Day);
        }

        private void OnDisable()
        {
            if (dayNightCycle != null)
            {
                dayNightCycle.OnPhaseChanged -= HandlePhaseChanged;
            }
        }

        private void HandlePhaseChanged(DayPhase phase, Core.TickContext _)
        {
            PlayForPhase(phase);
        }

        private void PlayForPhase(DayPhase phase)
        {
            AudioClip clip = phase == DayPhase.Night ? nightClip : dayClip;
            if (clip == null || AudioManager.Instance == null)
            {
                return;
            }

            AudioManager.Instance.PlayMusic(clip, musicVolume);
        }
    }
}
