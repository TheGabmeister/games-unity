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

        GenerateBuildSlotPrefab();

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

            // Sidebar panel — anchored to right edge, 288px wide, full height
            var sidebar = CreateRect("SidebarPanel", root.transform);
            sidebar.anchorMin = new Vector2(1, 0);
            sidebar.anchorMax = new Vector2(1, 1);
            sidebar.pivot = new Vector2(1, 1);
            sidebar.offsetMin = new Vector2(-288, 0);
            sidebar.offsetMax = Vector2.zero;
            var sidebarBG = sidebar.gameObject.AddComponent<Image>();
            sidebarBG.color = new Color(0.12f, 0.12f, 0.12f, 1f);

            // Credits — top strip
            var credits = CreateRect("CreditsPanel", sidebar);
            credits.anchorMin = new Vector2(0, 1);
            credits.anchorMax = new Vector2(1, 1);
            credits.pivot = new Vector2(0.5f, 1);
            credits.sizeDelta = new Vector2(0, 36);
            credits.anchoredPosition = Vector2.zero;
            credits.gameObject.AddComponent<Image>().color = new Color(0.08f, 0.08f, 0.08f);

            var creditsText = CreateTMP("CreditsText", credits, "$10,000", 22,
                TextAlignmentOptions.Center, Color.green);

            // Power bar — left edge vertical strip
            var powerBG = CreateRect("PowerBar", sidebar);
            powerBG.anchorMin = new Vector2(0, 0);
            powerBG.anchorMax = new Vector2(0, 1);
            powerBG.pivot = new Vector2(0, 0.5f);
            powerBG.anchoredPosition = new Vector2(4, 0);
            powerBG.sizeDelta = new Vector2(16, -80);
            powerBG.gameObject.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f);

            var powerFill = CreateRect("PowerFill", powerBG);
            powerFill.anchorMin = Vector2.zero;
            powerFill.anchorMax = Vector2.one;
            powerFill.offsetMin = new Vector2(2, 2);
            powerFill.offsetMax = new Vector2(-2, -2);
            var fillImg = powerFill.gameObject.AddComponent<Image>();
            fillImg.color = Color.green;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Vertical;
            fillImg.fillOrigin = 0;
            fillImg.fillAmount = 0.5f;

            // Sell + Repair buttons — below credits
            var btnRow = CreateRect("ButtonsPanel", sidebar);
            btnRow.anchorMin = new Vector2(0, 1);
            btnRow.anchorMax = new Vector2(1, 1);
            btnRow.pivot = new Vector2(0.5f, 1);
            btnRow.anchoredPosition = new Vector2(0, -38);
            btnRow.sizeDelta = new Vector2(-28, 28);
            var hlg = btnRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 4;
            hlg.padding = new RectOffset(24, 0, 0, 0);
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            var sellBtn = CreateUIButton("SellButton", btnRow, "SELL", new Color(0.6f, 0.15f, 0.15f));
            var repairBtn = CreateUIButton("RepairButton", btnRow, "REPAIR", new Color(0.15f, 0.4f, 0.6f));

            // Build area — two columns below buttons, fills remaining space
            var buildArea = CreateRect("BuildArea", sidebar);
            buildArea.anchorMin = Vector2.zero;
            buildArea.anchorMax = Vector2.one;
            buildArea.offsetMin = new Vector2(24, 4);
            buildArea.offsetMax = new Vector2(-4, -70);

            var buildHLG = buildArea.gameObject.AddComponent<HorizontalLayoutGroup>();
            buildHLG.spacing = 4;
            buildHLG.childForceExpandWidth = true;
            buildHLG.childForceExpandHeight = true;
            buildHLG.childControlWidth = true;
            buildHLG.childControlHeight = true;

            // Structure column (left)
            var structScroll = CreateScrollColumn("StructureGrid", buildArea);
            // Unit column (right)
            var unitScroll = CreateScrollColumn("UnitGrid", buildArea);

            // Wire SidebarUI
            var sidebarUI = root.AddComponent<SidebarUI>();
            var so = new SerializedObject(sidebarUI);
            so.FindProperty("_creditsText").objectReferenceValue = creditsText.GetComponent<TMP_Text>();
            so.FindProperty("_powerBarFill").objectReferenceValue = fillImg;
            so.FindProperty("_powerBarBG").objectReferenceValue = powerBG.gameObject.GetComponent<Image>();
            so.FindProperty("_sellButton").objectReferenceValue = sellBtn.GetComponent<Button>();
            so.FindProperty("_repairButton").objectReferenceValue = repairBtn.GetComponent<Button>();
            so.FindProperty("_sellButtonImage").objectReferenceValue = sellBtn.GetComponent<Image>();
            so.FindProperty("_repairButtonImage").objectReferenceValue = repairBtn.GetComponent<Image>();
            so.FindProperty("_structureGrid").objectReferenceValue = structScroll.transform;
            so.FindProperty("_unitGrid").objectReferenceValue = unitScroll.transform;

            var slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/UI/BuildSlot.prefab");
            if (slotPrefab != null)
                so.FindProperty("_buildSlotPrefab").objectReferenceValue = slotPrefab;

            so.ApplyModifiedPropertiesWithoutUndo();

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
        string path = "Assets/_Project/Prefabs/UI/BuildSlot.prefab";
        PrefabGeneratorUtils.EnsureFolder("Assets/_Project/Prefabs/UI");

        var slot = new GameObject("BuildSlot", typeof(RectTransform));
        try
        {
            var slotRT = slot.GetComponent<RectTransform>();
            slotRT.sizeDelta = new Vector2(120, 52);

            var bg = slot.AddComponent<Image>();
            bg.color = new Color(0.22f, 0.22f, 0.22f, 1f);
            slot.AddComponent<Button>();

            var layout = slot.AddComponent<LayoutElement>();
            layout.preferredHeight = 52;
            layout.minHeight = 52;
            layout.flexibleWidth = 1;

            // Icon — left side, square
            var iconRT = CreateRect("Icon", slotRT);
            iconRT.anchorMin = new Vector2(0, 0);
            iconRT.anchorMax = new Vector2(0, 1);
            iconRT.pivot = new Vector2(0, 0.5f);
            iconRT.offsetMin = new Vector2(2, 2);
            iconRT.offsetMax = new Vector2(50, -2);
            var iconImg = iconRT.gameObject.AddComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.color = Color.white;

            // Cost label — right side
            var nameRT = CreateRect("Name", slotRT);
            nameRT.anchorMin = new Vector2(0, 0);
            nameRT.anchorMax = new Vector2(1, 1);
            nameRT.offsetMin = new Vector2(52, 2);
            nameRT.offsetMax = new Vector2(-2, -2);
            var nameTMP = nameRT.gameObject.AddComponent<TextMeshProUGUI>();
            nameTMP.fontSize = 12;
            nameTMP.alignment = TextAlignmentOptions.MidlineLeft;
            nameTMP.color = Color.white;

            // Progress overlay
            var progressRT = CreateRect("Progress", slotRT);
            progressRT.anchorMin = Vector2.zero;
            progressRT.anchorMax = Vector2.one;
            progressRT.offsetMin = Vector2.zero;
            progressRT.offsetMax = Vector2.zero;
            var progressImg = progressRT.gameObject.AddComponent<Image>();
            progressImg.color = new Color(0.2f, 0.8f, 0.2f, 0.35f);
            progressImg.type = Image.Type.Filled;
            progressImg.fillMethod = Image.FillMethod.Horizontal;
            progressImg.fillAmount = 0f;
            progressImg.raycastTarget = false;

            // Ready label
            var readyRT = CreateRect("ReadyLabel", slotRT);
            readyRT.anchorMin = Vector2.zero;
            readyRT.anchorMax = Vector2.one;
            readyRT.offsetMin = Vector2.zero;
            readyRT.offsetMax = Vector2.zero;
            var readyTMP = readyRT.gameObject.AddComponent<TextMeshProUGUI>();
            readyTMP.text = "READY";
            readyTMP.fontSize = 16;
            readyTMP.fontStyle = FontStyles.Bold;
            readyTMP.alignment = TextAlignmentOptions.Center;
            readyTMP.color = Color.yellow;
            readyTMP.raycastTarget = false;
            readyRT.gameObject.SetActive(false);

            PrefabUtility.SaveAsPrefabAsset(slot, path);
        }
        finally
        {
            Object.DestroyImmediate(slot);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Generated BuildSlot prefab");
    }

    static GameObject CreateScrollColumn(string name, RectTransform parent)
    {
        // ScrollRect container
        var scrollGO = new GameObject(name, typeof(RectTransform));
        scrollGO.transform.SetParent(parent, false);
        var scrollRT = scrollGO.GetComponent<RectTransform>();
        scrollRT.anchorMin = Vector2.zero;
        scrollRT.anchorMax = Vector2.one;
        scrollRT.offsetMin = Vector2.zero;
        scrollRT.offsetMax = Vector2.zero;

        var scrollLE = scrollGO.AddComponent<LayoutElement>();
        scrollLE.flexibleWidth = 1;
        scrollLE.flexibleHeight = 1;

        var scrollRect = scrollGO.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        // Viewport with mask
        var viewportRT = CreateRect("Viewport", scrollRT);
        viewportRT.anchorMin = Vector2.zero;
        viewportRT.anchorMax = Vector2.one;
        viewportRT.offsetMin = Vector2.zero;
        viewportRT.offsetMax = Vector2.zero;
        viewportRT.gameObject.AddComponent<Image>().color = Color.clear;
        viewportRT.gameObject.AddComponent<RectMask2D>();

        // Content with vertical layout
        var contentRT = CreateRect("Content", viewportRT);
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.sizeDelta = new Vector2(0, 0);

        var vlg = contentRT.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 3;
        vlg.padding = new RectOffset(2, 2, 2, 2);
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;

        var csf = contentRT.gameObject.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewportRT;
        scrollRect.content = contentRT;

        return contentRT.gameObject;
    }

    static RectTransform CreateRect(string name, RectTransform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    static RectTransform CreateRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    static TextMeshProUGUI CreateTMP(string name, RectTransform parent, string text,
        int fontSize, TextAlignmentOptions alignment, Color color)
    {
        var rt = CreateRect(name, parent);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = color;
        return tmp;
    }

    static GameObject CreateUIButton(string name, RectTransform parent, string label, Color bgColor)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = bgColor;
        go.AddComponent<Button>();
        go.AddComponent<LayoutElement>().flexibleWidth = 1;

        var textRT = CreateRect("Text", go.GetComponent<RectTransform>());
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
        var tmp = textRT.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 13;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return go;
    }
}
