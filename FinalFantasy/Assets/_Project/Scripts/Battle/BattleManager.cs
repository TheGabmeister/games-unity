using System;
using System.Collections.Generic;
using UnityEngine;

/// Main battle flow controller. Manages the full battle lifecycle:
/// encounter start -> command input -> turn execution -> victory/defeat/flee.
public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    // Battle state
    public enum BattlePhase { Inactive, Starting, CommandInput, Executing, Victory, Defeat, Fleeing }
    public BattlePhase CurrentPhase { get; private set; } = BattlePhase.Inactive;

    // Actors
    public List<BattleActor> PartyActors { get; private set; } = new();
    public List<BattleActor> EnemyActors { get; private set; } = new();
    public List<BattleActor> AllActors { get; private set; } = new();

    // Turn state
    int turnNumber;
    List<BattleCommand> turnCommands = new();
    int currentInputSlot; // which party member is currently inputting
    bool autoBattleActive;
    bool isBossEncounter;
    bool godMode; // debug: party takes 0 damage


    // Formation being fought
    EncounterFormation currentFormation;

    // Events for UI
    public event Action<BattlePhase> OnPhaseChanged;
    public event Action<int> OnCommandInputStart; // party slot index
    public event Action<BattleCommand, List<ActionResult>> OnActionExecuted;
    public event Action<BattleActor, int> OnDamageDealt; // actor, amount
    public event Action<BattleActor, int> OnHealing; // actor, amount
    public event Action<BattleActor, string> OnBattleMessage; // actor, message
    public event Action<BattleActor> OnActorDefeated;
    public event Action<int, int, List<ItemDrop>> OnVictory; // totalEXP, totalGil, drops
    public event Action OnDefeat;
    public event Action<bool> OnFleeAttempt; // success

    // Callback for when battle ends (used by encounter system)
    Action onBattleComplete;

    void Awake()
    {
        Instance = this;
    }

    // --- Public API ---

    public async Awaitable StartBattle(EncounterFormation formation, Action onComplete = null)
    {
        if (CurrentPhase != BattlePhase.Inactive) return;

        currentFormation = formation;
        onBattleComplete = onComplete;
        isBossEncounter = formation.IsBoss;
        autoBattleActive = false;
        turnNumber = 0;

        SetPhase(BattlePhase.Starting);

        // Change game state
        GameManager.Instance?.StateManager?.ChangeState(GameState.Battle);
        GameManager.Instance?.InputManager?.EnableUI();

        // Play battle music
        var musicTrack = formation.HasOverrideMusic ? formation.OverrideMusic : (isBossEncounter ? MusicTrack.BossBattle : MusicTrack.Battle);
        GameManager.Instance?.Audio?.PlayBGM(musicTrack);
        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.EncounterStart);

        // Load battle scene additively
        await GameManager.Instance.SceneLoader.LoadSceneAdditive("Battle");

        // Wait a frame for scene objects to initialize
        await Awaitable.NextFrameAsync();

        // Create battle actors
        InitializeActors(formation);

        // Let BattleSceneSetup position enemies (it finds us via Instance)
        var sceneSetup = FindAnyObjectByType<BattleSceneSetup>();
        if (sceneSetup != null)
            sceneSetup.SetupBattle(PartyActors, EnemyActors);

        // Fade in
        if (FadeOverlay.Instance != null)
            await FadeOverlay.Instance.FadeIn(0.3f);

        // Start the battle loop
        await RunBattleLoop();
    }

    public void ToggleAutoBattle()
    {
        autoBattleActive = !autoBattleActive;
        Debug.Log($"[Battle] Auto-battle: {(autoBattleActive ? "ON" : "OFF")}");
    }

    public bool IsAutoBattleActive => autoBattleActive;
    public bool IsBossEncounter => isBossEncounter;
    public int TurnNumber => turnNumber;

    public void SetGodMode(bool enabled) => godMode = enabled;
    public bool GodMode => godMode;

    // Called by BattleUI when player confirms a command
    public void SubmitCommand(BattleCommand command)
    {
        if (CurrentPhase != BattlePhase.CommandInput) return;
        turnCommands.Add(command);

        // If flee, skip remaining party input
        if (command.IsFlee)
        {
            currentInputSlot = 4; // skip to end
            return;
        }

        // Store as last command for auto-battle
        command.Actor.LastCommand = command;

        // Advance to next living party member
        AdvanceInputSlot();
    }

    // --- Battle Loop ---

    async Awaitable RunBattleLoop()
    {
        while (CurrentPhase != BattlePhase.Inactive)
        {
            turnNumber++;
            turnCommands.Clear();

            // Reset defending state
            foreach (var actor in AllActors)
                actor.IsDefending = false;

            // Phase 1: Collect commands
            if (autoBattleActive)
            {
                CollectAutoBattleCommands();
            }
            else
            {
                SetPhase(BattlePhase.CommandInput);
                currentInputSlot = -1;
                AdvanceInputSlot();

                // Wait until all commands are collected
                while (currentInputSlot < 4 && CurrentPhase == BattlePhase.CommandInput)
                    await Awaitable.NextFrameAsync();

                if (CurrentPhase == BattlePhase.Inactive) break;
            }

            // Collect enemy commands
            CollectEnemyCommands();

            // Phase 2: Sort and execute
            SetPhase(BattlePhase.Executing);
            var sorted = TurnSystem.SortActions(turnCommands);

            foreach (var command in sorted)
            {
                if (CurrentPhase != BattlePhase.Executing) break;
                await ExecuteAction(command);

                // Check for battle end after each action
                if (CheckBattleEnd()) break;
            }

            if (CurrentPhase == BattlePhase.Executing)
            {
                // End-of-turn effects
                await ApplyEndOfTurnEffects();

                if (CheckBattleEnd()) continue;
            }

            // Handle battle end states
            if (CurrentPhase == BattlePhase.Victory || CurrentPhase == BattlePhase.Defeat
                || CurrentPhase == BattlePhase.Fleeing)
                break;
        }
    }

    void AdvanceInputSlot()
    {
        currentInputSlot++;
        while (currentInputSlot < 4)
        {
            var actor = currentInputSlot < PartyActors.Count ? PartyActors[currentInputSlot] : null;
            if (actor != null && actor.CanAct() && !actor.IsConfused)
            {
                OnCommandInputStart?.Invoke(currentInputSlot);
                return;
            }
            else if (actor != null && actor.IsConfused && actor.IsAlive)
            {
                // Confused: auto-select random target
                var confusedCmd = CreateConfusedCommand(actor);
                turnCommands.Add(confusedCmd);
            }
            currentInputSlot++;
        }
    }

    void CollectAutoBattleCommands()
    {
        for (int i = 0; i < PartyActors.Count; i++)
        {
            var actor = PartyActors[i];
            if (!actor.CanAct()) continue;

            if (actor.IsConfused)
            {
                turnCommands.Add(CreateConfusedCommand(actor));
                continue;
            }

            var lastCmd = actor.LastCommand;
            if (lastCmd != null && lastCmd.Type == BattleCommandType.Magic && lastCmd.Spell != null)
            {
                // Check MP
                if (actor.IsPartyMember && actor.PartyMember.CurrentMP >= lastCmd.Spell.MPCost)
                {
                    var targets = ResolveAutoTargets(lastCmd);
                    turnCommands.Add(new BattleCommand
                    {
                        Type = BattleCommandType.Magic,
                        Actor = actor,
                        Spell = lastCmd.Spell,
                        Targets = targets,
                        Target = targets.Count == 1 ? targets[0] : null,
                    });
                    continue;
                }
                // Insufficient MP: fall back to attack
            }

            // Default: attack random enemy
            var living = GetLivingEnemies();
            if (living.Count > 0)
            {
                var target = living[UnityEngine.Random.Range(0, living.Count)];
                turnCommands.Add(new BattleCommand
                {
                    Type = BattleCommandType.Attack,
                    Actor = actor,
                    Target = target,
                    Targets = new List<BattleActor> { target },
                });
            }
        }
    }

    List<BattleActor> ResolveAutoTargets(BattleCommand original)
    {
        // Re-resolve targets for auto-battle (original target may be dead)
        if (original.Spell != null)
        {
            switch (original.Spell.Targeting)
            {
                case SpellTarget.AllEnemies: return GetLivingEnemies();
                case SpellTarget.AllAllies: return GetLivingParty();
                case SpellTarget.Self: return new List<BattleActor> { original.Actor };
            }
        }

        // Single target: retarget if dead
        if (original.Target != null && original.Target.IsAlive)
            return new List<BattleActor> { original.Target };

        var pool = original.Type == BattleCommandType.Attack ? GetLivingEnemies() : GetLivingParty();
        if (pool.Count > 0)
            return new List<BattleActor> { pool[UnityEngine.Random.Range(0, pool.Count)] };
        return new List<BattleActor>();
    }

    void CollectEnemyCommands()
    {
        foreach (var enemy in EnemyActors)
        {
            if (!enemy.CanAct()) continue;

            if (enemy.IsConfused)
            {
                turnCommands.Add(CreateConfusedCommand(enemy));
                continue;
            }

            var cmd = EnemyAI.SelectAction(enemy, PartyActors, EnemyActors, turnNumber);
            if (cmd != null) turnCommands.Add(cmd);
        }
    }

    BattleCommand CreateConfusedCommand(BattleActor actor)
    {
        // Confused: basic attack on random target (ally or enemy)
        var allLiving = new List<BattleActor>();
        foreach (var a in AllActors)
            if (a.IsAlive) allLiving.Add(a);

        if (allLiving.Count == 0) return null;

        var target = allLiving[UnityEngine.Random.Range(0, allLiving.Count)];
        return new BattleCommand
        {
            Type = BattleCommandType.Attack,
            Actor = actor,
            Target = target,
            Targets = new List<BattleActor> { target },
        };
    }

    // --- Action Execution ---

    async Awaitable ExecuteAction(BattleCommand command)
    {
        if (command?.Actor == null) return;

        // Check if actor can still act
        if (!command.Actor.CanAct())
        {
            if (command.Actor.IsAlive)
                OnBattleMessage?.Invoke(command.Actor, $"{command.Actor.Name} can't move!");
            return;
        }

        // Auto-retarget
        if (BattleConfig.Instance.AutoRetarget)
            AutoRetarget(command);

        // Skip if no valid targets remain
        if (command.Targets == null || command.Targets.Count == 0)
        {
            if (!command.IsDefend && !command.IsFlee) return;
        }

        var config = BattleConfig.Instance;
        float delay = config.ActionDelay;
        if (autoBattleActive) delay /= config.AutoBattleSpeedMultiplier;

        switch (command.Type)
        {
            case BattleCommandType.Attack:
                await ExecuteAttack(command, delay);
                break;

            case BattleCommandType.Magic:
            case BattleCommandType.Use:
                await ExecuteSpell(command, delay);
                break;

            case BattleCommandType.Item:
                await ExecuteItem(command, delay);
                break;

            case BattleCommandType.Defend:
                command.Actor.IsDefending = true;
                OnBattleMessage?.Invoke(command.Actor, $"{command.Actor.Name} defends.");
                await Awaitable.WaitForSecondsAsync(delay);
                break;

            case BattleCommandType.Flee:
                await ExecuteFlee(command, delay);
                break;

            case BattleCommandType.EnemyAction:
                if (command.Spell != null)
                    await ExecuteSpell(command, delay);
                else if (command.Targets != null && command.Targets.Count > 0)
                    await ExecuteAttack(command, delay);
                break;
        }
    }

    async Awaitable ExecuteAttack(BattleCommand command, float delay)
    {
        var results = DamageCalculator.CalculatePhysicalAttack(command.Actor, command.Targets);
        OnActionExecuted?.Invoke(command, results);
        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Hit);

        foreach (var result in results)
        {
            ApplyActionResult(command.Actor, result);
            await Awaitable.WaitForSecondsAsync(delay * 0.5f);
        }

        await Awaitable.WaitForSecondsAsync(delay);
    }

    async Awaitable ExecuteSpell(BattleCommand command, float delay)
    {
        var spell = command.Spell;
        if (spell == null) return;

        // Silence check (at execution time per FF1 PR rules)
        if (command.Actor.IsSilenced && command.Type != BattleCommandType.Use)
        {
            OnBattleMessage?.Invoke(command.Actor, "Silenced!");
            await Awaitable.WaitForSecondsAsync(delay);
            return;
        }

        // MP cost (Use commands are free)
        if (command.Type == BattleCommandType.Magic && command.Actor.IsPartyMember)
        {
            if (command.Actor.PartyMember.CurrentMP < spell.MPCost)
            {
                OnBattleMessage?.Invoke(command.Actor, "Not enough MP!");
                await Awaitable.WaitForSecondsAsync(delay);
                return;
            }
            command.Actor.PartyMember.CurrentMP -= spell.MPCost;
        }

        OnBattleMessage?.Invoke(command.Actor, $"{command.Actor.Name} casts {spell.SpellName}!");
        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.MagicCast);

        var results = DamageCalculator.CalculateSpell(command.Actor, spell, command.Targets);
        OnActionExecuted?.Invoke(command, results);

        foreach (var result in results)
        {
            ApplyActionResult(command.Actor, result);
            await Awaitable.WaitForSecondsAsync(delay * 0.5f);
        }

        await Awaitable.WaitForSecondsAsync(delay);
    }

    async Awaitable ExecuteItem(BattleCommand command, float delay)
    {
        var item = command.Item;
        if (item == null || command.Targets == null || command.Targets.Count == 0) return;

        OnBattleMessage?.Invoke(command.Actor, $"{command.Actor.Name} uses {item.ItemName}!");

        foreach (var target in command.Targets)
        {
            var result = DamageCalculator.CalculateItemUse(command.Actor, item, target);
            ApplyActionResult(command.Actor, result);
        }

        // Consume item
        GameManager.Instance?.InventoryManager?.RemoveItem(item);

        GameManager.Instance?.Audio?.PlaySFX(
            item.EffectType == ItemEffectType.HealHP || item.EffectType == ItemEffectType.HealMP
                ? SoundEffect.Heal : SoundEffect.Hit);

        await Awaitable.WaitForSecondsAsync(delay);
    }

    async Awaitable ExecuteFlee(BattleCommand command, float delay)
    {
        if (isBossEncounter)
        {
            OnBattleMessage?.Invoke(command.Actor, "Can't flee!");
            return;
        }

        float partyAvg = GetAverageLevel(PartyActors);
        float enemyAvg = GetAverageLevel(EnemyActors);
        bool success = DamageCalculator.RollFlee(partyAvg, enemyAvg);

        OnFleeAttempt?.Invoke(success);

        if (success)
        {
            GameManager.Instance?.Audio?.PlaySFX(SoundEffect.FleeSuccess);
            OnBattleMessage?.Invoke(null, "Fled successfully!");
            await Awaitable.WaitForSecondsAsync(delay);
            SetPhase(BattlePhase.Fleeing);
            await EndBattle();
        }
        else
        {
            GameManager.Instance?.Audio?.PlaySFX(SoundEffect.FleeFail);
            OnBattleMessage?.Invoke(null, "Couldn't escape!");
            await Awaitable.WaitForSecondsAsync(delay);

            // Failed flee: discard all remaining party commands
            turnCommands.RemoveAll(c => c.Actor.IsPartyMember && c != command);
        }
    }

    void ApplyActionResult(BattleActor actor, ActionResult result)
    {
        if (result?.Target == null) return;
        var target = result.Target;

        if (result.IsHealing)
        {
            if (result.Message == "MP" && target.IsPartyMember)
            {
                // MP restore
                target.PartyMember.CurrentMP = Mathf.Min(
                    target.PartyMember.CurrentMP + result.TotalDamage,
                    target.PartyMember.MaxMP);
            }
            else
            {
                target.Heal(result.TotalDamage);
            }
            OnHealing?.Invoke(target, result.TotalDamage);
        }
        else if (result.TotalDamage > 0)
        {
            int damage = result.TotalDamage;
            if (godMode && target.IsPartyMember) damage = 0;
            target.TakeDamage(damage);
            OnDamageDealt?.Invoke(target, damage);

            if (result.IsCritical)
                GameManager.Instance?.Audio?.PlaySFX(SoundEffect.CriticalHit);

            if (!target.IsAlive)
            {
                OnActorDefeated?.Invoke(target);
                GameManager.Instance?.Audio?.PlaySFX(
                    target.IsEnemy ? SoundEffect.EnemyDeath : SoundEffect.CharacterDeath);
            }
        }
        else if (!result.Hit && result.Message != null)
        {
            OnBattleMessage?.Invoke(target, result.Message);
            if (result.Message == "Miss")
                GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Miss);
        }

        // Status effects
        if (result.StatusInflicted != StatusEffectFlags.None)
        {
            target.ApplyStatus(result.StatusInflicted);
            GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Debuff);
        }
        if (result.StatusCured != StatusEffectFlags.None)
        {
            target.CureStatus(result.StatusCured);
            // Special: Revive via cure of KO
            if ((result.StatusCured & StatusEffectFlags.KO) != 0 && result.IsHealing)
                target.Revive(result.TotalDamage > 0 ? result.TotalDamage * 100 / target.MaxHP : 25);
        }

        // Buff/debuff (already applied in DamageCalculator)
        if (result.BuffApplied != null)
            GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Buff);
    }

    async Awaitable ApplyEndOfTurnEffects()
    {
        // Poison tick
        foreach (var actor in AllActors)
        {
            if (!actor.IsAlive) continue;
            if (actor.IsPoisoned)
            {
                int damage = BattleConfig.Instance.PoisonBattleDamage;
                if (godMode && actor.IsPartyMember) damage = 0;
                actor.TakeDamage(damage);
                OnDamageDealt?.Invoke(actor, damage);

                if (!actor.IsAlive)
                    OnActorDefeated?.Invoke(actor);
            }
        }

        await Awaitable.WaitForSecondsAsync(0.1f);
    }

    // --- Auto-retarget ---

    void AutoRetarget(BattleCommand command)
    {
        if (command.Targets == null || command.Targets.Count == 0) return;

        // For AoE, just filter to living targets
        if (command.Targets.Count > 1)
        {
            command.Targets.RemoveAll(t => !t.IsAlive);
            return;
        }

        // Single target: if dead, pick a new one from same side
        var target = command.Targets[0];
        if (target.IsAlive) return;

        var pool = target.IsEnemy ? GetLivingEnemies() : GetLivingParty();
        if (pool.Count > 0)
        {
            var newTarget = pool[UnityEngine.Random.Range(0, pool.Count)];
            command.Targets[0] = newTarget;
            command.Target = newTarget;
        }
        else
        {
            command.Targets.Clear();
        }
    }

    // --- Battle End ---

    bool CheckBattleEnd()
    {
        if (AllEnemiesDead())
        {
            SetPhase(BattlePhase.Victory);
            _ = HandleVictory();
            return true;
        }
        if (AllPartyDead())
        {
            SetPhase(BattlePhase.Defeat);
            OnDefeat?.Invoke();
            return true;
        }
        return false;
    }

    async Awaitable HandleVictory()
    {
        GameManager.Instance?.Audio?.PlayBGM(MusicTrack.BattleVictory);

        // Calculate rewards
        int totalEXP = 0;
        int totalGil = 0;
        var drops = new List<ItemDrop>();

        foreach (var enemy in EnemyActors)
        {
            if (enemy.EnemyData != null)
            {
                totalEXP += enemy.EnemyData.EXPReward;
                totalGil += enemy.EnemyData.GilReward;

                if (enemy.EnemyData.DropTable != null)
                {
                    foreach (var drop in enemy.EnemyData.DropTable)
                    {
                        if (drop.Item != null && UnityEngine.Random.value < drop.DropRate)
                            drops.Add(drop);
                    }
                }
            }
        }

        // Apply rewards
        GameManager.Instance?.InventoryManager?.AddGil(totalGil);
        foreach (var drop in drops)
            GameManager.Instance?.InventoryManager?.AddItem(drop.Item);

        // EXP + level-ups handled by PartyManager
        var levelUps = GameManager.Instance?.PartyManager?.AddEXP(totalEXP);

        OnVictory?.Invoke(totalEXP, totalGil, drops);

        // Wait for victory UI to be dismissed (BattleVictoryUI handles this)
        // BattleVictoryUI calls EndBattle when done
    }

    public async Awaitable EndBattle()
    {
        // Sync final state back to party members
        foreach (var actor in PartyActors)
            actor.SyncToPartyMember();

        // Fade out
        if (FadeOverlay.Instance != null)
            await FadeOverlay.Instance.FadeOut(0.3f);

        // Unload battle scene
        try { await GameManager.Instance.SceneLoader.UnloadScene("Battle"); }
        catch (Exception) { /* Scene may not exist if debug-started */ }

        // Restore state
        GameManager.Instance?.StateManager?.ChangeState(GameState.Exploration);
        GameManager.Instance?.InputManager?.EnableGameplay();
        GameManager.Instance?.Audio?.ResumeBGM();

        if (FadeOverlay.Instance != null)
            await FadeOverlay.Instance.FadeIn(0.3f);

        SetPhase(BattlePhase.Inactive);
        onBattleComplete?.Invoke();
        onBattleComplete = null;

        Cleanup();
    }

    // --- Initialization ---

    void InitializeActors(EncounterFormation formation)
    {
        PartyActors.Clear();
        EnemyActors.Clear();
        AllActors.Clear();

        var pm = GameManager.Instance?.PartyManager;
        if (pm != null)
        {
            for (int i = 0; i < 4; i++)
            {
                var member = pm.GetMember(i);
                if (member != null)
                {
                    var actor = BattleActor.FromPartyMember(member, i);
                    PartyActors.Add(actor);
                    AllActors.Add(actor);
                }
            }
        }

        int enemyIndex = 0;
        if (formation.Groups != null)
        {
            foreach (var group in formation.Groups)
            {
                for (int i = 0; i < group.Count; i++)
                {
                    var actor = BattleActor.FromEnemy(group.Enemy, enemyIndex++);
                    EnemyActors.Add(actor);
                    AllActors.Add(actor);
                }
            }
        }
    }

    void Cleanup()
    {
        PartyActors.Clear();
        EnemyActors.Clear();
        AllActors.Clear();
        turnCommands.Clear();
        currentFormation = null;
    }


    // --- Helpers ---

    void SetPhase(BattlePhase phase)
    {
        CurrentPhase = phase;
        OnPhaseChanged?.Invoke(phase);
    }

    public List<BattleActor> GetLivingEnemies()
    {
        var result = new List<BattleActor>();
        foreach (var e in EnemyActors)
            if (e.IsAlive) result.Add(e);
        return result;
    }

    public List<BattleActor> GetLivingParty()
    {
        var result = new List<BattleActor>();
        foreach (var p in PartyActors)
            if (p.IsAlive) result.Add(p);
        return result;
    }

    bool AllEnemiesDead()
    {
        foreach (var e in EnemyActors)
            if (e.IsAlive) return false;
        return true;
    }

    bool AllPartyDead()
    {
        foreach (var p in PartyActors)
            if (p.IsAlive) return false;
        return true;
    }

    float GetAverageLevel(List<BattleActor> actors)
    {
        int total = 0; int count = 0;
        foreach (var a in actors)
        {
            if (a.IsAlive)
            {
                total += a.IsPartyMember ? a.PartyMember.Level : 1;
                count++;
            }
        }
        return count > 0 ? (float)total / count : 1;
    }
}
