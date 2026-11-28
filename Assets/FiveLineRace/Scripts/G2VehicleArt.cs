using System.Collections.Generic;
using UnityEngine;

namespace FiveLineRace
{
    /// <summary>
    /// Один прямоугольник машины в локальных координатах (вид сверху).
    /// Порядок в списке — порядок отрисовки: поздние детали выше.
    /// </summary>
    public struct VehiclePart
    {
        public readonly float X;
        public readonly float Y;
        public readonly float W;
        public readonly float H;
        public readonly Color Color;

        public VehiclePart(float x, float y, float w, float h, Color color)
        {
            X = x;
            Y = y;
            W = w;
            H = h;
            Color = color;
        }
    }

    /// <summary>
    /// Геометрия машин из прямоугольников. Один и тот же список используется
    /// и для машинки в игре, и для превью в гараже.
    /// </summary>
    public static class G2VehicleArt
    {
        static readonly Color Headlight = new Color(1f, 0.97f, 0.82f);
        static readonly Color Dark = new Color(0.10f, 0.11f, 0.15f);
        static readonly Color Metal = new Color(0.72f, 0.75f, 0.82f);
        static readonly Color PoliceBlue = new Color(0.2f, 0.5f, 1f);

        // Колёса и гонщик: одинаковые для всех машин, чтобы силуэт читался
        // и на тёмном асфальте, и на тёмной карточке гаража.
        static readonly Color Rubber = new Color(0.30f, 0.33f, 0.40f);
        static readonly Color RimMetal = new Color(0.82f, 0.86f, 0.92f);
        static readonly Color RiderJacket = new Color(0.20f, 0.24f, 0.34f);
        static readonly Color RiderStripe = new Color(0.13f, 0.94f, 1f);
        static readonly Color Helmet = new Color(0.94f, 0.96f, 1f);

        public static List<VehiclePart> Build(CarSkin skin)
        {
            var parts = new List<VehiclePart>(36);

            switch (skin.Kind)
            {
                case VehicleKind.Police: BuildPolice(parts, skin.Body); break;
                case VehicleKind.Ferrari: BuildFerrari(parts, skin.Body); break;
                case VehicleKind.Quad: BuildQuad(parts, skin.Body); break;
                case VehicleKind.Motorcycle: BuildMotorcycle(parts, skin.Body); break;
                case VehicleKind.Formula: BuildFormula(parts, skin.Body); break;
                case VehicleKind.TruckMedium: BuildTruckMedium(parts, skin.Body); break;
                case VehicleKind.TruckLong: BuildTruckLong(parts, skin.Body); break;
                default: BuildCar(parts, skin.Body); break;
            }

            return parts;
        }

        static void Add(List<VehiclePart> parts, float x, float y, float w, float h, Color color)
        {
            parts.Add(new VehiclePart(x, y, w, h, color));
        }

        static void Wheels(List<VehiclePart> parts, float x, float y, float w, float h)
        {
            for (int sx = -1; sx <= 1; sx += 2)
            {
                for (int sy = -1; sy <= 1; sy += 2)
                {
                    Add(parts, sx * x, sy * y, w, h, G2.Tire);
                }
            }
        }

        // ---------- Обычная легковушка ----------

        static void BuildCar(List<VehiclePart> parts, Color body)
        {
            Wheels(parts, 0.47f, 0.66f, 0.16f, 0.46f);

            Add(parts, 0f, 0f, 0.98f, 2.2f, body);
            Add(parts, -0.54f, 0.46f, 0.12f, 0.18f, body);
            Add(parts, 0.54f, 0.46f, 0.12f, 0.18f, body);
            Add(parts, 0f, 0.12f, 0.72f, 1.02f, G2.Darken(body, 0.62f));
            Add(parts, 0f, 0.62f, 0.62f, 0.3f, G2.Glass);
            Add(parts, 0f, -0.62f, 0.62f, 0.26f, G2.Glass);
            Add(parts, -0.29f, 1.05f, 0.24f, 0.12f, Headlight);
            Add(parts, 0.29f, 1.05f, 0.24f, 0.12f, Headlight);
            Add(parts, -0.29f, -1.05f, 0.24f, 0.12f, G2.Tail);
            Add(parts, 0.29f, -1.05f, 0.24f, 0.12f, G2.Tail);
        }

        // ---------- Полицейская машина ----------

