using UnityEngine;

namespace FiveLineRace
{
    /// <summary>
    /// Внешний вид машинок, фур, отбойников и монет — плоские фигуры, вид сверху.
    /// Машинка «смотрит» вверх (по +Y).
    /// </summary>
    public static class G2Visuals
    {
        public static GameObject CreatePlayer(Transform parent)
        {
            var root = new GameObject("PlayerCarVisual");
            root.transform.SetParent(parent, false);

            var body = new Color(1f, 0.33f, 0.12f);

            BuildCar(root.transform, body);
            return root;
        }

        public static GameObject CreateTrafficCar(Transform parent, Color body)
        {
            var root = new GameObject("TrafficCarVisual");
            root.transform.SetParent(parent, false);
            BuildCar(root.transform, body);
            return root;
        }

        static void BuildCar(Transform root, Color body)
        {
            // Колёса — за корпусом, чуть выглядывают по бокам.
            for (int sx = -1; sx <= 1; sx += 2)
            {
                for (int sy = -1; sy <= 1; sy += 2)
                {
                    G2Quad.Create(root.transform, "Wheel", sx * 0.47f, sy * 0.66f, 0.16f, 0.46f, G2.Tire, 0.02f);
                }
            }

            // Корпус и капот.
            G2Quad.Create(root.transform, "Body", 0f, 0f, 0.98f, 2.2f, body, 0f);
            G2Quad.Create(root.transform, "Roof", 0f, 0.12f, 0.72f, 1.02f, G2.Darken(body, 0.62f), -0.02f);
            G2Quad.Create(root.transform, "Windshield", 0f, 0.62f, 0.62f, 0.3f, G2.Glass, -0.03f);
            G2Quad.Create(root.transform, "RearWindow", 0f, -0.62f, 0.62f, 0.26f, G2.Glass, -0.03f);

            // Зеркала.
            G2Quad.Create(root.transform, "MirrorL", -0.54f, 0.46f, 0.12f, 0.18f, body, -0.02f);
            G2Quad.Create(root.transform, "MirrorR", 0.54f, 0.46f, 0.12f, 0.18f, body, -0.02f);

            // Фары и стопы.
            G2Quad.Create(root.transform, "HeadL", -0.29f, 1.05f, 0.24f, 0.12f, new Color(1f, 0.97f, 0.82f), -0.03f);
            G2Quad.Create(root.transform, "HeadR", 0.29f, 1.05f, 0.24f, 0.12f, new Color(1f, 0.97f, 0.82f), -0.03f);
            G2Quad.Create(root.transform, "TailL", -0.29f, -1.05f, 0.24f, 0.12f, G2.Tail, -0.03f);
            G2Quad.Create(root.transform, "TailR", 0.29f, -1.05f, 0.24f, 0.12f, G2.Tail, -0.03f);
        }

        public static GameObject CreateTruck(Transform parent, Color cabColor)
        {
            var root = new GameObject("TruckVisual");
            root.transform.SetParent(parent, false);

            // Колёса.
            for (int sx = -1; sx <= 1; sx += 2)
            {
                G2Quad.Create(root.transform, "Wheel", sx * 0.64f, 0.95f, 0.18f, 0.48f, G2.Tire, 0.02f);
                G2Quad.Create(root.transform, "Wheel", sx * 0.64f, 0.1f, 0.18f, 0.48f, G2.Tire, 0.02f);
                G2Quad.Create(root.transform, "Wheel", sx * 0.64f, -1.15f, 0.18f, 0.48f, G2.Tire, 0.02f);
            }

            // Кузов-фургон и кабина.
            G2Quad.Create(root.transform, "Cargo", 0f, -0.6f, 1.34f, 2.45f, G2.Cargo, 0f);
            G2Quad.Create(root.transform, "CargoLine", 0f, 0.62f, 1.34f, 0.06f, G2.Darken(G2.Cargo, 0.8f), -0.01f);
            G2Quad.Create(root.transform, "Cab", 0f, 1.5f, 1.3f, 1.15f, cabColor, -0.02f);
            G2Quad.Create(root.transform, "Windshield", 0f, 1.95f, 1.06f, 0.24f, G2.Glass, -0.03f);

            // Стопы.
            G2Quad.Create(root.transform, "TailL", -0.43f, -1.76f, 0.28f, 0.14f, G2.Tail, -0.03f);
            G2Quad.Create(root.transform, "TailR", 0.43f, -1.76f, 0.28f, 0.14f, G2.Tail, -0.03f);

            return root;
        }

        public static GameObject CreateBarrier(Transform parent)
        {
            var root = new GameObject("BarrierVisual");
            root.transform.SetParent(parent, false);

            G2Quad.Create(root.transform, "Base", 0f, -0.32f, 1.6f, 0.14f, new Color(0.3f, 0.32f, 0.38f), 0.01f);
            G2Quad.Create(root.transform, "Board", 0f, 0f, 1.46f, 0.5f, new Color(1f, 0.45f, 0.08f), 0f);

            for (int i = 0; i < 4; i++)
            {
                G2Quad.Create(root.transform, "Stripe", -0.54f + i * 0.36f, 0f, 0.2f, 0.42f,
                    new Color(1f, 0.85f, 0.2f), -0.01f);
            }

            G2Quad.Create(root.transform, "LampL", -0.66f, 0.38f, 0.14f, 0.14f, new Color(1f, 0.7f, 0.15f), -0.01f);
            G2Quad.Create(root.transform, "LampR", 0.66f, 0.38f, 0.14f, 0.14f, new Color(1f, 0.7f, 0.15f), -0.01f);

            return root;
        }

        public static GameObject CreateCoin(Transform parent)
        {
            var root = new GameObject("CoinVisual");
            root.transform.SetParent(parent, false);

            G2Quad.CreateRotated(root.transform, "Coin", 0f, 0f, 0.52f, 45f, G2.Gold, 0f);
            G2Quad.CreateRotated(root.transform, "CoinInner", 0f, 0f, 0.26f, 45f, G2.GoldLight, -0.01f);

            return root;
        }

        /// <summary>Круглый люк на дороге: цветное поле с решёткой.</summary>
        public static GameObject CreateManhole(Transform parent, Color accent)
        {
            var root = new GameObject("ManholeVisual");
            root.transform.SetParent(parent, false);

            G2Quad.Create(root.transform, "Rim", 0f, 0f, 1f, 1f, G2.ManholeMetal, 0f);
            G2Quad.Create(root.transform, "Face", 0f, 0f, 0.8f, 0.8f, accent, -0.01f);
            G2Quad.Create(root.transform, "FaceInner", 0f, 0f, 0.6f, 0.6f, G2.Darken(accent, 0.55f), -0.02f);

            for (int i = 0; i < 3; i++)
            {
                G2Quad.Create(root.transform, "Slot", 0f, -0.2f + i * 0.2f, 0.46f, 0.06f,
                    G2.Darken(accent, 0.28f), -0.03f);
            }

            return root;
        }
    }
}
