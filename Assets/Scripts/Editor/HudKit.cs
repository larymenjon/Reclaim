using Reclaim.UI.Hud;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Reclaim.EditorTools
{
    /// <summary>
    /// Peças de construção da HUD do redesign: carrega tema, fontes e sprites e cria
    /// RectTransforms, imagens, textos e layouts com os valores do Figma.
    /// </summary>
    internal sealed class HudKit
    {
        public const string RedesignFolder = "Assets/UI/Redesign";
        public const string IconFolder = RedesignFolder + "/Icones";
        public const string ShapeFolder = RedesignFolder + "/Formas";
        public const string FontFolder = RedesignFolder + "/Fontes";
        private const string SourceFontFolder = "Assets/UI/Fonts";
        private const string ThemePath = "Assets/Data/Config/HudTema.asset";

        // Caracteres que a HUD usa além do ASCII; entram no atlas logo na criação da fonte.
        private const string ExtraCharacters = "ÁÀÂÃÉÊÍÓÔÕÚÇáàâãéêíóôõúç·×°";

        public HudTheme Theme { get; private set; }
        public TMP_FontAsset Regular { get; private set; }
        public TMP_FontAsset SemiBold { get; private set; }
        public TMP_FontAsset Bold { get; private set; }
        public TMP_FontAsset ExtraBold { get; private set; }

        public static HudKit Load()
        {
            var kit = new HudKit
            {
                Theme = LoadOrCreateTheme(),
                Regular = LoadOrCreateFont("BarlowCondensed-Regular"),
                SemiBold = LoadOrCreateFont("BarlowCondensed-SemiBold"),
                Bold = LoadOrCreateFont("BarlowCondensed-Bold"),
                ExtraBold = LoadOrCreateFont("BarlowCondensed-ExtraBold")
            };

            if (kit.Regular == null || kit.SemiBold == null || kit.Bold == null || kit.ExtraBold == null)
            {
                Debug.LogError($"[Reclaim] Fontes Barlow Condensed não encontradas em {SourceFontFolder}.");
                return null;
            }

            return kit;
        }

        public Sprite Icon(string name) => LoadSprite($"{IconFolder}/Icone_{name}.png");

        public Sprite Shape(string name) => LoadSprite($"{ShapeFolder}/{name}.png");

        /// <summary>Configura todo PNG do redesign como sprite. Bordas 9-slice pelo sufixo "_9s".</summary>
        public static void EnsureSpriteImportSettings()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { RedesignFolder });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (!(AssetImporter.GetAtPath(path) is TextureImporter importer))
                {
                    continue;
                }

                bool isIcon = path.StartsWith(IconFolder);
                Vector4 border = path.EndsWith("_9s.png") ? new Vector4(4f, 4f, 4f, 4f) : Vector4.zero;
                bool needsReimport = importer.textureType != TextureImporterType.Sprite
                    || importer.spriteImportMode != SpriteImportMode.Single
                    || importer.mipmapEnabled != isIcon
                    || importer.spriteBorder != border
                    || !importer.alphaIsTransparency
                    || importer.textureCompression != TextureImporterCompression.Uncompressed;

                if (!needsReimport)
                {
                    continue;
                }

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = isIcon;
                importer.alphaIsTransparency = true;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.spriteBorder = border;
                importer.SaveAndReimport();
            }
        }

        public static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        /// <summary>Âncora num ponto, com posição e tamanho em pixels da referência 1920×1080.</summary>
        public static RectTransform Place(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        public static RectTransform Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        public Image Image(Transform parent, string name, Sprite sprite, Color color, bool raycast = false)
        {
            Image image = Rect(name, parent).gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = raycast;
            image.preserveAspect = sprite != null && sprite.border == Vector4.zero;
            if (sprite != null && sprite.border != Vector4.zero)
            {
                image.type = UnityEngine.UI.Image.Type.Sliced;
            }

            return image;
        }

        /// <summary>Borda fina por cima do pai inteiro, fora do layout.</summary>
        public Image Border(Transform parent, Color color, bool thick = false)
        {
            Image border = Image(parent, "Borda", Shape(thick ? "Borda2px_9s" : "Borda1px_9s"), color);
            Stretch(border.rectTransform);
            IgnoreLayout(border.gameObject);
            return border;
        }

        public TextMeshProUGUI Text(Transform parent, string name, string text, TMP_FontAsset font, float size, Color color,
            TextAlignmentOptions alignment = TextAlignmentOptions.MidlineLeft, float letterSpacing = 0f, bool upperCase = false,
            bool wrap = false)
        {
            TextMeshProUGUI tmp = Rect(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
            tmp.font = font;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.characterSpacing = letterSpacing;
            tmp.fontStyle = upperCase ? FontStyles.UpperCase : FontStyles.Normal;
            tmp.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.raycastTarget = false;
            tmp.text = text;
            tmp.rectTransform.sizeDelta = new Vector2(100f, size * 1.25f);
            return tmp;
        }

        public static HorizontalLayoutGroup Row(GameObject go, float spacing, TextAnchor alignment, RectOffset padding = null,
            bool controlWidth = true, bool controlHeight = false, bool expandWidth = false, bool expandHeight = false)
        {
            HorizontalLayoutGroup layout = go.AddComponent<HorizontalLayoutGroup>();
            Configure(layout, spacing, alignment, padding, controlWidth, controlHeight, expandWidth, expandHeight);
            return layout;
        }

        public static VerticalLayoutGroup Column(GameObject go, float spacing, TextAnchor alignment, RectOffset padding = null,
            bool controlWidth = true, bool controlHeight = true, bool expandWidth = true, bool expandHeight = false)
        {
            VerticalLayoutGroup layout = go.AddComponent<VerticalLayoutGroup>();
            Configure(layout, spacing, alignment, padding, controlWidth, controlHeight, expandWidth, expandHeight);
            return layout;
        }

        public static ContentSizeFitter Fit(GameObject go, bool horizontal, bool vertical)
        {
            ContentSizeFitter fitter = go.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = horizontal ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = vertical ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;
            return fitter;
        }

        /// <summary>Tamanho fixo dentro de um layout. Use -1 para deixar um eixo livre.</summary>
        public static LayoutElement FixedSize(GameObject go, float width, float height, float flexibleWidth = -1f)
        {
            LayoutElement element = go.GetComponent<LayoutElement>();
            if (element == null)
            {
                element = go.AddComponent<LayoutElement>();
            }

            if (width >= 0f)
            {
                element.minWidth = width;
                element.preferredWidth = width;
            }

            if (height >= 0f)
            {
                element.minHeight = height;
                element.preferredHeight = height;
            }

            element.flexibleWidth = flexibleWidth;
            ((RectTransform)go.transform).sizeDelta = new Vector2(width >= 0f ? width : 100f, height >= 0f ? height : 100f);
            return element;
        }

        public static void IgnoreLayout(GameObject go)
        {
            LayoutElement element = go.GetComponent<LayoutElement>();
            if (element == null)
            {
                element = go.AddComponent<LayoutElement>();
            }

            element.ignoreLayout = true;
        }

        private static void Configure(HorizontalOrVerticalLayoutGroup layout, float spacing, TextAnchor alignment, RectOffset padding,
            bool controlWidth, bool controlHeight, bool expandWidth, bool expandHeight)
        {
            layout.spacing = spacing;
            layout.childAlignment = alignment;
            layout.padding = padding ?? new RectOffset();
            layout.childControlWidth = controlWidth;
            layout.childControlHeight = controlHeight;
            layout.childForceExpandWidth = expandWidth;
            layout.childForceExpandHeight = expandHeight;
        }

        private static Sprite LoadSprite(string path)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                Debug.LogWarning($"[Reclaim] Sprite não encontrado: {path}");
            }

            return sprite;
        }

        private static HudTheme LoadOrCreateTheme()
        {
            HudTheme theme = AssetDatabase.LoadAssetAtPath<HudTheme>(ThemePath);
            if (theme != null)
            {
                return theme;
            }

            theme = ScriptableObject.CreateInstance<HudTheme>();
            AssetDatabase.CreateAsset(theme, ThemePath);
            return theme;
        }

        /// <summary>
        /// Os SDF antigos da pasta Fonts só têm ASCII (sem acento). A HUD usa fontes dinâmicas
        /// criadas a partir dos mesmos TTFs, que geram os caracteres que faltarem.
        /// </summary>
        private static TMP_FontAsset LoadOrCreateFont(string fontName)
        {
            string assetPath = $"{FontFolder}/{fontName} HUD.asset";
            TMP_FontAsset existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            if (existing != null)
            {
                return existing;
            }

            Font source = AssetDatabase.LoadAssetAtPath<Font>($"{SourceFontFolder}/{fontName}.ttf");
            if (source == null)
            {
                return null;
            }

            if (!AssetDatabase.IsValidFolder(FontFolder))
            {
                AssetDatabase.CreateFolder(RedesignFolder, "Fontes");
            }

            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(source);
            fontAsset.name = $"{fontName} HUD";
            AssetDatabase.CreateAsset(fontAsset, assetPath);

            fontAsset.atlasTextures[0].name = $"{fontName} HUD Atlas";
            AssetDatabase.AddObjectToAsset(fontAsset.atlasTextures[0], fontAsset);
            fontAsset.material.name = $"{fontName} HUD Material";
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);

            fontAsset.TryAddCharacters(ExtraCharacters);
            EditorUtility.SetDirty(fontAsset);
            AssetDatabase.SaveAssets();
            return fontAsset;
        }
    }
}
