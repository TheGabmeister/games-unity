public class StatusEffect
{
    public StatusEffectType Type;
    public int RemainingTurns;

    public StatusEffect(StatusEffectType type, int duration)
    {
        Type = type;
        RemainingTurns = duration;
    }
}
