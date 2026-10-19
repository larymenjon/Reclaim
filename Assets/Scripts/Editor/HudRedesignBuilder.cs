using System.Collections.Generic;
using Reclaim.Calendar;
using Reclaim.Construction;
using Reclaim.Economy;
using Reclaim.Gameplay;
using Reclaim.Road;
using Reclaim.Simulation;
using Reclaim.UI.Hud;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Reclaim.EditorTools
{
    /// <summary>
    /// Menu Reclaim > Criar HUD do redesign. Monta na cena aberta o Canvas da HUD do Figma
    /// (barra superior, cartão de evento, notificações, dia e noite, estação, calendário, aviso rápido
    /// e doca de construção), liga todas as referências e desliga a HUD antiga sem apagá-la.
    /// Rodar de novo recria a HUD do zero.
    /// </summary>
    public static class HudRedesignBuilder
    {
        private const string RootName = "HUD Redesign";
        private const float Margin = 24f;
        private const float TopBarHeight = 76f;

        private static readonly Vector2 TopCenter = new Vector2(0.5f, 1f);
        private static readonly Vector2 TopLeft = new Vector2(0f, 1f);
        private static readonly Vector2 BottomLeft = Vector2.zero;
        private static readonly Vector2 BottomCenter = new Vector2(0.5f, 0f);
        private static readonly Vector2 BottomRight = new Vector2(1f, 0f);
        private static readonly Vector2 Center = new Vector2(0.5f, 0.5f);

        [MenuItem("Reclaim/Criar HUD do redesign")]
        public static void Build()
        {
            ColonySimulation simulation = Find<ColonySimulation>();
            if (simulation == null)
            {
                EditorUtility.DisplayDialog("Reclaim", "Esta cena não tem ColonySimulation. Rode Reclaim > Configurar cena de jogo antes.", "OK");
                return;
            }

            GameObject previous = FindRoot();
            if (previous != null)
            {
                if (!EditorUtility.DisplayDialog("Reclaim", "Já existe uma HUD do redesign nesta cena. Recriar do zero?", "Recriar", "Cancelar"))
                {
                    return;
                }

                Undo.DestroyObjectImmediate(previous);
            }

            HudKit.EnsureSpriteImportSettings();
            HudKit kit = HudKit.Load();
            if (kit == null)
            {
                return;
            }

            AssignResourceIcons(kit);
            List<BuildingData> buildings = LoadBuildings(kit);

            var context = new SceneRefs
            {
                Simulation = simulation,
                Clock = Find<SimulationClock>(),
                DayNight = Find<DayNightCycle>(),
                Climate = simulation.Climate,
                Buildings = Find<BuildingSystem>(),
                Tools = Find<ToolModeController>()
            };

            RectTransform root = CreateCanvas();
            // Ponte própria com o ToolModeController, para não depender de nada dentro da HUD antiga.
            context.BuildHud = root.gameObject.AddComponent<BuildHudController>();
            Set(context.BuildHud, "toolModeController", context.Tools);

            BuildTopBar(kit, root, context);
            EventCardView eventCard = BuildEventCard(kit, root, context);
            NotificationFeedView feed = BuildNotifications(kit, root);
            BuildDayNight(kit, root, context);
            BuildSeason(kit, root, context);
            BuildCalendar(kit, root, context);
            ToastView toast = BuildToast(kit, root, context);
            BuildDock(kit, root, context, buildings);
            BuildFeedbackRouter(kit, root, context, feed, toast);
            eventCard.transform.SetAsLastSibling();

            Undo.RegisterCreatedObjectUndo(root.gameObject, "Criar HUD do redesign");
            DisableLegacyHud();
            AssetDatabase.SaveAssets();

            EditorSceneManager.MarkSceneDirty(root.gameObject.scene);
            Selection.activeGameObject = root.gameObject;
            ReportMissing(context);
            Debug.Log("[Reclaim] HUD do redesign criada. A HUD antiga foi desligada, não apagada.");
        }

        private sealed class SceneRefs
        {
            public ColonySimulation Simulation;
            public SimulationClock Clock;
            public DayNightCycle DayNight;
            public Reclaim.Climate.ClimateSystem Climate;
            public BuildingSystem Buildings;
            public ToolModeController Tools;
            public BuildHudController BuildHud;
        }

        // ---------- Canvas ----------

        private static RectTransform CreateCanvas()
        {
            RectTransform root = HudKit.Rect(RootName, null);
            Canvas canvas = root.gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            CanvasScaler scaler = root.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            root.gameObject.AddComponent<GraphicRaycaster>();
            return root;
        }

        // ---------- Barra superior ----------

        private static void BuildTopBar(HudKit kit, RectTransform root, SceneRefs refs)
        {
            RectTransform bar = HudKit.Place(HudKit.Rect("Barra superior", root), TopCenter, TopCenter, new Vector2(0f, -12f), new Vector2(1500f, TopBarHeight));
            TopBarView view = bar.gameObject.AddComponent<TopBarView>();

            Image left = kit.Image(bar, "Fundo esquerdo", kit.Shape("BarraSuperior_Esquerda"), Color.white, raycast: true);
            HudKit.Place(left.rectTransform, TopLeft, TopLeft, Vector2.zero, new Vector2(585f, 66f));
            left.preserveAspect = false;
            Image right = kit.Image(bar, "Fundo direito", kit.Shape("BarraSuperior_Direita"), Color.white, raycast: true);
            HudKit.Place(right.rectTransform, TopLeft, TopLeft, new Vector2(915f, 0f), new Vector2(585f, 66f));
            right.preserveAspect = false;

            RectTransform leftGroup = CounterGroup(bar, "Recursos esquerda", 40f);
            RectTransform rightGroup = CounterGroup(bar, "Recursos direita", 950f);

            TMP_Text cityName = BuildNamePlate(kit, bar);

            Set(view, "simulation", refs.Simulation);
            Set(view, "buildingSystem", refs.Buildings);
            Set(view, "theme", kit.Theme);
            Set(view, "cityNameText", cityName);
            Set(view, "houses", Counter(kit, leftGroup, "Casas", "construcao_moradia", false));
            Set(view, "population", Counter(kit, leftGroup, "População", "recurso_populacao", true));
            Set(view, "food", Counter(kit, leftGroup, "Comida", "recurso_comida", true));
            Set(view, "water", Counter(kit, leftGroup, "Água", "recurso_agua", true));
            Set(view, "medicine", Counter(kit, leftGroup, "Remédios", "recurso_remedios", true));
            Set(view, "wood", Counter(kit, rightGroup, "Madeira", "recurso_madeira", false));
            Set(view, "scrap", Counter(kit, rightGroup, "Sucata", "recurso_sucata", true));
            Set(view, "fuel", Counter(kit, rightGroup, "Combustível", "recurso_combustivel", true));
        }

        private static RectTransform CounterGroup(RectTransform bar, string name, float x)
        {
            RectTransform group = HudKit.Place(HudKit.Rect(name, bar), TopLeft, TopLeft, new Vector2(x, 0f), new Vector2(520f, 66f));
            HudKit.Row(group.gameObject, 6f, TextAnchor.MiddleLeft);
            return group;
        }

        private static TMP_Text BuildNamePlate(HudKit kit, RectTransform bar)
        {
            Image plate = kit.Image(bar, "Placa do nome", kit.Shape("PlacaNome"), Color.white, raycast: true);
            plate.preserveAspect = false;
            HudKit.Place(plate.rectTransform, TopLeft, TopLeft, new Vector2(585f, 0f), new Vector2(330f, TopBarHeight));
            Shadow shadow = plate.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
            shadow.effectDistance = new Vector2(0f, -6f);
            HudKit.Row(plate.gameObject, 20f, TextAnchor.MiddleCenter, new RectOffset(16, 16, 0, 0));

            Color ornament = kit.Theme.TextTertiary;
            HudKit.FixedSize(kit.Image(plate.transform, "Ornamento", kit.Icon("especialidade_bussola"), ornament).gameObject, 24f, 24f);
            TextMeshProUGUI name = kit.Text(plate.transform, "Nome", "Nova Cidade", kit.ExtraBold, 34f, kit.Theme.TextInverse,
                TextAlignmentOptions.Center, 6f, upperCase: true);
            name.rectTransform.sizeDelta = new Vector2(150f, 40f);
            HudKit.FixedSize(kit.Image(plate.transform, "Ornamento", kit.Icon("especialidade_bussola"), ornament).gameObject, 24f, 24f);
            return name;
        }

        private static ResourceCounterView Counter(HudKit kit, Transform parent, string name, string icon, bool separator)
        {
            RectTransform rect = HudKit.Rect($"Contador {name}", parent);
            rect.sizeDelta = new Vector2(100f, 38f);
            HudKit.Row(rect.gameObject, 8f, TextAnchor.MiddleLeft, new RectOffset(6, 6, 6, 6));

            GameObject diamond = null;
            if (separator)
            {
                diamond = kit.Image(rect, "Losango", kit.Shape("Losango"), kit.Theme.TextTertiary).gameObject;
                HudKit.FixedSize(diamond, 8f, 8f);
            }

            Image iconImage = kit.Image(rect, "Ícone", kit.Icon(icon), kit.Theme.TextPrimary);
            HudKit.FixedSize(iconImage.gameObject, 26f, 26f);
            TextMeshProUGUI value = kit.Text(rect, "Valor", "0", kit.Bold, 24f, kit.Theme.TextPrimary);
            TextMeshProUGUI unit = kit.Text(rect, "Unidade", "un.", kit.SemiBold, 12f, kit.Theme.TextSecondary, letterSpacing: 8f, upperCase: true);

            ResourceCounterView view = rect.gameObject.AddComponent<ResourceCounterView>();
            Set(view, "theme", kit.Theme);
            Set(view, "icon", iconImage);
            Set(view, "valueText", value);
            Set(view, "unitText", unit);
            Set(view, "separator", diamond);
            return view;
        }

        // ---------- Cartão de evento ----------

        private static EventCardView BuildEventCard(HudKit kit, RectTransform root, SceneRefs refs)
        {
            HudTheme theme = kit.Theme;
            RectTransform holder = HudKit.Place(HudKit.Rect("Cartão de evento", root), TopCenter, TopCenter,
                new Vector2(0f, -(12f + TopBarHeight + 16f)), new Vector2(580f, 164f));
            EventCardView view = holder.gameObject.AddComponent<EventCardView>();

            Image card = kit.Image(holder, "Cartão", null, theme.Panel, raycast: true);
            HudKit.Place(card.rectTransform, TopCenter, TopCenter, Vector2.zero, new Vector2(580f, 164f));
            HudKit.Column(card.gameObject, 14f, TextAnchor.UpperLeft, new RectOffset(20, 20, 22, 18));
            HudKit.Fit(card.gameObject, false, true);
            kit.Border(card.transform, theme.Border);
            Image stripe = kit.Image(card.transform, "Faixa", null, theme.Critical);
            HudKit.Place(stripe.rectTransform, TopLeft, TopLeft, Vector2.zero, new Vector2(0f, 3f));
            stripe.rectTransform.anchorMax = Vector2.one;
            HudKit.IgnoreLayout(stripe.gameObject);

            RectTransform top = HudKit.Rect("Topo", card.transform);
            HudKit.Row(top.gameObject, 16f, TextAnchor.UpperLeft, controlWidth: true, controlHeight: true);
            Image iconBackground = kit.Image(top, "Fundo do ícone", null, theme.CriticalDark);
            HudKit.FixedSize(iconBackground.gameObject, 55f, 55f, 0f);
            Image icon = kit.Image(iconBackground.transform, "Ícone", kit.Icon("ameaca_doenca"), theme.TextPrimary);
            HudKit.Place(icon.rectTransform, Center, Center, Vector2.zero, new Vector2(31f, 31f));

            RectTransform texts = HudKit.Rect("Texto", top);
            HudKit.Column(texts.gameObject, 4f, TextAnchor.UpperLeft);
            HudKit.FixedSize(texts.gameObject, -1f, -1f, 1f);
            TextMeshProUGUI title = kit.Text(texts, "Título", "Evento", kit.ExtraBold, 28f, theme.TextPrimary, TextAlignmentOptions.TopLeft, upperCase: true, wrap: true);
            TextMeshProUGUI description = kit.Text(texts, "Descrição", "", kit.Regular, 18f, theme.TextSecondary, TextAlignmentOptions.TopLeft, wrap: true);

            RectTransform choices = HudKit.Rect("Escolhas", card.transform);
            HudKit.Row(choices.gameObject, 8f, TextAnchor.MiddleCenter, controlWidth: true, controlHeight: true, expandWidth: true, expandHeight: true);
            HudKit.FixedSize(choices.gameObject, -1f, 50f);

            var buttons = new Object[3];
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i] = ChoiceButton(kit, choices, $"Escolha {i + 1}");
            }

            Set(view, "simulation", refs.Simulation);
            Set(view, "theme", theme);
            Set(view, "card", card.gameObject);
            Set(view, "topStripe", stripe);
            Set(view, "iconBackground", iconBackground);
            Set(view, "icon", icon);
            Set(view, "fallbackIcon", kit.Icon("ameaca_doenca"));
            Set(view, "titleText", title);
            Set(view, "descriptionText", description);
            SetArray(view, "choiceButtons", buttons);
            Set(view, "acceptIcon", kit.Icon("acao_aceitar"));
            Set(view, "refuseIcon", kit.Icon("acao_recusar"));

            card.gameObject.SetActive(false);
            return view;
        }

        private static EventChoiceButtonView ChoiceButton(HudKit kit, Transform parent, string name)
        {
            Image background = kit.Image(parent, name, null, kit.Theme.PanelRaised, raycast: true);
            Button button = background.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            background.gameObject.AddComponent<CanvasGroup>();
            HudKit.Row(background.gameObject, 8f, TextAnchor.MiddleCenter, new RectOffset(12, 12, 12, 12));

            Image border = kit.Border(background.transform, kit.Theme.Good);
            Image icon = kit.Image(background.transform, "Ícone", kit.Icon("acao_aceitar"), kit.Theme.Good);
            HudKit.FixedSize(icon.gameObject, 24f, 24f);
            TextMeshProUGUI label = kit.Text(background.transform, "Rótulo", "Escolha", kit.ExtraBold, 18f, kit.Theme.Good,
                TextAlignmentOptions.Center, 4f, upperCase: true);

            EventChoiceButtonView view = background.gameObject.AddComponent<EventChoiceButtonView>();
            Set(view, "border", border);
            Set(view, "icon", icon);
            Set(view, "label", label);
            return view;
        }

        // ---------- Notificações ----------

        private static NotificationFeedView BuildNotifications(HudKit kit, RectTransform root)
        {
            RectTransform feedRect = HudKit.Place(HudKit.Rect("Notificações", root), TopLeft, TopLeft,
                new Vector2(Margin, -(12f + TopBarHeight + 16f)), new Vector2(380f, 0f));
            HudKit.Column(feedRect.gameObject, 8f, TextAnchor.UpperLeft);
            HudKit.Fit(feedRect.gameObject, false, true);
            NotificationFeedView feed = feedRect.gameObject.AddComponent<NotificationFeedView>();

            var slots = new Object[4];
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i] = NotificationItem(kit, feedRect, $"Notificação {i + 1}");
            }

            Set(feed, "theme", kit.Theme);
            SetArray(feed, "slots", slots);
            Set(feed, "criticalIcon", kit.Icon("ameaca_morte"));
            Set(feed, "alertIcon", kit.Icon("recurso_agua"));
            Set(feed, "successIcon", kit.Icon("acao_martelo"));
            Set(feed, "radioIcon", kit.Icon("painel_radio"));
            return feed;
        }

        private static NotificationItemView NotificationItem(HudKit kit, Transform parent, string name)
        {
            HudTheme theme = kit.Theme;
            Image background = kit.Image(parent, name, null, theme.PanelRaised, raycast: true);
            background.gameObject.AddComponent<CanvasGroup>();
            Button button = background.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            HudKit.FixedSize(background.gameObject, -1f, 60f);
            HudKit.Row(background.gameObject, 12f, TextAnchor.MiddleLeft, new RectOffset(0, 12, 0, 0), true, true, false, true);

            Image stripe = kit.Image(background.transform, "Faixa", null, theme.Critical);
            HudKit.FixedSize(stripe.gameObject, 6f, -1f, 0f);

            RectTransform iconFrame = HudKit.Rect("Ícone", background.transform);
            HudKit.FixedSize(iconFrame.gameObject, 24f, -1f, 0f);
            Image icon = kit.Image(iconFrame, "Imagem", kit.Icon("ameaca_morte"), theme.Critical);
            HudKit.Place(icon.rectTransform, Center, Center, Vector2.zero, new Vector2(24f, 24f));

            RectTransform texts = HudKit.Rect("Texto", background.transform);
            HudKit.Column(texts.gameObject, 2f, TextAnchor.MiddleLeft, new RectOffset(0, 0, 10, 10));
            HudKit.FixedSize(texts.gameObject, -1f, -1f, 1f);

            RectTransform line = HudKit.Rect("Linha", texts);
            HudKit.Row(line.gameObject, 6f, TextAnchor.MiddleLeft, controlWidth: true, controlHeight: true);
            TextMeshProUGUI title = kit.Text(line, "Título", "Título", kit.Bold, 14f, theme.Critical, letterSpacing: 4f, upperCase: true);
            TextMeshProUGUI message = kit.Text(line, "Mensagem", "", kit.Regular, 14f, theme.TextPrimary);
            HudKit.FixedSize(message.gameObject, -1f, -1f, 1f);
            TextMeshProUGUI detail = kit.Text(texts, "Detalhe", "", kit.Regular, 14f, theme.TextTertiary);

            NotificationItemView view = background.gameObject.AddComponent<NotificationItemView>();
            Set(view, "stripe", stripe);
            Set(view, "icon", icon);
            Set(view, "titleText", title);
            Set(view, "messageText", message);
            Set(view, "detailText", detail);
            Set(view, "button", button);
            return view;
        }

        // ---------- Dia e noite, estação, calendário ----------

        private static void BuildDayNight(HudKit kit, RectTransform root, SceneRefs refs)
        {
            RectTransform panel = HudKit.Place(HudKit.Rect("Dia e noite", root), BottomLeft, BottomLeft, new Vector2(Margin, Margin), new Vector2(190f, 110f));
            DayNightView view = panel.gameObject.AddComponent<DayNightView>();
            Orb(kit, panel, view, "night", "Noite", "tempo_noite", 0f);
            Orb(kit, panel, view, "day", "Dia", "tempo_dia", 100f);
            Set(view, "dayNightCycle", refs.DayNight);
            Set(view, "theme", kit.Theme);
        }

        private static void Orb(HudKit kit, RectTransform parent, DayNightView view, string field, string label, string icon, float x)
        {
            RectTransform frame = HudKit.Place(HudKit.Rect(label, parent), TopLeft, TopLeft, new Vector2(x, 0f), new Vector2(90f, 110f));
            CanvasGroup group = frame.gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;

            Image orb = kit.Image(frame, "Orbe", kit.Shape("Orbe"), Color.white);
            HudKit.Place(orb.rectTransform, TopCenter, TopCenter, Vector2.zero, new Vector2(90f, 90f));
            Image ring = kit.Image(frame, "Anel", kit.Shape("OrbeAnel"), kit.Theme.TextPrimary);
            HudKit.Place(ring.rectTransform, TopCenter, TopCenter, Vector2.zero, new Vector2(90f, 90f));
            Image glyph = kit.Image(frame, "Ícone", kit.Icon(icon), kit.Theme.TextPrimary);
            HudKit.Place(glyph.rectTransform, TopCenter, TopCenter, new Vector2(0f, -26f), new Vector2(38f, 38f));
            TextMeshProUGUI text = kit.Text(frame, "Rótulo", label, kit.SemiBold, 12f, kit.Theme.TextPrimary, TextAlignmentOptions.Center, 8f, upperCase: true);
            HudKit.Place(text.rectTransform, BottomCenter, BottomCenter, Vector2.zero, new Vector2(90f, 14f));

            Set(view, field + ".group", group);
            Set(view, field + ".ring", ring);
            Set(view, field + ".label", text);
        }

        private static void BuildSeason(HudKit kit, RectTransform root, SceneRefs refs)
        {
            HudTheme theme = kit.Theme;
            RectTransform panel = HudKit.Place(HudKit.Rect("Estação", root), BottomLeft, BottomLeft,
                new Vector2(Margin + 190f + Margin, Margin + 16f), new Vector2(240f, 79f));
            SeasonView view = panel.gameObject.AddComponent<SeasonView>();

            TextMeshProUGUI seasonName = kit.Text(panel, "Nome", "Inverno", kit.Bold, 14f, theme.TextPrimary, TextAlignmentOptions.MidlineLeft, 4f, upperCase: true);
            HudKit.Place(seasonName.rectTransform, TopLeft, TopLeft, Vector2.zero, new Vector2(140f, 17f));
            TextMeshProUGUI day = kit.Text(panel, "Dia", "Dia 1/20", kit.SemiBold, 12f, theme.TextTertiary, TextAlignmentOptions.MidlineRight, 4f, upperCase: true);
            HudKit.Place(day.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), Vector2.zero, new Vector2(100f, 17f));

            string[] icons = { "tempo_inverno", "tempo_primavera", "tempo_verao", "tempo_outono" };
            var images = new Object[icons.Length];
            for (int i = 0; i < icons.Length; i++)
            {
                RectTransform slot = HudKit.Place(HudKit.Rect($"Estação {i + 1}", panel), TopLeft, TopLeft, new Vector2(i * 60f, -25f), new Vector2(60f, 36f));
                Image image = kit.Image(slot, "Ícone", kit.Icon(icons[i]), theme.TextTertiary);
                HudKit.Place(image.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(24f, 24f));
                images[i] = image;
            }

            Image track = kit.Image(panel, "Barra", null, theme.PanelSunken);
            HudKit.Place(track.rectTransform, BottomLeft, BottomLeft, Vector2.zero, new Vector2(240f, 10f));
            Image fill = kit.Image(track.transform, "Preenchimento", kit.Shape("Branco"), theme.Progress);
            fill.preserveAspect = false;
            HudKit.Stretch(fill.rectTransform);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 0f;
            kit.Border(track.transform, theme.Border);

            Set(view, "clock", refs.Clock);
            Set(view, "theme", theme);
            Set(view, "seasonNameText", seasonName);
            Set(view, "dayOfSeasonText", day);
            Set(view, "yearFill", fill);
            SetArray(view, "seasonIcons", images);
        }

        private static void BuildCalendar(HudKit kit, RectTransform root, SceneRefs refs)
        {
            HudTheme theme = kit.Theme;
            RectTransform panel = HudKit.Place(HudKit.Rect("Calendário", root), BottomRight, BottomRight, new Vector2(-Margin, Margin), new Vector2(240f, 117f));
            CalendarView view = panel.gameObject.AddComponent<CalendarView>();

            string[] fields = { "pauseButton", "normalButton", "fastButton", "fastestButton" };
            string[] icons = { "tempo_pausa", "tempo_1x", "tempo_2x", "tempo_4x" };
            string[] names = { "Pausar", "1×", "2×", "4×" };
            for (int i = 0; i < fields.Length; i++)
            {
                Image hitArea = kit.Image(panel, names[i], null, Color.clear, raycast: true);
                HudKit.Place(hitArea.rectTransform, TopLeft, TopLeft, new Vector2(i * 64f, 0f), new Vector2(48f, 48f));
                Image icon = kit.Image(hitArea.transform, "Ícone", kit.Icon(icons[i]), theme.TextTertiary);
                HudKit.Place(icon.rectTransform, Center, Center, Vector2.zero, new Vector2(36f, 36f));
                Button button = hitArea.gameObject.AddComponent<Button>();
                button.targetGraphic = icon;
                Set(view, fields[i], button);
            }

            TextMeshProUGUI date = kit.Text(panel, "Data", "Ano 1 · Dia 1", kit.Bold, 22f, theme.TextPrimary, TextAlignmentOptions.MidlineRight, 2f, upperCase: true);
            HudKit.Place(date.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(0f, -58f), new Vector2(240f, 25f));

            RectTransform hourRow = HudKit.Place(HudKit.Rect("Hora", panel), BottomRight, BottomRight, Vector2.zero, new Vector2(240f, 24f));
            HudKit.Row(hourRow.gameObject, 6f, TextAnchor.MiddleRight);
            TextMeshProUGUI time = kit.Text(hourRow, "Hora", "Manhã 07:00", kit.Bold, 14f, theme.TextSecondary, letterSpacing: 4f, upperCase: true);
            Image thermometer = kit.Image(hourRow, "Ícone temperatura", kit.Icon("status_temperatura"), theme.Info);
            HudKit.FixedSize(thermometer.gameObject, 24f, 24f);
            TextMeshProUGUI temperature = kit.Text(hourRow, "Temperatura", "0°C", kit.Bold, 14f, theme.Info);

            Set(view, "clock", refs.Clock);
            Set(view, "climate", refs.Climate);
            Set(view, "theme", theme);
            Set(view, "dateText", date);
            Set(view, "timeText", time);
            Set(view, "temperatureText", temperature);
            Set(view, "temperatureIcon", thermometer);
        }

        // ---------- Aviso rápido e doca ----------

        private static ToastView BuildToast(HudKit kit, RectTransform root, SceneRefs refs)
        {
            HudTheme theme = kit.Theme;
            RectTransform holder = HudKit.Place(HudKit.Rect("Aviso rápido", root), BottomCenter, BottomCenter,
                new Vector2(0f, Margin + 186f + 16f), new Vector2(600f, 39f));
            ToastView view = holder.gameObject.AddComponent<ToastView>();

            Image info = kit.Image(holder, "Info", null, theme.PanelSunken);
            HudKit.Place(info.rectTransform, BottomCenter, BottomCenter, Vector2.zero, new Vector2(318f, 39f));
            HudKit.Row(info.gameObject, 0f, TextAnchor.MiddleCenter, new RectOffset(20, 20, 10, 10), true, true);
            HudKit.Fit(info.gameObject, true, true);
            kit.Border(info.transform, theme.Border);
            TextMeshProUGUI message = kit.Text(info.transform, "Mensagem", "", kit.Bold, 14f, theme.TextPrimary, TextAlignmentOptions.Center, 6f, upperCase: true);

            TextMeshProUGUI paused = kit.Text(holder, "Pausado", "Pausado", kit.Bold, 14f, theme.Warning, TextAlignmentOptions.Center, 40f, upperCase: true);
            HudKit.Place(paused.rectTransform, BottomCenter, BottomCenter, Vector2.zero, new Vector2(300f, 39f));

            Set(view, "clock", refs.Clock);
            Set(view, "infoRoot", info.gameObject);
            Set(view, "infoText", message);
            Set(view, "pausedRoot", paused.gameObject);
            return view;
        }

        private static void BuildDock(HudKit kit, RectTransform root, SceneRefs refs, List<BuildingData> buildings)
        {
            RectTransform dock = HudKit.Place(HudKit.Rect("Doca de construção", root), BottomCenter, BottomCenter, new Vector2(0f, Margin), new Vector2(0f, 186f));
            HudKit.Row(dock.gameObject, 8f, TextAnchor.LowerCenter, controlWidth: false);
            HudKit.Fit(dock.gameObject, true, false);
            BuildDockView view = dock.gameObject.AddComponent<BuildDockView>();

            BuildCardView template = BuildCard(kit, dock);

            Set(view, "simulation", refs.Simulation);
            Set(view, "buildingSystem", refs.Buildings);
            Set(view, "toolModeController", refs.Tools);
            Set(view, "buildHudController", refs.BuildHud);
            Set(view, "cardTemplate", template);
            Set(view, "cardsParent", dock);
            SetArray(view, "buildings", buildings.ToArray());
            Set(view, "fallbackArt", kit.Icon("acao_martelo"));
            Set(view, "roadArt", kit.Icon("construcao_estrada"));
            template.gameObject.SetActive(false);
        }

        private static BuildCardView BuildCard(HudKit kit, Transform parent)
        {
            HudTheme theme = kit.Theme;
            Image background = kit.Image(parent, "Cartão modelo", null, theme.PanelRaised, raycast: true);
            background.rectTransform.sizeDelta = new Vector2(172f, 186f);
            Button button = background.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            HudKit.Column(background.gameObject, 8f, TextAnchor.UpperLeft, new RectOffset(12, 12, 12, 12));
            Image border = kit.Border(background.transform, theme.Border);

            Image artFrame = kit.Image(background.transform, "Arte", null, theme.Panel);
            HudKit.FixedSize(artFrame.gameObject, -1f, 70f);
            Image art = kit.Image(artFrame.transform, "Ícone", kit.Icon("construcao_moradia"), theme.TextPrimary);
            HudKit.Place(art.rectTransform, Center, Center, Vector2.zero, new Vector2(38f, 38f));
            TextMeshProUGUI size = kit.Text(artFrame.transform, "Tamanho", "2×2", kit.SemiBold, 12f, theme.TextTertiary, TextAlignmentOptions.TopRight);
            HudKit.Place(size.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-6f, -6f), new Vector2(40f, 14f));

            TextMeshProUGUI name = kit.Text(background.transform, "Nome", "Construção", kit.Bold, 22f, theme.TextPrimary, TextAlignmentOptions.TopLeft, wrap: true);
            HudKit.FixedSize(name.gameObject, -1f, 50f);

            RectTransform costs = HudKit.Rect("Custo", background.transform);
            HudKit.Row(costs.gameObject, 12f, TextAnchor.MiddleLeft);
            HudKit.FixedSize(costs.gameObject, -1f, 24f);

            BuildCardView view = background.gameObject.AddComponent<BuildCardView>();
            for (int i = 0; i < 2; i++)
            {
                RectTransform slot = HudKit.Rect($"Custo {i + 1}", costs);
                slot.sizeDelta = new Vector2(50f, 24f);
                HudKit.Row(slot.gameObject, 4f, TextAnchor.MiddleLeft);
                Image icon = kit.Image(slot, "Ícone", null, theme.TextPrimary);
                HudKit.FixedSize(icon.gameObject, 24f, 24f);
                TextMeshProUGUI amount = kit.Text(slot, "Quantidade", "0", kit.Bold, 18f, theme.TextPrimary);

                Set(view, $"costSlots.Array.data[{i}].root", slot.gameObject);
                Set(view, $"costSlots.Array.data[{i}].icon", icon);
                Set(view, $"costSlots.Array.data[{i}].amount", amount);
            }

            Set(view, "theme", theme);
            Set(view, "border", border);
            Set(view, "borderNormal", kit.Shape("Borda1px_9s"));
            Set(view, "borderSelected", kit.Shape("Borda2px_9s"));
            Set(view, "art", art);
            Set(view, "sizeText", size);
            Set(view, "nameText", name);
            return view;
        }

        private static void BuildFeedbackRouter(HudKit kit, RectTransform root, SceneRefs refs, NotificationFeedView feed, ToastView toast)
        {
            HudFeedbackRouter router = root.gameObject.AddComponent<HudFeedbackRouter>();
            Set(router, "simulation", refs.Simulation);
            Set(router, "buildingSystem", refs.Buildings);
            Set(router, "toolModeController", refs.Tools);
            Set(router, "theme", kit.Theme);
            Set(router, "notifications", feed);
            Set(router, "toast", toast);
        }

        // ---------- Dados ----------

        /// <summary>Recursos sem ícone recebem o do redesign. Ícones já escolhidos não são trocados.</summary>
        private static void AssignResourceIcons(HudKit kit)
        {
            var iconByType = new Dictionary<ResourceType, string>
            {
                { ResourceType.Food, "recurso_comida" },
                { ResourceType.Water, "recurso_agua" },
                { ResourceType.Wood, "recurso_madeira" },
                { ResourceType.Scrap, "recurso_sucata" },
                { ResourceType.Fuel, "recurso_combustivel" },
                { ResourceType.Medicine, "recurso_remedios" }
            };

            foreach (ResourceDefinition definition in LoadAll<ResourceDefinition>())
            {
                if (definition.Icon == null && iconByType.TryGetValue(definition.ResourceType, out string icon))
                {
                    Set(definition, "icon", kit.Icon(icon));
                }
            }
        }

        /// <summary>Todas as BuildingData com prefab, por nome. As que não têm ícone recebem um do redesign.</summary>
        private static List<BuildingData> LoadBuildings(HudKit kit)
        {
            var buildings = new List<BuildingData>();
            foreach (BuildingData data in LoadAll<BuildingData>())
            {
                if (data.Prefab == null)
                {
                    Debug.LogWarning($"[Reclaim] '{data.name}' não tem prefab e ficou fora da doca.", data);
                    continue;
                }

                if (data.Icon == null)
                {
                    Set(data, "icon", kit.Icon(GuessBuildingIcon(data)));
                }

                buildings.Add(data);
            }

            buildings.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            return buildings;
        }

        private static string GuessBuildingIcon(BuildingData data)
        {
            string id = data.Id.ToLowerInvariant();
            if (data.CountsAsHouse || id.Contains("house") || id.Contains("casa")) return "construcao_moradia";
            if (id.Contains("storage") || id.Contains("armaz")) return "construcao_armazem";
            if (id.Contains("industry") || id.Contains("oficina")) return "construcao_producao";
            return "acao_martelo";
        }

        /// <summary>Desliga a barra e a doca antigas. A confirmação de estrada continua ligada.</summary>
        private static void DisableLegacyHud()
        {
            TopHeaderHudController[] legacy = Object.FindObjectsByType<TopHeaderHudController>(FindObjectsInactive.Include);
            for (int i = 0; i < legacy.Length; i++)
            {
                Undo.RecordObject(legacy[i], "Desligar HUD antiga");
                legacy[i].enabled = false;

                Transform hud = legacy[i].transform;
                for (int c = 0; c < hud.childCount; c++)
                {
                    GameObject child = hud.GetChild(c).gameObject;
                    if (child.GetComponentInChildren<ConfirmBuildUI>(true) != null || !child.activeSelf)
                    {
                        continue;
                    }

                    Undo.RecordObject(child, "Desligar HUD antiga");
                    child.SetActive(false);
                    Debug.Log($"[Reclaim] HUD antiga desligada: {child.name}", child);
                }
            }
        }

        private static void ReportMissing(SceneRefs refs)
        {
            if (refs.Clock == null) Debug.LogWarning("[Reclaim] SimulationClock não encontrado; calendário e estação ficam desligados.");
            if (refs.DayNight == null) Debug.LogWarning("[Reclaim] DayNightCycle não encontrado; dia e noite fica desligado.");
            if (refs.Buildings == null || refs.Tools == null) Debug.LogWarning("[Reclaim] BuildingSystem ou ToolModeController não encontrado; a doca fica desligada.");
        }

        // ---------- Utilitários ----------

        private static GameObject FindRoot()
        {
            foreach (Canvas canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include))
            {
                if (canvas.name == RootName && canvas.transform.parent == null)
                {
                    return canvas.gameObject;
                }
            }

            return null;
        }

        private static T Find<T>() where T : Object => Object.FindAnyObjectByType<T>(FindObjectsInactive.Include);

        private static IEnumerable<T> LoadAll<T>() where T : Object
        {
            foreach (string guid in AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { "Assets/Data" }))
            {
                T asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset != null)
                {
                    yield return asset;
                }
            }
        }

        private static void Set(Object target, string path, Object value)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(path);
            if (property == null)
            {
                Debug.LogError($"[Reclaim] Campo '{path}' não existe em {target.GetType().Name}.", target);
                return;
            }

            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            if (EditorUtility.IsPersistent(target))
            {
                EditorUtility.SetDirty(target);
            }
        }

        private static void SetArray(Object target, string path, Object[] values)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(path);
            if (property == null || !property.isArray)
            {
                Debug.LogError($"[Reclaim] Lista '{path}' não existe em {target.GetType().Name}.", target);
                return;
            }

            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
