using System.Collections;
using UnityEngine;

namespace FiveLineRace
{
    /// <summary>
    /// Игровая машинка: пять полос, плавный переезд, крен кузова, авария.
    /// </summary>
    public class PlayerCar2D : MonoBehaviour
    {
        public int Lane { get; private set; }

        Transform visual;
        float prevX;
        bool crashed;

        public void Init(Transform carVisual)
        {
            visual = carVisual;
            ResetToCenter();
        }

        public void ResetToCenter()
        {
            StopAllCoroutines();
            crashed = false;
            Lane = (G2.LaneCount - 1) / 2;
            prevX = G2.LaneX(Lane);

            transform.position = new Vector3(prevX, G2.PlayerY, -1f);
            transform.rotation = Quaternion.identity;

            if (visual != null)
            {
                visual.localRotation = Quaternion.identity;
                visual.localScale = Vector3.one;
                visual.localPosition = Vector3.zero;
            }
        }

        void Update()
        {
            var gm = Game2Manager.Instance;
            if (gm == null) return;

            Input2D.EnsureTick();

            if (!crashed && gm.State == Game2State.Playing)
            {
                int swipe = Input2D.SwipeX;
                if (swipe < 0 && Lane > 0) Lane--;
                else if (swipe > 0 && Lane < G2.LaneCount - 1) Lane++;
            }

            float targetX = G2.LaneX(Lane);
            float x = Mathf.MoveTowards(transform.position.x, targetX, 9.5f * Time.deltaTime);
            float velocity = Time.deltaTime > 0f ? (x - prevX) / Time.deltaTime : 0f;
            prevX = x;

            transform.position = new Vector3(x, G2.PlayerY, -1f);

            if (visual != null && !crashed)
            {
                float tilt = Mathf.Clamp(-velocity * 1.7f, -13f, 13f);
                visual.localRotation = Quaternion.Lerp(visual.localRotation,
                    Quaternion.Euler(0f, 0f, tilt), 14f * Time.deltaTime);
            }
        }

        public void Crash()
        {
            if (crashed) return;
            crashed = true;

            if (visual != null) StartCoroutine(CrashRoutine());
        }

        IEnumerator CrashRoutine()
        {
            float t = 0f;
            float startZ = visual.localRotation.eulerAngles.z;
            float startX = visual.localPosition.x;
            float side = Random.value < 0.5f ? -1f : 1f;

            while (t < 0.45f)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / 0.45f);

                visual.localRotation = Quaternion.Euler(0f, 0f,
                    Mathf.LerpAngle(startZ, side * 26f, k));
                visual.localPosition = new Vector3(
                    Mathf.Lerp(startX, side * 0.25f, k), 0f, 0f);

                yield return null;
            }
        }
    }
}
