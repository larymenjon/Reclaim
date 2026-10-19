using System;
using System.Collections.Generic;
using Reclaim.Gameplay;
using Reclaim.Grid;
using Reclaim.Input;
using Reclaim.Settlement;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Reclaim.Work
{
    /// <summary>
    /// Painel para designar trabalhadores. Abre ao clicar numa construção de trabalho pronta
    /// (com nenhuma ferramenta ativa) e permite aumentar ou diminuir cada ofício.
    /// </summary>
    public class WorkAssignmentPanel : MonoBehaviour
    {
        private static readonly Color PanelColor = new Color(0.09f, 0.09f, 0.08f, 0.94f);
        private static readonly Color ButtonColor = new Color(0.22f, 0.21f, 0.18f, 1f);
        private static readonly Color AccentColor = new Color(0.95f, 0.72f, 0.15f, 1f);

        private const float RefreshInterval = 0.25f;
        private const float PanelWidth = 460f;
        private const float RowHeight = 56f;

        private sealed class Row
        {
            public JobType Job;
            public TMP_Text Count;
        }

        private readonly List<Row> _rows = new List<Row>();
        private SettlementContext _context;
        private WorkSystem _workSystem;
        private ToolModeController _tools;
        private InputHandler _input;
        private Workplace _current;
        private RectTransform _panel;
        private TMP_Text _title;
        private TMP_Text _freeWorkers;
        private float _nextRefresh;

        public void Initialize(SettlementContext context, WorkSystem workSystem, InputHandler input, ToolModeController tools)
        {
            _context = context;
            _workSystem = workSystem;
            _input = input;
            _tools = tools;

            BuildCanvas();
            Close();

            if (_input != null)
            {
                _input.OnPrimaryPressed += HandlePrimaryPressed;
            }
        }

        private void OnDestroy()
        {
            if (_input != null)
            {
                _input.OnPrimaryPressed -= HandlePrimaryPressed;
            }
        }

        private void Update()
        {
            if (_current == null)
            {
                return;
            }

            if (PointerInput.KeyPressedThisFrame(Key.Escape) || _tools != null && _tools.CurrentMode != GameMode.None)
            {
                Close();
                return;
            }

            if (UnityEngine.Time.unscaledTime >= _nextRefresh)
            {
                _nextRefresh = UnityEngine.Time.unscaledTime + RefreshInterval;
                RefreshCounts();
            }
        }

        // ---------- Seleção ----------

        private void HandlePrimaryPressed(GridCoordinate cell, Vector3 worldPoint)
        {
            if (_tools != null && _tools.CurrentMode != GameMode.None)
            {
                return;
            }

            Workplace workplace = FindWorkplaceAt(cell);
            if (workplace == null)
            {
                Close();
                return;
            }

            Open(workplace);
        }

        private Workplace FindWorkplaceAt(GridCoordinate cell)
        {
            if (_context.Grid == null || !_context.Grid.TryGetCell(cell, out GridCellData data) || data.Occupant == null)
            {
                return null;
            }

            GameObject occupant = data.Occupant as GameObject;
            return occupant != null ? occupant.GetComponent<Workplace>() : null;
        }

        private void Open(Workplace workplace)
        {
            _current = workplace;
            _title.text = workplace.Title;
            RebuildRows();
            RefreshCounts();
            _panel.gameObject.SetActive(true);
        }

        private void Close()
        {
            _current = null;
            if (_panel != null)
            {
                _panel.gameObject.SetActive(false);
            }
        }

        // ---------- Conteúdo ----------

        private void RebuildRows()
        {
            for (int i = _panel.childCount - 1; i >= 0; i--)
            {
                Transform child = _panel.GetChild(i);
                if (child.name.StartsWith("Row_"))
                {
                    Destroy(child.gameObject);
                }
            }

            _rows.Clear();
            IReadOnlyList<JobSlot> slots = _current.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                _rows.Add(BuildRow(slots[i].job, i));
            }

            float height = 130f + slots.Count * RowHeight + 40f;
            _panel.sizeDelta = new Vector2(PanelWidth, height);
        }

        private void RefreshCounts()
        {
            if (_current == null)
            {
                return;
            }

            for (int i = 0; i < _rows.Count; i++)
            {
                Row row = _rows[i];
                row.Count.text = $"{_current.AssignedTo(row.Job)} / {_current.CapacityOf(row.Job)}";
            }

            _freeWorkers.text = $"Moradores livres: {_workSystem.Roster.CountFreeAdults()}";
        }

        private Row BuildRow(JobType job, int index)
        {
            RectTransform row = NewRect("Row_" + job, _panel);
            row.anchorMin = row.anchorMax = new Vector2(0.5f, 1f);
            row.sizeDelta = new Vector2(PanelWidth - 40f, RowHeight - 8f);
            row.anchoredPosition = new Vector2(0f, -100f - index * RowHeight);

            TMP_Text label = NewText("Label", row, JobTypeNames.Display(job), 26f, TextAlignmentOptions.Left);
            Stretch(label.rectTransform, new Vector2(0f, 0f), new Vector2(0.55f, 1f));

            Row result = new Row { Job = job };
            CreateButton(row, "-", new Vector2(0.62f, 0.5f), () => Change(job, false));
            result.Count = NewText("Count", row, "0 / 0", 26f, TextAlignmentOptions.Center);
            Stretch(result.Count.rectTransform, new Vector2(0.68f, 0f), new Vector2(0.88f, 1f));
            CreateButton(row, "+", new Vector2(0.94f, 0.5f), () => Change(job, true));
            return result;
        }

        private void Change(JobType job, bool add)
        {
            if (_current == null)
            {
                return;
            }

            if (add)
            {
                _current.TryAssign(job);
            }
            else
            {
                _current.Unassign(job);
            }

            RefreshCounts();
        }

        // ---------- Construção da interface ----------

        private void BuildCanvas()
        {
            GameObject canvasObject = new GameObject("WorkAssignmentCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            _panel = NewRect("Panel", canvasObject.transform);
            _panel.anchorMin = _panel.anchorMax = new Vector2(1f, 0.5f);
            _panel.pivot = new Vector2(1f, 0.5f);
            _panel.anchoredPosition = new Vector2(-40f, 0f);
            _panel.sizeDelta = new Vector2(PanelWidth, 300f);
            Image background = _panel.gameObject.AddComponent<Image>();
            background.color = PanelColor;

            _title = NewText("Title", _panel, "Local de trabalho", 32f, TextAlignmentOptions.Left);
            _title.color = AccentColor;
            _title.fontStyle = FontStyles.Bold;
            Anchor(_title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20f, -14f), new Vector2(-70f, -58f));

            _freeWorkers = NewText("Free", _panel, "", 22f, TextAlignmentOptions.Left);
            _freeWorkers.color = new Color(0.75f, 0.75f, 0.7f);
            Anchor(_freeWorkers.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20f, -62f), new Vector2(-20f, -92f));

            RectTransform closeHost = NewRect("CloseHost", _panel);
            closeHost.anchorMin = closeHost.anchorMax = new Vector2(1f, 1f);
            closeHost.sizeDelta = Vector2.zero;
            closeHost.anchoredPosition = Vector2.zero;
            CreateButton(closeHost, "X", Vector2.zero, Close, new Vector2(-34f, -34f), 44f);
        }

        private static void CreateButton(RectTransform parent, string text, Vector2 anchor, Action onClick, Vector2? position = null, float size = 44f)
        {
            RectTransform rect = NewRect("Button_" + text, parent);
            rect.anchorMin = rect.anchorMax = anchor;
            rect.sizeDelta = new Vector2(size, size);
            rect.anchoredPosition = position ?? Vector2.zero;

            Image image = rect.gameObject.AddComponent<Image>();
            image.color = ButtonColor;
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => onClick());

            TMP_Text label = NewText("Text", rect, text, 28f, TextAlignmentOptions.Center);
            Stretch(label.rectTransform, Vector2.zero, Vector2.one);
        }

        private static RectTransform NewRect(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static TMP_Text NewText(string name, Transform parent, string text, float size, TextAlignmentOptions alignment)
        {
            RectTransform rect = NewRect(name, parent);
            TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = size;
            label.alignment = alignment;
            label.color = Color.white;
            label.raycastTarget = false;
            return label;
        }

        private static void Stretch(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void Anchor(RectTransform rect, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = new Vector2(offsetMin.x, offsetMax.y);
            rect.offsetMax = new Vector2(offsetMax.x, offsetMin.y);
        }
    }
}
