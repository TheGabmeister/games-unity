using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleSystem : Singleton<BattleSystem>
{
    private bool _inBattle;
    private bool _executingTurn;
    private DigimonInstance _partner;
    private WildDigimonInstance _enemy;
    private StatusEffect _partnerStatusEffect;
    private StatusEffect _enemyStatusEffect;
    private Action<BattleResult> _onBattleEnd;

    public bool InBattle => _inBattle;
    public DigimonInstance Partner => _partner;
    public WildDigimonInstance Enemy => _enemy;

    public void StartBattle(DigimonInstance partner, WildDigimonInstance enemy, Action<BattleResult> onEnd)
    {
        if (_inBattle) return;

        _inBattle = true;
        _executingTurn = false;
        _partner = partner;
        _enemy = enemy;
        _partnerStatusEffect = null;
        _enemyStatusEffect = null;
        _onBattleEnd = onEnd;

        InputManager.Instance.SetPlayerInputEnabled(false);
        TimeSystem.Instance.SetPaused(true);
        HUD.Instance.SetVisible(false);

        BattleUI.Instance.Show(_partner, _enemy);
        BattleUI.Instance.AddLogLine($"A wild {_enemy.Species.SpeciesName} appeared!");
        BattleUI.Instance.ShowCommandMenu();
    }

    public void SelectAttack()
    {
        if (!_inBattle || _executingTurn) return;
        ExecutePlayerTurn(new BattleAction { Type = BattleActionType.Attack });
    }

    public void SelectTechnique(TechniqueData technique)
    {
        if (!_inBattle || _executingTurn) return;

        if (!_partner.SpendMP(technique.MpCost))
        {
            BattleUI.Instance.AddLogLine("Not enough MP!");
            return;
        }

        ExecutePlayerTurn(new BattleAction { Type = BattleActionType.Technique, Technique = technique });
    }

    public void SelectItem(ItemData item)
    {
        if (!_inBattle || _executingTurn) return;
        ExecutePlayerTurn(new BattleAction { Type = BattleActionType.Item, Item = item });
    }

    public void SelectFlee()
    {
        if (!_inBattle || _executingTurn) return;
        EndBattle(BattleResult.Fled);
    }

    public void SelectAuto()
    {
        if (!_inBattle || _executingTurn) return;

        TechniqueData best = null;
        foreach (var tech in _partner.KnownTechniques)
        {
            if (tech.MpCost <= _partner.CurrentMP)
            {
                if (best == null || tech.Power > best.Power)
                    best = tech;
            }
        }

        if (best != null)
            SelectTechnique(best);
        else
            SelectAttack();
    }

    private async void ExecutePlayerTurn(BattleAction action)
    {
        _executingTurn = true;
        BattleUI.Instance.HideCommandMenu();

        if (action.Type != BattleActionType.Flee && action.Type != BattleActionType.Item)
            action = ApplyObedienceCheck(action);

        if (action.Type != BattleActionType.DoNothing)
        {
            bool skipTurn = ProcessStatusEffects(ref _partnerStatusEffect, _partner.Species.SpeciesName, true);
            if (skipTurn)
            {
                action = new BattleAction { Type = BattleActionType.DoNothing };
            }
        }

        ExecuteAction(action, true);
        BattleUI.Instance.RefreshStats(_partner, _enemy);

        await Awaitable.WaitForSecondsAsync(0.5f);

        if (!_enemy.IsAlive)
        {
            BattleUI.Instance.AddLogLine($"{_enemy.Species.SpeciesName} was defeated!");
            BattleUI.Instance.RefreshStats(_partner, _enemy);
            await Awaitable.WaitForSecondsAsync(1f);
            EndBattle(BattleResult.Win);
            return;
        }

        await ExecuteEnemyTurn();
    }

    private async Awaitable ExecuteEnemyTurn()
    {
        BattleAction enemyAction = EnemyBattleAI.ChooseAction(_enemy);

        if (enemyAction.Type == BattleActionType.Flee)
        {
            BattleUI.Instance.AddLogLine($"{_enemy.Species.SpeciesName} fled!");
            BattleUI.Instance.RefreshStats(_partner, _enemy);
            await Awaitable.WaitForSecondsAsync(1f);
            EndBattle(BattleResult.Win);
            return;
        }

        bool skipTurn = ProcessStatusEffects(ref _enemyStatusEffect, _enemy.Species.SpeciesName, false);
        if (!skipTurn)
        {
            if (enemyAction.Type == BattleActionType.Technique && !_enemy.SpendMP(enemyAction.Technique.MpCost))
                enemyAction = new BattleAction { Type = BattleActionType.Attack };

            ExecuteAction(enemyAction, false);
        }

        BattleUI.Instance.RefreshStats(_partner, _enemy);

        await Awaitable.WaitForSecondsAsync(0.5f);

        if (!_partner.IsAlive)
        {
            BattleUI.Instance.AddLogLine($"{_partner.Species.SpeciesName} was defeated...");
            BattleUI.Instance.RefreshStats(_partner, _enemy);
            await Awaitable.WaitForSecondsAsync(1f);
            EndBattle(BattleResult.Lose);
            return;
        }

        _executingTurn = false;
        BattleUI.Instance.ShowCommandMenu();
    }

    private void ExecuteAction(BattleAction action, bool isPartnerAction)
    {
        string attackerName = isPartnerAction ? _partner.Species.SpeciesName : _enemy.Species.SpeciesName;
        string defenderName = isPartnerAction ? _enemy.Species.SpeciesName : _partner.Species.SpeciesName;

        switch (action.Type)
        {
            case BattleActionType.Attack:
            {
                int atkOffense = isPartnerAction ? _partner.Offense : _enemy.Offense;
                int defDefense = isPartnerAction ? _enemy.Defense : _partner.Defense;
                int damage = BattleFormulas.CalculateBasicAttackDamage(atkOffense, defDefense);

                if (isPartnerAction)
                    _enemy.TakeDamage(damage);
                else
                    _partner.TakeDamage(damage);

                BattleUI.Instance.AddLogLine($"{attackerName} attacks {defenderName} for {damage} damage!");
                break;
            }

            case BattleActionType.Technique:
            {
                TechniqueData tech = action.Technique;
                int atkOffense = isPartnerAction ? _partner.Offense : _enemy.Offense;
                int defDefense = isPartnerAction ? _enemy.Defense : _partner.Defense;
                DigimonAttribute defAttribute = isPartnerAction
                    ? _enemy.Species.Attribute
                    : _partner.Species.Attribute;

                if (tech.Accuracy < 100 && UnityEngine.Random.Range(0, 100) >= tech.Accuracy)
                {
                    BattleUI.Instance.AddLogLine($"{attackerName} used {tech.TechniqueName} but missed!");
                    break;
                }

                float typeMult = BattleFormulas.GetTypeMultiplier(tech.Category, defAttribute);
                int damage = BattleFormulas.CalculateDamage(atkOffense, tech.Power, defDefense, typeMult);

                if (isPartnerAction)
                    _enemy.TakeDamage(damage);
                else
                    _partner.TakeDamage(damage);

                string effectiveness = typeMult > 1f ? " It's super effective!" : typeMult < 1f ? " It's not very effective." : "";
                BattleUI.Instance.AddLogLine($"{attackerName} used {tech.TechniqueName} for {damage} damage!{effectiveness}");

                TryApplyStatusEffect(tech, isPartnerAction, defenderName);
                break;
            }

            case BattleActionType.Item:
            {
                if (action.Item != null)
                {
                    Inventory.Instance.UseItem(action.Item);
                    BattleUI.Instance.AddLogLine($"Used {action.Item.ItemName}!");
                }
                break;
            }

            case BattleActionType.DoNothing:
                break;
        }
    }

    private void TryApplyStatusEffect(TechniqueData tech, bool isPartnerAction, string targetName)
    {
        if (tech.StatusEffect == StatusEffectType.None || tech.StatusChance <= 0) return;
        if (UnityEngine.Random.Range(0, 100) >= tech.StatusChance) return;

        StatusEffect existing = isPartnerAction ? _enemyStatusEffect : _partnerStatusEffect;
        if (existing != null) return;

        int duration = tech.StatusEffect == StatusEffectType.Poison ? 5 : 3;
        StatusEffect effect = new StatusEffect(tech.StatusEffect, duration);

        if (isPartnerAction)
            _enemyStatusEffect = effect;
        else
            _partnerStatusEffect = effect;

        BattleUI.Instance.AddLogLine($"{targetName} is now {GetStatusName(tech.StatusEffect)}!");
    }

    private bool ProcessStatusEffects(ref StatusEffect effect, string name, bool isPartner)
    {
        if (effect == null) return false;

        switch (effect.Type)
        {
            case StatusEffectType.Poison:
            {
                int maxHP = isPartner ? _partner.MaxHP : _enemy.MaxHP;
                int poisonDmg = Mathf.Max(1, maxHP / 16);
                if (isPartner)
                    _partner.TakeDamage(poisonDmg);
                else
                    _enemy.TakeDamage(poisonDmg);
                BattleUI.Instance.AddLogLine($"{name} took {poisonDmg} poison damage!");
                break;
            }

            case StatusEffectType.Paralysis:
                if (UnityEngine.Random.value < 0.4f)
                {
                    BattleUI.Instance.AddLogLine($"{name} is paralyzed and can't move!");
                    TickStatusEffect(ref effect);
                    return true;
                }
                break;

            case StatusEffectType.Sleep:
                BattleUI.Instance.AddLogLine($"{name} is asleep!");
                TickStatusEffect(ref effect);
                return true;

            case StatusEffectType.Confusion:
                if (UnityEngine.Random.value < 0.5f)
                {
                    int maxHP = isPartner ? _partner.MaxHP : _enemy.MaxHP;
                    int confusionDmg = Mathf.Max(1, maxHP / 20);
                    if (isPartner)
                        _partner.TakeDamage(confusionDmg);
                    else
                        _enemy.TakeDamage(confusionDmg);
                    BattleUI.Instance.AddLogLine($"{name} hurt itself in confusion for {confusionDmg} damage!");
                    TickStatusEffect(ref effect);
                    return true;
                }
                break;
        }

        TickStatusEffect(ref effect);
        return false;
    }

    private void TickStatusEffect(ref StatusEffect effect)
    {
        if (effect == null) return;
        effect.RemainingTurns--;
        if (effect.RemainingTurns <= 0)
        {
            BattleUI.Instance.AddLogLine($"The {GetStatusName(effect.Type)} wore off!");
            effect = null;
        }
    }

    private BattleAction ApplyObedienceCheck(BattleAction intended)
    {
        float obedience = Mathf.Clamp(_partner.Brains / 100f + _partner.Discipline / 200f, 0.1f, 1f);
        if (UnityEngine.Random.value < obedience)
            return intended;

        float roll = UnityEngine.Random.value;

        if (roll < 0.1f)
            return intended;

        if (roll < 0.3f)
        {
            BattleUI.Instance.AddLogLine($"{_partner.Species.SpeciesName} ignored your command!");
            return new BattleAction { Type = BattleActionType.DoNothing };
        }

        if (roll < 0.6f)
        {
            List<TechniqueData> affordable = new List<TechniqueData>();
            foreach (var tech in _partner.KnownTechniques)
            {
                if (tech.MpCost <= _partner.CurrentMP)
                    affordable.Add(tech);
            }

            if (affordable.Count > 0)
            {
                TechniqueData picked = affordable[UnityEngine.Random.Range(0, affordable.Count)];
                _partner.SpendMP(picked.MpCost);
                BattleUI.Instance.AddLogLine($"{_partner.Species.SpeciesName} disobeyed and used {picked.TechniqueName}!");
                return new BattleAction { Type = BattleActionType.Technique, Technique = picked };
            }
        }

        BattleUI.Instance.AddLogLine($"{_partner.Species.SpeciesName} disobeyed and attacked on its own!");
        return new BattleAction { Type = BattleActionType.Attack };
    }

    private void EndBattle(BattleResult result)
    {
        _partnerStatusEffect = null;
        _enemyStatusEffect = null;
        _executingTurn = false;

        string resultText = result switch
        {
            BattleResult.Win => "Victory!",
            BattleResult.Lose => "Defeated...",
            BattleResult.Fled => "Fled from battle.",
            _ => ""
        };
        BattleUI.Instance.AddLogLine(resultText);

        TimeSystem.Instance.SetPaused(false);
        HUD.Instance.SetVisible(true);
        InputManager.Instance.SetPlayerInputEnabled(true);
        BattleUI.Instance.Hide();

        _inBattle = false;

        var callback = _onBattleEnd;
        _onBattleEnd = null;
        _partner = null;
        _enemy = null;
        callback?.Invoke(result);
    }

    private string GetStatusName(StatusEffectType type)
    {
        return type switch
        {
            StatusEffectType.Poison => "Poisoned",
            StatusEffectType.Paralysis => "Paralyzed",
            StatusEffectType.Sleep => "Asleep",
            StatusEffectType.Confusion => "Confused",
            _ => ""
        };
    }

    public string GetPartnerStatusText()
    {
        if (_partnerStatusEffect == null) return "";
        return $"[{GetStatusName(_partnerStatusEffect.Type)}]";
    }

    public string GetEnemyStatusText()
    {
        if (_enemyStatusEffect == null) return "";
        return $"[{GetStatusName(_enemyStatusEffect.Type)}]";
    }
}
