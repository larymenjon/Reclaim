using System;
using Reclaim.Calendar;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reclaim.UI.Hud
{
    /// <summary>"Dia e noite" do Figma: a orbe da fase atual fica acesa e a outra apagada.</summary>
    public class DayNightView : MonoBehaviour
    {
        [Serializable]
        private struct Orb
        {
            public CanvasGroup group;
            public Image ring;
            public TMP_Text label;
        }

        [SerializeField] private DayNightCycle dayNightCycle;
        [SerializeField] private HudTheme theme;
        [SerializeField] private Orb night;
        [SerializeField] private Orb day;

        private void Awake()
        {
            if (dayNightCycle == null || theme == null)
            {
                Debug.LogError("[DayNightView] DayNightCycle ou HudTheme não configurados.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            dayNightCycle.OnPhaseChanged += HandlePhaseChanged;
            Apply(dayNightCycle.IsNight);
        }

        private void OnDisable()
        {
            dayNightCycle.OnPhaseChanged -= HandlePhaseChanged;
        }

        private void HandlePhaseChanged(DayPhase phase) => Apply(phase == DayPhase.Night);

        private void Apply(bool isNight)
        {
            SetOrb(night, isNight);
            SetOrb(day, !isNight);
        }

        private void SetOrb(Orb orb, bool active)
        {
            orb.group.alpha = active ? 1f : theme.InactiveAlpha;
            orb.ring.color = active ? theme.TextPrimary : theme.Border;
            orb.label.color = active ? theme.TextPrimary : theme.TextTertiary;
        }
    }
}
