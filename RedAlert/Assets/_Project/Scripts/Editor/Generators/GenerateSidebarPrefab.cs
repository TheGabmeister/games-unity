using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class GenerateSidebarPrefab
{
    public static void Generate()
    {
        string path = "Assets/_Project/Prefabs/SidebarCanvas.prefab";
        PrefabGeneratorUtils.EnsureFolder("Assets/_Project/Prefabs");

        GameObject root = new GameObject("SidebarCanvas");
        try
        {
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            root.AddComponent<GraphicRaycaster>();

            // Sidebar panel (right side)
            var sidebarPanel = CreatePanel("SidebarPanel", root.transform,
                new Vector2(1, 0), new Vector2(1, 1),
                new Vector2(-288, 0), Vector2.zero);
            var sidebarBG = sidebarPanel.AddComponent<Image>();
            sidebarBG.color = new Color(0.15f, 0.15f, 0.15f, 1f);

            // ---- Credits Display ----
            var creditsPanel = CreatePanel("CreditsPanel", sidebarPanel.transform,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -40), new Vector2(0, 0));
            var creditsRT = creditsPanel.GetComponent<RectTransform>();
            creditsRT.sizeDelta = new Vector2(0, 40);
            creditsRT.anchoredPosition = new Vector2(0, -20);

            var creditsBG = creditsPanel.AddComponent<Image>();
            creditsBG.color = new Color(0.1f, 0.1f, 0.1f, 1f);

            var creditsText = CreateTMP("CreditsText", creditsPanel.transform, "$10,000", 20,
                TextAlignmentOptions.Center, Color.green);
            var creditsTextRT = creditsText.GetComponent<RectTransform>();
            creditsTextRT.anchorMin = Vector2.zero;
            creditsTextRT.anchorMax = Vector2.one;
            creditsTextRT.offsetMin = Vector2.zero;
            creditsTextRT.offsetMax = Vector2.zero;

            // ---- Power Bar ----
            var powerPanel = CreatePanel("PowerPanel", sidebarPanel.transform,
                new Vector2(0, 1), new Vector2(0, 1),
                Vector2.zero, Vector2.zero);
            var powerRT = powerPanel.GetComponent<RectTransform>();
            powerRT.anchoredPosition = new Vector2(15, -250);
            powerRT.sizeDelta = new Vector2(20, 400);

            var powerBG = powerPanel.AddComponent<Image>();
            powerBG.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            var powerFill = new GameObject("PowerFill", typeof(RectTransform));
            powerFill.transform.SetParent(powerPanel.transform, false);
            var fillImage = powerFill.AddComponent<Image>();
            fillImage.color = Color.green;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Vertical;
            fillImage.fillOrigin = 0;
            fillImage.fillAmount = 0.5f;
            var fillRT = powerFill.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.offsetMin = new Vector2(2, 2);
            fillRT.offsetMax = new Vector2(-2, -2);

            // ---- Sell / Repair Buttons ----
            var buttonsPanel = CreatePanel("ButtonsPanel", sidebarPanel.transform,
                new Vector2(0, 1), new Vector2(1, 1),
                Vector2.zero, Vector2.zero);
            var buttonsRT = buttonsPanel.GetComponent<RectTransform>();
            buttonsRT.anchoredPosition = new Vector2(0, -60);
            buttonsRT.sizeDelta = new Vector2(0, 30);
            var buttonsLayout = buttonsPanel.AddComponent<HorizontalLayoutGroup>();
            buttonsLayout.spacing = 4;
            buttonsLayout.padding = new RectOffset(40, 4, 2, 2);

            var sellBtn = CreateButton("SellButton", buttonsPanel.transform, "SELL",
                new Color(0.7f, 0.2f, 0.2f), 14);
            var repairBtn = CreateButton("RepairButton", buttonsPanel.transform, "REPAIR",
                new Color(0.2f, 0.5f, 0.7f), 14);

            // ---- Build Grid Area ----
            var buildArea = CreatePanel("BuildArea", sidebarPanel.transform,
                new Vector2(0, 0), new Vector2(1, 1),
                Vector2.zero, Vector2.zero);
            var buildAreaRT = buildArea.GetComponent<RectTransform>();
            buildAreaRT.offsetMin = new Vector2(36, 4);
            buildAreaRT.offsetMax = new Vector2(-4, -96);

            var buildLayout = buildArea.AddComponent<HorizontalLayoutGroup>();
            buildLayout.spacing = 4;
            buildLayout.childForceExpandWidth = true;
            buildLayout.childForceExpandHeight = true;

            // Structure column
            var structCol = CreateScrollColumn("StructureGrid", buildArea.transform);
            // Unit column
            var unitCol = CreateScrollColumn("UnitGrid", buildArea.transform);

            // ---- Build Slot Prefab ----
            var buildSlot = CreateBuildSlotPrefab();

            // ---- Sidebar UI Component ----
            var sidebarUI = root.AddComponent<SidebarUI>();
            var uiSO = new SerializedObject(sidebarUI);
            uiSO.FindProperty("_creditsText").objectReferenceValue = creditsText;
            uiSO.FindProperty("_powerBarFill").objectReferenceValue = fillImage;
            uiSO.FindProperty("_powerBarBG").objectReferenceValue = powerBG;
            uiSO.FindProperty("_sellButton").objectReferenceValue = sellBtn.GetComponent<Button>();
            uiSO.FindProperty("_repairButton").objectReferenceValue = repairBtn.GetComponent<Button>();
            uiSO.FindProperty("_sellButtonImage").objectReferenceValue = sellBtn.GetComponent<Image>();
            uiSO.FindProperty("_repairButtonImage").objectReferenceValue = repairBtn.GetComponent<Image>();
            uiSO.FindProperty("_structureGrid").objectReferenceValue = structCol.transform;
            uiSO.FindProperty("_unitGrid").objectReferenceValue = unitCol.transform;

            var slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/UI/BuildSlot.prefab");
            if (slotPrefab != null)
                uiSO.FindProperty("_buildSlotPrefab").objectReferenceValue = slotPrefab;

            uiSO.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally
        {
            Object.DestroyImmediate(root);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Generated Sidebar Canvas prefab");
    }

    public static void GenerateBuildSlotPrefab()
    {
        CreateBuildSlotPrefab();
    }

    static GameObject CreateBuildSlotPrefab()
    {
        string path = "Assets/_Project/Prefabs/UI/BuildSlot.prefab";
        PrefabGeneratorUtils.EnsureFolder("Assets/_Project/Prefabs/UI");

        var slot = new GameObject("BuildSlot", typeof(RectTransform));
        try
        {
            var slotRT = slot.GetComponent<RectTransform>();
            slotRT.sizeDelta = new Vector2(120, 48);

            var bg = slot.AddComponent<Image>();
            bg.color = new Color(0.25f, 0.25f, 0.25f, 1f);
            slot.AddComponent<Button>();

            var layout = slot.AddComponent<LayoutElement>();
            layout.preferredHeight = 48;
            layout.minHeight = 48;

            // Icon
            var iconGO = new GameObject("Icon", typeof(RectTransform));
            iconGO.transform.SetParent(slot.transform, false);
            var iconImage = iconGO.AddComponent<Image>();
            iconImage.preserveAspect = true;
            var iconRT = iconGO.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0, 0);
            iconRT.anchorMax = new Vector2(0, 1);
            iconRT.offsetMin = new Vector2(2, 2);
            iconRT.offsetMax = new Vector2(46, -2);

            // Name/Cost text
            var nameGO = new GameObject("Name", typeof(RectTransform));
            nameGO.transform.SetParent(slot.transform, false);
            var nameText = nameGO.AddComponent<TextMeshProUGUI>();
            nameText.fontSize = 11;
            nameText.alignment = TextAlignmentOptions.Left;
            nameText.color = Color.white;
            var nameRT = nameGO.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0);
            nameRT.anchorMax = new Vector2(1, 1);
            nameRT.offsetMin = new Vector2(48, 2);
            nameRT.offsetMax = new Vector2(-4, -2);

            // Progress bar
            var progressGO = new GameObject("Progress", typeof(RectTransform));
            progressGO.transform.SetParent(slot.transform, false);
            var progressImage = progressGO.AddComponent<Image>();
            progressImage.color = new Color(0.2f, 0.8f, 0.2f, 0.4f);
            progressImage.type = Image.Type.Filled;
            progressImage.fillMethod = Image.FillMethod.Horizontal;
            progressImage.fillAmount = 0f;
            var progressRT = progressGO.GetComponent<RectTransform>();
            progressRT.anchorMin = Vector2.zero;
            progressRT.anchorMax = Vector2.one;
            progressRT.offsetMin = Vector2.zero;
            progressRT.offsetMax = Vector2.zero;

            // Ready label
            var readyGO = new GameObject("ReadyLabel", typeof(RectTransform));
            readyGO.transform.SetParent(slot.transform, false);
            var readyText = readyGO.AddComponent<TextMeshProUGUI>();
            readyText.text = "READY";
            readyText.fontSize = 14;
            readyText.fontStyle = FontStyles.Bold;
            readyText.alignment = TextAlignmentOptions.Center;
            readyText.color = Color.yellow;
            var readyRT = readyGO.GetComponent<RectTransform>();
            readyRT.anchorMin = Vector2.zero;
            readyRT.anchorMax = Vector2.one;
            readyRT.offsetMin = Vector2.zero;
            readyRT.offsetMax = Vector2.zero;
            readyGO.SetActive(false);

            PrefabUtility.SaveAsPrefabAsset(slot, path);
        }
        finally
        {
            Object.DestroyImmediate(slot);
        }

        AssetDatabase.SaveAssets();
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }

    static GameObject CreatePanel(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
        return go;
    }

    static TextMeshProUGUI CreateTMP(string name, Transform parent, string text,
        int fontSize, TextAlignmentOptions alignment, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = color;
        return tmp;
    }

    static GameObject CreateButton(string name, Transform parent, string label,
        Color bgColor, int fontSize)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var image = go.AddComponent<Image>();
        image.color = bgColor;
        go.AddComponent<Button>();

        var layout = go.AddComponent<LayoutElement>();
        layout.flexibleWidth = 1;

        var textGO = new GameObject("Text", typeof(RectTransform));
        textGO.transform.SetParent(go.transform, false);
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        var textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        return go;
    }

    static GameObject CreateScrollColumn(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var scrollRect = go.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        var viewport = new GameObject("Viewport", typeof(RectTransform));
        viewport.transform.SetParent(go.transform, false);
        viewport.AddComponent<Image>().color = Color.clear;
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        var vpRT = viewport.GetComponent<RectTransform>();
        vpRT.anchorMin = Vector2.zero;
        vpRT.anchorMax = Vector2.one;
        vpRT.offsetMin = Vector2.zero;
        vpRT.offsetMax = Vector2.zero;

        var content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(viewport.transform, false);
        var contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.sizeDelta = new Vector2(0, 0);

        var vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 2;
        vlg.padding = new RectOffset(2, 2, 2, 2);
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;

        var csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = vpRT;
        scrollRect.content = contentRT;

        return content;
    }
}
