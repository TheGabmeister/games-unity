using System;

public enum EquipmentSlot { Weapon, Shield, Helmet, Armor }

public enum WeaponType { Sword, Dagger, Axe, Hammer, Staff, Nunchaku, Katana }
public enum ArmorType { Shield, LightArmor, HeavyArmor, Robe, Helmet, HeavyHelmet, Hat, Armlet, Gloves }

public enum MagicSchool { White, Black }

public enum Element { Fire, Ice, Lightning, Earth, Water, Wind, Poison, Holy }

public enum ElementalAffinity { Normal, Weak, Resist, Null, Absorb }

public enum SpellTarget { SingleEnemy, AllEnemies, SingleAlly, AllAllies, Self }

public enum SpellEffectType { Damage, Heal, Buff, Debuff, StatusInflict, StatusCure }

public enum EnemyCategory { Normal, Boss, Undead, Dragon }

[Flags]
public enum StatusEffectFlags
{
    None      = 0,
    KO        = 1 << 0,
    Poison    = 1 << 1,
    Stone     = 1 << 2,
    Blind     = 1 << 3,
    Silence   = 1 << 4,
    Sleep     = 1 << 5,
    Paralysis = 1 << 6,
    Confuse   = 1 << 7,
    Mini      = 1 << 8,
}
