using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// Displays battle victory results: EXP, Gil, item drops, and level-up panels.
/// Player presses Confirm to advance through each display.
public class BattleVictoryUI : MonoBehaviour
{
    Canvas canvas;
    GameObject rootPanel;
    GameObject rewardsPanel;
    TextMeshProUGUI rewardsText;

    // Level-up display
    GameObject levelUpPanel;
    TextMeshProUGUI levelUpText;

    bool waitingForConfirm;
    bool confirmed;

    void Awake()
    {
        BuildUI();
        rootPanel.SetActive(false);
    }

    void OnEnable()
    {
        var bm = BattleManager.Instance;
        if (bm != null)
            bm.OnVictory += OnVictory;
    }

    void OnDisable()
    {
        var bm = BattleManager.Instance;
        if (bm != null)
            bm.OnVictory -= OnVictory;
    }

    void Update()
    {
        if (!waitingForConfirm) return;
        var input = GameManager.Instance?.InputManager;
        if (input?.ConfirmAction != null && input.ConfirmAction.WasPressedThisFrame())
        {
            confirmed = true;
            GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Confirm);
        }
    }

    async void OnVictory(int totalEXP, int totalGil, List<ItemDrop> drops)
    {
        rootPanel.SetActive(true);

        // Show rewards summary
        string text = "Victory!\n\n";
        text += $"EXP: {totalEXP}\n";
        text += $"Gil: {totalGil}\n";

        if (drops != null && drops.Count > 0)
        {
            text += "\nItems found:\n";
            foreach (var drop in drops)
                text += $"  {drop.Item.ItemName}\n";
        }

        rewardsText.text = text;
        rewardsPanel.SetActive(true);
        levelUpPanel.SetActive(false);

        await WaitForConfirm();

        // Show level-up panels
        var pm = GameManager.Instance?.PartyManager;
        if (pm != null)
        {
            for (int i = 0; i < 4; i++)
            {
                var member = pm.GetMember(i);
                if (member == null || !member.IsAlive) continue;

                // Check if member just leveled (level-ups already applied by PartyManager.AddEXP)
                // We show the current stats — the level-up already happened
                var bm = BattleManager.Instance;
                if (bm != null && i < bm.PartyActors.Count)
                {
                    var actor = bm.PartyActors[i];
                    // If the actor's level changed from when battle started
                    if (member.Level > actor.PartyMember.Level)
                    {
                        // This shouldn't happen since we're referencing the same object
                        // Level-ups are already applied — just show current state
                    }
                }
            }

            // Show level-up for each member that leveled
            // PartyManager.AddEXP returns LevelUpResults, but we need to capture those
            // For now, check each member's level against what we'd expect
            for (int i = 0; i < 4; i++)
            {
                var member = pm.GetMember(i);
                if (member == null) continue;

                // Show stat summary for living members
                if (member.IsAlive)
                {
                    rewardsPanel.SetActive(false);
                    levelUpPanel.SetActive(true);

                    string lvlText = $"{member.Name}  Lv {member.Level}\n\n";
                    lvlText += $"HP: {member.CurrentHP}/{member.MaxHP}\n";
                    lvlText += $"MP: {member.CurrentMP}/{member.MaxMP}\n";
                    lvlText += $"STR: {member.BaseStr}  AGI: {member.BaseAgi}\n";
                    lvlText += $"VIT: {member.BaseVit}  INT: {member.BaseInt}\n";
                    lvlText += $"LCK: {member.BaseLuck}\n";
                    lvlText += $"\nEXP: {member.CurrentEXP} / {member.EXPForNextLevel()}";

                    levelUpText.text = lvlText;
                    GameManager.Instance?.Audio?.PlaySFX(SoundEffect.LevelUp);

                    await WaitForConfirm();
                }
            }
        }

        rootPanel.SetActive(false);

        // End the battle
        if (BattleManager.Instance != null)
            await BattleManager.Instance.EndBattle();
    }

    async Awaitable WaitForConfirm()
    {
        waitingForConfirm = true;
        confirmed = false;
        while (!confirmed)
            await Awaitable.NextFrameAsync();
        waitingForConfirm = false;
    }

    void BuildUI()
    {
        canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 110;
        }
        if (GetComponent<CanvasScaler>() == null)
        {
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
        }

        rootPanel = new GameObject("VictoryRoot");
        var rootRect = rootPanel.AddComponent<RectTransform>();
        rootRect.SetParent(canvas.transform, false);
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.sizeDelta = Vector2.zero;

        // Dim overlay
        var dim = rootPanel.AddComponent<Image>();
        dim.color = new Color(0, 0, 0, 0.6f);

        // Rewards panel
        rewardsPanel = new GameObject("RewardsPanel");
        var rpRect = rewardsPanel.AddComponent<RectTransform>();
        rpRect.SetParent(rootRect, false);
        rpRect.anchorMin = new Vector2(0.2f, 0.2f);
        rpRect.anchorMax = new Vector2(0.8f, 0.8f);
        rpRect.sizeDelta = Vector2.zero;
        rewardsPanel.AddComponent<UIWindow>();

        var rtGO = new GameObject("RewardsText");
        rtGO.transform.SetParent(rpRect, false);
        rewardsText = rtGO.AddComponent<TextMeshProUGUI>();
        rewardsText.fontSize = 22;
        rewardsText.color = Color.white;
        rewardsText.alignment = TextAlignmentOptions.TopLeft;
        var rtRect = rewardsText.rectTransform;
        rtRect.anchorMin = Vector2.zero;
        rtRect.anchorMax = Vector2.one;
        rtRect.offsetMin = new Vector2(20, 20);
        rtRect.offsetMax = new Vector2(-20, -20);

        // Level-up panel
        levelUpPanel = new GameObject("LevelUpPanel");
        var lpRect = levelUpPanel.AddComponent<RectTransform>();
        lpRect.SetParent(rootRect, false);
        lpRect.anchorMin = new Vector2(0.25f, 0.15f);
        lpRect.anchorMax = new Vector2(0.75f, 0.85f);
        lpRect.sizeDelta = Vector2.zero;
        levelUpPanel.AddComponent<UIWindow>();

        var ltGO = new GameObject("LevelUpText");
        ltGO.transform.SetParent(lpRect, false);
        levelUpText = ltGO.AddComponent<TextMeshProUGUI>();
        levelUpText.fontSize = 22;
        levelUpText.color = Color.white;
        levelUpText.alignment = TextAlignmentOptions.TopLeft;
        var ltRect = levelUpText.rectTransform;
        ltRect.anchorMin = Vector2.zero;
        ltRect.anchorMax = Vector2.one;
        ltRect.offsetMin = new Vector2(20, 20);
        ltRect.offsetMax = new Vector2(-20, -20);

        levelUpPanel.SetActive(false);
    }
}
