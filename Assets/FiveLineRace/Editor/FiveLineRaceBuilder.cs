using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FiveLineRace.EditorTools
{
    /// <summary>
    /// Настройка проекта и сборка билдов двумерного варианта.
    /// Меню: Five Line Race → ...
    /// Батч: Unity.exe -batchmode -quit -projectPath . -executeMethod FiveLineRace.EditorTools.FiveLineRaceBuilder.BuildAll
    /// </summary>
    public static class FiveLineRaceBuilder
    {
        public const string ScenePath = "Assets/Scenes/FiveLineRace.unity";
        const string ProjectRoot = "Assets/FiveLineRace";
        const string ResourcesDir = ProjectRoot + "/Resources";
        const string MaterialPath = ResourcesDir + "/TurboUnlit.mat";
        const string WindowsBuildPath = "Builds/Windows/FiveLineRace.exe";
        const string AndroidBuildPath = "Builds/Android/FiveLineRace.apk";

        // ---------------- Меню ----------------

        [MenuItem("Five Line Race/1. Настроить проект и открыть сцену", false, 10)]
        public static void SetupAndOpen()
        {
            SetupProject();
            if (File.Exists(ScenePath))
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
        }

        [MenuItem("Five Line Race/2. Собрать Windows (.exe)", false, 20)]
        public static void BuildWindows()
        {
            try
            {
                SetupProject();
                BuildWindowsInternal();
                Exit(0);
            }
            catch (Exception e)
            {
                Debug.LogError("[FiveLineRace] Ошибка сборки Windows: " + e);
                Exit(1);
            }
        }

        [MenuItem("Five Line Race/3. Собрать Android (.apk)", false, 20)]
        public static void BuildAndroid()
        {
            try
            {
                SetupProject();
                BuildAndroidInternal();
                Exit(0);
            }
            catch (Exception e)
            {
                Debug.LogError("[FiveLineRace] Ошибка сборки Android: " + e);
                Exit(1);
            }
        }

        [MenuItem("Five Line Race/4. Собрать всё (Windows + Android)", false, 30)]
        public static void BuildAll()
        {
            try
            {
                SetupProject();
                BuildWindowsInternal();
                BuildAndroidInternal();
                Exit(0);
            }
            catch (Exception e)
            {
                Debug.LogError("[FiveLineRace] Ошибка сборки: " + e);
                Exit(1);
            }
        }

        [MenuItem("Five Line Race/Открыть папку сборок", false, 40)]
        public static void OpenBuildsFolder()
        {
            string path = Path.GetFullPath("Builds");
            Directory.CreateDirectory(path);
            EditorUtility.RevealInFinder(path);
        }

        // ---------------- Настройка ----------------

        public static void SetupProject()
        {
            EnsureFolders();
            ConfigurePlayer();
            EnsureMaterialAsset();
            EnsureScene();
            EnsureBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[FiveLineRace] Проект настроен. Сцена: " + ScenePath);
        }

        static void EnsureFolders()
        {
            if (!Directory.Exists("Assets/Scenes")) Directory.CreateDirectory("Assets/Scenes");
            if (!Directory.Exists(ResourcesDir)) Directory.CreateDirectory(ResourcesDir);
            if (!Directory.Exists("Builds")) Directory.CreateDirectory("Builds");
        }

        static void ConfigurePlayer()
        {
            PlayerSettings.companyName = "Fallt Games";
            PlayerSettings.productName = "Five Line Race";
            PlayerSettings.bundleVersion = "0.2.0";
            PlayerSettings.colorSpace = ColorSpace.Linear;

            // Портретная ориентация.
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            // Окно Windows-теста в пропорциях телефона.
            PlayerSettings.defaultScreenWidth = 540;
            PlayerSettings.defaultScreenHeight = 960;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = true;

            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.falltgames.fiveline");
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, "com.falltgames.fiveline");

            // Android: IL2CPP + ARM64.
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.bundleVersionCode = 2;

            EditorUserBuildSettings.buildAppBundle = false;
        }

        /// <summary>
        /// Материал в Resources гарантирует, что URP Unlit попадёт в билд,
        /// хотя все материалы игра создаёт в рантайме.
        /// </summary>
        static void EnsureMaterialAsset()
        {
            if (AssetDatabase.LoadAssetAtPath<Material>(MaterialPath) != null) return;

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader == null)
            {
                Debug.LogWarning("[FiveLineRace] Не найден unlit-шейдер URP.");
                return;
            }

            var material = new Material(shader);
            material.name = "TurboUnlit";
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", Color.white);
            else material.color = Color.white;

            AssetDatabase.CreateAsset(material, MaterialPath);
        }

        static void EnsureScene()
        {
            if (File.Exists(ScenePath)) return;

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // --- Камера (ортографическая, вид строго сверху-вниз по плоскости XY) ---
            var cameraGO = new GameObject("Main Camera");
            cameraGO.tag = "MainCamera";
            var camera = cameraGO.AddComponent<Camera>();
            cameraGO.AddComponent<AudioListener>();
            cameraGO.AddComponent<CameraShake2D>();
            cameraGO.transform.position = new Vector3(0f, 0f, -10f);
            cameraGO.transform.rotation = Quaternion.identity;
            camera.orthographic = true;
            camera.orthographicSize = G2.OrthoSize;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = G2.Night;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 60f;

            // --- Игра ---
            var game = new GameObject("Game");
            game.AddComponent<Game2Manager>();

            // Плоская графика: свет и туман не нужны.
            RenderSettings.fog = false;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = Color.white;

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log("[FiveLineRace] Создана сцена " + ScenePath);
        }

        static void EnsureBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes;
            var cleaned = new List<EditorBuildSettingsScene>();

            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i] == null || string.IsNullOrEmpty(scenes[i].path)) continue;
                if (scenes[i].path == ScenePath) continue;
                if (!File.Exists(scenes[i].path)) continue;
                cleaned.Add(scenes[i]);
            }

            cleaned.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = cleaned.ToArray();
        }

        // ---------------- Сборка ----------------

        static void BuildWindowsInternal()
        {
            Directory.CreateDirectory("Builds/Windows");

            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = WindowsBuildPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            CheckReport(report, "Windows", WindowsBuildPath);
        }

        static void BuildAndroidInternal()
        {
            Directory.CreateDirectory("Builds/Android");

            EditorUserBuildSettings.buildAppBundle = false;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = AndroidBuildPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            CheckReport(report, "Android", AndroidBuildPath);
        }

        static void CheckReport(BuildReport report, string name, string path)
        {
            BuildSummary summary = report.summary;

            if (summary.result != BuildResult.Succeeded)
            {
                throw new Exception(string.Format(
                    "Сборка {0} завершилась с результатом {1} (ошибок: {2}). Смотри консоль/лог.",
                    name, summary.result, summary.totalErrors));
            }

            Debug.Log(string.Format("[FiveLineRace] Сборка {0} готова: {1} ({2:0.0} МБ)",
                name, path, summary.totalSize / (1024f * 1024f)));
        }

        static void Exit(int code)
        {
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(code);
            }
        }

        // ---------------- Авто-настройка при первом открытии ----------------

        [InitializeOnLoadMethod]
        static void AutoSetup()
        {
            if (Application.isBatchMode) return;

            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
                {
                    EnsureBuildSettings();
                    return;
                }

                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

                SetupProject();

                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
                {
                    EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                }
            };
        }
    }
}
