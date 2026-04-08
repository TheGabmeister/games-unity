using System.Collections.Generic;
using UnityEngine;

/// Implements all FF1 battle formulas: physical damage, magical damage,
/// healing, elemental interactions, status effects, and buff/debuff application.
public static class DamageCalculator
{
    // --- Physical Attack ---

    public static List<ActionResult> CalculatePhysicalAttack(BattleActor attacker, List<BattleActor> targets)
    {
        var results = new List<ActionResult>();
        var config = BattleConfig.Instance;

        foreach (var target in targets)
        {
            var result = new ActionResult { Target = target };

            if (!target.IsAlive)
            {
                result.Hit = false;
                result.Message = "Ineffective";
                results.Add(result);
                continue;
            }

            int hitCount = attacker.HitCount;
            int attackPower = attacker.Attack;
            int defense = target.Defense;
            if (target.IsDefending) defense *= 2;

            // Blind reduces hit rate
            int hitRate = attacker.Accuracy;
            if (attacker.IsBlind) hitRate = hitRate / 2;

            int hitChance = config.BasePhysicalHitChance + hitRate - target.Evasion;
            hitChance = Mathf.Clamp(hitChance, 0, config.HitRollMax);

            var perHitDamages = new List<int>();
            int totalDamage = 0;
            bool anyCrit = false;

            for (int i = 0; i < hitCount; i++)
            {
                int roll = Random.Range(0, config.HitRollMax);
                if (roll >= hitChance)
                {
                    perHitDamages.Add(0); // miss on this hit
                    continue;
                }

                bool isCrit = Random.Range(0, 100) < attacker.CritRate;
                if (isCrit) anyCrit = true;

                int damage = isCrit ? attackPower : Mathf.Max(1, attackPower - defense);
                perHitDamages.Add(damage);
                totalDamage += damage;
            }

            // Apply elemental modifiers from weapon
            float elementMod = GetElementalModifier(attacker.WeaponElements, target);
            if (elementMod < 0)
            {
                // Absorb: heal instead
                result.IsHealing = true;
                totalDamage = Mathf.RoundToInt(Mathf.Abs(totalDamage * elementMod));
            }
            else
            {
                totalDamage = Mathf.RoundToInt(totalDamage * elementMod);
            }

            result.Hit = totalDamage > 0 || perHitDamages.Exists(d => d > 0);
            result.IsCritical = anyCrit;
            result.TotalDamage = totalDamage;
            result.PerHitDamage = perHitDamages.ToArray();

            if (!result.Hit)
                result.Message = "Miss";

            // Wake up sleeping targets
            if (result.Hit && (target.StatusEffects & StatusEffectFlags.Sleep) != 0)
                result.StatusCured = StatusEffectFlags.Sleep;

            // Cure confusion from ally hit
            if (result.Hit && attacker.IsPartyMember && target.IsPartyMember
                && (target.StatusEffects & StatusEffectFlags.Confuse) != 0)
                result.StatusCured |= StatusEffectFlags.Confuse;

            result.TargetKilled = target.CurrentHP - totalDamage <= 0 && !result.IsHealing;

            results.Add(result);
        }

        return results;
    }

    // --- Magical Attack / Healing ---

