using UnityEngine;

namespace FiveLineRace
{
    /// <summary>
    /// Гладкая лента вдоль оси трассы: асфальт, неоновые борта, тротуары.
    /// Меш перестраивается каждый кадр и повторяет повороты дороги.
    /// </summary>
    public enum RibbonSide
    {
        None,
        Left,
        Right,
        Both
    }

    public class Ribbon2D : MonoBehaviour
    {
        float left;
        float right;
        RibbonSide side;
        int segments;
        Mesh mesh;
        Vector3[] vertices;

        public static Ribbon2D Create(Transform parent, string name, float left, float right,
            RibbonSide side, Color color, float z)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, 0f, z);

            var ribbon = go.AddComponent<Ribbon2D>();
            ribbon.Init(left, right, side, color);
            return ribbon;
        }

        void Init(float leftEdge, float rightEdge, RibbonSide ribbonSide, Color color)
        {
            left = leftEdge;
            right = rightEdge;
            side = ribbonSide;
            segments = 96;

            vertices = new Vector3[(segments + 1) * 2];
            var triangles = new int[segments * 6];

            for (int i = 0; i < segments; i++)
            {
                int a = i * 2;
                int t = i * 6;
                triangles[t] = a;
                triangles[t + 1] = a + 2;
                triangles[t + 2] = a + 1;
                triangles[t + 3] = a + 1;
                triangles[t + 4] = a + 2;
                triangles[t + 5] = a + 3;
            }

            mesh = new Mesh();
            mesh.name = name + "Mesh";
            mesh.MarkDynamic();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();

            var filter = gameObject.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;

            var renderer = gameObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = G2Mats.Make(color);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            Rebuild(0f);
        }

        void Update()
        {
            var gm = Game2Manager.Instance;
            if (gm != null) Rebuild(gm.Distance);
        }

        void Rebuild(float travelled)
        {
            float bottom = G2.WrapBottom - 2f;
            float step = (G2.WrapLength + 4f) / segments;

            for (int i = 0; i <= segments; i++)
            {
                float y = bottom + i * step;
                float shift = G2Path.OffsetAtY(y, travelled);
                float extra = G2Path.ExtraAtY(y, travelled);

                float lo = left + shift;
                float hi = right + shift;

                // Дорога расширяется в обе стороны, борта и тротуары — вслед за своей обочиной.
                if (side == RibbonSide.Both)
                {
                    lo -= extra;
                    hi += extra;
                }
                else if (side == RibbonSide.Left)
                {
                    lo -= extra;
                    hi -= extra;
                }
                else if (side == RibbonSide.Right)
                {
                    lo += extra;
                    hi += extra;
                }

                vertices[i * 2] = new Vector3(lo, y, 0f);
                vertices[i * 2 + 1] = new Vector3(hi, y, 0f);
            }

            mesh.vertices = vertices;
            mesh.RecalculateBounds();
        }
    }
}
