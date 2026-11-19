using UnityEngine;

namespace FiveLineRace
{
    /// <summary>
    /// Создание плоских прямоугольников (quad'ов). Вся графика игры собирается из них.
    /// Чем меньше z, тем ближе к камере.
    /// </summary>
    public static class G2Quad
    {
        public static GameObject Create(Transform parent, string name, float x, float y,
            float width, float height, Color color, float z = 0f)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = name;

            var collider = go.GetComponent<Collider>();
            if (collider != null) Object.Destroy(collider);

            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(x, y, z);
            go.transform.localScale = new Vector3(width, height, 1f);

            var renderer = go.GetComponent<Renderer>();
            renderer.sharedMaterial = G2Mats.Make(color);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            return go;
        }

        public static GameObject CreateRotated(Transform parent, string name, float x, float y,
            float size, float angle, Color color, float z = 0f)
        {
            var go = Create(parent, name, x, y, size, size, color, z);
            go.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
            return go;
        }
    }
}