        static void BuildPolice(List<VehiclePart> parts, Color body)
        {
            Wheels(parts, 0.47f, 0.66f, 0.16f, 0.46f);

            Add(parts, 0f, 0f, 0.98f, 2.2f, body);

            // Чёрные капот и багажник.
            Add(parts, 0f, 0.8f, 0.86f, 0.44f, Dark);
            Add(parts, 0f, -0.9f, 0.86f, 0.3f, Dark);

            // Синяя полоса по бортам.
            Add(parts, -0.45f, -0.1f, 0.07f, 1.3f, PoliceBlue);
            Add(parts, 0.45f, -0.1f, 0.07f, 1.3f, PoliceBlue);

            Add(parts, -0.54f, 0.46f, 0.12f, 0.18f, Dark);
            Add(parts, 0.54f, 0.46f, 0.12f, 0.18f, Dark);
            Add(parts, 0f, 0.12f, 0.72f, 1.02f, body);
            Add(parts, 0f, 0.62f, 0.62f, 0.3f, G2.Glass);
            Add(parts, 0f, -0.62f, 0.62f, 0.26f, G2.Glass);

            // Проблесковые маячки на крыше.
            Add(parts, -0.17f, 0.15f, 0.26f, 0.16f, new Color(1f, 0.12f, 0.18f));
            Add(parts, 0.17f, 0.15f, 0.26f, 0.16f, PoliceBlue);

            Add(parts, -0.29f, 1.05f, 0.24f, 0.12f, Headlight);
            Add(parts, 0.29f, 1.05f, 0.24f, 0.12f, Headlight);
            Add(parts, -0.29f, -1.05f, 0.24f, 0.12f, G2.Tail);
            Add(parts, 0.29f, -1.05f, 0.24f, 0.12f, G2.Tail);
        }

        // ---------- Красный спорткар ----------

        static void BuildFerrari(List<VehiclePart> parts, Color body)
        {
            Wheels(parts, 0.5f, 0.68f, 0.18f, 0.5f);

            // Кузов с вытянутым носом.
            Add(parts, 0f, 0f, 0.96f, 2.2f, body);
            Add(parts, 0f, 1.15f, 0.72f, 0.38f, body);

            // Белая гоночная полоса.
            Add(parts, 0f, 0.05f, 0.16f, 2.5f, new Color(0.96f, 0.96f, 0.98f));

            // Воздухозаборники по бокам.
            Add(parts, -0.38f, -0.15f, 0.14f, 0.42f, Dark);
            Add(parts, 0.38f, -0.15f, 0.14f, 0.42f, Dark);

            // Стекло кабины и моторный отсек.
            Add(parts, 0f, 0.28f, 0.58f, 0.52f, G2.Glass);
            Add(parts, 0f, -0.65f, 0.72f, 0.58f, G2.Darken(body, 0.55f));

            // Антикрыло.
            Add(parts, 0f, -1.12f, 1.12f, 0.13f, G2.Darken(body, 0.45f));
            Add(parts, -0.55f, -1.05f, 0.07f, 0.24f, Dark);
            Add(parts, 0.55f, -1.05f, 0.07f, 0.24f, Dark);

            Add(parts, -0.28f, 1.3f, 0.26f, 0.1f, Headlight);
            Add(parts, 0.28f, 1.3f, 0.26f, 0.1f, Headlight);
            Add(parts, -0.3f, -1.02f, 0.22f, 0.1f, G2.Tail);
            Add(parts, 0.3f, -1.02f, 0.22f, 0.1f, G2.Tail);
        }

        // ---------- Квадроцикл ----------

        static void BuildQuad(List<VehiclePart> parts, Color body)
        {
            // Колёса с дисками: на асфальте и в гараже их хорошо видно.
            for (int sx = -1; sx <= 1; sx += 2)
            {
                for (int sy = -1; sy <= 1; sy += 2)
                {
                    Add(parts, sx * 0.52f, sy * 0.52f, 0.28f, 0.44f, Rubber);
                    Add(parts, sx * 0.52f, sy * 0.52f, 0.13f, 0.24f, RimMetal);
                }
            }

            // Крылья над колёсами.
            Add(parts, -0.43f, 0.45f, 0.24f, 0.56f, G2.Darken(body, 0.82f));
            Add(parts, 0.43f, 0.45f, 0.24f, 0.56f, G2.Darken(body, 0.82f));

            Add(parts, 0f, 0f, 0.72f, 1.4f, body);
            Add(parts, 0f, 0.74f, 0.6f, 0.22f, Dark);
            Add(parts, 0f, -0.66f, 0.56f, 0.18f, Dark);
            Add(parts, 0f, 0.36f, 0.62f, 0.08f, Dark);
            Add(parts, 0f, -0.14f, 0.5f, 0.5f, Dark);
            Add(parts, 0f, 0.84f, 0.2f, 0.12f, Headlight);
            Add(parts, -0.18f, -0.8f, 0.14f, 0.08f, G2.Tail);
            Add(parts, 0.18f, -0.8f, 0.14f, 0.08f, G2.Tail);

            // Гонщик: корпус, руки к рулю, шлем.
            Add(parts, 0f, 0.06f, 0.42f, 0.5f, RiderJacket);
            Add(parts, 0f, 0.06f, 0.09f, 0.46f, RiderStripe);
            Add(parts, 0f, 0.27f, 0.52f, 0.16f, RiderJacket);
            Add(parts, -0.25f, 0.45f, 0.1f, 0.44f, RiderJacket);
            Add(parts, 0.25f, 0.45f, 0.1f, 0.44f, RiderJacket);
            Add(parts, -0.28f, 0.66f, 0.12f, 0.1f, Dark);
            Add(parts, 0.28f, 0.66f, 0.12f, 0.1f, Dark);
            Add(parts, 0f, 0.36f, 0.32f, 0.3f, Helmet);
            Add(parts, 0f, 0.42f, 0.24f, 0.09f, Dark);
        }

