using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class DebugOverlay : MonoBehaviour
{
    TextMeshProUGUI displayText;
    bool isVisible;
    float fpsTimer;
    float currentFPS;

    Canvas canvas;

    void Awake()
    {
        SetupUI();
        gameObject.SetActive(true);
        SetVisible(false);
    }

    void SetupUI()
    {
        // Find or create canvas - this should be on a DontDestroyOnLoad canvas
        canvas = GetComponentInParent<Canvas>();

        // Create text display
        var textGO = new GameObject("DebugText");
        textGO.transform.SetParent(transform, false);

        displayText = textGO.AddComponent<TextMeshProUGUI>();
        displayText.fontSize = 14;
        displayText.color = Color.green;
        displayText.alignment = TextAlignmentOptions.TopLeft;
        displayText.fontStyle = FontStyles.Normal;

        // Add a dark semi-transparent background
        var bg = gameObject.AddComponent<UnityEngine.UI.Image>();
        bg.color = new Color(0, 0, 0, 0.7f);
        bg.raycastTarget = false;

        var rect = GetComponent<RectTransform>();
        if (rect == null) rect = gameObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(10, -10);
        rect.sizeDelta = new Vector2(400, 360);

        var textRect = displayText.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(5, 5);
        textRect.offsetMax = new Vector2(-5, -5);
    }

    void Update()
    {
        // Toggle with F1
        bool togglePressed = Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame;
        if (!togglePressed)
        {
            var input = GameManager.Instance?.InputManager;
            if (input?.DebugOverlayAction != null && input.DebugOverlayAction.WasPressedThisFrame())
                togglePressed = true;
        }

        if (togglePressed)
            SetVisible(!isVisible);

        if (!isVisible) return;

        // FPS calculation
        fpsTimer += (Time.unscaledDeltaTime - fpsTimer) * 0.1f;
        currentFPS = 1f / fpsTimer;

        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        var gm = GameManager.Instance;
        string state = gm?.StateManager?.CurrentState.ToString() ?? "N/A";
        string scene = gm?.SceneLoader?.CurrentSceneName ?? "N/A";
        string explorationMode = gm?.StateManager?.CurrentExplorationMode.ToString() ?? "N/A";

        var player = Object.FindFirstObjectByType<PlayerController>();
        string playerPos = player != null ? player.GridPosition.ToString() : "N/A";
        string facing = player != null ? GetDirectionName(player.FacingDirection) : "N/A";

        // Party summary
        string partySummary = "";
        var pm = gm?.PartyManager;
        if (pm != null && pm.IsPartyCreated)
        {
            for (int i = 0; i < 4; i++)
            {
                var m = pm.GetMember(i);
                if (m != null)
                {
                    string statusStr = m.StatusEffects != StatusEffectFlags.None ? $" [{m.StatusEffects}]" : "";
                    partySummary += $"{m.Name}({m.ClassDef?.Abbreviation}) Lv{m.Level} {m.CurrentHP}/{m.MaxHP}{statusStr}\n";
                }
            }
        }

        string gilStr = gm?.InventoryManager != null ? $"Gil: {gm.InventoryManager.Gil:N0}" : "";

        // Encounter info
        string encounterStr = "";
        var enc = Object.FindFirstObjectByType<EncounterSystem>();
        if (enc != null)
            encounterStr = $"Enc: {enc.TableName} Steps: {enc.StepsRemaining}{(enc.EncountersDisabled ? " [OFF]" : "")}";

        // Battle info
        string battleStr = "";
        var bm = BattleManager.Instance;
        if (bm != null && bm.CurrentPhase != BattleManager.BattlePhase.Inactive)
        {
            battleStr = $"Battle: {bm.CurrentPhase} Turn:{bm.TurnNumber}";
            if (bm.GodMode) battleStr += " [GOD]";
            if (bm.IsAutoBattleActive) battleStr += " [AUTO]";
            battleStr += "\n";

            // Enemy HP
            foreach (var e in bm.EnemyActors)
            {
                string eStatus = e.StatusEffects != StatusEffectFlags.None ? $" [{e.StatusEffects}]" : "";
                string alive = e.IsAlive ? "" : " DEAD";
                battleStr += $"  {e.Name}: {e.CurrentHP}/{e.MaxHP}{eStatus}{alive}\n";
            }

            // Buff states
            foreach (var a in bm.PartyActors)
            {
                var b = a.Buffs;
                string buffs = "";
                if (b.HasHaste) buffs += "Haste ";
                if (b.HasSlow) buffs += "Slow ";
                if (b.TemperStacks > 0) buffs += $"Temper({b.TemperStacks}) ";
                if (b.ProtectStacks > 0) buffs += $"Protect({b.ProtectStacks}) ";
                if (b.HasSaber) buffs += "Saber ";
                if (b.NulFire) buffs += "NulFire ";
                if (b.NulIce) buffs += "NulIce ";
                if (b.NulLit) buffs += "NulLit ";
                if (buffs.Length > 0)
                    battleStr += $"  {a.Name}: {buffs}\n";
            }
        }

        displayText.text =
            $"FPS: {currentFPS:F1}\n" +
            $"State: {state} | Scene: {scene}\n" +
            $"Mode: {explorationMode} | Pos: {playerPos} | {facing}\n" +
            $"{gilStr}  {encounterStr}\n" +
            partySummary +
            battleStr;
    }

    string GetDirectionName(int dir) => dir switch
    {
        0 => "Up", 1 => "Right", 2 => "Down", 3 => "Left", _ => "?"
    };

    void SetVisible(bool visible)
    {
        isVisible = visible;
        if (displayText != null) displayText.gameObject.SetActive(visible);
        var bg = GetComponent<UnityEngine.UI.Image>();
        if (bg != null) bg.enabled = visible;
    }
}
