using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DebugConsole : MonoBehaviour
{
    TMP_InputField inputField;
    TextMeshProUGUI outputText;
    ScrollRect scrollRect;
    bool isVisible;

    Canvas canvas;
    string outputLog = "";

    void Awake()
    {
        SetupUI();
        SetVisible(false);
    }

    void SetupUI()
    {
        canvas = GetComponentInParent<Canvas>();

        var rect = GetComponent<RectTransform>();
        if (rect == null) rect = gameObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 0.35f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // Background
        var bg = gameObject.GetComponent<Image>();
        if (bg == null) bg = gameObject.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.85f);
        bg.raycastTarget = true;

        // Output text area (scrollable)
        var outputGO = new GameObject("Output");
        outputGO.transform.SetParent(transform, false);
        var outputRect = outputGO.AddComponent<RectTransform>();
        outputRect.anchorMin = new Vector2(0, 0.12f);
        outputRect.anchorMax = Vector2.one;
        outputRect.offsetMin = new Vector2(10, 0);
        outputRect.offsetMax = new Vector2(-10, -5);

        outputText = outputGO.AddComponent<TextMeshProUGUI>();
        outputText.fontSize = 14;
        outputText.color = Color.green;
        outputText.alignment = TextAlignmentOptions.BottomLeft;
        outputText.enableWordWrapping = true;
        outputText.overflowMode = TextOverflowModes.Truncate;
        outputText.raycastTarget = false;

        // Input field
        var inputGO = new GameObject("Input");
        inputGO.transform.SetParent(transform, false);
        var inputRect = inputGO.AddComponent<RectTransform>();
        inputRect.anchorMin = Vector2.zero;
        inputRect.anchorMax = new Vector2(1, 0.12f);
        inputRect.offsetMin = new Vector2(10, 5);
        inputRect.offsetMax = new Vector2(-10, 0);

        // TMP Input field needs a background image on the inputGO
        var inputBG = inputGO.AddComponent<Image>();
        inputBG.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        // Create text area child for the input field
        var textAreaGO = new GameObject("Text Area");
        textAreaGO.transform.SetParent(inputGO.transform, false);
        var textAreaRect = textAreaGO.AddComponent<RectTransform>();
        textAreaRect.anchorMin = Vector2.zero;
        textAreaRect.anchorMax = Vector2.one;
        textAreaRect.offsetMin = new Vector2(5, 0);
        textAreaRect.offsetMax = new Vector2(-5, 0);

        // The actual text child
        var textGO = new GameObject("Text");
        textGO.transform.SetParent(textAreaGO.transform, false);
        var textComponent = textGO.AddComponent<TextMeshProUGUI>();
        textComponent.fontSize = 14;
        textComponent.color = Color.white;
        var textR = textComponent.rectTransform;
        textR.anchorMin = Vector2.zero;
        textR.anchorMax = Vector2.one;
        textR.offsetMin = Vector2.zero;
        textR.offsetMax = Vector2.zero;

        // Placeholder
        var placeholderGO = new GameObject("Placeholder");
        placeholderGO.transform.SetParent(textAreaGO.transform, false);
        var placeholder = placeholderGO.AddComponent<TextMeshProUGUI>();
        placeholder.text = "Enter command...";
        placeholder.fontSize = 14;
        placeholder.color = new Color(1, 1, 1, 0.3f);
        placeholder.fontStyle = FontStyles.Italic;
        var phRect = placeholder.rectTransform;
        phRect.anchorMin = Vector2.zero;
        phRect.anchorMax = Vector2.one;
        phRect.offsetMin = Vector2.zero;
        phRect.offsetMax = Vector2.zero;

        inputField = inputGO.AddComponent<TMP_InputField>();
        inputField.textComponent = textComponent;
        inputField.textViewport = textAreaRect;
        inputField.placeholder = placeholder;
        inputField.onSubmit.AddListener(OnSubmitCommand);
    }

    void Update()
    {
        // Toggle with backtick key
        bool togglePressed = UnityEngine.Input.GetKeyDown(KeyCode.BackQuote);
        if (!togglePressed)
        {
            var input = GameManager.Instance?.InputManager;
            if (input?.DebugConsoleAction != null && input.DebugConsoleAction.WasPressedThisFrame())
                togglePressed = true;
        }

        if (togglePressed)
        {
            SetVisible(!isVisible);
        }
    }

    void SetVisible(bool visible)
    {
        isVisible = visible;

        // Show/hide all children
        foreach (Transform child in transform)
            child.gameObject.SetActive(visible);

        var bg = GetComponent<Image>();
        if (bg != null) bg.enabled = visible;

        if (visible && inputField != null)
        {
            inputField.ActivateInputField();
            inputField.Select();
        }
    }

    void OnSubmitCommand(string command)
    {
        if (string.IsNullOrWhiteSpace(command)) return;

        Log($"> {command}");
        ExecuteCommand(command.Trim());

        inputField.text = "";
        inputField.ActivateInputField();
    }

    void ExecuteCommand(string command)
    {
        var parts = command.Split(' ');
        string cmd = parts[0].ToLower();

        switch (cmd)
        {
            case "help":
                Log("Commands: help, state <state>, pos <x> <y>, save <slot>, load <slot>, scene <name>");
                break;

            case "state":
                if (parts.Length < 2) { Log("Usage: state <stateName>"); break; }
                if (System.Enum.TryParse<GameState>(parts[1], true, out var newState))
                {
                    GameManager.Instance?.StateManager?.ChangeState(newState);
                    Log($"State changed to {newState}");
                }
                else Log($"Unknown state: {parts[1]}");
                break;

            case "pos":
                if (parts.Length < 3) { Log("Usage: pos <x> <y>"); break; }
                if (int.TryParse(parts[1], out int px) && int.TryParse(parts[2], out int py))
                {
                    var player = Object.FindFirstObjectByType<PlayerController>();
                    if (player != null)
                    {
                        player.SetPosition(new Vector2Int(px, py));
                        Log($"Teleported to ({px}, {py})");
                    }
                    else Log("No PlayerController found");
                }
                else Log("Invalid coordinates");
                break;

            case "save":
                if (parts.Length < 2) { Log("Usage: save <slot 0-3>"); break; }
                if (int.TryParse(parts[1], out int saveSlot))
                {
                    var player = Object.FindFirstObjectByType<PlayerController>();
                    var data = new SaveData
                    {
                        Type = SaveType.Manual,
                        CurrentScene = GameManager.Instance?.SceneLoader?.CurrentSceneName ?? "Exploration",
                        PlayerGridX = player?.GridPosition.x ?? 0,
                        PlayerGridY = player?.GridPosition.y ?? 0,
                        FacingDirection = player?.FacingDirection ?? 2,
                        PlayTimeSeconds = Time.time
                    };
                    GameManager.Instance?.SaveManager?.Save(saveSlot, data);
                    Log($"Saved to slot {saveSlot}");
                }
                break;

            case "load":
                if (parts.Length < 2) { Log("Usage: load <slot>"); break; }
                if (int.TryParse(parts[1], out int loadSlot))
                {
                    var data = GameManager.Instance?.SaveManager?.Load(loadSlot);
                    if (data != null)
                    {
                        Log($"Loaded slot {loadSlot}: scene={data.CurrentScene} pos=({data.PlayerGridX},{data.PlayerGridY})");
                        // Apply position
                        var player = Object.FindFirstObjectByType<PlayerController>();
                        player?.SetPosition(new Vector2Int(data.PlayerGridX, data.PlayerGridY));
                    }
                    else Log($"No save at slot {loadSlot}");
                }
                break;

            case "scene":
                if (parts.Length < 2) { Log("Usage: scene <sceneName>"); break; }
                Log($"Loading scene: {parts[1]}");
                _ = GameManager.Instance?.SceneLoader?.LoadScene(parts[1]);
                break;

            default:
                Log($"Unknown command: {cmd}. Type 'help' for commands.");
                break;
        }
    }

    void Log(string message)
    {
        outputLog += message + "\n";
        // Keep last 20 lines
        var lines = outputLog.Split('\n');
        if (lines.Length > 21)
        {
            outputLog = string.Join("\n", lines, lines.Length - 21, 21);
        }
        if (outputText != null)
            outputText.text = outputLog;
    }
}
