using System.Text;
using Reclaim.Construction;
using Reclaim.Work;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reclaim.Settlement
{
    /// <summary>
    /// Pendências do começo: erguer uma casa por família, o armazém e a oficina, e designar
    /// lenhador e catador. Só mostra o que falta; não trava nem libera nada na simulação.
    /// </summary>
    public class StartingObjectives : MonoBehaviour
    {
        private const float RefreshInterval = 0.5f;
        private const float HideDelayAfterDone = 10f;

        private SettlementSettings _settings;
        private BuildingSystem _buildings;
        private CitizenRoster _roster;
        private int _housesNeeded;
        private int _storageBuilt;
        private int _industryBuilt;
        private TMP_Text _text;
        private GameObject _panel;
        private float _nextRefresh;
        private float _doneAt = -1f;

        public bool IsComplete { get; private set; }

        public void Initialize(SettlementSettings settings, BuildingSystem buildings, CitizenRoster roster, int housesNeeded)
        {
            _settings = settings;
            _buildings = buildings;
            _roster = roster;
            _housesNeeded = housesNeeded;

            BuildUi();
            if (_buildings != null)
            {
                _buildings.OnBuildingCompleted += HandleBuildingCompleted;
            }

            Refresh();
        }

        private void OnDestroy()
        {
            if (_buildings != null)
            {
                _buildings.OnBuildingCompleted -= HandleBuildingCompleted;
            }
        }

        private void Update()
        {
            if (UnityEngine.Time.unscaledTime < _nextRefresh)
            {
                return;
            }

            _nextRefresh = UnityEngine.Time.unscaledTime + RefreshInterval;
            Refresh();

            if (IsComplete && _doneAt >= 0f && UnityEngine.Time.unscaledTime - _doneAt > HideDelayAfterDone)
            {
                _panel.SetActive(false);
            }
        }

        private void HandleBuildingCompleted(Building building)
        {
            if (_settings.StorageBuilding != null && building.Data == _settings.StorageBuilding)
            {
                _storageBuilt++;
            }

            if (_settings.IndustryBuilding != null && building.Data == _settings.IndustryBuilding)
            {
                _industryBuilt++;
            }
        }

        private void Refresh()
        {
            int houses = _buildings != null ? _buildings.CompletedHouses : 0;
            bool needStorage = _settings.StorageBuilding != null;
            bool needIndustry = _settings.IndustryBuilding != null;
            int lumberjacks = CountJob(JobType.Lumberjack);
            int scavengers = CountJob(JobType.Scavenger);

            bool done = houses >= _housesNeeded
                && (!needStorage || _storageBuilt >= 1)
                && (!needIndustry || _industryBuilt >= 1)
                && lumberjacks >= 1 && scavengers >= 1;

            StringBuilder text = new StringBuilder("<b>PENDÊNCIAS</b>\n");
            if (done)
            {
                text.Append("Colônia de pé. Boa sorte no inverno.");
                if (!IsComplete)
                {
                    _doneAt = UnityEngine.Time.unscaledTime;
                }
            }
            else
            {
                Line(text, $"Casas {Mathf.Min(houses, _housesNeeded)}/{_housesNeeded}", houses >= _housesNeeded);
                if (needStorage) Line(text, $"Armazém {Mathf.Min(_storageBuilt, 1)}/1", _storageBuilt >= 1);
                if (needIndustry) Line(text, $"Oficina {Mathf.Min(_industryBuilt, 1)}/1", _industryBuilt >= 1);
                Line(text, $"Lenhador designado {Mathf.Min(lumberjacks, 1)}/1", lumberjacks >= 1);
                Line(text, $"Catador designado {Mathf.Min(scavengers, 1)}/1", scavengers >= 1);
                text.Append("<size=70%><color=#B8B4A6>Sem ferramenta ativa, clique na Oficina pronta para designar trabalhadores.</color></size>");
            }

            IsComplete = done;
            _text.text = text.ToString();
        }

        private int CountJob(JobType job)
        {
            int count = 0;
            for (int i = 0; i < _roster.All.Count; i++)
            {
                if (_roster.All[i] != null && _roster.All[i].Job == job)
                {
                    count++;
                }
            }

            return count;
        }

        private static void Line(StringBuilder builder, string text, bool done)
        {
            builder.Append(done ? "<color=#6DD16F>[x]</color> " : "[ ] ").Append(text).Append('\n');
        }

        private void BuildUi()
        {
            GameObject canvasObject = new GameObject("ObjectivesCanvas", typeof(Canvas), typeof(CanvasScaler));
            canvasObject.transform.SetParent(transform, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            _panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            _panel.transform.SetParent(canvasObject.transform, false);
            RectTransform rect = (RectTransform)_panel.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(24f, -130f);
            rect.sizeDelta = new Vector2(430f, 230f);
            Image background = _panel.GetComponent<Image>();
            background.color = new Color(0.08f, 0.08f, 0.07f, 0.85f);
            background.raycastTarget = false;

            GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(_panel.transform, false);
            RectTransform textRect = (RectTransform)textObject.transform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(16f, 10f);
            textRect.offsetMax = new Vector2(-16f, -10f);

            _text = textObject.GetComponent<TextMeshProUGUI>();
            _text.fontSize = 24f;
            _text.color = Color.white;
            _text.alignment = TextAlignmentOptions.TopLeft;
            _text.raycastTarget = false;
        }
    }
}
