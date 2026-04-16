using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reclaim.UI
{
    public class FlagCreationUI : MonoBehaviour
    {
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

        private Color currentBackgroundColor;
        private Color currentSymbolColor;
        private Sprite currentSymbol;
        private int selectedBackgroundIndex = 0;
        private int selectedSymbolColorIndex = 0;
        private int selectedSymbolIndex = 0;

        [Header("Manager References")]
        [SerializeField] private NewGameSetupManager setupManager;

        private void Start()
        {
            InitializeFlagCreation();
        }

        private void InitializeFlagCreation()
        {
            if (setupManager == null)
            {
                setupManager = FindObjectOfType<NewGameSetupManager>();
            }

            // Configurações iniciais
            if (backgroundColors.Length > 0) currentBackgroundColor = backgroundColors[0];
            if (symbolColors.Length > 0) currentSymbolColor = symbolColors[0];
            if (symbols.Length > 0) currentSymbol = symbols[0];

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
            if (button != null && index < colors.Length)
            {
                var image = button.GetComponent<Image>();
                if (image != null)
                {
                    image.color = colors[index];
                }

                button.onClick.AddListener(() => SelectColor(index, colors));
            }
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
            if (button != null && index < symbols.Length)
            {
                button.onClick.AddListener(() => SelectSymbol(index));
            }
        }

        private void SetupInputField()
        {
            if (cityNameInput != null)
            {
                // Carrega nome salvo ou usa padrão
                string savedName = PlayerPrefs.GetString("reclaim.newgame.city_name", "Nova Cidade");
                cityNameInput.text = savedName;
                
                cityNameInput.onValueChanged.AddListener(OnCityNameChanged);
            }
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
            if (index < colors.Length)
            {
                if (index < 3) // Cores de fundo
                {
                    selectedBackgroundIndex = index;
                    currentBackgroundColor = colors[index];
                }
                else // Cores de símbolo
                {
                    selectedSymbolColorIndex = index - 3;
                    currentSymbolColor = colors[selectedSymbolColorIndex];
                }

                UpdateFlagPreview();
            }
        }

        private void SelectSymbol(int index)
        {
            if (index < symbols.Length)
            {
                selectedSymbolIndex = index;
                currentSymbol = symbols[index];
                UpdateFlagPreview();
            }
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
                // Atualiza a imagem de preview (pode ser um renderizado combinado)
                flagPreview.color = currentBackgroundColor;
            }
        }

        private void OnCityNameChanged(string cityName)
        {
            // Salva o nome da cidade
            PlayerPrefs.SetString("reclaim.newgame.city_name", cityName);
            
            // Atualiza no setup manager
            if (setupManager != null)
            {
                setupManager.SetLeaderName(cityName);
            }
        }

        private void OnBackPressed()
        {
            // Volta para a seleção de personagem
            // Aqui você pode implementar a navegação entre telas
            Debug.Log("Voltar para seleção de personagem");
        }

        private void OnContinuePressed()
        {
            // Salva as configurações da bandeira
            SaveFlagConfiguration();
            
            // Continua para a próxima fase (mapa e dificuldade)
            if (setupManager != null)
            {
                setupManager.OnContinuePressed();
            }
        }

        private void SaveFlagConfiguration()
        {
            // Salva as configurações da bandeira nos PlayerPrefs
            PlayerPrefs.SetInt("reclaim.newgame.flag_background", selectedBackgroundIndex);
            PlayerPrefs.SetInt("reclaim.newgame.flag_symbol_color", selectedSymbolColorIndex);
            PlayerPrefs.SetInt("reclaim.newgame.flag_symbol", selectedSymbolIndex);
            
            // Salva as cores como strings (opcional, para persistência mais robusta)
            PlayerPrefs.SetString("reclaim.newgame.flag_bg_color", ColorToString(currentBackgroundColor));
            PlayerPrefs.SetString("reclaim.newgame.flag_symbol_color_hex", ColorToString(currentSymbolColor));
            
            PlayerPrefs.Save();
        }

        private string ColorToString(Color color)
        {
            return ColorUtility.ToHtmlStringRGBA(color);
        }

        private Color StringToColor(string colorString)
        {
            ColorUtility.TryParseHtmlString($"#{colorString}", out Color color);
            return color;
        }

        // Métodos públicos para obter as configurações da bandeira
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
            return cityNameInput != null ? cityNameInput.text : "Nova Cidade";
        }
    }
}