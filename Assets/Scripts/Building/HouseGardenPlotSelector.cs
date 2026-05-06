using UnityEngine;

namespace Reclaim.Building
{
    /// <summary>
    /// Lightweight runtime selector for house garden size right after construction.
    /// Uses an on-screen panel with clickable options.
    /// </summary>
    public class HouseGardenPlotSelector : MonoBehaviour
    {
        [SerializeField] private float selectionWindowSeconds = 8f;
        [SerializeField] private Vector2Int smallPlotSize = new Vector2Int(2, 2);
        [SerializeField] private Vector2Int mediumPlotSize = new Vector2Int(3, 3);
        [SerializeField] private Vector2Int largePlotSize = new Vector2Int(4, 4);
        [SerializeField] private string uiTitle = "Selecione o tamanho da horta";
        [SerializeField] private bool showCountdown = true;

        public Vector2Int SelectedPlotSize { get; private set; } = new Vector2Int(2, 2);
        public bool IsSelectionComplete { get; private set; }

        private float _selectionDeadline;
        private bool _isSelecting;
        private Rect _windowRect;
        private GUIStyle _titleStyle;
        private GUIStyle _bodyStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _countdownStyle;
        private bool _stylesReady;

        public void BeginSelection(Vector2Int defaultSize)
        {
            SelectedPlotSize = defaultSize.x > 0 && defaultSize.y > 0 ? defaultSize : mediumPlotSize;
            IsSelectionComplete = false;
            _isSelecting = true;
            _selectionDeadline = Time.time + Mathf.Max(0.5f, selectionWindowSeconds);
            _windowRect = new Rect((Screen.width - 380f) * 0.5f, Screen.height - 210f, 380f, 170f);

            Debug.Log($"Garden plot selection opened for '{name}'.");
        }

        private void Update()
        {
            if (!_isSelecting)
            {
                return;
            }

            if (Time.time >= _selectionDeadline)
            {
                CompleteSelection(SelectedPlotSize);
            }
        }

        private void OnGUI()
        {
            if (!_isSelecting)
            {
                return;
            }

            EnsureStyles();

            GUILayout.BeginArea(_windowRect, GUI.skin.window);
            GUILayout.Space(8f);
            GUILayout.Label(uiTitle, _titleStyle);

            if (showCountdown)
            {
                float remaining = Mathf.Max(0f, _selectionDeadline - Time.time);
                GUILayout.Label($"Tempo restante: {remaining:0.0}s", _countdownStyle);
            }
            else
            {
                GUILayout.Space(6f);
            }

            GUILayout.Label("Escolha uma opção para confirmar:", _bodyStyle);
            GUILayout.Space(6f);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button($"Pequena\n{smallPlotSize.x}x{smallPlotSize.y}", _buttonStyle, GUILayout.Height(58f)))
            {
                CompleteSelection(smallPlotSize);
            }

            if (GUILayout.Button($"Média\n{mediumPlotSize.x}x{mediumPlotSize.y}", _buttonStyle, GUILayout.Height(58f)))
            {
                CompleteSelection(mediumPlotSize);
            }

            if (GUILayout.Button($"Grande\n{largePlotSize.x}x{largePlotSize.y}", _buttonStyle, GUILayout.Height(58f)))
            {
                CompleteSelection(largePlotSize);
            }
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        private void EnsureStyles()
        {
            if (_stylesReady)
            {
                return;
            }

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            _bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft
            };

            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter
            };

            _countdownStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleCenter
            };

            _stylesReady = true;
        }

        private void CompleteSelection(Vector2Int chosenSize)
        {
            _isSelecting = false;
            IsSelectionComplete = true;
            SelectedPlotSize = new Vector2Int(Mathf.Max(1, chosenSize.x), Mathf.Max(1, chosenSize.y));
            Debug.Log($"Garden plot selected for '{name}': {SelectedPlotSize.x}x{SelectedPlotSize.y}.");
        }
    }
}
