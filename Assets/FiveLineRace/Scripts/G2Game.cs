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
        public int Lives { get; private set; }
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
        readonly List<Manhole2D> manholes = new List<Manhole2D>();

        float spawnTimer;
        float crashTime;
        float inputUnlockTime;
        float camX;

        // Скорость = разгон * множитель от люков; множитель плавно возвращается к 1.
        float baseSpeed;
        float speedMul = 1f;

        float invulnerableUntil;
        float nextLifeAt;
        int carId;
        int crashPlace;
        bool pressConsumed;

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
            G2Audio.Init(gameObject);

            WorldScroller2D scroller;
            var world = G2World.Build(out scroller);
            worldRoot = world.transform;

            var carRoot = new GameObject("Player");
            carId = G2Cars.Selected;
            var visual = G2Visuals.CreatePlayer(carRoot.transform, G2Cars.All[carId]);
            car = carRoot.AddComponent<PlayerCar2D>();
            car.Init(visual.transform);

            // В меню машину не показываем: на экране только трасса и заголовок.
            carVisual = visual;
            carVisual.SetActive(false);

            ui = UIManager2D.Create();
            ui.SetHud(0, Best, 0, 0f);
            ui.SetLives(G2.StartLives);
            ui.ShowLives(false);
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

            camX = CameraTarget(0f);
            shake.BasePosition = new Vector3(camX, 0f, -10f);
            cam.transform.position = shake.BasePosition;
        }

        void Update()
        {
            Input2D.EnsureTick();

            // Кнопки меню проверяем вручную: EventSystem в проекте не нужен,
            // весь ввод уже идёт через Input2D.
            bool pointerOnButton = ui != null && ui.HandlePointer();

            // Нажатие, попавшее в кнопку, не должно запускать заезд,
            // даже если кнопка закрыла экран и палец ещё не отпущен.
            if (pointerOnButton) pressConsumed = true;
            if (Input2D.Released) pressConsumed = false;

            if (State == Game2State.Menu)
            {
                Speed = 0f;
                G2Audio.SetEngine(0f, false);

                if (!pointerOnButton && !pressConsumed && !ui.OverlayOpen &&
                    Input2D.Confirm && Time.unscaledTime > inputUnlockTime)
                {
                    StartRun();
                }
            }
            else if (State == Game2State.Playing)
            {
                float dt = Time.deltaTime;

                baseSpeed = Mathf.MoveTowards(baseSpeed, G2.MaxSpeed, G2.Acceleration * dt);
                speedMul = Mathf.MoveTowards(speedMul, 1f, G2.SpeedMulReturn * dt);
                Speed = baseSpeed * speedMul;
                Distance += Speed * dt;
                G2Audio.SetEngine(Mathf.InverseLerp(G2.StartSpeed, G2.MaxSpeed, Speed), true);

                // Каждые 500 м возвращаем по одной жизни, но не больше пяти.
                if (Distance >= nextLifeAt)
                {
                    nextLifeAt += G2.LifeEveryMeters;
                    if (Lives < G2.MaxLives)
                    {
                        Lives++;
                        ui.SetLives(Lives);
                        G2Audio.Play(Sfx.Life);
                    }
                }

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
                G2Audio.SetEngine(0f, false);

                if (!pointerOnButton && !pressConsumed && !ui.OverlayOpen &&
                    Input2D.Confirm && Time.unscaledTime - crashTime > 0.8f)
                {
                    StartRun();
                }
            }

            if (Input2D.EscPressed)
            {
                if (ui.OverlayOpen) ui.CloseOverlays();
                else if (Application.isMobilePlatform) Application.Quit();
            }

            FollowCurve();
        }

        // Камера смотрит чуть вперёд по трассе: в резком повороте машина
        // уходит к внутренней стороне, а дорога не «улетает» из кадра.
        const float CameraLookAhead = 4f;

        static float CameraTarget(float distance)
        {
            return G2Path.Center(distance + CameraLookAhead);
        }

        /// <summary>Камера плавно едет за поворотом трассы.</summary>
        void FollowCurve()
        {
            if (shake == null) return;

            float target = CameraTarget(Distance);
            float k = 1f - Mathf.Exp(-12f * Mathf.Max(0.0001f, Time.unscaledDeltaTime));
            camX = Mathf.Lerp(camX, target, k);

            shake.BasePosition = new Vector3(camX, 0f, -10f);
        }

        void Crash()
        {
            if (State != Game2State.Playing) return;

            State = Game2State.Crashed;
            crashTime = Time.unscaledTime;

            Best = Mathf.Max(Best, Score);
            PlayerPrefs.SetInt(BestKey, Best);
            PlayerPrefs.Save();

            crashPlace = G2Records.Submit(Score, Mathf.FloorToInt(Distance), Coins);

            car.Crash();
            if (shake != null) shake.Shake(0.45f, 0.35f);
            G2Audio.Play(Sfx.Crash);
            G2Audio.SetEngine(0f, false);
            ui.Flash();
            ui.ShowLives(false);

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
                ui.ShowCrash(true, Score, Best, crashPlace);
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
            camX = CameraTarget(0f);
            if (shake != null) shake.BasePosition = new Vector3(camX, 0f, -10f);
            Coins = 0;
            Lives = G2.StartLives;
            nextLifeAt = G2.LifeEveryMeters;
            invulnerableUntil = 0f;
            baseSpeed = G2.StartSpeed;
            speedMul = 1f;
            Speed = G2.StartSpeed;
            spawnTimer = 1.1f;

            ui.ShowMenu(false);
            ui.CloseOverlays();
            ui.ShowCrash(false, 0, Best);
            ui.ResetHud();
            ui.SetLives(Lives);
            ui.ShowLives(true);

            G2Audio.Play(Sfx.Start);
            G2Audio.SetEngine(0f, true);

            State = Game2State.Playing;
        }

        /// <summary>Пересобирает машину игрока под выбранную в гараже.</summary>
        public void ApplySelectedCar()
        {
            int id = G2Cars.Selected;
            if (id == carId && carVisual != null) return;

            carId = id;

            if (carVisual != null) Destroy(carVisual);

            GameObject visual = G2Visuals.CreatePlayer(car.transform, G2Cars.All[id]);
            carVisual = visual;
            car.Init(visual.transform);
            carVisual.SetActive(State == Game2State.Playing);
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

            for (int i = 0; i < manholes.Count; i++)
            {
                if (manholes[i] != null) Destroy(manholes[i].gameObject);
            }
            manholes.Clear();
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
            int coinLane = -1;
            if (Random.value < 0.55f)
            {
                coinLane = freeLane;
                SpawnCoins(coinLane);
            }

            // Люк — в другой свободной полосе, чтобы не накрывать монеты.
            if (Random.value < 0.5f)
            {
                for (int i = blocked; i < lanes.Count; i++)
                {
                    if (lanes[i] == coinLane) continue;
                    SpawnManhole(lanes[i], Random.value < 0.5f);
                    break;
                }
            }
        }

        void SpawnObstacle(int lane)
        {
            var go = new GameObject("Obstacle");
            go.transform.SetParent(worldRoot, false);

            float laneBase = G2.LaneX(lane);
            float spawnY = G2.SpawnY + Random.Range(-1.5f, 1.5f);
            go.transform.position = new Vector3(G2Path.XAtY(laneBase, spawnY, Distance), spawnY, 0f);

            var mover = go.AddComponent<Mover2D>();
            mover.baseX = laneBase;
            float roll = Random.value;

            if (roll < 0.22f)
            {
                G2Visuals.CreateBarrier(go.transform);
                mover.relativeSpeed = 1f;
                mover.halfX = 0.74f;
                mover.halfY = 0.36f;
            }
            else if (roll < 0.4f)
            {
                Color color = G2.TrafficColors[Random.Range(0, G2.TrafficColors.Length)];
                G2Visuals.CreateTruck(go.transform, color);
                mover.relativeSpeed = 0.55f;
                mover.halfX = 0.68f;
                mover.halfY = 1.95f;
            }
            else
            {
                Color color = G2.TrafficColors[Random.Range(0, G2.TrafficColors.Length)];
                G2Visuals.CreateTrafficCar(go.transform, color);
                mover.relativeSpeed = 0.75f;
                mover.halfX = 0.5f;
                mover.halfY = 1.15f;
            }

            obstacles.Add(mover);
        }

        void SpawnCoins(int lane)
        {
            float laneBase = G2.LaneX(lane);

            for (int i = 0; i < 4; i++)
            {
                float y = G2.SpawnY + 1.5f + i * 1.6f;

                var go = new GameObject("Coin");
                go.transform.SetParent(worldRoot, false);
                go.transform.position = new Vector3(G2Path.XAtY(laneBase, y, Distance), y, -0.5f);

                var visual = G2Visuals.CreateCoin(go.transform);
                var mover = go.AddComponent<Mover2D>();
                mover.baseX = laneBase;
                mover.relativeSpeed = 1f;
                mover.spin = true;
                mover.visual = visual.transform;
                mover.halfX = 0.3f;
                mover.halfY = 0.3f;

                coins.Add(go);
            }
        }

        void SpawnManhole(int lane, bool faster)
        {
            var go = new GameObject("Manhole");
            go.transform.SetParent(worldRoot, false);

            float laneBase = G2.LaneX(lane);
            float y = G2.SpawnY + Random.Range(-2.5f, 2.5f);
            go.transform.position = new Vector3(G2Path.XAtY(laneBase, y, Distance), y, 4f);

            G2Visuals.CreateManhole(go.transform, faster ? G2.ManholeFast : G2.ManholeSlow);

            // Люк лежит на асфальте и едет вместе с дорогой, как монета.
            var mover = go.AddComponent<Mover2D>();
            mover.baseX = laneBase;
            mover.relativeSpeed = 1f;

            var manhole = go.AddComponent<Manhole2D>();
            manhole.faster = faster;

            manholes.Add(manhole);
        }

        void CheckHits()
        {
            Vector3 p = car.transform.position;
            float travelled = Distance;
            CarSkin skin = G2Cars.All[carId];

            // X считаем вдоль дороги: на повороте соседняя полоса смещена по мировой X,
            // и проверка «в лоб» ловила машины из соседних полос.
            float carLocalX = p.x - G2Path.OffsetAtY(p.y, travelled);

            // Столкновения с трафиком: теряем жизнь, но заезд продолжается.
            for (int i = 0; i < obstacles.Count; i++)
            {
                var obstacle = obstacles[i];
                if (obstacle == null) continue;

                Vector3 q = obstacle.transform.position;
                float qLocalX = q.x - G2Path.OffsetAtY(q.y, travelled);

                if (Mathf.Abs(q.y - p.y) < skin.HalfY + obstacle.halfY &&
                    Mathf.Abs(qLocalX - carLocalX) < skin.HalfX + obstacle.halfX)
                {
                    HitObstacle(obstacle);
                    break;
                }
            }

            // Люки: красный прибавляет скорость, зелёный отнимает.
            for (int i = manholes.Count - 1; i >= 0; i--)
            {
                var manhole = manholes[i];
                if (manhole == null)
                {
                    manholes.RemoveAt(i);
                    continue;
                }

                Vector3 q = manhole.transform.position;
                float qLocalX = q.x - G2Path.OffsetAtY(q.y, travelled);

                if (Mathf.Abs(q.y - p.y) < 1.05f && Mathf.Abs(qLocalX - carLocalX) < 0.95f)
                {
                    ApplySpeedChange(manhole.faster);
                    Destroy(manhole.gameObject);
                    manholes.RemoveAt(i);
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
                float qLocalX = q.x - G2Path.OffsetAtY(q.y, travelled);

                if (Mathf.Abs(q.y - p.y) < 1.25f && Mathf.Abs(qLocalX - carLocalX) < 0.78f)
                {
                    Coins++;
                    G2Audio.Play(Sfx.Coin);
                    Destroy(coin);
                    coins.RemoveAt(i);
                }
            }
        }

        /// <summary>Удар о препятствие: минус жизнь, короткая неуязвимость и мигание.</summary>
        void HitObstacle(Mover2D obstacle)
        {
            if (State != Game2State.Playing) return;
            if (Time.time < invulnerableUntil) return;

            // Убираем препятствие, чтобы не таранить его повторно.
            obstacles.Remove(obstacle);
            if (obstacle != null) Destroy(obstacle.gameObject);

            Lives--;
            ui.SetLives(Lives);
            ui.Flash();
            if (shake != null) shake.Shake(0.28f, 0.24f);

            if (Lives <= 0)
            {
                Crash();
                return;
            }

            G2Audio.Play(Sfx.Hit);
            invulnerableUntil = Time.time + G2.InvulnerableTime;
            StartCoroutine(BlinkCar(G2.InvulnerableTime));
        }

        IEnumerator BlinkCar(float duration)
        {
            float t = 0f;

            while (t < duration)
            {
                if (carVisual != null) carVisual.SetActive(!carVisual.activeSelf);
                t += 0.09f;
                yield return new WaitForSecondsRealtime(0.09f);
            }

            if (carVisual != null) carVisual.SetActive(true);
        }

        void ApplySpeedChange(bool faster)
        {
            float factor = faster ? 1f + G2.ManholeSpeedStep : 1f - G2.ManholeSpeedStep;

            speedMul = Mathf.Clamp(speedMul * factor, G2.SpeedMulMin, G2.SpeedMulMax);
            Speed = baseSpeed * speedMul;

            G2Audio.Play(faster ? Sfx.Boost : Sfx.Slow);
            ui.PulseSpeed(faster);
        }
    }
}
