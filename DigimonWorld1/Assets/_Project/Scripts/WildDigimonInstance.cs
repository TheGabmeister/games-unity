using System.Collections.Generic;
using UnityEngine;

public class WildDigimonInstance
{
    public DigimonSpeciesData Species { get; private set; }
    public int CurrentHP { get; private set; }
    public int MaxHP { get; private set; }
    public int CurrentMP { get; private set; }
    public int MaxMP { get; private set; }
    public int Offense { get; private set; }
    public int Defense { get; private set; }
    public int Speed { get; private set; }
    public int Brains { get; private set; }
    public List<TechniqueData> Techniques { get; private set; }
    public bool IsAlive => CurrentHP > 0;

    public WildDigimonInstance(DigimonSpeciesData species, float statScale)
    {
        Species = species;
        MaxHP = (int)(species.BaseHP * statScale);
        MaxMP = (int)(species.BaseMP * statScale);
        Offense = (int)(species.BaseOffense * statScale);
        Defense = (int)(species.BaseDefense * statScale);
        Speed = (int)(species.BaseSpeed * statScale);
        Brains = (int)(species.BaseBrains * statScale);
        CurrentHP = MaxHP;
        CurrentMP = MaxMP;

        Techniques = new List<TechniqueData>();
        if (species.LearnableTechniques != null)
        {
            int count = Mathf.Min(3, species.LearnableTechniques.Length);
            for (int i = 0; i < count; i++)
            {
                if (species.LearnableTechniques[i] != null)
                    Techniques.Add(species.LearnableTechniques[i]);
            }
        }
    }

    public void TakeDamage(int amount)
    {
        CurrentHP = Mathf.Max(0, CurrentHP - amount);
    }

    public bool SpendMP(int amount)
    {
        if (CurrentMP < amount) return false;
        CurrentMP -= amount;
        return true;
    }

    public void Heal(int amount)
    {
        CurrentHP = Mathf.Min(CurrentHP + amount, MaxHP);
    }
}
