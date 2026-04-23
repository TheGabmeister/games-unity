using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

public static class GeneratePrefabs
{
    private const string BootstrapperPrefabPath = PrefabGeneratorUtils.PrefabDir + "/Bootstrapper.prefab";
    private const string AudioSystemPrefabPath = PrefabGeneratorUtils.PrefabDir + "/AudioSystem.prefab";
    private const string GameManagerPrefabPath = PrefabGeneratorUtils.PrefabDir + "/GameManager.prefab";
    private const string SplashscreenControllerPrefabPath = PrefabGeneratorUtils.PrefabDir + "/SplashscreenController.prefab";
    private const string IntroControllerPrefabPath = PrefabGeneratorUtils.PrefabDir + "/IntroController.prefab";

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

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate GameManager")]
    public static void GenerateGameManager()
    {
        PrefabGeneratorUtils.SavePrefab("GameManager", GameManagerPrefabPath, go => go.AddComponent<GameManager>());

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GameManagerPrefabPath);
        GameManager gm = prefab.GetComponent<GameManager>();
        SerializedObject so = new SerializedObject(gm);

        PrefabGeneratorUtils.SetSceneReference(so, "_splashscreenScene", SplashscreenScenePath);
        PrefabGeneratorUtils.SetSceneReference(so, "_introScene", IntroScenePath);
        PrefabGeneratorUtils.SetSceneReference(so, "_mainMenuScene", MainMenuScenePath);
        PrefabGeneratorUtils.SetSceneReference(so, "_nameScene", NameScenePath);
        PrefabGeneratorUtils.SetSceneReference(so, "_gameplayScene", GameplayScenePath);

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
}
