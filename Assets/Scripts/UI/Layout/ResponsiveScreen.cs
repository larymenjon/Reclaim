using System.Collections.Generic;
using Reclaim.UI.Bottom;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Reclaim.UI.Layout
{
    /// <summary>
    /// Adapta a UI (desenhada em 1920x1080) para qualquer proporção, de 4:3 a 32:9.
    /// Nasce sozinho antes da primeira cena e reaplica o layout a cada cena e a cada mudança de resolução.
    ///
    /// - CanvasScaler: telas mais largas que 16:9 casam com a altura (a UI não incha); mais estreitas casam com a largura.
    /// - HUD do jogo: barras viram largura total e os itens das pontas acompanham as bordas da tela.
    /// - Telas de arte fixa (ex.: Novo Jogo): a arte fica centralizada em 16:9 e um fundo escurecido preenche as sobras.
    /// - Fundos simples (ex.: vídeo da Intro): cobrem a tela inteira sem distorcer.
    /// </summary>
    public class ResponsiveScreen : MonoBehaviour
    {
        private const float ReferenceAspect = 16f / 9f;
        private const float EdgeThresholdX = 480f;
        private const float EdgeThresholdY = 300f;
        private const float FullWidthMinimum = 1800f;
        private const float BackdropBrightness = 0.35f;
        private const float ArtMinimumWidth = 1700f;
        private const float ArtMinimumHeight = 900f;

        private const string HudName = "HUD";
        private const string StretchedBarName = "Bottom_selection";
        private static readonly string[] HudContainers = { "Top", "Bottom" };
        private const string NestedGroupName = "Time";

        private struct CoverBase
        {
            public Vector2 Size;
            public Vector2 Offset;
        }

        private struct LayoutBase
        {
            public Vector2 Position;
            public Vector2 Size;
        }

        private readonly Dictionary<RectTransform, LayoutBase> _bases = new Dictionary<RectTransform, LayoutBase>();
        private readonly Dictionary<RectTransform, BottomHudHoverLift> _lifts = new Dictionary<RectTransform, BottomHudHoverLift>();
        private readonly HashSet<RectTransform> _handledBackgrounds = new HashSet<RectTransform>();
        private readonly HashSet<RectTransform> _composedBackgrounds = new HashSet<RectTransform>();
        private readonly Dictionary<RectTransform, CoverBase> _coverBases = new Dictionary<RectTransform, CoverBase>();
        private Vector2Int _lastScreenSize;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            GameObject host = new GameObject(nameof(ResponsiveScreen));
            DontDestroyOnLoad(host);
            host.AddComponent<ResponsiveScreen>();
        }

        private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;

        private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _bases.Clear();
            _lifts.Clear();
            _handledBackgrounds.Clear();
            _composedBackgrounds.Clear();
            _coverBases.Clear();
            ApplyAll();
        }

        private void Update()
        {
            Vector2Int size = new Vector2Int(Screen.width, Screen.height);
            if (size == _lastScreenSize)
            {
                return;
            }

            _lastScreenSize = size;
            ApplyAll();
        }

        private void ApplyAll()
        {
            if (Screen.height <= 0)
            {
                return;
            }

            foreach (Canvas canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                bool isScreenCanvas = canvas.isRootCanvas && canvas.renderMode != RenderMode.WorldSpace;
                if (isScreenCanvas && canvas.TryGetComponent(out CanvasScaler scaler)
                    && scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
                {
                    ApplyCanvas(canvas, scaler);
                }
            }
        }

        private void ApplyCanvas(Canvas canvas, CanvasScaler scaler)
        {
            float aspect = (float)Screen.width / Screen.height;
            bool matchHeight = aspect >= ReferenceAspect;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = matchHeight ? 1f : 0f;

            Vector2 reference = scaler.referenceResolution;
            Vector2 canvasSize = matchHeight
                ? new Vector2(reference.y * aspect, reference.y)
                : new Vector2(reference.x, reference.x / aspect);
            Vector2 halfExtra = new Vector2(
                Mathf.Max(0f, canvasSize.x - reference.x) * 0.5f,
                Mathf.Max(0f, canvasSize.y - reference.y) * 0.5f);

            RectTransform canvasRect = (RectTransform)canvas.transform;
            CacheLifts(canvasRect);
            ApplyBackgrounds(canvasRect, reference);
            ApplyNestedArt(canvasRect, canvasSize);

            Transform hud = canvasRect.Find(HudName);
            if (hud is RectTransform hudRect)
            {
                ApplyHud(hudRect, canvasSize.x / reference.x, halfExtra);
            }
        }

        private void CacheLifts(RectTransform canvasRect)
        {
            foreach (BottomHudHoverLift lift in canvasRect.GetComponentsInChildren<BottomHudHoverLift>(true))
            {
                if (lift.Target != null)
                {
                    _lifts[lift.Target] = lift;
                }
            }
        }

        // ---------- Fundos ----------

        private void ApplyBackgrounds(RectTransform canvasRect, Vector2 reference)
        {
            foreach (RectTransform child in SnapshotChildren(canvasRect))
            {
                if (child.name == HudName || !IsStretched(child) || !_handledBackgrounds.Add(child))
                {
                    continue;
                }

                if (child.TryGetComponent(out RawImage raw))
                {
                    FillScreen(child, TextureAspect(raw.texture));
                }
                else if (child.TryGetComponent(out Image image) && image.sprite != null && !image.preserveAspect)
                {
                    if (HasRectChildren(child))
                    {
                        KeepCenteredWithBackdrop(child, image, reference);
                    }
                    else
                    {
                        FillScreen(child, SpriteAspect(image.sprite));
                    }
                }
            }
        }

        /// <summary>
        /// Artes grandes dentro de painéis (ex.: floresta do Menu) só crescem, mantendo a proporção,
        /// até cobrir o canvas. Em 16:9 nada muda.
        /// </summary>
        private void ApplyNestedArt(RectTransform canvasRect, Vector2 canvasSize)
        {
            foreach (Image image in canvasRect.GetComponentsInChildren<Image>(true))
            {
                RectTransform rect = image.rectTransform;
                if (!IsNestedArt(image, canvasRect) || IsInsideComposed(rect))
                {
                    continue;
                }

                CoverBase cover = CoverBaseOf(rect, canvasRect);
                float scale = Mathf.Max(
                    1f,
                    (canvasSize.x + 2f * Mathf.Abs(cover.Offset.x)) / cover.Size.x,
                    (canvasSize.y + 2f * Mathf.Abs(cover.Offset.y)) / cover.Size.y);

                rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = cover.Size * scale;
            }
        }

        private bool IsNestedArt(Image image, RectTransform canvasRect)
        {
            RectTransform rect = image.rectTransform;
            if (rect.parent == canvasRect || image.sprite == null || image.preserveAspect || HasRectChildren(rect))
            {
                return false;
            }

            Transform hud = canvasRect.Find(HudName);
            bool insideHud = hud != null && rect.IsChildOf(hud);
            bool interactive = image.GetComponentInParent<Selectable>() != null;
            bool isLarge = rect.rect.width >= ArtMinimumWidth && rect.rect.height >= ArtMinimumHeight;
            return isLarge && !insideHud && !interactive || _coverBases.ContainsKey(rect);
        }

        private bool IsInsideComposed(RectTransform rect)
        {
            foreach (RectTransform composed in _composedBackgrounds)
            {
                if (rect.IsChildOf(composed))
                {
                    return true;
                }
            }

            return false;
        }

        private CoverBase CoverBaseOf(RectTransform rect, RectTransform canvasRect)
        {
            if (!_coverBases.TryGetValue(rect, out CoverBase cover))
            {
                Vector3 local = canvasRect.InverseTransformPoint(rect.position);
                cover = new CoverBase { Size = rect.rect.size, Offset = new Vector2(local.x, local.y) };
                _coverBases[rect] = cover;
            }

            return cover;
        }

        private static List<RectTransform> SnapshotChildren(RectTransform parent)
        {
            List<RectTransform> children = new List<RectTransform>(parent.childCount);
            foreach (Transform child in parent)
            {
                if (child is RectTransform childRect)
                {
                    children.Add(childRect);
                }
            }

            return children;
        }

        private static void FillScreen(RectTransform rect, float aspect)
        {
            AspectRatioFitter fitter = rect.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = aspect;
        }

        private void KeepCenteredWithBackdrop(RectTransform rect, Image source, Vector2 reference)
        {
            _composedBackgrounds.Add(rect);
            GameObject backdrop = new GameObject("Backdrop_" + rect.name, typeof(RectTransform), typeof(Image));
            RectTransform backdropRect = (RectTransform)backdrop.transform;
            backdropRect.SetParent(rect.parent, false);
            backdropRect.SetSiblingIndex(rect.GetSiblingIndex());

            Image backdropImage = backdrop.GetComponent<Image>();
            backdropImage.sprite = source.sprite;
            backdropImage.color = source.color * BackdropBrightness;
            backdropImage.raycastTarget = false;
            FillScreen(backdropRect, SpriteAspect(source.sprite));

            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = reference;
            rect.anchoredPosition = Vector2.zero;
        }

        private static bool IsStretched(RectTransform rect)
        {
            return rect.anchorMin == Vector2.zero && rect.anchorMax == Vector2.one;
        }

        private static bool HasRectChildren(RectTransform rect) => rect.childCount > 0;

        private static float SpriteAspect(Sprite sprite) => sprite.rect.width / sprite.rect.height;

        private static float TextureAspect(Texture texture)
        {
            return texture != null && texture.height > 0 ? (float)texture.width / texture.height : ReferenceAspect;
        }

        // ---------- HUD do jogo ----------

        private void ApplyHud(RectTransform hud, float widthScale, Vector2 halfExtra)
        {
            foreach (string containerName in HudContainers)
            {
                Transform container = hud.Find(containerName);
                if (container is RectTransform containerRect)
                {
                    ApplyContainer(containerRect, Vector2.zero, widthScale, halfExtra);
                }
            }
        }

        private void ApplyContainer(RectTransform container, Vector2 parentOrigin, float widthScale, Vector2 halfExtra)
        {
            Vector2 origin = parentOrigin + BaseOf(container).Position;
            foreach (RectTransform child in SnapshotChildren(container))
            {
                if (child.name == NestedGroupName)
                {
                    ApplyContainer(child, origin, widthScale, halfExtra);
                }
                else if (child.name == StretchedBarName)
                {
                    ApplyStretchedBar(child, origin, widthScale, halfExtra);
                }
                else
                {
                    PlaceAtEdge(child, origin, halfExtra, true, true);
                }
            }
        }

        private void ApplyStretchedBar(RectTransform bar, Vector2 origin, float widthScale, Vector2 halfExtra)
        {
            LayoutBase layout = BaseOf(bar);
            bar.sizeDelta = new Vector2(layout.Size.x * Mathf.Max(1f, widthScale), layout.Size.y);
            PlaceAtEdge(bar, origin, halfExtra, false, true);

            Vector2 barOrigin = origin + layout.Position;
            foreach (RectTransform child in SnapshotChildren(bar))
            {
                PlaceAtEdge(child, barOrigin, halfExtra, true, false);
            }
        }

        private void PlaceAtEdge(RectTransform rect, Vector2 origin, Vector2 halfExtra, bool horizontal, bool vertical)
        {
            LayoutBase layout = BaseOf(rect);
            Vector2 absolute = origin + layout.Position;
            Vector2 shift = Vector2.zero;
            if (horizontal)
            {
                shift.x = EdgeShift(absolute.x, EdgeThresholdX, halfExtra.x);
            }

            if (vertical)
            {
                shift.y = EdgeShift(absolute.y, EdgeThresholdY, halfExtra.y);
            }

            SetPosition(rect, layout.Position + shift);
        }

        private static float EdgeShift(float position, float threshold, float amount)
        {
            if (position > threshold)
            {
                return amount;
            }

            return position < -threshold ? -amount : 0f;
        }

        private LayoutBase BaseOf(RectTransform rect)
        {
            if (_bases.TryGetValue(rect, out LayoutBase known))
            {
                return known;
            }

            Vector2 position = _lifts.TryGetValue(rect, out BottomHudHoverLift lift)
                ? lift.BasePosition
                : rect.anchoredPosition;
            LayoutBase created = new LayoutBase { Position = position, Size = rect.sizeDelta };
            _bases[rect] = created;
            return created;
        }

        private void SetPosition(RectTransform rect, Vector2 position)
        {
            if (_lifts.TryGetValue(rect, out BottomHudHoverLift lift))
            {
                lift.BasePosition = position;
                return;
            }

            rect.anchoredPosition = position;
        }
    }
}
