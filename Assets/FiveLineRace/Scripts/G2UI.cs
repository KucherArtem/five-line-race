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
        Text speedPop;
        GameObject menuPanel;
        GameObject crashPanel;
        GameObject livesRoot;
        CanvasGroup[] tireIcons;
        Image flash;
        Coroutine flashRoutine;
        Coroutine speedPopRoutine;

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

            speedPop = MakeText(root, "SpeedPop", "", 38, Color.white,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-40f, -102f), new Vector2(460f, 50f), TextAnchor.MiddleRight);
            speedPop.color = new Color(1f, 1f, 1f, 0f);

            // ---------- Жизни: покрышки на трассе ----------
            BuildLives(root);

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

            MakeText(menu, "Bonuses", "КРАСНЫЙ ЛЮК +5%  ·  ЗЕЛЁНЫЙ -5%", 30, FaintText, center, center,
                new Vector2(0f, -108f), new Vector2(1000f, 46f));

            menuPrompt = MakeText(menu, "Prompt", "НАЖМИТЕ, ЧТОБЫ НАЧАТЬ", 56, G2.Cyan, center, center,
                new Vector2(0f, -420f), new Vector2(1100f, 90f));

            MakeText(menu, "Controls", "WINDOWS:  ←   →   ИЛИ   A / D", 30, FaintText, center, center,
                new Vector2(0f, -520f), new Vector2(1000f, 50f));

            MakeText(menu, "Version", "ПРОТОТИП · 5 ПОЛОС · 0.5", 24, new Color(0.7f, 0.76f, 0.9f, 0.3f), center, center,
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

        /// <summary>Покрышки-жизни внизу слева, прямо на асфальте.</summary>
        void BuildLives(RectTransform root)
        {
            var holder = MakeRect(root, "Lives");
            holder.anchorMin = Vector2.zero;
            holder.anchorMax = Vector2.zero;
            holder.pivot = Vector2.zero;
            holder.anchoredPosition = Vector2.zero;
            holder.sizeDelta = Vector2.zero;
            livesRoot = holder.gameObject;

            tireIcons = new CanvasGroup[G2.MaxLives];
            Sprite tireSprite = CreateTireSprite();
            Vector2 center = new Vector2(0.5f, 0.5f);

            // Лёгкая подложка: трафик за иконками не мешает читать жизни.
            MakeImage(holder, "Plate", new Color(0.02f, 0.03f, 0.07f, 0.5f), Vector2.zero,
                new Vector2(0f, 0.5f), new Vector2(46f, 170f), new Vector2(428f, 84f));

            for (int i = 0; i < G2.MaxLives; i++)
            {
                var cell = MakeRect(holder, "Tire" + i);
                cell.anchorMin = Vector2.zero;
                cell.anchorMax = Vector2.zero;
                cell.pivot = new Vector2(0.5f, 0.5f);
                cell.anchoredPosition = new Vector2(84f + i * 88f, 170f);
                cell.sizeDelta = new Vector2(72f, 72f);

                tireIcons[i] = cell.gameObject.AddComponent<CanvasGroup>();

                var icon = MakeImage(cell, "Tire", Color.white, center, center, Vector2.zero, new Vector2(72f, 72f));
                icon.sprite = tireSprite;
                icon.preserveAspect = true;
            }

            SetLives(G2.StartLives);
            ShowLives(false);
        }

        /// <summary>Круглая покрышка сверху: резиновый протектор, диск и ступица.</summary>
        static Sprite CreateTireSprite()
        {
            const int size = 96;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color rubber = new Color(0.05f, 0.05f, 0.08f);
            Color tread = new Color(0.13f, 0.13f, 0.19f);
            Color rim = new Color(0.58f, 0.61f, 0.69f);
            Color hub = new Color(0.16f, 0.17f, 0.22f);
            var px = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + 0.5f) / size * 2f - 1f;
                    float dy = (y + 0.5f) / size * 2f - 1f;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);

                    Color c;
                    float alpha = 1f;

                    if (r > 0.98f)
                    {
                        c = new Color(0f, 0f, 0f, 0f);
                        alpha = 0f;
                    }
                    else if (r > 0.66f)
                    {
                        // Протектор: чередующиеся сектора.
                        float angle = Mathf.Atan2(dy, dx) + Mathf.PI;
                        int sector = Mathf.FloorToInt(angle / (Mathf.PI * 2f) * 24f);
                        c = (sector % 2 == 0) ? rubber : tread;
                    }
                    else if (r > 0.2f)
                    {
                        c = rim;
                    }
                    else
                    {
                        c = hub;
                    }

                    // Мягкая кромка, чтобы кружок не был рваным.
                    if (alpha > 0f && r > 0.9f)
                    {
                        alpha = Mathf.InverseLerp(0.98f, 0.9f, r);
                    }

                    c.a = alpha;
                    px[y * size + x] = c;
                }
            }

            tex.SetPixels(px);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
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
            speedPop.color = new Color(1f, 1f, 1f, 0f);
        }

        public void SetLives(int lives)
        {
            if (tireIcons == null) return;

            for (int i = 0; i < tireIcons.Length; i++)
            {
                tireIcons[i].alpha = i < lives ? 1f : 0.18f;
            }
        }

        public void ShowLives(bool show)
        {
            if (livesRoot != null) livesRoot.SetActive(show);
        }

        /// <summary>Короткая подпись у спидометра: +5% или -5%.</summary>
        public void PulseSpeed(bool faster)
        {
            if (speedPop == null) return;

            speedPop.text = faster ? "+5%" : "-5%";
            Color c = faster ? G2.ManholeFast : G2.ManholeSlow;
            speedPop.color = c;

            if (speedPopRoutine != null) StopCoroutine(speedPopRoutine);
            speedPopRoutine = StartCoroutine(SpeedPopRoutine(c));
        }

        IEnumerator SpeedPopRoutine(Color color)
        {
            const float duration = 0.9f;
            float t = 0f;

            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                speedPop.color = new Color(color.r, color.g, color.b, 1f - k);
                yield return null;
            }

            speedPop.color = new Color(color.r, color.g, color.b, 0f);
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
