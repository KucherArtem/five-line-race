using UnityEngine;

namespace FiveLineRace
{
    public enum VehicleKind
    {
        Car,
        Police,
        Ferrari,
        Quad,
        Motorcycle,
        Formula,
        TruckMedium,
        TruckLong
    }

    /// <summary>
    /// Описание машины: имя, тип силуэта, цвет, рекорд для открытия
    /// и полуразмеры для столкновений (кузов грузовика длиннее легковушки).
    /// </summary>
    public class CarSkin
    {
        public readonly string Name;
        public readonly VehicleKind Kind;
        public readonly Color Body;
        public readonly int RequiredBest;
        public readonly float HalfX;
        public readonly float HalfY;

        public CarSkin(string name, VehicleKind kind, Color body, int requiredBest,
            float halfX, float halfY)
        {
            Name = name;
            Kind = kind;
            Body = body;
            RequiredBest = requiredBest;
            HalfX = halfX;
            HalfY = halfY;
        }
    }

    /// <summary>
    /// Каталог машин и выбранная машина в PlayerPrefs.
    /// Скорость у всех одинаковая, отличаются силуэт, цвет и размер кузова.
    /// </summary>
    public static class G2Cars
    {
        const string SelectedKey = "FiveLineRace.Car";

        public static readonly CarSkin[] All =
        {
            new CarSkin("НЕОН",      VehicleKind.Car,         new Color(1f, 0.33f, 0.12f),     0,    0.46f, 1.02f),
            new CarSkin("ПОЛИЦИЯ",   VehicleKind.Police,      new Color(0.92f, 0.94f, 0.97f),  250,  0.46f, 1.02f),
            new CarSkin("СПОРТКАР",   VehicleKind.Ferrari,     new Color(0.92f, 0.08f, 0.12f),  600,  0.48f, 1.06f),
            new CarSkin("КВАДРО",    VehicleKind.Quad,        new Color(0.46f, 0.86f, 0.24f),  1000, 0.46f, 0.84f),
            new CarSkin("МОТОЦИКЛ",  VehicleKind.Motorcycle,  new Color(1f, 0.78f, 0.2f),      1500, 0.32f, 0.95f),
            new CarSkin("ФОРМУЛА",   VehicleKind.Formula,     new Color(0.14f, 0.78f, 1f),     2000, 0.62f, 1.18f),
            new CarSkin("ГРУЗОВИК",  VehicleKind.TruckMedium, new Color(0.25f, 0.55f, 1f),     3000, 0.62f, 1.70f),
            new CarSkin("ФУРА",      VehicleKind.TruckLong,   new Color(0.72f, 0.33f, 1f),     4000, 0.64f, 2.05f)
        };

        /// <summary>Индекс выбранной машины (0, если сохранённого значения нет).</summary>
        public static int Selected
        {
            get
            {
                int id = PlayerPrefs.GetInt(SelectedKey, 0);
                if (id < 0 || id >= All.Length) id = 0;
                return id;
            }
        }

        public static bool IsUnlocked(int index, int best)
        {
            if (index < 0 || index >= All.Length) return false;
            return best >= All[index].RequiredBest;
        }

        /// <summary>Выбирает машину, если она открыта по рекорду.</summary>
        public static bool Select(int index, int best)
        {
            if (!IsUnlocked(index, best)) return false;

            PlayerPrefs.SetInt(SelectedKey, index);
            PlayerPrefs.Save();
            return true;
        }
    }
}
