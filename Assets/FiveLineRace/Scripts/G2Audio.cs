using UnityEngine;

namespace FiveLineRace
{
    public enum Sfx
    {
        Click,
        Coin,
        Hit,
        Crash,
        Boost,
        Slow,
        Life,
        Start
    }

    /// <summary>
    /// Все звуки синтезируются кодом при запуске: внешних аудиофайлов нет,
    /// поэтому проект остаётся полностью процедурным. Выключение хранится
    /// в PlayerPrefs (ключ FiveLineRace.Sound).
    /// </summary>
    public static class G2Audio
    {
        const string SoundKey = "FiveLineRace.Sound";
        const int SampleRate = 44100;

        static AudioSource sfx;
        static AudioSource engine;
        static bool ready;

        static AudioClip click;
        static AudioClip coin;
        static AudioClip hit;
        static AudioClip crash;
        static AudioClip boost;
        static AudioClip slow;
        static AudioClip life;
        static AudioClip start;

        public static bool Enabled
        {
            get { return PlayerPrefs.GetInt(SoundKey, 1) != 0; }
        }

        static bool Ready
        {
            get { return ready && sfx != null && engine != null; }
        }

        /// <summary>Создаёт источники и клипы. Вызывается один раз при старте игры.</summary>
        public static void Init(GameObject host)
        {
            if (Ready) return;

            ready = false;

            sfx = host.AddComponent<AudioSource>();
            sfx.playOnAwake = false;
            sfx.spatialBlend = 0f;
            sfx.volume = 1f;

            engine = host.AddComponent<AudioSource>();
            engine.playOnAwake = false;
            engine.loop = true;
            engine.spatialBlend = 0f;
            engine.volume = 0f;

            click = BuildClick();
            coin = BuildCoin();
            hit = BuildHit();
            crash = BuildCrash();
            boost = BuildBoost();
            slow = BuildSlow();
            life = BuildLife();
            start = BuildStart();

            engine.clip = BuildEngine();

            if (Object.FindAnyObjectByType<AudioListener>() == null)
            {
                var cam = Object.FindAnyObjectByType<Camera>();
                if (cam != null) cam.gameObject.AddComponent<AudioListener>();
            }

            ready = true;
        }

        public static bool Toggle()
        {
            bool value = !Enabled;
            PlayerPrefs.SetInt(SoundKey, value ? 1 : 0);
            PlayerPrefs.Save();

            if (!value && engine != null)
            {
                engine.volume = 0f;
                engine.Stop();
            }

            return value;
        }

        public static void Play(Sfx sound)
        {
            if (!Ready || !Enabled) return;

            AudioClip clip = Get(sound);
            if (clip != null) sfx.PlayOneShot(clip);
        }

        /// <summary>
        /// Гул мотора: включается в заезде, тон растёт со скоростью,
        /// при выключении звука или аварии плавно затихает.
        /// </summary>
        public static void SetEngine(float speed01, bool running)
        {
            if (!Ready) return;

            if (!Enabled || !running)
            {
                engine.volume = Mathf.MoveTowards(engine.volume, 0f, 0.6f * Time.unscaledDeltaTime);
                if (engine.volume <= 0.001f && engine.isPlaying) engine.Stop();
                return;
            }

            if (!engine.isPlaying) engine.Play();

            float k = Mathf.Clamp01(speed01);
            engine.pitch = 0.78f + 0.72f * k;
            engine.volume = Mathf.MoveTowards(engine.volume, 0.10f + 0.05f * k,
                0.4f * Time.unscaledDeltaTime);
        }

        static AudioClip Get(Sfx sound)
        {
            switch (sound)
            {
                case Sfx.Click: return click;
                case Sfx.Coin: return coin;
                case Sfx.Hit: return hit;
                case Sfx.Crash: return crash;
                case Sfx.Boost: return boost;
                case Sfx.Slow: return slow;
                case Sfx.Life: return life;
                case Sfx.Start: return start;
            }
            return null;
        }

        // ---------- Синтез ----------

        delegate float Wave(float t);

