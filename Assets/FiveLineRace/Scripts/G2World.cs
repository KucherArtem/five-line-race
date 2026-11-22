using System.Collections.Generic;
using UnityEngine;

namespace FiveLineRace
{
    /// <summary>
    /// Плоская трасса: асфальт, разметка, неоновые борта, город по бокам, фонари.
    /// Всё «едущее» складывается в WorldScroller2D.
    /// </summary>
    public static class G2World
    {
        public static GameObject Build(out WorldScroller2D scroller)
        {
            var root = new GameObject("World");
            var moving = new List<Transform>();

            // --- Фон ---
            G2Quad.Create(root.transform, "Backdrop", 0f, 0f, 70f, 70f, G2.Night, 9f);

            // --- Дорога, борта и тротуары ---
            // Это гладкие ленты: они изгибаются вслед за поворотами трассы
            // и раздвигаются, когда дорога становится шире.
            float hw = G2.RoadHalfWidth;
            float walk = hw + 0.05f;

            Ribbon2D.Create(root.transform, "Road", -hw, hw, RibbonSide.Both, G2.Asphalt, 5.1f);
            Ribbon2D.Create(root.transform, "SidewalkL", -walk - 1.4f, -walk, RibbonSide.Left, G2.Sidewalk, 5.3f);
            Ribbon2D.Create(root.transform, "SidewalkR", walk, walk + 1.4f, RibbonSide.Right, G2.Sidewalk, 5.3f);

            float edgeX = hw - 0.09f;
            Ribbon2D.Create(root.transform, "EdgeCyan", -edgeX - 0.1f, -edgeX + 0.1f, RibbonSide.Left, G2.Cyan, 4.95f);
            Ribbon2D.Create(root.transform, "EdgeMagenta", edgeX - 0.1f, edgeX + 0.1f, RibbonSide.Right, G2.Magenta, 4.95f);

            // --- Прерывистая разметка между полосами (4 разделителя) ---
            for (int divider = 0; divider < G2.LaneCount - 1; divider++)
            {
                float x = G2.LaneX(divider) + G2.LaneWidth * 0.5f;

                for (int i = 0; i < 11; i++)
                {
                    float y = G2.WrapBottom + i * 3.8f + divider * 0.45f;
                    var dash = G2Quad.Create(root.transform, "Dash", x, y, 0.08f, 1.7f, G2.Line, 4.9f);
                    moving.Add(dash.transform);
                }
            }

            // --- Город по бокам ---
            Color[] neonColors =
            {
                G2.Cyan,
                G2.Magenta,
                G2.Gold,
                new Color(0.55f, 0.35f, 1f)
            };

            for (int side = -1; side <= 1; side += 2)
            {
                for (int i = 0; i < 7; i++)
                {
                    float y = G2.WrapBottom + i * 5.6f + Random.Range(-1.2f, 1.2f);
                    float w = Random.Range(1.8f, 3.2f);
                    float h = Random.Range(3f, 7.5f);
                    float cx = side * Random.Range(6.95f, 8.45f);

                    var building = G2Quad.Create(root.transform, "Building", cx, y, w, h, G2.Building, 6.6f);
                    moving.Add(building.transform);

                    // Неоновая полоса на ближнем к дороге торце.
                    Color neon = neonColors[Random.Range(0, neonColors.Length)];
                    float stripX = cx - side * (w * 0.5f - 0.09f);
                    var strip = G2Quad.Create(root.transform, "NeonStrip", stripX, y, 0.07f, h * 0.8f, neon, 6.5f);
                    moving.Add(strip.transform);

                    // Окна.
                    int rows = Mathf.Clamp(Mathf.FloorToInt(h / 0.85f), 1, 6);
                    for (int r = 0; r < rows; r++)
                    {
                        float wy = y - h * 0.5f + 0.55f + r * 0.85f;
                        var window = G2Quad.Create(root.transform, "Window",
                            cx + Random.Range(-0.55f, 0.55f), wy, 0.24f, 0.32f, G2.Window, 6.55f);
                        moving.Add(window.transform);
                    }
                }

                // --- Фонари вдоль дороги ---
                for (int i = 0; i < 4; i++)
                {
                    float y = G2.WrapBottom + i * 9.5f + 2f;
                    float x = side * 5.85f;

                    var pole = G2Quad.Create(root.transform, "Pole", x, y, 0.1f, 1.1f,
                        new Color(0.16f, 0.17f, 0.24f), 5.2f);
                    moving.Add(pole.transform);

                    var lamp = G2Quad.Create(root.transform, "Lamp", x - side * 0.22f, y + 0.5f, 0.34f, 0.16f,
                        G2.Lamp, 5.1f);
                    moving.Add(lamp.transform);
                }
            }

            // --- Отражения/блики на асфальте (лёгкие блики скорости) ---
            for (int i = 0; i < 10; i++)
            {
                float x = Random.Range(-4.0f, 4.0f);
                float y = G2.WrapBottom + i * 3.8f + Random.Range(-1f, 1f);
                var streak = G2Quad.Create(root.transform, "Streak", x, y, 0.04f, Random.Range(0.9f, 2.2f),
                    new Color(0.24f, 0.28f, 0.4f), 4.8f);
                moving.Add(streak.transform);
            }

            scroller = root.AddComponent<WorldScroller2D>();
            scroller.Init(moving.ToArray());

            return root;
        }
    }

    /// <summary>
    /// Бесконечная прокрутка декораций вниз (на игрока) с зацикливанием.
    /// Каждый элемент помнит свою базовую X и смещается по кривой трассы;
    /// всё, что стоит за обочиной, дополнительно отъезжает при расширении дороги.
    /// </summary>
    public class WorldScroller2D : MonoBehaviour
    {
        /// <summary>Всё, что левее/правее этой координаты, считаем придорожным.</summary>
        const float RoadsideX = 4.5f;

        Transform[] items;
        float[] baseX;
        int[] sides;

        public void Init(Transform[] items)
        {
            this.items = items;
            baseX = new float[items.Length];
            sides = new int[items.Length];

            for (int i = 0; i < items.Length; i++)
            {
                baseX[i] = items[i] != null ? items[i].localPosition.x : 0f;
                sides[i] = baseX[i] > RoadsideX ? 1 : (baseX[i] < -RoadsideX ? -1 : 0);
            }
        }

        void Update()
        {
            var gm = Game2Manager.Instance;
            if (gm == null || items == null) return;

            float dy = gm.ScrollSpeed * Time.deltaTime;
            float travelled = gm.Distance;

            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                if (item == null) continue;

                var p = item.position;
                p.y -= dy;
                if (p.y < G2.WrapBottom) p.y += G2.WrapLength;

                p.x = baseX[i]
                    + G2Path.OffsetAtY(p.y, travelled)
                    + sides[i] * G2Path.ExtraAtY(p.y, travelled);
                item.position = p;
            }
        }
    }
}
