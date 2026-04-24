using UnityEngine;

public static class BattleFormulas
{
    public static float GetTypeMultiplier(TechniqueCategory category, DigimonAttribute defenderAttribute)
    {
        switch (category)
        {
            case TechniqueCategory.Fire:
                if (defenderAttribute == DigimonAttribute.Data) return 1.5f;
                if (defenderAttribute == DigimonAttribute.Vaccine) return 0.75f;
                return 1f;

            case TechniqueCategory.Air:
                if (defenderAttribute == DigimonAttribute.Virus) return 1.5f;
                if (defenderAttribute == DigimonAttribute.Data) return 0.75f;
                return 1f;

            case TechniqueCategory.Earth:
                if (defenderAttribute == DigimonAttribute.Vaccine) return 1.5f;
                if (defenderAttribute == DigimonAttribute.Virus) return 0.75f;
                return 1f;

            case TechniqueCategory.Water:
                if (defenderAttribute == DigimonAttribute.Virus) return 1.5f;
                if (defenderAttribute == DigimonAttribute.Vaccine) return 0.75f;
                return 1f;

            default:
                return 1f;
        }
    }

    public static int CalculateDamage(int attackerOffense, int techPower, int defenderDefense, float typeMultiplier)
    {
        float raw = (attackerOffense * techPower / 100f - defenderDefense / 2f) * typeMultiplier;
        return Mathf.Max(1, (int)raw);
    }

    public static int CalculateBasicAttackDamage(int attackerOffense, int defenderDefense)
    {
        return Mathf.Max(1, attackerOffense - defenderDefense / 2);
    }
}
