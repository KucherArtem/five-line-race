using System.Collections.Generic;
using UnityEngine;

namespace FiveLineRace
{
    /// <summary>
    /// Плоские (unlit) материалы: сцена двумерная, свет не нужен.
    /// </summary>
    public static class G2Mats
    {
        static readonly Dictionary<string, Material> Cache = new Dictionary<string, Material>();
        static Shader unlitShader;

        static Shader UnlitShader
        {
            get
            {
                if (unlitShader == null) unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
                if (unlitShader == null) unlitShader = Shader.Find("Unlit/Color");
                if (unlitShader == null) unlitShader = Shader.Find("Sprites/Default");
                return unlitShader;
            }
        }

        public static Material Make(Color color)
        {
            string key = ColorUtility.ToHtmlStringRGB(color);
            Material cached;
            if (Cache.TryGetValue(key, out cached) && cached != null)
            {
                return cached;
            }

            var m = new Material(UnlitShader);
            m.name = "M2_" + key;

            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            else m.color = color;

            // Квады должны быть видны с любой стороны — не зависим от ориентации.
            if (m.HasProperty("_Cull")) m.SetInt("_Cull", 0);

            m.enableInstancing = true;
            Cache[key] = m;
            return m;
        }

        public static void Clear()
        {
            Cache.Clear();
        }
    }
}
