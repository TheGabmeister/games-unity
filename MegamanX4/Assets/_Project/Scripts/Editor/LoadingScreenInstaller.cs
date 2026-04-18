using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class LoadingScreenInstaller
{
    const string GameServicesPath = "Assets/Resources/GameServices.prefab";
    const int LoadingScreenSortingOrder = 100;
    const int ScreenFaderSortingOrder = 200;

    [MenuItem("Tools/MegamanX4/Install LoadingScreen on GameServices")]
    public static void Install()
    {
        var root = PrefabUtility.LoadPrefabContents(GameServicesPath);
        try
        {
            var existing = root.GetComponentInChildren<LoadingScreen>(true);
            if (existing)
            {
                Debug.Log("LoadingScreen already installed on GameServices.");
                return;
            }

            var canvasGO = new GameObject(
                "LoadingScreen",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(LoadingScreen));
            canvasGO.transform.SetParent(root.transform, false);

            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = LoadingScreenSortingOrder;

            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var bgGO = new GameObject("Background", typeof(RectTransform));
            bgGO.transform.SetParent(canvasGO.transform, false);
            var bgRT = bgGO.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
            bgGO.AddComponent<Image>().color = Color.black;

            var labelGO = new GameObject("Label", typeof(RectTransform));
            labelGO.transform.SetParent(canvasGO.transform, false);
            var labelRT = labelGO.GetComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0.5f, 0.5f);
            labelRT.anchorMax = new Vector2(0.5f, 0.5f);
            labelRT.sizeDelta = new Vector2(800, 120);
            labelRT.anchoredPosition = Vector2.zero;
            var label = labelGO.AddComponent<TextMeshProUGUI>();
            label.text = "NOW LOADING";
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 72f;
            label.color = Color.white;

            EnsureScreenFaderCanvas(root);

            canvasGO.SetActive(false);

            PrefabUtility.SaveAsPrefabAsset(root, GameServicesPath);
            Debug.Log($"Installed LoadingScreen + Canvas on {GameServicesPath}");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
        AssetDatabase.Refresh();
    }

    static void EnsureScreenFaderCanvas(GameObject root)
    {
        var fader = root.GetComponentInChildren<ScreenFader>(true);
        if (!fader)
            return;

        var faderGO = fader.gameObject;
        var faderCanvas = faderGO.GetComponent<Canvas>();
        if (!faderCanvas)
        {
            faderCanvas = faderGO.AddComponent<Canvas>();
            faderCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
        faderCanvas.overrideSorting = true;
        faderCanvas.sortingOrder = ScreenFaderSortingOrder;

        var faderImage = faderGO.GetComponent<Image>();
        if (faderImage)
        {
            var rt = faderGO.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            faderImage.raycastTarget = false;
            faderImage.color = new Color(0f, 0f, 0f, 0f);
        }
    }
}