        static AudioClip MakeClip(string name, float duration, Wave wave)
        {
            int count = Mathf.CeilToInt(duration * SampleRate);
            var data = new float[count];

            for (int i = 0; i < count; i++)
            {
                float t = i / (float)SampleRate;
                data[i] = Mathf.Clamp(wave(t), -1f, 1f);
            }

            var clip = AudioClip.Create(name, count, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static float Saw(float phase)
        {
            float p = phase / (2f * Mathf.PI);
            return 2f * (p - Mathf.Floor(p + 0.5f));
        }

        static float Tone(float frequency, float t)
        {
            return Mathf.Sin(2f * Mathf.PI * frequency * t);
        }

        static AudioClip BuildClick()
        {
            return MakeClip("flr_click", 0.06f, t =>
            {
                float env = Mathf.Exp(-70f * t);
                return (Tone(880f, t) * 0.6f + Tone(1760f, t) * 0.25f) * env * 0.45f;
            });
        }

        static AudioClip BuildCoin()
        {
            return MakeClip("flr_coin", 0.22f, t =>
            {
                float frequency = t < 0.055f ? 1318.5f : 1760f;
                float env = Mathf.Exp(-15f * t);
                return (Tone(frequency, t) + Tone(frequency * 2f, t) * 0.3f) * env * 0.3f;
            });
        }

        static AudioClip BuildHit()
        {
            var rng = new System.Random(7);
            return MakeClip("flr_hit", 0.3f, t =>
            {
                float thud = Tone(95f, t) * Mathf.Exp(-9f * t);
                float crack = (float)(rng.NextDouble() * 2.0 - 1.0) * Mathf.Exp(-28f * t);
                return (thud * 0.7f + crack * 0.35f) * Mathf.Exp(-7f * t) * 0.8f;
            });
        }

        static AudioClip BuildCrash()
        {
            var rng = new System.Random(21);
            return MakeClip("flr_crash", 0.8f, t =>
            {
                float boom = Tone(150f - 90f * t, t) * 0.5f;
                float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
                return (noise * 0.55f + boom) * Mathf.Exp(-3.5f * t) * 0.8f;
            });
        }

        static AudioClip BuildBoost()
        {
            return MakeClip("flr_boost", 0.4f, t =>
            {
                float frequency = Mathf.Lerp(280f, 950f, t / 0.4f);
                float env = Mathf.Min(1f, t * 40f) * Mathf.Exp(-3f * t);
                return (Tone(frequency, t) * 0.45f + Tone(frequency * 0.5f, t) * 0.2f) * env;
            });
        }

        static AudioClip BuildSlow()
        {
            return MakeClip("flr_slow", 0.4f, t =>
            {
                float frequency = Mathf.Lerp(900f, 260f, t / 0.4f);
                float env = Mathf.Min(1f, t * 40f) * Mathf.Exp(-3f * t);
                return (Tone(frequency, t) * 0.45f + Tone(frequency * 0.5f, t) * 0.2f) * env;
            });
        }

        static AudioClip BuildLife()
        {
            return MakeClip("flr_life", 0.45f, t =>
            {
                float frequency = t < 0.11f ? 880f : 1318.5f;
                float local = t < 0.11f ? t : t - 0.11f;
                float env = Mathf.Exp(-9f * local);
                return (Tone(frequency, t) + Tone(frequency * 2f, t) * 0.25f) * env * 0.28f;
            });
        }

        static AudioClip BuildStart()
        {
            return MakeClip("flr_start", 0.55f, t =>
            {
                float frequency = Mathf.Lerp(90f, 320f, Mathf.Clamp01(t / 0.4f));
                float env = Mathf.Min(1f, t * 30f) * Mathf.Exp(-2.2f * t);
                return Saw(2f * Mathf.PI * frequency * t) * env * 0.22f;
            });
        }

        static AudioClip BuildEngine()
        {
            // 55 Гц и 7 Гц укладываются в секунду целое число раз — петля без щелчка.
            return MakeClip("flr_engine", 1f, t =>
            {
                float phase = 2f * Mathf.PI * 55f * t;
                float body = Saw(phase) * 0.35f + Tone(110f, t) * 0.3f + Tone(27.5f, t) * 0.25f;
                float wobble = 1f + 0.06f * Mathf.Sin(2f * Mathf.PI * 7f * t);
                return body * wobble * 0.5f;
            });
        }
    }
}
