using Eflatun.SceneReference;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

public static class GenerateBootstrapPrefabs
{
    private const string PrefabDir = "Assets/_Project/Prefabs";
    private const string BootstrapperPrefabPath = PrefabDir + "/Bootstrapper.prefab";
    private const string AudioSystemPrefabPath = PrefabDir + "/AudioSystem.prefab";
    private const string GameManagerPrefabPath = PrefabDir + "/GameManager.prefab";
    private const string SplashscreenControllerPrefabPath = PrefabDir + "/SplashscreenController.prefab";
    private const string IntroControllerPrefabPath = PrefabDir + "/IntroController.prefab";
    private const string MainMenuControllerPrefabPath = PrefabDir + "/MainMenuController.prefab";
    private const string NameControllerPrefabPath = PrefabDir + "/NameController.prefab";

    private const string SplashscreenScenePath = "Assets/_Project/Scenes/_Splashscreen.unity";
    private const string IntroScenePath = "Assets/_Project/Scenes/_Intro.unity";
    private const string MainMenuScenePath = "Assets/_Project/Scenes/_MainMenu.unity";
    private const string NameScenePath = "Assets/_Project/Scenes/_Name.unity";
    private const string GameplayScenePath = "Assets/_Project/Scenes/_Gameplay.unity";

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate Bootstrapper")]
    public static void GenerateBootstrapper()
    {
        SavePrefab("Bootstrapper", BootstrapperPrefabPath, go =>
        {
            //go.AddComponent<Bootstrapper>();
        });
    }

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate AudioSystem")]
    public static void GenerateAudioSystem()
    {
        SavePrefab("AudioSystem", AudioSystemPrefabPath, go => go.AddComponent<AudioSystem>());
    }

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate GameManager")]
    public static void GenerateGameManager()
    {
        SavePrefab("GameManager", GameManagerPrefabPath, go => go.AddComponent<GameManager>());

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GameManagerPrefabPath);
        GameManager gm = prefab.GetComponent<GameManager>();
        SerializedObject so = new SerializedObject(gm);

        SetSceneReference(so, "_splashscreenScene", SplashscreenScenePath);
        SetSceneReference(so, "_introScene", IntroScenePath);
        SetSceneReference(so, "_mainMenuScene", MainMenuScenePath);
        SetSceneReference(so, "_nameScene", NameScenePath);
        SetSceneReference(so, "_gameplayScene", GameplayScenePath);

        so.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate SplashscreenController")]
    public static void GenerateSplashscreenController()
    {
        SavePrefab("SplashscreenController", SplashscreenControllerPrefabPath, go =>
        {
            go.AddComponent<SplashscreenController>();
        });
    }

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate IntroController")]
    public static void GenerateIntroController()
    {
        SavePrefab("IntroController", IntroControllerPrefabPath, go =>
        {
            VideoPlayer vp = go.AddComponent<VideoPlayer>();
            vp.playOnAwake = true;
            vp.renderMode = VideoRenderMode.CameraNearPlane;

            IntroController controller = go.AddComponent<IntroController>();

            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("_videoPlayer").objectReferenceValue = vp;
            so.ApplyModifiedPropertiesWithoutUndo();
        });
    }

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate MainMenuController")]
    public static void GenerateMainMenuController()
    {
        SavePrefab("MainMenuController", MainMenuControllerPrefabPath, go =>
        {
            go.AddComponent<MainMenuController>();
        });
    }

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate NameController")]
    public static void GenerateNameController()
    {
        SavePrefab("NameController", NameControllerPrefabPath, go =>
        {
            go.AddComponent<NameController>();
        });
    }

    private static void SetSceneReference(SerializedObject so, string fieldName, string scenePath)
    {
        string guid = AssetDatabase.AssetPathToGUID(scenePath);
        if (string.IsNullOrEmpty(guid))
        {
            Debug.LogWarning($"Scene not found at {scenePath} — {fieldName} will be empty. Generate scenes first.");
            return;
        }

        SerializedProperty prop = so.FindProperty(fieldName);
        SerializedProperty guidProp = prop.FindPropertyRelative("guid");
        SerializedProperty assetProp = prop.FindPropertyRelative("asset");

        guidProp.stringValue = guid;
        assetProp.objectReferenceValue = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
    }

    private static void SavePrefab(string name, string path, System.Action<GameObject> configure)
    {
        EnsureFolder(PrefabDir);

        GameObject source = new GameObject(name);
        try
        {
            configure(source);
            PrefabUtility.SaveAsPrefabAsset(source, path, out bool success);
            if (!success)
            {
                Debug.LogError($"Failed to save prefab at {path}");
                return;
            }
        }
        finally
        {
            Object.DestroyImmediate(source);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Prefab generated at {path}");
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;

        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
