using Reclaim.Flow;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reclaim.UI.NewGame
{
    /// <summary>
    /// Passo da bandeira e do nome da cidade no Novo Jogo.
    /// Cada botão de cor escolhe o fundo; cada botão de símbolo escolhe o ícone.
    /// A cor do símbolo segue a paleta de símbolos na mesma posição do fundo escolhido.
    /// </summary>
    public class FlagCreationUI : MonoBehaviour
    {
        [Header("Campos")]
        [SerializeField] private TMP_InputField cityNameInput;
        [SerializeField] private Button backButton;
        [SerializeField] private Button continueButton;

        [Header("Prévia da bandeira")]
        [SerializeField] private Image flagPreview;
        [SerializeField] private Image flagBackground;
        [SerializeField] private Image flagSymbol;

        [Header("Botões de cor e de símbolo")]
        [SerializeField] private Button color1Button;
        [SerializeField] private Button color2Button;
        [SerializeField] private Button color3Button;
        [SerializeField] private Button color4Button;
        [SerializeField] private Button color5Button;
        [SerializeField] private Button symbol1Button;
        [SerializeField] private Button symbol2Button;
        [SerializeField] private Button symbol3Button;
        [SerializeField] private Button symbol4Button;
        [SerializeField] private Button symbol5Button;

        [Header("Paletas (Ferrugem, Musgo, Osso, Aço, Carvão no Figma)")]
        [SerializeField] private Color[] backgroundColors;
        [SerializeField] private Color[] symbolColors;
        [SerializeField] private Sprite[] symbols;

        [Header("Navegação")]
        [SerializeField] private NewGameSetupManager setupManager;
        [Tooltip("Painel anterior do Novo Jogo, reaberto pelo botão Voltar.")]
        [SerializeField] private GameObject previousStepPanel;

        private int _backgroundIndex;
        private int _symbolIndex;

        public string CityName => cityNameInput != null ? cityNameInput.text : NewGameSettings.CityName;

        private void Start()
        {
            _backgroundIndex = ClampIndex(NewGameSettings.FlagBackgroundIndex, backgroundColors);
            _symbolIndex = ClampIndex(NewGameSettings.FlagSymbolIndex, symbols);

            BindColorButtons();
            BindSymbolButtons();
            BindNavigation();

            if (cityNameInput != null)
            {
                cityNameInput.text = NewGameSettings.CityName;
                cityNameInput.onEndEdit.AddListener(SaveCityName);
            }

            UpdatePreview();
        }

        private void BindColorButtons()
        {
            Button[] buttons = { color1Button, color2Button, color3Button, color4Button, color5Button };
            for (int i = 0; i < buttons.Length; i++)
            {
                BindColorButton(buttons[i], i);
            }
        }

        private void BindColorButton(Button button, int index)
        {
            if (button == null || backgroundColors == null || index >= backgroundColors.Length)
            {
                return;
            }

            if (button.TryGetComponent(out Image image))
            {
                image.color = backgroundColors[index];
            }

            button.onClick.AddListener(() => SelectBackground(index));
        }

        private void BindSymbolButtons()
        {
            Button[] buttons = { symbol1Button, symbol2Button, symbol3Button, symbol4Button, symbol5Button };
            for (int i = 0; i < buttons.Length; i++)
            {
                int index = i;
                if (buttons[i] != null && symbols != null && index < symbols.Length)
                {
                    buttons[i].onClick.AddListener(() => SelectSymbol(index));
                }
            }
        }

        private void BindNavigation()
        {
            if (backButton != null)
            {
                backButton.onClick.AddListener(GoBack);
            }

            if (continueButton != null)
            {
                continueButton.onClick.AddListener(Continue);
            }
        }

        private void SelectBackground(int index)
        {
            _backgroundIndex = index;
            UpdatePreview();
        }

        private void SelectSymbol(int index)
        {
            _symbolIndex = index;
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            Color background = PickColor(backgroundColors, _backgroundIndex, Color.gray);
            Color symbolColor = PickColor(symbolColors, _backgroundIndex, Color.white);

            if (flagBackground != null)
            {
                flagBackground.color = background;
            }

            if (flagPreview != null)
            {
                flagPreview.color = background;
            }

            if (flagSymbol != null)
            {
                flagSymbol.color = symbolColor;
                flagSymbol.sprite = symbols != null && _symbolIndex < symbols.Length ? symbols[_symbolIndex] : null;
                flagSymbol.enabled = flagSymbol.sprite != null;
            }
        }

        private void SaveCityName(string cityName)
        {
            NewGameSettings.CityName = cityName;
        }

        private void GoBack()
        {
            if (previousStepPanel == null)
            {
                Debug.LogWarning("[FlagCreationUI] Previous Step Panel não configurado; o botão Voltar não tem destino.", this);
                return;
            }

            previousStepPanel.SetActive(true);
            gameObject.SetActive(false);
        }

        private void Continue()
        {
            SaveCityName(CityName);
            NewGameSettings.FlagBackgroundIndex = _backgroundIndex;
            NewGameSettings.FlagSymbolColorIndex = _backgroundIndex;
            NewGameSettings.FlagSymbolIndex = _symbolIndex;

            if (setupManager != null)
            {
                setupManager.OnContinuePressed();
            }
        }

        private static Color PickColor(Color[] palette, int index, Color fallback)
        {
            return palette != null && index >= 0 && index < palette.Length ? palette[index] : fallback;
        }

        private static int ClampIndex<T>(int index, T[] array)
        {
            int max = array != null && array.Length > 0 ? array.Length - 1 : 0;
            return Mathf.Clamp(index, 0, max);
        }
    }
}
