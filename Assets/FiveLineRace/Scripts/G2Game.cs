using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FiveLineRace
{
    /// <summary>
    /// Сердце двумерной игры: состояния, скорость, спавн, столкновения, счёт.
    /// Сцена может быть почти пустой — мир и UI создаются здесь.
    /// </summary>
    public class Game2Manager : MonoBehaviour
    {
        public static Game2Manager Instance { get; private set; }

        const string BestKey = "FiveLineRace.Best";
        const int CoinValue = 25;

        public Game2State State { get; private set; } = Game2State.Menu;
        public float Speed { get; private set; }
        public float Distance { get; private set; }
        public int Coins { get; private set; }
        public int Best { get; private set; }
        public int Score { get { return Mathf.FloorToInt(Distance) + Coins * CoinValue; } }

        public float ScrollSpeed
        {
            get { return State == Game2State.Menu ? G2.MenuSpeed : Speed; }
        }

        PlayerCar2D car;
        GameObject carVisual;
        CameraShake2D shake;
        UIManager2D ui;
        Transform worldRoot;

        readonly List<Mover2D> obstacles = new List<Mover2D>();
        readonly List<GameObject> coins = new List<GameObject>();

        float spawnTimer;
        float crashTime;
        float inputUnlockTime;

        /// <summary>
        /// Страховка: если в открытой сцене нет Game2Manager, игра всё равно запустится.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;

            var go = new GameObject("Game");
            go.AddComponent<Game2Manager>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Time.timeScale = 1f;
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Screen.orientation = ScreenOrientation.Portrait;

            // На старте приложение может «поймать» синтетическое нажатие окна —
            // первые доли секунды ввод игнорируем, чтобы заезд не начался сам.
            inputUnlockTime = Time.unscaledTime + 0.6f;

            Best = PlayerPrefs.GetInt(BestKey, 0);

            SetupCamera();

            WorldScroller2D scroller;
            var world = G2World.Build(out scroller);
            worldRoot = world.transform;

            var carRoot = new GameObject("Player");
            var visual = G2Visuals.CreatePlayer(carRoot.transform);
            car = carRoot.AddComponent<PlayerCar2D>();
            car.Init(visual.transform);

            // В меню машину не показываем: на экране только трасса и заголовок.
            carVisual = visual;
            carVisual.SetActive(false);

            ui = UIManager2D.Create();
            ui.SetHud(0, Best, 0, 0f);
            ui.ShowMenu(true);
        }

        void SetupCamera()
        {
            var cam = Object.FindAnyObjectByType<Camera>();
            if (cam == null)
            {
                var go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                cam = go.AddComponent<Camera>();
            }

            cam.orthographic = true;
            cam.orthographicSize = G2.OrthoSize;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.transform.rotation = Quaternion.identity;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = G2.Night;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 60f;

            shake = cam.GetComponent<CameraShake2D>();
            if (shake == null) shake = cam.gameObject.AddComponent<CameraShake2D>();
        }

        void Update()
        {
            Input2D.EnsureTick();

            if (State == Game2State.Menu)
            {
                Speed = 0f;

                if (Input2D.Confirm && Time.unscaledTime > inputUnlockTime)
                {
                    StartRun();
                }
            }
            else if (State == Game2State.Playing)
            {
                float dt = Time.deltaTime;

                Speed = Mathf.MoveTowards(Speed, G2.MaxSpeed, G2.Acceleration * dt);
                Distance += Speed * dt;

                spawnTimer -= dt;
                if (spawnTimer <= 0f)
                {
                    SpawnRow();
                    spawnTimer = (12f + Speed * 0.55f) / Mathf.Max(1f, Speed);
                }

                CheckHits();
                ui.SetHud(Score, Best, Coins, Speed);
            }
            else
            {
                Speed = Mathf.MoveTowards(Speed, 0f, 28f * Time.deltaTime);
                ui.SetHud(Score, Best, Coins, Speed);

                if (Input2D.Confirm && Time.unscaledTime - crashTime > 0.8f)
                {
                    StartRun();
                }
            }

            // Android: системная кнопка «Назад» закрывает приложение.
            if (Input2D.EscPressed && Application.isMobilePlatform)
            {
                Application.Quit();
            }
        }

        void Crash()
        {
            if (State != Game2State.Playing) return;

            State = Game2State.Crashed;
            crashTime = Time.unscaledTime;

            Best = Mathf.Max(Best, Score);
            PlayerPrefs.SetInt(BestKey, Best);
            PlayerPrefs.Save();

            car.Crash();
            if (shake != null) shake.Shake(0.45f, 0.35f);
            ui.Flash();

            StartCoroutine(SlowMotion());
            StartCoroutine(ShowCrashLater());
        }

        IEnumerator SlowMotion()
        {
            Time.timeScale = 0.35f;
            float t = 0f;
            while (t < 0.55f)
            {
                t += Time.unscaledDeltaTime;
                Time.timeScale = Mathf.Lerp(0.35f, 1f, t / 0.55f);
                yield return null;
            }
            Time.timeScale = 1f;
        }

        IEnumerator ShowCrashLater()
        {
            yield return new WaitForSecondsRealtime(0.6f);
            if (State == Game2State.Crashed)
            {
                ui.ShowCrash(true, Score, Best);
            }
        }

        void StartRun()
        {
            StopAllCoroutines();
            Time.timeScale = 1f;

            ClearSpawned();

            if (carVisual != null) carVisual.SetActive(true);

            car.ResetToCenter();
            Distance = 0f;
            Coins = 0;
            Speed = G2.StartSpeed;
            spawnTimer = 1.1f;

            ui.ShowMenu(false);
            ui.ShowCrash(false, 0, Best);
            ui.ResetHud();

            State = Game2State.Playing;
        }

        void ClearSpawned()
        {
            for (int i = 0; i < obstacles.Count; i++)
            {
                if (obstacles[i] != null) Destroy(obstacles[i].gameObject);
            }
            obstacles.Clear();

            for (int i = 0; i < coins.Count; i++)
            {
                if (coins[i] != null) Destroy(coins[i]);
            }
            coins.Clear();
        }

        void SpawnRow()
        {
            float difficulty = Mathf.InverseLerp(5f, 60f, Distance);

            int blocked = 2;
            if (difficulty < 0.2f)
            {
                blocked = Random.value < 0.4f ? 1 : 2;
            }
            else if (difficulty > 0.5f)
            {
                blocked = Random.value < 0.35f ? 3 : 2;
            }

            // Перемешиваем полосы, чтобы занятые шли в случайном порядке.
            var lanes = new List<int>(G2.LaneCount);
            for (int i = 0; i < G2.LaneCount; i++) lanes.Add(i);

            for (int i = 0; i < lanes.Count; i++)
            {
                int j = Random.Range(i, lanes.Count);
                int tmp = lanes[i];
                lanes[i] = lanes[j];
                lanes[j] = tmp;
            }

            for (int i = 0; i < blocked; i++)
            {
                SpawnObstacle(lanes[i]);
            }

            // Монеты в свободной полосе.
            int freeLane = lanes[blocked];
            if (Random.value < 0.55f)
            {
                SpawnCoins(freeLane);
            }
        }

        void SpawnObstacle(int lane)
        {
            var go = new GameObject("Obstacle");
            go.transform.SetParent(worldRoot, false);
            go.transform.position = new Vector3(G2.LaneX(lane), G2.SpawnY + Random.Range(-1.5f, 1.5f), 0f);

            var mover = go.AddComponent<Mover2D>();
            float roll = Random.value;

            if (roll < 0.22f)
            {
                G2Visuals.CreateBarrier(go.transform);
                mover.relativeSpeed = 1f;
                mover.halfX = 0.88f;
                mover.halfY = 0.4f;
            }
            else if (roll < 0.4f)
            {
                Color color = G2.TrafficColors[Random.Range(0, G2.TrafficColors.Length)];
                G2Visuals.CreateTruck(go.transform, color);
                mover.relativeSpeed = 0.55f;
                mover.halfX = 0.74f;
                mover.halfY = 1.95f;
            }
            else
            {
                Color color = G2.TrafficColors[Random.Range(0, G2.TrafficColors.Length)];
                G2Visuals.CreateTrafficCar(go.transform, color);
                mover.relativeSpeed = 0.75f;
                mover.halfX = 0.6f;
                mover.halfY = 1.15f;
            }

            obstacles.Add(mover);
        }

        void SpawnCoins(int lane)
        {
            float x = G2.LaneX(lane);

            for (int i = 0; i < 4; i++)
            {
                var go = new GameObject("Coin");
                go.transform.SetParent(worldRoot, false);
                go.transform.position = new Vector3(x, G2.SpawnY + 1.5f + i * 1.6f, -0.5f);

                var visual = G2Visuals.CreateCoin(go.transform);
                var mover = go.AddComponent<Mover2D>();
                mover.relativeSpeed = 1f;
                mover.spin = true;
                mover.visual = visual.transform;
                mover.halfX = 0.3f;
                mover.halfY = 0.3f;

                coins.Add(go);
            }
        }

        void CheckHits()
        {
            Vector3 p = car.transform.position;

            // Столкновения с трафиком.
            for (int i = 0; i < obstacles.Count; i++)
            {
                var obstacle = obstacles[i];
                if (obstacle == null) continue;

                Vector3 q = obstacle.transform.position;
                if (Mathf.Abs(q.y - p.y) < G2.PlayerHalfY + obstacle.halfY &&
                    Mathf.Abs(q.x - p.x) < G2.PlayerHalfX + obstacle.halfX)
                {
                    Crash();
                    return;
                }
            }

            // Сбор монет.
            for (int i = coins.Count - 1; i >= 0; i--)
            {
                var coin = coins[i];
                if (coin == null)
                {
                    coins.RemoveAt(i);
                    continue;
                }

                Vector3 q = coin.transform.position;
                if (Mathf.Abs(q.y - p.y) < 1.25f && Mathf.Abs(q.x - p.x) < 0.8f)
                {
                    Coins++;
                    Destroy(coin);
                    coins.RemoveAt(i);
                }
            }
        }
    }
}
