using UnityEngine;

namespace FiveLineRace
{
    /// <summary>
    /// Константы двумерного варианта: пять полос, плоская графика, вертикальная трасса.
    /// Ось Y — вперёд по трассе, игрок внизу экрана.
    /// </summary>
    public static class G2
    {
        // --- Полосы ---
        public const int LaneCount = 5;
        public const float LaneWidth = 1.55f;
        public const float RoadHalfWidth = 4.4f;

        // --- Камера ---
        public const float OrthoSize = 10.6f;

        // --- Вертикальные границы ---
        public const float PlayerY = -7f;
        public const float SpawnY = 12.5f;
        public const float DespawnY = -13f;

        // --- Скорость (метры в секунду) ---
        public const float StartSpeed = 15f;
        public const float MaxSpeed = 45f;
        public const float Acceleration = 0.55f;
        public const float MenuSpeed = 11f;

        // --- Повороты трассы ---
        // Две синусоиды разной длины дают чередующиеся резкие повороты.
        public const float CurveAmp1 = 3.2f;
        public const float CurveFreq1 = 0.045f;
        public const float CurveAmp2 = 1.9f;
        public const float CurveFreq2 = 0.019f;

        // --- Сужение и расширение дороги ---
        // Дополнительная полуширина гуляет от -WidthNarrow до +WidthWiden.
        public const float WidthFreq = 0.012f;
        public const float WidthNarrow = 0.30f;
        public const float WidthWiden = 1.10f;

        // --- Люки на дороге ---
        // Красный ускоряет, зелёный замедляет; множитель плавно возвращается к 1.
        public const float ManholeSpeedStep = 0.05f;
        public const float SpeedMulMin = 0.75f;
        public const float SpeedMulMax = 1.25f;
        public const float SpeedMulReturn = 0.012f;

        // --- Жизни ---
        public const int StartLives = 3;
        public const int MaxLives = 5;
        public const float LifeEveryMeters = 500f;
        public const float InvulnerableTime = 1.5f;

        // --- Столкновения (полуразмеры коробки игрока) ---
        public const float PlayerHalfX = 0.52f;
        public const float PlayerHalfY = 1.05f;

        // --- Прокрутка фона ---
        public const float WrapLength = 38f;
        public const float WrapBottom = -20f;

        // --- Палитра (значения подобраны под неосвещённую плоскую графику) ---
        public static readonly Color Night = new Color(0.075f, 0.085f, 0.18f);
        public static readonly Color Asphalt = new Color(0.175f, 0.185f, 0.245f);
        public static readonly Color Sidewalk = new Color(0.12f, 0.13f, 0.2f);
        public static readonly Color Building = new Color(0.13f, 0.14f, 0.24f);
        public static readonly Color Window = new Color(0.32f, 0.38f, 0.64f);
        public static readonly Color Line = new Color(0.88f, 0.92f, 1f);
        public static readonly Color Cyan = new Color(0.13f, 0.94f, 1f);
        public static readonly Color Magenta = new Color(1f, 0.18f, 0.59f);
        public static readonly Color Gold = new Color(1f, 0.78f, 0.2f);
        public static readonly Color GoldLight = new Color(1f, 0.93f, 0.58f);
        public static readonly Color Glass = new Color(0.13f, 0.22f, 0.38f);
        public static readonly Color Tail = new Color(1f, 0.16f, 0.22f);
        public static readonly Color Lamp = new Color(1f, 0.91f, 0.68f);
        public static readonly Color Tire = new Color(0.06f, 0.06f, 0.09f);
        public static readonly Color Cargo = new Color(0.78f, 0.82f, 0.9f);
        public static readonly Color ManholeMetal = new Color(0.14f, 0.15f, 0.19f);
        public static readonly Color ManholeSlow = new Color(0.22f, 0.95f, 0.48f);
        public static readonly Color ManholeFast = new Color(1f, 0.24f, 0.32f);

        public static readonly Color[] TrafficColors =
        {
            new Color(0.20f, 0.55f, 1.00f),
            new Color(0.25f, 0.90f, 0.55f),
            new Color(1.00f, 0.82f, 0.20f),
            new Color(0.70f, 0.35f, 1.00f),
            new Color(0.95f, 0.95f, 0.98f),
            new Color(0.35f, 0.38f, 0.45f)
        };

        public static float LaneX(int lane)
        {
            return (lane - (LaneCount - 1) * 0.5f) * LaneWidth;
        }

        public static Color Darken(Color c, float k)
        {
            return new Color(c.r * k, c.g * k, c.b * k, c.a);
        }
    }

    public enum Game2State
    {
        Menu,
        Playing,
        Crashed
    }
}
