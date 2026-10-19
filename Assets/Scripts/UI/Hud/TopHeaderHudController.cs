using Reclaim.Construction;
using Reclaim.Economy;
using Reclaim.Flow;
using Reclaim.Simulation;
using TMPro;
using UnityEngine;

namespace Reclaim.UI.Hud
{
    /// <summary>
    /// Barra superior da HUD atual. Só exibe: todos os números vêm da simulação
    /// (ResourceStock, FamilyManager, BuildingSystem). Consumíveis aparecem em dias de reserva.
    /// Será substituída pela barra do redesign do Figma, que segue o mesmo princípio.
    /// </summary>
    public class TopHeaderHudController : MonoBehaviour
    {
        [Header("Fonte de dados")]
        [SerializeField] private ColonySimulation simulation;
        [SerializeField] private BuildingSystem buildingSystem;

        [Header("Textos (TMP)")]
        [SerializeField] private TMP_Text cidadeText;
        [SerializeField] private TMP_Text populacaoText;
        [SerializeField] private TMP_Text comidaText;
        [SerializeField] private TMP_Text aguaText;
        [SerializeField] private TMP_Text madeiraText;
        [SerializeField] private TMP_Text combustivelText;
        [SerializeField] private TMP_Text sucataText;
        [SerializeField] private TMP_Text casasText;

        [Header("Fora do alpha (ocultados ao iniciar)")]
        [SerializeField] private TMP_Text moedasText;
        [SerializeField] private TMP_Text contaminacaoText;
        [SerializeField] private TMP_Text reflorestamentoText;
        [SerializeField] private TMP_Text municaoText;
        [SerializeField] private TMP_Text defesaText;

        [Header("Estados")]
        [Tooltip("Abaixo desta reserva o contador fica em estado Baixo.")]
        [SerializeField, Min(0f)] private float lowReserveDays = 3f;
        [SerializeField] private Color normalColor = new Color32(0xEC, 0xE7, 0xDA, 0xFF);
        [SerializeField] private Color lowColor = new Color32(0xCF, 0x4A, 0x3C, 0xFF);

        private bool _isDirty = true;

        private void Awake()
        {
            HideUnsupported(moedasText, contaminacaoText, reflorestamentoText, municaoText, defesaText);

            if (simulation == null)
            {
                Debug.LogError("[TopHeaderHudController] ColonySimulation não configurada.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (simulation == null)
            {
                return;
            }

            simulation.Resources.OnResourceChanged += HandleResourceChanged;
            simulation.Families.OnPopulationChanged += HandlePopulationChanged;
            if (buildingSystem != null)
            {
                buildingSystem.OnBuildingCompleted += HandleBuildingChanged;
                buildingSystem.OnBuildingPlaced += HandleBuildingChanged;
            }

            _isDirty = true;
        }

        private void OnDisable()
        {
            if (simulation == null)
            {
                return;
            }

            simulation.Resources.OnResourceChanged -= HandleResourceChanged;
            simulation.Families.OnPopulationChanged -= HandlePopulationChanged;
            if (buildingSystem != null)
            {
                buildingSystem.OnBuildingCompleted -= HandleBuildingChanged;
                buildingSystem.OnBuildingPlaced -= HandleBuildingChanged;
            }
        }

        // Recursos mudam várias vezes por tick; a tela só precisa ser redesenhada uma vez por frame.
        private void LateUpdate()
        {
            if (!_isDirty)
            {
                return;
            }

            _isDirty = false;
            Refresh();
        }

        private void HandleResourceChanged(ResourceType type, float amount, float capacity) => _isDirty = true;

        private void HandlePopulationChanged(int population) => _isDirty = true;

        private void HandleBuildingChanged(Building building) => _isDirty = true;

        private void Refresh()
        {
            SetText(cidadeText, NewGameSettings.CityName.ToUpperInvariant());
            SetText(populacaoText, simulation.Families.TotalPopulation.ToString());
            SetText(casasText, buildingSystem != null ? buildingSystem.CompletedHouses.ToString() : "0");

            ShowReserveDays(comidaText, ResourceType.Food);
            ShowReserveDays(aguaText, ResourceType.Water);
            ShowUnits(madeiraText, ResourceType.Wood);
            ShowUnits(sucataText, ResourceType.Scrap);
            ShowUnits(combustivelText, ResourceType.Fuel);
        }

        private void ShowReserveDays(TMP_Text text, ResourceType type)
        {
            if (text == null)
            {
                return;
            }

            float demand = simulation.Needs.GetDailyDemand(type);
            float amount = simulation.Resources.GetAmount(type);
            bool hasDemand = demand > 0f;
            float days = hasDemand ? amount / demand : 0f;

            text.text = hasDemand ? Mathf.FloorToInt(days).ToString() : "-";
            text.color = hasDemand && days < lowReserveDays ? lowColor : normalColor;
        }

        private void ShowUnits(TMP_Text text, ResourceType type)
        {
            SetText(text, Mathf.FloorToInt(simulation.Resources.GetAmount(type)).ToString());
        }

        private static void SetText(TMP_Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }

        private static void HideUnsupported(params TMP_Text[] texts)
        {
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] != null)
                {
                    texts[i].gameObject.SetActive(false);
                }
            }
        }
    }
}
