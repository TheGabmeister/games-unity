using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public static class GeneratePrefabs
{
    private const string BootstrapperPrefabPath = PrefabGeneratorUtils.PrefabDir + "/Bootstrapper.prefab";
    private const string AudioSystemPrefabPath = PrefabGeneratorUtils.PrefabDir + "/AudioSystem.prefab";
    private const string GameManagerPrefabPath = PrefabGeneratorUtils.PrefabDir + "/GameManager.prefab";
    private const string ScreenFaderPrefabPath = PrefabGeneratorUtils.PrefabDir + "/ScreenFader.prefab";
    private const string SplashscreenControllerPrefabPath = PrefabGeneratorUtils.PrefabDir + "/SplashscreenController.prefab";
    private const string IntroControllerPrefabPath = PrefabGeneratorUtils.PrefabDir + "/IntroController.prefab";
    private const string PlayerPrefabPath = PrefabGeneratorUtils.PrefabDir + "/Player.prefab";

    private const string SplashscreenScenePath = "Assets/_Project/Scenes/_Splashscreen.unity";
    private const string IntroScenePath = "Assets/_Project/Scenes/_Intro.unity";
    private const string MainMenuScenePath = "Assets/_Project/Scenes/_MainMenu.unity";
    private const string NameScenePath = "Assets/_Project/Scenes/_Name.unity";
    private const string GameplayScenePath = "Assets/_Project/Scenes/_Gameplay.unity";
    private const string IntroVideoPath = "Assets/_Project/Videos/IntroVideo.mp4";

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate Bootstrapper")]
    public static void GenerateBootstrapper()
    {
        PrefabGeneratorUtils.SavePrefab("Bootstrapper", BootstrapperPrefabPath, go =>
        {
            //go.AddComponent<Bootstrapper>();
        });
    }

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate AudioSystem")]
    public static void GenerateAudioSystem()
    {
        PrefabGeneratorUtils.SavePrefab("AudioSystem", AudioSystemPrefabPath, go => go.AddComponent<AudioSystem>());
    }

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate ScreenFader")]
    public static void GenerateScreenFader()
    {
        PrefabGeneratorUtils.EnsureFolder(PrefabGeneratorUtils.PrefabDir);

        GameObject root = new GameObject("ScreenFader");
        try
        {
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            GameObject imageGo = new GameObject("FadeImage", typeof(RectTransform));
            imageGo.transform.SetParent(root.transform, false);
            Image fadeImage = imageGo.AddComponent<Image>();
            fadeImage.color = new Color(0f, 0f, 0f, 0f);
            fadeImage.raycastTarget = false;
            RectTransform rt = imageGo.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            ScreenFader fader = root.AddComponent<ScreenFader>();

            SerializedObject so = new SerializedObject(fader);
            so.FindProperty("_fadeImage").objectReferenceValue = fadeImage;
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, ScreenFaderPrefabPath, out bool success);
            if (!success)
            {
                Debug.LogError($"Failed to save prefab at {ScreenFaderPrefabPath}");
                return;
            }
        }
        finally
        {
            Object.DestroyImmediate(root);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Prefab generated at {ScreenFaderPrefabPath}");
    }

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate GameManager")]
    public static void GenerateGameManager()
    {
        PrefabGeneratorUtils.SavePrefab("GameManager", GameManagerPrefabPath, go => go.AddComponent<GameManager>());

        GameObject screenFaderPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ScreenFaderPrefabPath);
        ScreenFader screenFader = screenFaderPrefab != null ? screenFaderPrefab.GetComponent<ScreenFader>() : null;
        if (screenFader == null)
            Debug.LogWarning($"ScreenFader prefab not found at {ScreenFaderPrefabPath}. Generate it first. _screenFader will be empty.");

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GameManagerPrefabPath);
        GameManager gm = prefab.GetComponent<GameManager>();
        SerializedObject so = new SerializedObject(gm);

        PrefabGeneratorUtils.SetSceneReference(so, "_splashscreenScene", SplashscreenScenePath);
        PrefabGeneratorUtils.SetSceneReference(so, "_introScene", IntroScenePath);
        PrefabGeneratorUtils.SetSceneReference(so, "_mainMenuScene", MainMenuScenePath);
        PrefabGeneratorUtils.SetSceneReference(so, "_nameScene", NameScenePath);
        PrefabGeneratorUtils.SetSceneReference(so, "_gameplayScene", GameplayScenePath);

        if (screenFader != null)
            so.FindProperty("_screenFader").objectReferenceValue = screenFader;

        so.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate SplashscreenController")]
    public static void GenerateSplashscreenController()
    {
        PrefabGeneratorUtils.SavePrefab("SplashscreenController", SplashscreenControllerPrefabPath, go =>
        {
            go.AddComponent<SplashscreenController>();
        });
    }

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate IntroController")]
    public static void GenerateIntroController()
    {
        PrefabGeneratorUtils.SavePrefab("IntroController", IntroControllerPrefabPath, go =>
        {
            VideoPlayer vp = go.AddComponent<VideoPlayer>();
            vp.playOnAwake = true;
            vp.renderMode = VideoRenderMode.CameraNearPlane;

            VideoClip clip = AssetDatabase.LoadAssetAtPath<VideoClip>(IntroVideoPath);
            if (clip != null)
                vp.clip = clip;
            else
                Debug.LogWarning($"Intro video not found at {IntroVideoPath} — VideoPlayer clip will be empty.");

            IntroController controller = go.AddComponent<IntroController>();

            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("_videoPlayer").objectReferenceValue = vp;
            so.ApplyModifiedPropertiesWithoutUndo();
        });
    }

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate Player")]
    public static void GeneratePlayer()
    {
        PrefabGeneratorUtils.EnsureFolder(PrefabGeneratorUtils.PrefabDir);

        GameObject root = new GameObject("Player");
        try
        {
            CharacterController cc = root.AddComponent<CharacterController>();
            cc.center = new Vector3(0f, 1f, 0f);
            cc.height = 2f;
            cc.radius = 0.5f;

            root.AddComponent<PlayerController>();

            GameObject model = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            model.name = "PlayerModel";
            model.transform.SetParent(root.transform, false);
            model.transform.localPosition = new Vector3(0f, 1f, 0f);
            Object.DestroyImmediate(model.GetComponent<Collider>());

            PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath, out bool success);
            if (!success)
            {
                Debug.LogError($"Failed to save prefab at {PlayerPrefabPath}");
                return;
            }
        }
        finally
        {
            Object.DestroyImmediate(root);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Prefab generated at {PlayerPrefabPath}");
    }
}
