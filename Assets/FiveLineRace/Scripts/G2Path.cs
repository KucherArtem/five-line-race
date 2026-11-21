using UnityEngine;

namespace FiveLineRace
{
    /// <summary>
    /// Осевая линия трассы: плавные повороты влево-вправо.
    /// Аргумент — пройденная дистанция; для точки впереди она больше текущей.
    /// </summary>
    public static class G2Path
    {
        /// <summary>Смещение осевой линии на дистанции distance.</summary>
        public static float Center(float distance)
        {
            // Косинусы дают нулевой наклон на старте: заезд начинается с прямой.
            return G2.CurveAmp1 * (Mathf.Cos(distance * G2.CurveFreq1) - 1f)
                 + G2.CurveAmp2 * (Mathf.Cos(distance * G2.CurveFreq2) - 1f);
        }

        /// <summary>Смещение осевой линии на высоте y при текущей дистанции travelled.</summary>
        public static float OffsetAtY(float y, float travelled)
        {
            return Center(travelled + y - G2.PlayerY);
        }

        /// <summary>Мировая X точки, которая стоит в полосе с базовой координатой baseX.</summary>
        public static float XAtY(float baseX, float y, float travelled)
        {
            return baseX + OffsetAtY(y, travelled);
        }

        /// <summary>
        /// Дополнительная полуширина дороги: отрицательная — дорога уже,
        /// положительная — шире (появляются широкие обочины).
        /// </summary>
        public static float ExtraHalfWidth(float distance)
        {
            float k = 0.5f
                    + 0.35f * Mathf.Sin(distance * G2.WidthFreq)
                    + 0.15f * Mathf.Sin(distance * G2.WidthFreq * 0.37f);

            return Mathf.Lerp(-G2.WidthNarrow, G2.WidthWiden, Mathf.Clamp01(k));
        }

        /// <summary>Дополнительная полуширина дороги на высоте y.</summary>
        public static float ExtraAtY(float y, float travelled)
        {
            return ExtraHalfWidth(travelled + y - G2.PlayerY);
        }
    }
}
