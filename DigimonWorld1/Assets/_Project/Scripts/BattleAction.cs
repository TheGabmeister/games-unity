public enum BattleActionType
{
    Attack,
    Technique,
    Item,
    Flee,
    DoNothing
}

public struct BattleAction
{
    public BattleActionType Type;
    public TechniqueData Technique;
    public ItemData Item;
}
