using UnityEngine;

namespace FiveLineRace
{
    /// <summary>
    /// Гладкая лента вдоль оси трассы: асфальт, неоновые борта, тротуары.
    /// Меш перестраивается каждый кадр и повторяет повороты дороги.
    /// </summary>
    public class Ribbon2D : MonoBehaviour
    {
        float left;
        float right;
        int segments;
        Mesh mesh;
        Vector3[] vertices;

        public static Ribbon2D Create(Transform parent, string name, float left, float right,
            Color color, float z)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, 0f, z);

            var ribbon = go.AddComponent<Ribbon2D>();
            ribbon.Init(left, right, color);
            return ribbon;
        }

        void Init(float leftEdge, float rightEdge, Color color)
        {
            left = leftEdge;
            right = rightEdge;
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

                vertices[i * 2] = new Vector3(left + shift, y, 0f);
                vertices[i * 2 + 1] = new Vector3(right + shift, y, 0f);
            }

            mesh.vertices = vertices;
            mesh.RecalculateBounds();
        }
    }
}
