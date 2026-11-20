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
            return Mathf.Sin(distance * G2.CurveFreq1) * G2.CurveAmp1
                 + Mathf.Sin(distance * G2.CurveFreq2) * G2.CurveAmp2;
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
    }
}