        // ---------- Мотоцикл ----------

        static void BuildMotorcycle(List<VehiclePart> parts, Color body)
        {
            Add(parts, 0f, 0.85f, 0.22f, 0.54f, Rubber);
            Add(parts, 0f, 0.85f, 0.09f, 0.3f, RimMetal);
            Add(parts, 0f, -0.78f, 0.24f, 0.58f, Rubber);
            Add(parts, 0f, -0.78f, 0.1f, 0.32f, RimMetal);

            Add(parts, 0f, 0.62f, 0.26f, 0.3f, body);
            Add(parts, 0f, -0.55f, 0.28f, 0.3f, body);

            Add(parts, 0f, 0f, 0.34f, 1.5f, body);
            Add(parts, 0f, 0.18f, 0.42f, 0.42f, G2.Darken(body, 0.85f));
            Add(parts, 0f, -0.36f, 0.34f, 0.44f, Dark);

            // Руль и выхлоп.
            Add(parts, 0f, 0.55f, 0.72f, 0.07f, Dark);
            Add(parts, 0.27f, -0.45f, 0.08f, 0.72f, Metal);

            // Гонщик: колени, корпус, руки на руле, шлем.
            Add(parts, -0.19f, -0.3f, 0.12f, 0.4f, Dark);
            Add(parts, 0.19f, -0.3f, 0.12f, 0.4f, Dark);
            Add(parts, 0f, -0.02f, 0.5f, 0.55f, RiderJacket);
            Add(parts, 0f, -0.02f, 0.08f, 0.5f, RiderStripe);
            Add(parts, 0f, 0.23f, 0.6f, 0.16f, RiderJacket);
            Add(parts, -0.28f, 0.4f, 0.1f, 0.4f, RiderJacket);
            Add(parts, 0.28f, 0.4f, 0.1f, 0.4f, RiderJacket);
            Add(parts, -0.3f, 0.55f, 0.12f, 0.09f, Dark);
            Add(parts, 0.3f, 0.55f, 0.12f, 0.09f, Dark);
            Add(parts, 0f, 0.34f, 0.34f, 0.3f, Helmet);
            Add(parts, 0f, 0.4f, 0.24f, 0.09f, Dark);

            Add(parts, 0f, 1.0f, 0.16f, 0.1f, Headlight);
            Add(parts, 0f, -0.95f, 0.16f, 0.08f, G2.Tail);
        }

        // ---------- Болид ----------

        static void BuildFormula(List<VehiclePart> parts, Color body)
        {
            // Шины светлее асфальта и со светлыми дисками:
            // на тёмной дороге и в гараже колёса не сливаются с фоном.
            Color tire = Rubber;
            Color rim = RimMetal;

            // Колёса вынесены наружу.
            Add(parts, -0.62f, 0.95f, 0.3f, 0.6f, tire);
            Add(parts, 0.62f, 0.95f, 0.3f, 0.6f, tire);
            Add(parts, -0.66f, -0.72f, 0.34f, 0.64f, tire);
            Add(parts, 0.66f, -0.72f, 0.34f, 0.64f, tire);

            Add(parts, -0.62f, 0.95f, 0.14f, 0.34f, rim);
            Add(parts, 0.62f, 0.95f, 0.14f, 0.34f, rim);
            Add(parts, -0.66f, -0.72f, 0.16f, 0.38f, rim);
            Add(parts, 0.66f, -0.72f, 0.16f, 0.38f, rim);

            // Переднее крыло и нос.
            Add(parts, 0f, 1.34f, 1.66f, 0.13f, G2.Darken(body, 0.65f));
            Add(parts, 0f, 0.9f, 0.28f, 0.9f, body);

            // Монокок и боковые понтоны.
            Add(parts, 0f, 0.02f, 0.62f, 2.25f, body);
            Add(parts, -0.4f, -0.22f, 0.3f, 0.9f, G2.Darken(body, 0.8f));
            Add(parts, 0.4f, -0.22f, 0.3f, 0.9f, G2.Darken(body, 0.8f));

            // Кокпит и шлем.
            Add(parts, 0f, 0.18f, 0.36f, 0.5f, G2.Glass);
            Add(parts, 0f, 0.12f, 0.2f, 0.2f, new Color(0.95f, 0.95f, 0.98f));

            // Заднее крыло с краями.
            Add(parts, 0f, -1.16f, 1.36f, 0.19f, G2.Darken(body, 0.6f));
            Add(parts, -0.68f, -1.12f, 0.08f, 0.28f, Dark);
            Add(parts, 0.68f, -1.12f, 0.08f, 0.28f, Dark);

            // Белые акценты.
            Add(parts, 0f, 0.55f, 0.1f, 0.6f, new Color(0.95f, 0.97f, 1f));
            Add(parts, 0f, -0.55f, 0.1f, 0.3f, new Color(0.95f, 0.97f, 1f));
        }

