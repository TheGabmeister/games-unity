using UnityEngine;

/// Manages random encounters during exploration.
/// Decrements a step counter on each move; triggers a battle when it hits 0.
/// Attach to the same scene as PlayerController (or as a persistent manager).
public class EncounterSystem : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] EncounterTable encounterTable;

    int stepsRemaining;
    bool encountersDisabled; // debug toggle: nobattles

    // Public accessors for debug overlay
    public int StepsRemaining => stepsRemaining;
    public string TableName => encounterTable != null ? encounterTable.TableName : "None";
    public bool EncountersDisabled { get => encountersDisabled; set => encountersDisabled = value; }

    void Start()
    {
        if (encounterTable == null)
            encounterTable = CreateDefaultTable();
        RollStepCounter();
    }

    /// Call this each time the player completes a tile move.
    public void OnPlayerStep()
    {
        if (encountersDisabled) return;
        if (encounterTable == null) return;
        if (GameManager.Instance?.StateManager?.CurrentState != GameState.Exploration) return;

        stepsRemaining--;

        if (stepsRemaining <= 0)
        {
            TriggerEncounter();
        }
    }

    /// Force a specific encounter (used by debug console).
    public async void ForceEncounter(EncounterFormation formation)
    {
        if (formation == null) return;
        await StartBattle(formation);
    }

    void TriggerEncounter()
    {
        var formation = encounterTable.GetRandomFormation();
        if (formation == null) return;

        _ = StartBattle(formation);
    }

    async Awaitable StartBattle(EncounterFormation formation)
    {
        // Transition effect: flash
        if (FadeOverlay.Instance != null)
        {
            // Quick white flash
            await FadeOverlay.Instance.FadeOut(0.15f);
            await Awaitable.WaitForSecondsAsync(0.1f);
        }

        // Start battle
        var bm = BattleManager.Instance;
        if (bm != null)
        {
            await bm.StartBattle(formation, OnBattleComplete);
        }
        else
        {
            Debug.LogWarning("[Encounter] No BattleManager found!");
            if (FadeOverlay.Instance != null)
                await FadeOverlay.Instance.FadeIn(0.3f);
            RollStepCounter();
        }
    }

    void OnBattleComplete()
    {
        RollStepCounter();
    }

    void RollStepCounter()
    {
        if (encounterTable != null)
            stepsRemaining = encounterTable.RollStepCount();
        else
            stepsRemaining = 30; // fallback
    }

    /// Assign or change the encounter table (e.g., when entering a new area).
    public void SetEncounterTable(EncounterTable table)
    {
        encounterTable = table;
        RollStepCounter();
    }

    EncounterTable CreateDefaultTable()
    {
        var goblin = ScriptableObject.CreateInstance<EnemyData>();
        goblin.EnemyName = "Goblin";
        goblin.MaxHP = 20; goblin.Attack = 8; goblin.Defense = 2;
        goblin.Accuracy = 10; goblin.Evasion = 2; goblin.Agility = 5;
        goblin.HitCount = 1; goblin.MagicDefense = 5; goblin.MagicEvasion = 5;
        goblin.EXPReward = 15; goblin.GilReward = 10;
        goblin.PrimaryColor = new Color(0.7f, 0.2f, 0.2f);
        goblin.Shape = EnemyShape.Circle;
        goblin.Actions = new[] { new EnemyAction { Type = EnemyActionType.Attack, Weight = 10 } };

        var table = ScriptableObject.CreateInstance<EncounterTable>();
        table.TableName = "Default";
        table.BaseStepCount = 25;
        table.StepVariance = 8;
        table.Formations = new[]
        {
            new EncounterFormation
            {
                FormationName = "2x Goblin",
                Groups = new[] { new EnemyGroup { Enemy = goblin, Count = 2 } },
                Weight = 10,
            },
            new EncounterFormation
            {
                FormationName = "3x Goblin",
                Groups = new[] { new EnemyGroup { Enemy = goblin, Count = 3 } },
                Weight = 5,
            },
        };
        return table;
    }
}
