using System;
using System.Collections.Generic;
using Reclaim.Economy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reclaim.UI.Hud
{
    public enum BuildCardState
    {
        Available,
        Selected,
        Insufficient
    }

    /// <summary>
    /// "Cartão de construção" do Figma. Mostra arte, tamanho, nome e até dois custos;
    /// o estado é decidido pela BuildDockView.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class BuildCardView : MonoBehaviour
    {
        [Serializable]
        private struct CostSlot
        {
            public GameObject root;
            public Image icon;
            public TMP_Text amount;
        }

        [SerializeField] private HudTheme theme;
        [SerializeField] private Image border;
        [SerializeField] private Sprite borderNormal;
        [SerializeField] private Sprite borderSelected;
        [SerializeField] private Image art;
        [SerializeField] private TMP_Text sizeText;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private CostSlot[] costSlots = new CostSlot[2];

        private readonly List<ResourceAmount> _costs = new List<ResourceAmount>(2);
        private Button _button;
        private Action _onClick;

        private void Awake()
        {
            EnsureButton();
        }

        public void Bind(string displayName, Sprite artSprite, bool tintArt, Vector2Int size,
            IReadOnlyList<ResourceAmount> costs, ResourceStock stock, Action onClick)
        {
            EnsureButton();
            nameText.text = displayName;
            nameText.color = theme.TextPrimary;
            art.sprite = artSprite;
            art.color = tintArt ? theme.TextPrimary : Color.white;
            bool hasSize = size.x > 0 && size.y > 0;
            sizeText.gameObject.SetActive(hasSize);
            sizeText.text = hasSize ? $"{size.x}×{size.y}" : string.Empty;
            sizeText.color = theme.TextTertiary;

            _costs.Clear();
            for (int i = 0; costs != null && i < costs.Count; i++)
            {
                if (costs[i].Amount > 0f)
                {
                    _costs.Add(costs[i]);
                }
            }

            for (int i = 0; i < costSlots.Length; i++)
            {
                bool visible = i < _costs.Count;
                costSlots[i].root.SetActive(visible);
                if (visible)
                {
                    ResourceDefinition definition = stock != null ? stock.GetDefinition(_costs[i].ResourceType) : null;
                    costSlots[i].icon.sprite = definition != null ? definition.Icon : null;
                    costSlots[i].amount.text = Mathf.RoundToInt(_costs[i].Amount).ToString();
                }
            }

            if (_costs.Count > costSlots.Length)
            {
                Debug.LogWarning($"[BuildCardView] '{displayName}' tem {_costs.Count} custos; o cartão mostra {costSlots.Length}.", this);
            }

            _onClick = onClick;
        }

        public void SetState(BuildCardState state, ResourceStock stock)
        {
            bool selected = state == BuildCardState.Selected;
            border.sprite = selected ? borderSelected : borderNormal;
            border.color = selected ? theme.Selected : theme.Border;

            for (int i = 0; i < costSlots.Length && i < _costs.Count; i++)
            {
                bool lacking = stock != null && stock.GetAmount(_costs[i].ResourceType) < _costs[i].Amount;
                Color color = lacking ? theme.Critical : theme.TextPrimary;
                costSlots[i].amount.color = color;
                costSlots[i].icon.color = color;
            }
        }

        private void HandleClick() => _onClick?.Invoke();

        private void EnsureButton()
        {
            if (_button != null)
            {
                return;
            }

            _button = GetComponent<Button>();
            _button.onClick.AddListener(HandleClick);
        }
    }
}
