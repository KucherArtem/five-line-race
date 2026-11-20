using UnityEngine;

namespace FiveLineRace
{
    /// <summary>
    /// Движение препятствий и монет сверху вниз.
    /// relativeSpeed: 1 — объект стоит на месте, меньше 1 — «едет» в ту же сторону.
    /// </summary>
    public class Mover2D : MonoBehaviour
    {
        public float relativeSpeed = 1f;
        public bool spin;

        // Базовая X полосы: мировая X зависит от поворота трассы.
        public float baseX;

        // Полуразмеры для проверки столкновения.
        public float halfX = 0.6f;
        public float halfY = 1.15f;

        public Transform visual;
        float phase;

        void Update()
        {
            var gm = Game2Manager.Instance;
            if (gm == null) return;

            float dy = gm.ScrollSpeed * relativeSpeed * Time.deltaTime;
            var p = transform.position;
            p.y -= dy;
            p.x = G2Path.XAtY(baseX, p.y, gm.Distance);
            transform.position = p;

            if (spin && visual != null)
            {
                phase += Time.deltaTime * 3.4f;
                float sx = Mathf.Max(0.14f, Mathf.Abs(Mathf.Cos(phase)));
                visual.localScale = new Vector3(sx, 1f, 1f);
                visual.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(phase) * 6f);
            }

            if (transform.position.y < G2.DespawnY)
            {
                Destroy(gameObject);
            }
        }
    }
}
