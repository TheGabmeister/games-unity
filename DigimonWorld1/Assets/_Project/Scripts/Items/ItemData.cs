using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "DigimonWorld/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string _itemName;
    [SerializeField] private ItemCategory _category;
    [TextArea(1, 3)]
    [SerializeField] private string _description;

    [Header("Economy")]
    [SerializeField] private int _buyPrice;
    [SerializeField] private int _sellPrice;

    [Header("Inventory")]
    [SerializeField] private int _maxStack = 10;

    [Header("Effects")]
    [SerializeField] private int _hungerReduction;
    [SerializeField] private int _weightGain;
    [SerializeField] private int _hpRestore;
    [SerializeField] private int _mpRestore;
    [SerializeField] private int _happinessChange;
    [SerializeField] private int _disciplineChange;
    [SerializeField] private int _tirednessReduction;

    public string ItemName => _itemName;
    public ItemCategory Category => _category;
    public string Description => _description;
    public int BuyPrice => _buyPrice;
    public int SellPrice => _sellPrice;
    public int MaxStack => _maxStack;
    public int HungerReduction => _hungerReduction;
    public int WeightGain => _weightGain;
    public int HpRestore => _hpRestore;
    public int MpRestore => _mpRestore;
    public int HappinessChange => _happinessChange;
    public int DisciplineChange => _disciplineChange;
    public int TirednessReduction => _tirednessReduction;
}
