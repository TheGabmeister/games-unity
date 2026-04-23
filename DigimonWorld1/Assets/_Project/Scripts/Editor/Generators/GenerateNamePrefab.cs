using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class GenerateNamePrefab
{
    private const string PrefabPath = PrefabGeneratorUtils.ControllersPrefabDir + "/NameController.prefab";

    [MenuItem("Tools/DigimonWorld/Prefabs/Generate NameController")]
    public static void Generate()
    {
        GameObject root = PrefabGeneratorUtils.CreateCanvasRoot("NameController");

        PrefabGeneratorUtils.CreateText("Title", root.transform,
            "Enter Names", 48, FontStyles.Bold, TextAlignmentOptions.Center,
            new Vector2(0f, 200f), new Vector2(600f, 70f));

        PrefabGeneratorUtils.CreateText("PlayerNameLabel", root.transform,
            "Player Name", 28, FontStyles.Normal, TextAlignmentOptions.MidlineLeft,
            new Vector2(-100f, 80f), new Vector2(300f, 40f));

        TMP_InputField playerNameInput = PrefabGeneratorUtils.CreateInputField("PlayerNameInput", root.transform,
            new Vector2(0f, 30f), new Vector2(400f, 50f));

        PrefabGeneratorUtils.CreateText("DigimonNameLabel", root.transform,
            "Digimon Name", 28, FontStyles.Normal, TextAlignmentOptions.MidlineLeft,
            new Vector2(-100f, -40f), new Vector2(300f, 40f));

        TMP_InputField digimonNameInput = PrefabGeneratorUtils.CreateInputField("DigimonNameInput", root.transform,
            new Vector2(0f, -90f), new Vector2(400f, 50f));

        // Confirm button
        GameObject buttonGo = new GameObject("ConfirmButton", typeof(RectTransform));
        buttonGo.transform.SetParent(root.transform, false);
        Image buttonImage = buttonGo.AddComponent<Image>();
        buttonImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        Button button = buttonGo.AddComponent<Button>();
        RectTransform buttonRt = buttonGo.GetComponent<RectTransform>();
        buttonRt.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRt.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRt.anchoredPosition = new Vector2(0f, -180f);
        buttonRt.sizeDelta = new Vector2(200f, 50f);

        TMP_Text buttonText = PrefabGeneratorUtils.CreateText("Text", buttonGo.transform,
            "Confirm", 28, FontStyles.Bold, TextAlignmentOptions.Center,
            Vector2.zero, new Vector2(200f, 50f));
        buttonText.color = Color.white;

        // Controller
        NameController controller = root.AddComponent<NameController>();

        SerializedObject so = new SerializedObject(controller);
        so.FindProperty("_playerNameInput").objectReferenceValue = playerNameInput;
        so.FindProperty("_digimonNameInput").objectReferenceValue = digimonNameInput;
        so.ApplyModifiedPropertiesWithoutUndo();

        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            button.onClick,
            new UnityEngine.Events.UnityAction(controller.OnConfirm));

        PrefabGeneratorUtils.SaveAndCleanup(root, PrefabPath);
    }
}
