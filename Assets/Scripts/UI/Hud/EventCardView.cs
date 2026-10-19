using System.Collections.Generic;
using Reclaim.Economy;
using Reclaim.Events;
using Reclaim.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reclaim.UI.Hud
{
    /// <summary>
    /// "Cartão de evento" do Figma: um evento por vez, logo abaixo da barra superior.
    /// Cada botão mostra o custo da escolha e fica bloqueado se faltar recurso.
    /// A pausa de eventos críticos é feita pela ColonySimulation, não aqui.
    /// </summary>
    public class EventCardView : MonoBehaviour
    {
        [SerializeField] private ColonySimulation simulation;
        [SerializeField] private HudTheme theme;
        [Tooltip("Objeto do cartão; fica desligado quando não há evento.")]
        [SerializeField] private GameObject card;
        [SerializeField] private Image topStripe;
        [SerializeField] private Image iconBackground;
        [SerializeField] private Image icon;
        [SerializeField] private Sprite fallbackIcon;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private EventChoiceButtonView[] choiceButtons = new EventChoiceButtonView[3];

        [Header("Ícones das escolhas")]
        [SerializeField] private Sprite acceptIcon;
        [SerializeField] private Sprite refuseIcon;

        private bool _isAffordabilityDirty;

        private void Awake()
        {
            if (simulation == null || theme == null || card == null)
            {
                Debug.LogError("[EventCardView] Configure ColonySimulation, HudTheme e o cartão.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            simulation.Events.OnEventTriggered += HandleEventTriggered;
            simulation.Events.OnEventResolved += HandleEventResolved;
            simulation.Resources.OnResourceChanged += HandleResourceChanged;

            if (simulation.Events.CurrentEvent.HasValue)
            {
                Show(simulation.Events.CurrentEvent.Value.Data);
            }
            else
            {
                card.SetActive(false);
            }
        }

        private void OnDisable()
        {
            simulation.Events.OnEventTriggered -= HandleEventTriggered;
            simulation.Events.OnEventResolved -= HandleEventResolved;
            simulation.Resources.OnResourceChanged -= HandleResourceChanged;
        }

        private void LateUpdate()
        {
            if (_isAffordabilityDirty && card.activeSelf)
            {
                _isAffordabilityDirty = false;
                RefreshAffordability();
            }
        }

        private void HandleEventTriggered(ActiveEvent activeEvent) => Show(activeEvent.Data);

        private void HandleEventResolved(ActiveEvent activeEvent, EventChoice choice) => card.SetActive(false);

        private void HandleResourceChanged(ResourceType type, float amount, float capacity) => _isAffordabilityDirty = true;

        private void Show(EventData data)
        {
            Color accent = data.IsCritical ? theme.Critical : theme.Warning;
            topStripe.color = accent;
            iconBackground.color = data.IsCritical ? theme.CriticalDark : theme.PanelRaised;
            icon.sprite = data.Icon != null ? data.Icon : fallbackIcon;
            icon.color = theme.TextPrimary;
            titleText.text = data.Title;
            descriptionText.text = data.Description;

            IReadOnlyList<EventChoice> choices = data.Choices;
            if (choices.Count > choiceButtons.Length)
            {
                Debug.LogWarning($"[EventCardView] '{data.EventId}' tem {choices.Count} escolhas; o cartão mostra {choiceButtons.Length}.", data);
            }

            for (int i = 0; i < choiceButtons.Length; i++)
            {
                if (i >= choices.Count)
                {
                    choiceButtons[i].Hide();
                    continue;
                }

                int index = i;
                ChoiceStyle(i, choices.Count, out Color choiceColor, out Sprite choiceIcon);
                choiceButtons[i].Bind(ChoiceLabel(choices[i]), choiceColor, choiceIcon, () => Choose(index));
            }

            card.SetActive(true);
            RefreshAffordability();
        }

        private void RefreshAffordability()
        {
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                if (choiceButtons[i].gameObject.activeSelf)
                {
                    choiceButtons[i].SetAffordable(simulation.Events.CanChoose(i));
                }
            }
        }

        private void Choose(int index)
        {
            simulation.Events.ResolveCurrentEvent(index);
        }

        private string ChoiceLabel(EventChoice choice)
        {
            string costs = HudFormat.Costs(choice.ResourceDeltas, simulation.Resources);
            return costs.Length > 0 ? choice.Label + HudFormat.Separator + costs : choice.Label;
        }

        /// <summary>Primeira escolha em verde (aceitar), última em vermelho (recusar), as do meio neutras.</summary>
        private void ChoiceStyle(int index, int count, out Color color, out Sprite sprite)
        {
            if (index == 0)
            {
                color = theme.Good;
                sprite = acceptIcon;
            }
            else if (index == count - 1)
            {
                color = theme.Critical;
                sprite = refuseIcon;
            }
            else
            {
                color = theme.TextPrimary;
                sprite = null;
            }
        }
    }
}
