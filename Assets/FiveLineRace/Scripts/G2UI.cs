using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FiveLineRace
{
    /// <summary>
    /// Весь интерфейс создаётся кодом: HUD, меню и экран аварии.
    /// </summary>
    public class UIManager2D : MonoBehaviour
    {
        Font font;
        Text scoreText;
        Text speedText;
        Text bestText;
        Text coinText;
        Text crashScoreText;
        Text crashBestText;
        Text menuPrompt;
        Text crashPrompt;
        GameObject menuPanel;
        GameObject crashPanel;
        Image flash;
        Coroutine flashRoutine;

        static readonly Color SubText = new Color(0.78f, 0.84f, 0.96f, 0.8f);
        static readonly Color FaintText = new Color(0.78f, 0.84f, 0.96f, 0.45f);

        public static UIManager2D Create()
        {
            var go = new GameObject("UI");
            var ui = go.AddComponent<UIManager2D>();
            ui.Build();
            return ui;
        }

        void Build()
        {
            LoadFont();

            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;

            var safe = MakeRect(transform, "SafeArea");
            Stretch(safe);
            safe.gameObject.AddComponent<SafeAreaFitter2D>();

            RectTransform root = safe;
            Vector2 center = new Vector2(0.5f, 0.5f);

            // ---------- HUD ----------
            scoreText = MakeText(root, "Score", "0 М", 104, Color.white,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -28f), new Vector2(860f, 170f));
            AddShadow(scoreText, new Color(0f, 0.45f, 0.65f, 0.85f), new Vector2(0f, -7f));

            bestText = MakeText(root, "Best", "РЕКОРД 0 М", 34, SubText,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -40f), new Vector2(560f, 50f), TextAnchor.MiddleLeft);

            coinText = MakeText(root, "Coins", "◆ 0", 34, G2.Gold,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -92f), new Vector2(420f, 50f), TextAnchor.MiddleLeft);

            speedText = MakeText(root, "Speed", "0 КМ/Ч", 42, G2.Cyan,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-40f, -42f), new Vector2(460f, 60f), TextAnchor.MiddleRight);

            // ---------- Меню ----------
            menuPanel = MakePanel(root, "Menu", new Color(0.012f, 0.02f, 0.06f, 0.62f)).gameObject;
            Transform menu = menuPanel.transform;

            var title = MakeText(menu, "Title", "FIVE LINE", 132, G2.Cyan, center, center,
                new Vector2(0f, 360f), new Vector2(1000f, 240f));
            AddShadow(title, new Color(0f, 0.6f, 0.8f, 0.65f), new Vector2(0f, -8f));

            var title2 = MakeText(menu, "Title2", "RACE", 158, G2.Magenta, center, center,
                new Vector2(0f, 175f), new Vector2(1000f, 240f));
            AddShadow(title2, new Color(0.5f, 0f, 0.3f, 0.65f), new Vector2(0f, -8f));

            MakeImage(menu, "Bar", new Color(0.13f, 0.94f, 1f, 0.9f), center, center,
                new Vector2(0f, 52f), new Vector2(460f, 4f));

            MakeText(menu, "Sub", "ПЯТЬ ПОЛОС · СВАЙП ВЛЕВО / ВПРАВО", 40, SubText, center, center,
                new Vector2(0f, 0f), new Vector2(1000f, 70f));

            menuPrompt = MakeText(menu, "Prompt", "НАЖМИТЕ, ЧТОБЫ НАЧАТЬ", 56, G2.Cyan, center, center,
                new Vector2(0f, -420f), new Vector2(1100f, 90f));

            MakeText(menu, "Controls", "WINDOWS:  ←   →   ИЛИ   A / D", 30, FaintText, center, center,
                new Vector2(0f, -520f), new Vector2(1000f, 50f));

            MakeText(menu, "Version", "ПРОТОТИП · 5 ПОЛОС · 0.3", 24, new Color(0.7f, 0.76f, 0.9f, 0.3f), center, center,
                new Vector2(0f, -700f), new Vector2(600f, 40f));

            // ---------- Экран аварии ----------
            crashPanel = MakePanel(root, "Crash", new Color(0.05f, 0.01f, 0.05f, 0.72f)).gameObject;
            Transform crash = crashPanel.transform;

            var crashTitle = MakeText(crash, "Title", "АВАРИЯ", 158, G2.Magenta, center, center,
                new Vector2(0f, 380f), new Vector2(1000f, 220f));
            AddShadow(crashTitle, new Color(0.45f, 0f, 0.25f, 0.8f), new Vector2(0f, -8f));

            MakeText(crash, "ScoreLabel", "ДИСТАНЦИЯ", 38, SubText, center, center,
                new Vector2(0f, 205f), new Vector2(800f, 60f));

            crashScoreText = MakeText(crash, "Score", "0 М", 140, Color.white, center, center,
                new Vector2(0f, 90f), new Vector2(900f, 180f));

            crashBestText = MakeText(crash, "Best", "РЕКОРД 0 М", 44, G2.Cyan, center, center,
                new Vector2(0f, -60f), new Vector2(900f, 70f));

            crashPrompt = MakeText(crash, "Prompt", "НАЖМИТЕ, ЧТОБЫ ПОВТОРИТЬ", 44, Color.white, center, center,
                new Vector2(0f, -420f), new Vector2(1150f, 90f));

            crashPanel.SetActive(false);

            // ---------- Вспышка при аварии ----------
            flash = MakePanel(root, "Flash", new Color(1f, 0.12f, 0.2f, 0f));

            StartCoroutine(Blink(menuPrompt));
            StartCoroutine(Blink(crashPrompt));
        }

        void LoadFont()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            if (font == null)
            {
                try
                {
                    font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                }
                catch
                {
                    font = null;
                }
            }

            if (font == null)
            {
                font = Font.CreateDynamicFontFromOSFont(new[] { "Segoe UI", "Roboto", "Arial", "Liberation Sans" }, 64);
            }
        }

        public void SetHud(int score, int best, int coins, float speed)
        {
            scoreText.text = score + " М";
            bestText.text = "РЕКОРД " + best + " М";
            coinText.text = "◆ " + coins;
            speedText.text = Mathf.RoundToInt(speed * 3.6f) + " КМ/Ч";
        }

        public void ShowMenu(bool show)
        {
            menuPanel.SetActive(show);
        }

        public void ShowCrash(bool show, int score, int best)
        {
            if (show)
            {
                crashScoreText.text = score + " М";
                crashBestText.text = "РЕКОРД " + best + " М";
            }
            crashPanel.SetActive(show);
        }

        public void ResetHud()
        {
            scoreText.text = "0 М";
            coinText.text = "◆ 0";
            speedText.text = "0 КМ/Ч";
        }

        public void Flash()
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashRoutine());
        }

        IEnumerator FlashRoutine()
        {
            float duration = 0.4f;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float a = Mathf.Lerp(0.55f, 0f, t / duration);
                flash.color = new Color(1f, 0.12f, 0.2f, a);
                yield return null;
            }
            flash.color = new Color(1f, 0.12f, 0.2f, 0f);
        }

        IEnumerator Blink(Text text)
        {
            while (true)
            {
                Color c = text.color;
                float a = 0.35f + 0.65f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * 2.4f));
                text.color = new Color(c.r, c.g, c.b, a);
                yield return null;
            }
        }

        // ---------- Хелперы ----------

        Text MakeText(Transform parent, string name, string content, int size, Color color,
            Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 sizeDelta,
            TextAnchor align = TextAnchor.MiddleCenter, FontStyle style = FontStyle.Bold)
        {
            RectTransform rt = MakeRect(parent, name);
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = sizeDelta;

            var text = rt.gameObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.fontStyle = style;
            text.text = content;
            text.color = color;
            text.alignment = align;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            text.supportRichText = false;
            return text;
        }

        Image MakeImage(Transform parent, string name, Color color, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            RectTransform rt = MakeRect(parent, name);
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var image = rt.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        Image MakePanel(Transform parent, string name, Color color)
        {
            RectTransform rt = MakeRect(parent, name);
            Stretch(rt);
            var image = rt.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        static RectTransform MakeRect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            return rt;
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static void AddShadow(Graphic target, Color color, Vector2 distance)
        {
            var shadow = target.gameObject.AddComponent<Shadow>();
            shadow.effectColor = color;
            shadow.effectDistance = distance;
            shadow.useGraphicAlpha = true;
        }
    }

    /// <summary>
    /// Подгоняет UI под «безопасную зону» экрана (вырезы, скругления, статус-бар).
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter2D : MonoBehaviour
    {
        RectTransform rt;
        Rect lastSafeArea;
        Vector2Int lastResolution;

        void Awake()
        {
            rt = GetComponent<RectTransform>();
            Apply();
        }

        void Update()
        {
            if (Screen.safeArea != lastSafeArea ||
                Screen.width != lastResolution.x ||
                Screen.height != lastResolution.y)
            {
                Apply();
            }
        }

        void Apply()
        {
            if (rt == null) rt = GetComponent<RectTransform>();

            Rect safe = Screen.safeArea;
            lastSafeArea = safe;
            lastResolution = new Vector2Int(Screen.width, Screen.height);

            Vector2 min = safe.position;
            Vector2 max = safe.position + safe.size;

            min.x /= Mathf.Max(1, Screen.width);
            min.y /= Mathf.Max(1, Screen.height);
            max.x /= Mathf.Max(1, Screen.width);
            max.y /= Mathf.Max(1, Screen.height);

            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
