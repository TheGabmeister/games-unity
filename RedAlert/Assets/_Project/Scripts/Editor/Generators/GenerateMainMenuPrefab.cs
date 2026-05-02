using TMPro;
using UnityEditor;
using UnityEngine;

public static class GenerateMainMenuPrefab
{
    private const string PrefabPath = PrefabGeneratorUtils.ControllersPrefabDir + "/MainMenuController.prefab";

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate MainMenuController")]
    public static void Generate()
    {
        GameObject root = PrefabGeneratorUtils.CreateCanvasRoot("MainMenuController");

        // Press Start Panel
        GameObject pressStartPanel = PrefabGeneratorUtils.CreatePanel("PressStartPanel", root.transform);

        TMP_Text pressStartTitle = PrefabGeneratorUtils.CreateText("Title", pressStartPanel.transform,
            "DIGIMON WORLD", 72, FontStyles.Bold, TextAlignmentOptions.Center,
            new Vector2(0f, 80f), new Vector2(800f, 100f));
        pressStartTitle.color = Color.white;

        TMP_Text pressStartText = PrefabGeneratorUtils.CreateText("PressStartText", pressStartPanel.transform,
            "Press Start", 36, FontStyles.Normal, TextAlignmentOptions.Center,
            new Vector2(0f, -60f), new Vector2(400f, 50f));
        pressStartText.color = Color.white;

        // Menu Panel (hidden by default)
        GameObject menuPanel = PrefabGeneratorUtils.CreatePanel("MenuPanel", root.transform);
        menuPanel.SetActive(false);

        TMP_Text menuTitle = PrefabGeneratorUtils.CreateText("Title", menuPanel.transform,
            "DIGIMON WORLD", 72, FontStyles.Bold, TextAlignmentOptions.Center,
            new Vector2(0f, 160f), new Vector2(800f, 100f));
        menuTitle.color = Color.white;

        string[] labels = { "New Game", "Continue Game", "Delete Game", "Battle Mode" };
        TMP_Text[] optionTexts = new TMP_Text[labels.Length];
        for (int i = 0; i < labels.Length; i++)
        {
            string prefix = (i == 0) ? "> " : "  ";
            TMP_Text optionText = PrefabGeneratorUtils.CreateText(labels[i].Replace(" ", ""), menuPanel.transform,
                prefix + labels[i], 36, FontStyles.Normal, TextAlignmentOptions.MidlineLeft,
                new Vector2(-60f, 40f - i * 50f), new Vector2(400f, 50f));
            optionText.color = (i == 0) ? Color.white : Color.gray;
            optionTexts[i] = optionText;
        }

        // Controller
        MainMenuController controller = root.AddComponent<MainMenuController>();

        SerializedObject so = new SerializedObject(controller);
        so.FindProperty("_pressStartPanel").objectReferenceValue = pressStartPanel;
        so.FindProperty("_menuPanel").objectReferenceValue = menuPanel;
        so.FindProperty("_pressStartText").objectReferenceValue = pressStartText;

        SerializedProperty menuOptionsProp = so.FindProperty("_menuOptionTexts");
        menuOptionsProp.arraySize = optionTexts.Length;
        for (int i = 0; i < optionTexts.Length; i++)
            menuOptionsProp.GetArrayElementAtIndex(i).objectReferenceValue = optionTexts[i];

        so.ApplyModifiedPropertiesWithoutUndo();

        PrefabGeneratorUtils.SaveAndCleanup(root, PrefabPath);
    }
}
