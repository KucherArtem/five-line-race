using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace FiveLineRace
{
    /// <summary>
    /// Единая точка ввода: Input System или legacy Input.
    /// Android — свайпы, Windows — мышь-драг, стрелки, A/D.
    /// </summary>
    public static class Input2D
    {
        public static Vector2 Position { get; private set; }
        public static bool Pressed { get; private set; }
        public static bool Released { get; private set; }
        public static bool Held { get; private set; }

        /// <summary>Горизонтальный свайп за кадр: -1 влево, 1 вправо, 0 — нет.</summary>
        public static int SwipeX { get; private set; }

        /// <summary>Подтверждение действия (тап/пробел/Enter/R).</summary>
        public static bool Confirm
        {
            get { return Pressed || KeyDown(KeyCode.Space) || KeyDown(KeyCode.Return) || KeyDown(KeyCode.R); }
        }

        public static bool EscPressed
        {
            get { return KeyDown(KeyCode.Escape); }
        }

        static Vector2 pressPos;
        static Vector2 lastPos;
        static bool tracking;
        static int lastTickFrame = -1;

        static bool KeyDown(KeyCode key)
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb == null) return false;
            switch (key)
            {
                case KeyCode.Space: return kb.spaceKey.wasPressedThisFrame;
                case KeyCode.Return: return kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame;
                case KeyCode.R: return kb.rKey.wasPressedThisFrame;
                case KeyCode.Escape: return kb.escapeKey.wasPressedThisFrame;
                case KeyCode.LeftArrow: return kb.leftArrowKey.wasPressedThisFrame;
                case KeyCode.RightArrow: return kb.rightArrowKey.wasPressedThisFrame;
                case KeyCode.A: return kb.aKey.wasPressedThisFrame;
                case KeyCode.D: return kb.dKey.wasPressedThisFrame;
            }
            return false;
#else
            return Input.GetKeyDown(key);
#endif
        }

        public static void EnsureTick()
        {
            if (lastTickFrame == Time.frameCount) return;
            lastTickFrame = Time.frameCount;
            Tick();
        }

        static void Tick()
        {
            SwipeX = 0;

            Vector2 pos = lastPos;
            bool down = false;
            bool up = false;
            bool held = false;

#if ENABLE_INPUT_SYSTEM
            var touch = Touchscreen.current;
            var mouse = Mouse.current;

            bool touchActive = touch != null &&
                               (touch.primaryTouch.press.isPressed ||
                                touch.primaryTouch.press.wasPressedThisFrame ||
                                touch.primaryTouch.press.wasReleasedThisFrame);

            if (touchActive)
            {
                pos = touch.primaryTouch.position.ReadValue();
                down = touch.primaryTouch.press.wasPressedThisFrame;
                up = touch.primaryTouch.press.wasReleasedThisFrame;
                held = touch.primaryTouch.press.isPressed;
            }
            else if (mouse != null)
            {
                pos = mouse.position.ReadValue();
                down = mouse.leftButton.wasPressedThisFrame;
                up = mouse.leftButton.wasReleasedThisFrame;
                held = mouse.leftButton.isPressed;
            }
#else
            if (Input.touchCount > 0)
            {
                Touch t = Input.GetTouch(0);
                pos = t.position;
                down = t.phase == TouchPhase.Began;
                up = t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled;
                held = t.phase == TouchPhase.Began || t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary;
            }
            else
            {
                pos = Input.mousePosition;
                down = Input.GetMouseButtonDown(0);
                up = Input.GetMouseButtonUp(0);
                held = Input.GetMouseButton(0);
            }
#endif

            // На некоторых устройствах позиция в кадре отпускания приходит нулевой.
            if (up && pos == Vector2.zero) pos = lastPos;

            Position = pos;
            Pressed = down;
            Released = up;
            Held = held;

            if (down)
            {
                pressPos = pos;
                tracking = true;
            }

            if (up && tracking)
            {
                Vector2 d = pos - pressPos;
                float minDistance = Mathf.Max(45f, Screen.height * 0.028f);

                if (Mathf.Abs(d.x) > minDistance && Mathf.Abs(d.x) > Mathf.Abs(d.y) * 1.1f)
                {
                    SwipeX = d.x > 0f ? 1 : -1;
                }

                tracking = false;
            }

            if (KeyDown(KeyCode.LeftArrow) || KeyDown(KeyCode.A)) SwipeX = -1;
            if (KeyDown(KeyCode.RightArrow) || KeyDown(KeyCode.D)) SwipeX = 1;

            lastPos = pos;
        }
    }
}