    public static List<ActionResult> CalculateSpell(BattleActor caster, SpellData spell, List<BattleActor> targets)
    {
        var results = new List<ActionResult>();
        var config = BattleConfig.Instance;

        foreach (var target in targets)
        {
            var result = new ActionResult { Target = target };

            // Check spell effects
            bool isDamage = HasEffect(spell, SpellEffectType.Damage);
            bool isHeal = HasEffect(spell, SpellEffectType.Heal);
            bool isBuff = HasEffect(spell, SpellEffectType.Buff);
            bool isDebuff = HasEffect(spell, SpellEffectType.Debuff);
            bool isStatusInflict = HasEffect(spell, SpellEffectType.StatusInflict);
            bool isStatusCure = HasEffect(spell, SpellEffectType.StatusCure);

            // Undead healing inversion
            bool undeadInversion = isHeal && target.IsUndead;
            if (undeadInversion)
            {
                isDamage = true;
                isHeal = false;
            }

            if (isDamage)
            {
                // Magic hit check
                int hitChance = config.BaseMagicHitChance + spell.SpellAccuracy - target.MagicEvasion;
                hitChance = Mathf.Clamp(hitChance, 0, config.HitRollMax);
                int roll = Random.Range(0, config.HitRollMax);

                if (roll < hitChance || undeadInversion) // undead inversion always hits
                {
                    int baseDamage = spell.BasePower + (caster.Intellect / 2)
                        + Random.Range(0, Mathf.Max(1, spell.BasePower));
                    int resistance = target.MagicDefense;

                    float elementMod = GetElementalModifier(
                        spell.Element != default ? new[] { spell.Element } : System.Array.Empty<Element>(),
                        target);

                    if (elementMod < 0)
                    {
                        // Absorb
                        result.IsHealing = true;
                        result.TotalDamage = Mathf.Max(1, Mathf.RoundToInt(Mathf.Abs(baseDamage * elementMod)));
                    }
                    else if (elementMod == 0)
                    {
                        // Null
                        result.TotalDamage = 0;
                        result.Message = "Ineffective";
                    }
                    else
                    {
                        int finalDamage = Mathf.Max(1, baseDamage - resistance);
                        finalDamage = Mathf.RoundToInt(finalDamage * elementMod);
                        result.TotalDamage = Mathf.Max(1, finalDamage);
                    }
                    result.Hit = true;
                }
                else
                {
                    result.Hit = false;
                    result.Message = "Miss";
                }
            }

            if (isHeal && !undeadInversion)
            {
                int healAmount = spell.BasePower + (caster.Intellect / 2)
                    + Random.Range(0, Mathf.Max(1, spell.BasePower));
                result.TotalDamage = healAmount;
                result.IsHealing = true;
                result.Hit = true; // healing always hits
            }

            if (isStatusInflict)
            {
                if (target.HasStatusImmunity(spell.InflictsStatus))
                {
                    result.Message = "Ineffective";
                }
                else
                {
                    // Hit check for status spells
                    int hitChance = config.BaseMagicHitChance + spell.SpellAccuracy - target.MagicEvasion;
                    int roll = Random.Range(0, config.HitRollMax);
                    if (roll < hitChance)
                    {
                        result.StatusInflicted = spell.InflictsStatus;
                        result.Hit = true;
                    }
                    else
                    {
                        result.Message = "Miss";
                    }
                }
            }

            if (isStatusCure)
            {
                result.StatusCured = spell.CuresStatus;
                result.Hit = true;
            }

            if (isBuff && target.IsAlive)
            {
                result.BuffApplied = ResolveBuff(spell, target);
                result.Hit = true;
            }

            if (isDebuff && target.IsAlive)
            {
                // Debuffs need hit check
                int hitChance = config.BaseMagicHitChance + spell.SpellAccuracy - target.MagicEvasion;
                int roll = Random.Range(0, config.HitRollMax);
                if (roll < hitChance)
                {
                    result.BuffApplied = ResolveDebuff(spell, target);
                    result.Hit = true;
                }
                else
                {
                    result.Message = "Miss";
                }
            }

            result.TargetKilled = result.Hit && !result.IsHealing
                && target.CurrentHP - result.TotalDamage <= 0;

            results.Add(result);
        }

        return results;
    }

    // --- Item Usage ---

    public static ActionResult CalculateItemUse(BattleActor user, ItemData item, BattleActor target)
    {
        var result = new ActionResult { Target = target, Hit = true };

        // Undead healing inversion for items
        bool undeadInversion = (item.EffectType == ItemEffectType.HealHP
            || item.EffectType == ItemEffectType.Revive) && target.IsUndead;

        switch (item.EffectType)
        {
            case ItemEffectType.HealHP:
                if (undeadInversion)
                {
                    result.TotalDamage = item.Power;
                    result.IsHealing = false;
                    result.TargetKilled = target.CurrentHP - item.Power <= 0;
                }
                else
                {
                    result.TotalDamage = item.Power;
                    result.IsHealing = true;
                }
                break;

            case ItemEffectType.HealMP:
                // MP restore — handled specially by BattleManager
                result.TotalDamage = item.Power;
                result.IsHealing = true;
                result.Message = "MP";
                break;

            case ItemEffectType.CureStatus:
                result.StatusCured = item.CuresStatus;
                break;

            case ItemEffectType.Revive:
                if (undeadInversion)
                {
                    result.TotalDamage = target.MaxHP * item.Power / 100;
                    result.IsHealing = false;
                    result.TargetKilled = target.CurrentHP - result.TotalDamage <= 0;
                }
                else if ((target.StatusEffects & StatusEffectFlags.KO) != 0)
                {
                    result.StatusCured = StatusEffectFlags.KO;
                    result.TotalDamage = Mathf.Max(1, target.MaxHP * item.Power / 100);
                    result.IsHealing = true;
                }
                else
                {
                    result.Hit = false;
                    result.Message = "Ineffective";
                }
                break;

            case ItemEffectType.Damage:
                result.TotalDamage = item.Power;
                result.IsHealing = false;
                result.TargetKilled = target.CurrentHP - item.Power <= 0;
                break;

            case ItemEffectType.InflictStatus:
                if (target.HasStatusImmunity(item.InflictsStatus))
                    result.Message = "Ineffective";
                else
                    result.StatusInflicted = item.InflictsStatus;
                break;
        }

        return result;
    }

