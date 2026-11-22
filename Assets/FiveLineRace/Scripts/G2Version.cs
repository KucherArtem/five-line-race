using UnityEngine;

namespace FiveLineRace
{
    /// <summary>
    /// Версия игры из Resources/version.txt:
    /// первая строка — имя версии, вторая — код сборки.
    /// Код сборки при каждой сборке увеличивается сам (см. FiveLineRaceBuilder),
    /// поэтому новый APK ставится поверх старого.
    /// </summary>
    public static class G2Version
    {
        static bool loaded;
        static string name = "0.0.0";
        static int code;

        public static string Name
        {
            get { Load(); return name; }
        }

        public static int Code
        {
            get { Load(); return code; }
        }

        /// <summary>Строка для меню: «0.6.0 (6)».</summary>
        public static string Display
        {
            get { return Name + " (" + Code + ")"; }
        }

        static void Load()
        {
            if (loaded) return;
            loaded = true;

            var asset = Resources.Load<TextAsset>("version");
            if (asset == null)
            {
                Debug.LogWarning("[FiveLineRace] Не найден Resources/version.txt");
                return;
            }

            string[] lines = asset.text.Replace("\r", string.Empty).Split('\n');
            if (lines.Length > 0 && lines[0].Trim().Length > 0) name = lines[0].Trim();
            if (lines.Length > 1) int.TryParse(lines[1].Trim(), out code);
        }
    }
}
