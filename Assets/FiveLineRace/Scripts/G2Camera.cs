using UnityEngine;

namespace FiveLineRace
{
    /// <summary>
    /// Тряска ортографической камеры при аварии.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraShake2D : MonoBehaviour
    {
        /// <summary>Позиция камеры без тряски; её обновляет Game2Manager (следит за поворотом).</summary>
        public Vector3 BasePosition { get; set; }

        float remaining;
        float duration;
        float strength;

        void Awake()
        {
            BasePosition = transform.position;
        }

        public void Shake(float time, float amount)
        {
            duration = Mathf.Max(0.01f, time);
            remaining = duration;
            strength = amount;
        }

        Vector2 Offset;

        void LateUpdate()
        {
            if (remaining <= 0f)
            {
                Offset = Vector2.zero;
            }
            else
            {
                remaining -= Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(remaining / duration);
                Offset = Random.insideUnitCircle * (strength * k);
            }

            transform.position = BasePosition + new Vector3(Offset.x, Offset.y, 0f);
        }
    }
}
