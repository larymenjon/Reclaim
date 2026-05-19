using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reclaim.UI
{
    public class FlagCreationUI : MonoBehaviour
    {
        private const string CityNameKey = "reclaim.newgame.city_name";
        private const string FlagBackgroundIndexKey = "reclaim.newgame.flag_background";
        private const string FlagSymbolColorIndexKey = "reclaim.newgame.flag_symbol_color";
        private const string FlagSymbolIndexKey = "reclaim.newgame.flag_symbol";
        private const string FlagBackgroundColorKey = "reclaim.newgame.flag_bg_color";
        private const string FlagSymbolColorHexKey = "reclaim.newgame.flag_symbol_color_hex";
        private const string DefaultCityName = "Nova Cidade";

        [Header("UI References")]
        [SerializeField] private TMP_InputField cityNameInput;
        [SerializeField] private Button backButton;
        [SerializeField] private Button continueButton;

        [Header("Flag Customization")]
        [SerializeField] private Image flagPreview;
        [SerializeField] private Image flagBackground;
        [SerializeField] private Image flagSymbol;

        [Header("Color Selection")]
        [SerializeField] private Button color1Button;
        [SerializeField] private Button color2Button;
        [SerializeField] private Button color3Button;
        [SerializeField] private Button color4Button;
        [SerializeField] private Button color5Button;

        [Header("Symbol Selection")]
        [SerializeField] private Button symbol1Button;
        [SerializeField] private Button symbol2Button;
        [SerializeField] private Button symbol3Button;
        [SerializeField] private Button symbol4Button;
        [SerializeField] private Button symbol5Button;

        [Header("Color Palettes")]
        [SerializeField] private Color[] backgroundColors;
        [SerializeField] private Color[] symbolColors;
        [SerializeField] private Sprite[] symbols;

        [Header("Manager References")]
        [SerializeField] private NewGameSetupManager setupManager;

        private Color currentBackgroundColor;
        private Color currentSymbolColor;
        private Sprite currentSymbol;
        private int selectedBackgroundIndex;
        private int selectedSymbolColorIndex;
        private int selectedSymbolIndex;

        private void Start()
        {
            InitializeFlagCreation();
        }

        private void InitializeFlagCreation()
        {
            if (setupManager == null)
            {
                setupManager = FindFirstObjectByType<NewGameSetupManager>();
            }

            if (backgroundColors.Length > 0)
            {
                currentBackgroundColor = backgroundColors[0];
            }

            if (symbolColors.Length > 0)
            {
                currentSymbolColor = symbolColors[0];
            }

            if (symbols.Length > 0)
            {
                currentSymbol = symbols[0];
            }

            UpdateFlagPreview();
            SetupColorButtons();
            SetupSymbolButtons();
            SetupInputField();
            SetupButtons();
        }

        private void SetupColorButtons()
        {
            SetupColorButton(color1Button, 0, backgroundColors);
            SetupColorButton(color2Button, 1, backgroundColors);
            SetupColorButton(color3Button, 2, backgroundColors);
            SetupColorButton(color4Button, 3, symbolColors);
            SetupColorButton(color5Button, 4, symbolColors);
        }

        private void SetupColorButton(Button button, int index, Color[] colors)
        {
            if (button == null || index >= colors.Length)
            {
                return;
            }

            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = colors[index];
            }

            button.onClick.AddListener(() => SelectColor(index, colors));
        }

        private void SetupSymbolButtons()
        {
            SetupSymbolButton(symbol1Button, 0);
            SetupSymbolButton(symbol2Button, 1);
            SetupSymbolButton(symbol3Button, 2);
            SetupSymbolButton(symbol4Button, 3);
            SetupSymbolButton(symbol5Button, 4);
        }

        private void SetupSymbolButton(Button button, int index)
        {
            if (button == null || index >= symbols.Length)
            {
                return;
            }

            button.onClick.AddListener(() => SelectSymbol(index));
        }

        private void SetupInputField()
        {
            if (cityNameInput == null)
            {
                return;
            }

            cityNameInput.text = PlayerPrefs.GetString(CityNameKey, DefaultCityName);
            cityNameInput.onValueChanged.AddListener(OnCityNameChanged);
        }

        private void SetupButtons()
        {
            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackPressed);
            }

            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinuePressed);
            }
        }

        private void SelectColor(int index, Color[] colors)
        {
            if (index >= colors.Length)
            {
                return;
            }

            if (index < 3)
            {
                selectedBackgroundIndex = index;
                currentBackgroundColor = colors[index];
            }
            else
            {
                selectedSymbolColorIndex = index - 3;
                currentSymbolColor = colors[selectedSymbolColorIndex];
            }

            UpdateFlagPreview();
        }

        private void SelectSymbol(int index)
        {
            if (index >= symbols.Length)
            {
                return;
            }

            selectedSymbolIndex = index;
            currentSymbol = symbols[index];
            UpdateFlagPreview();
        }

        private void UpdateFlagPreview()
        {
            if (flagBackground != null)
            {
                flagBackground.color = currentBackgroundColor;
            }

            if (flagSymbol != null)
            {
                flagSymbol.color = currentSymbolColor;
                flagSymbol.sprite = currentSymbol;
            }

            if (flagPreview != null)
            {
                flagPreview.color = currentBackgroundColor;
            }
        }

        private void OnCityNameChanged(string cityName)
        {
            PlayerPrefs.SetString(CityNameKey, cityName);

            if (setupManager != null)
            {
                setupManager.SetLeaderName(cityName);
            }
        }

        private void OnBackPressed()
        {
            Debug.Log("Voltar para seleção de personagem");
        }

        private void OnContinuePressed()
        {
            SaveFlagConfiguration();

            if (setupManager != null)
            {
                setupManager.OnContinuePressed();
            }
        }

        private void SaveFlagConfiguration()
        {
            PlayerPrefs.SetInt(FlagBackgroundIndexKey, selectedBackgroundIndex);
            PlayerPrefs.SetInt(FlagSymbolColorIndexKey, selectedSymbolColorIndex);
            PlayerPrefs.SetInt(FlagSymbolIndexKey, selectedSymbolIndex);

            PlayerPrefs.SetString(FlagBackgroundColorKey, ColorToString(currentBackgroundColor));
            PlayerPrefs.SetString(FlagSymbolColorHexKey, ColorToString(currentSymbolColor));
            PlayerPrefs.Save();
        }

        private static string ColorToString(Color color)
        {
            return ColorUtility.ToHtmlStringRGBA(color);
        }

        public Color GetBackgroundColor()
        {
            return currentBackgroundColor;
        }

        public Color GetSymbolColor()
        {
            return currentSymbolColor;
        }

        public Sprite GetSymbol()
        {
            return currentSymbol;
        }

        public string GetCityName()
        {
            return cityNameInput != null ? cityNameInput.text : DefaultCityName;
        }
    }
}
