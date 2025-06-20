using EventBus;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    int _rupees = 0;
    int _arrows = 0;
    int _bombs = 0;

    private void OnEnable()
    {
        Bus<E_Inventory_Add>.Add(AddItem);
    }
    private void OnDisable()
    {
        Bus<E_Inventory_Add>.Remove(AddItem);
    }

    private void Start()
    {
        
    }

    void AddItem(E_Inventory_Add message)
    {
        switch(message.value.type)
        {
            case ItemType.Rupee:
                _rupees += message.amount;
                break;
            case ItemType.Arrow:
                _arrows += message.amount;
                break;
            case ItemType.Bomb:
                _bombs += message.amount;
                break;
        }
    }
}