    // --- Elemental Modifier ---

    /// Returns multiplier: 2.0 = weak, 0.5 = resist, 0 = null, -1 = absorb
    static float GetElementalModifier(Element[] attackElements, BattleActor target)
    {
        if (attackElements == null || attackElements.Length == 0)
            return 1f;

        var config = BattleConfig.Instance;
        bool hasWeak = false;
        bool hasResist = false;
        bool hasAbsorb = false;
        bool hasNull = false;

        foreach (var element in attackElements)
        {
            var affinity = target.GetElementalAffinity(element);
            switch (affinity)
            {
                case ElementalAffinity.Weak: hasWeak = true; break;
                case ElementalAffinity.Resist: hasResist = true; break;
                case ElementalAffinity.Absorb: hasAbsorb = true; break;
                case ElementalAffinity.Null: hasNull = true; break;
            }
        }

        // FF1 rule: weakness takes priority over resistance
        if (hasAbsorb && !hasWeak) return -1f;
        if (hasWeak) return config.WeakMultiplier;
        if (hasNull && !hasWeak) return 0f;
        if (hasResist) return config.ResistMultiplier;
        return 1f;
    }

    // --- Flee ---

    public static bool RollFlee(float partyAvgLevel, float enemyAvgLevel)
    {
        var config = BattleConfig.Instance;
        int chance = Mathf.RoundToInt(partyAvgLevel * 2 + config.FleeBaseChance - enemyAvgLevel);
        int roll = Random.Range(0, 256);
        return roll < chance;
    }

    // --- Buff/Debuff Resolution ---

    static BuffChange ResolveBuff(SpellData spell, BattleActor target)
    {
        var change = new BuffChange();
        string lower = spell.SpellName?.ToLower() ?? "";

        if (lower.Contains("haste"))
        {
            target.Buffs.HasHaste = true;
            target.Buffs.HasSlow = false;
            change.Haste = true;
        }
        if (lower.Contains("temper") || lower.Contains("tmpr"))
        {
            int max = BattleConfig.Instance?.TemperMaxStacks ?? 4;
            if (target.Buffs.TemperStacks < max)
                target.Buffs.TemperStacks++;
            change.Temper = true;
        }
        if (lower.Contains("protect") || lower.Contains("fog"))
        {
            target.Buffs.ProtectStacks++;
            change.Protect = true;
        }
        if (lower.Contains("saber"))
        {
            target.Buffs.HasSaber = true;
            target.Buffs.SaberElement = spell.Element;
            change.Saber = true;
        }
        if (lower.Contains("nulfire") || lower.Contains("nul fire") || lower.Contains("barfire"))
        {
            target.Buffs.NulFire = true;
            change.NulFire = true;
        }
        if (lower.Contains("nulice") || lower.Contains("nul ice") || lower.Contains("barice"))
        {
            target.Buffs.NulIce = true;
            change.NulIce = true;
        }
        if (lower.Contains("nullit") || lower.Contains("nul lit") || lower.Contains("barlit"))
        {
            target.Buffs.NulLit = true;
            change.NulLit = true;
        }

        return change;
    }

    static BuffChange ResolveDebuff(SpellData spell, BattleActor target)
    {
        var change = new BuffChange();
        string lower = spell.SpellName?.ToLower() ?? "";

        if (lower.Contains("slow"))
        {
            target.Buffs.HasSlow = true;
            target.Buffs.HasHaste = false;
            change.Slow = true;
        }

        return change;
    }

    // --- Helpers ---

    static bool HasEffect(SpellData spell, SpellEffectType type)
    {
        if (spell.Effects == null) return false;
        foreach (var e in spell.Effects)
            if (e == type) return true;
        return false;
    }
}