        // ---------- Средний грузовик ----------

        static void BuildTruckMedium(List<VehiclePart> parts, Color body)
        {
            Add(parts, -0.62f, 1.12f, 0.18f, 0.52f, G2.Tire);
            Add(parts, 0.62f, 1.12f, 0.18f, 0.52f, G2.Tire);
            Add(parts, -0.62f, 0.05f, 0.18f, 0.52f, G2.Tire);
            Add(parts, 0.62f, 0.05f, 0.18f, 0.52f, G2.Tire);
            Add(parts, -0.62f, -1.05f, 0.18f, 0.52f, G2.Tire);
            Add(parts, 0.62f, -1.05f, 0.18f, 0.52f, G2.Tire);

            // Кабина.
            Add(parts, 0f, 1.32f, 1.24f, 1.05f, body);
            Add(parts, 0f, 1.7f, 1.02f, 0.2f, G2.Glass);
            Add(parts, -0.42f, 1.8f, 0.22f, 0.1f, Headlight);
            Add(parts, 0.42f, 1.8f, 0.22f, 0.1f, Headlight);

            // Кузов.
            Add(parts, 0f, -0.55f, 1.3f, 1.9f, G2.Cargo);
            Add(parts, 0f, 0.44f, 1.3f, 0.06f, Dark);
            Add(parts, 0f, -0.55f, 1.3f, 0.05f, G2.Darken(G2.Cargo, 0.78f));
            Add(parts, 0f, 0.74f, 1.3f, 0.1f, Dark);

            Add(parts, -0.42f, -1.55f, 0.26f, 0.12f, G2.Tail);
            Add(parts, 0.42f, -1.55f, 0.26f, 0.12f, G2.Tail);
        }

        // ---------- Длинная фура ----------

        static void BuildTruckLong(List<VehiclePart> parts, Color body)
        {
            Add(parts, -0.64f, 1.86f, 0.2f, 0.54f, G2.Tire);
            Add(parts, 0.64f, 1.86f, 0.2f, 0.54f, G2.Tire);
            Add(parts, -0.64f, 0.92f, 0.2f, 0.54f, G2.Tire);
            Add(parts, 0.64f, 0.92f, 0.2f, 0.54f, G2.Tire);
            Add(parts, -0.64f, -0.92f, 0.2f, 0.54f, G2.Tire);
            Add(parts, 0.64f, -0.92f, 0.2f, 0.54f, G2.Tire);
            Add(parts, -0.64f, -1.62f, 0.2f, 0.54f, G2.Tire);
            Add(parts, 0.64f, -1.62f, 0.2f, 0.54f, G2.Tire);

            // Кабина.
            Add(parts, 0f, 1.9f, 1.26f, 1.0f, body);
            Add(parts, 0f, 2.26f, 1.04f, 0.2f, G2.Glass);
            Add(parts, -0.42f, 2.34f, 0.22f, 0.1f, Headlight);
            Add(parts, 0.42f, 2.34f, 0.22f, 0.1f, Headlight);

            // Длинный прицеп.
            Add(parts, 0f, -0.42f, 1.36f, 2.78f, G2.Cargo);
            Add(parts, 0f, 0.98f, 1.36f, 0.06f, Dark);
            Add(parts, 0f, -0.42f, 1.36f, 0.05f, G2.Darken(G2.Cargo, 0.75f));
            Add(parts, 0f, -0.98f, 1.36f, 0.06f, Dark);
            Add(parts, 0f, 1.1f, 1.36f, 0.1f, Dark);

            Add(parts, -0.44f, -1.82f, 0.28f, 0.12f, G2.Tail);
            Add(parts, 0.44f, -1.82f, 0.28f, 0.12f, G2.Tail);
        }
    }
}
