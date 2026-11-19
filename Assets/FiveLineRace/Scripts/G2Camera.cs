using UnityEngine;

namespace FiveLineRace
{
    /// <summary>
    /// Тряска ортографической камеры при аварии.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraShake2D : MonoBehaviour
    {
        Vector3 basePosition;
        float remaining;
        float duration;
        float strength;

        void Awake()
        {
            basePosition = transform.position;
        }

        public void Shake(float time, float amount)
        {
            basePosition = transform.position - (Vector3)Offset;
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
                transform.position = basePosition;
                return;
            }

            remaining -= Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(remaining / duration);
            Offset = Random.insideUnitCircle * (strength * k);

            transform.position = basePosition + new Vector3(Offset.x, Offset.y, 0f);
        }
    }
}
