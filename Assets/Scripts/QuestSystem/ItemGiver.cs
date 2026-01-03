using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ItemGiver : ChainedInteractable
{
    public string itemName;

    public override void Interact()
    {
        GameManager.Instance.AddInventoryItem(itemName, 1);
        CallNext();
    }
}



