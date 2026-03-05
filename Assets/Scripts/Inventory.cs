using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;
using TMPro;
using System.Linq;

public class PointerHoverHandler : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    public Action onEnter;
    public Action onExit;

    public void OnPointerEnter(PointerEventData eventData)
    {
        onEnter?.Invoke();
        AudioManager.Instance.PlaySoundEffect("MenuHover");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onExit?.Invoke();
    }
}

public class Inventory : MonoBehaviour
{
    public Transform itemContainer;
    public TMP_Text itemDescriptionText;
    public void UpdateInventoryImages(List<InventoryItem> inventory)
    {
        foreach(Transform child in itemContainer){Destroy(child.gameObject);}
        //If in battle, only show items with gameActions. Out of battle, show all items.
        var inventory2 = inventory;
        if(GameObject.FindFirstObjectByType<BattleManager>() != null)
        {
            inventory2 = inventory.Where(item => item.gameAction != null).ToList();
        }
        foreach(var item in inventory2)
        {
            if(item.quantity == 0)continue;
            var itemGO = Instantiate(Resources.Load<GameObject>("InventoryItem"), itemContainer);
            var itemText = itemGO.GetComponentInChildren<TMPro.TMP_Text>();
            if(itemText != null)
            {
                itemText.text = $"{item.itemName} x{item.quantity}";
            }
            var itemImage = itemGO.transform.Find("Image").GetComponent<UnityEngine.UI.Image>();
            if(itemImage != null)
            {
                var sprite = Resources.Load<Sprite>($"Items/{item.itemName}");
                if(sprite != null)
                {
                    itemImage.sprite = sprite;
                }
            }
            var itemButton = itemGO.GetComponent<UnityEngine.UI.Button>();
            itemButton.onClick.AddListener(() => {
                UseItem(item);
            });
            //mouse enter to show description
            var hover = itemGO.AddComponent<PointerHoverHandler>();

            hover.onEnter = () =>{ShowItemDescription(item);};

            hover.onExit = () =>{HideItemDescription(item);};
        }

        

        void UseItem(InventoryItem item)
        {
            var menu = FindFirstObjectByType<Menu>();
            var battleManager = FindFirstObjectByType<BattleManager>();
            bool success = false;
            
            // Equipment handling (out-of-battle only)
            if(item is Equipment equipment && menu != null)
            {
                try
                {
                    menu.EquipItem(item);
                    success = true;  // EquipItem handles consumption internally
                }
                catch(System.Exception ex)
                {
                    Debug.LogError($"Error equipping '{item.itemName}': {ex.Message}");
                }
            }
            // Battle usage
            else if(item.gameAction != null && battleManager != null)
            {
               try
               {
                Debug.Log("Trying to use item in battle");
                item.gameAction.caller = battleManager.activeCombatant;
                   // If it's a targeting action, queue it for targeting
                   if(item.gameAction.targetType == TargetType.SingleAlly || 
                      item.gameAction.targetType == TargetType.SingleEnemy ||
                      item.gameAction.targetType == TargetType.Any)
                   {
                       var targetAction = new ChooseTargetsAction()
                       {
                           targetType = item.gameAction.targetType,
                           prompt = $"Choose target for {item.itemName}",
                           gameAction = item.gameAction,
                           caller = item.gameAction.caller
                       };
                       battleManager.actionQueue.Add(targetAction);
                       GameManager.Instance.ConsumeInventoryItem(item.itemName, true, 1);
                       UpdateInventoryImages(GameManager.Instance.inventory);
                       battleManager.HideInventory();
                   }
                   else
                   {
                       // Direct execution for non-targeted actions
                       battleManager.actionQueue.Add(item.gameAction);
                   }
                   success = true;
               }
               catch(System.Exception ex)
               {
                   Debug.LogError($"Error using '{item.itemName}' in battle: {ex.Message}");
               }
            }
            // Out-of-battle usage
            else if(item.outOfBattleAction != null && menu != null)
            {
                try
                {
                    Debug.Log("Out of battle item");
                    item.outOfBattleAction(menu);
                    success = true;
                    GameManager.Instance.ConsumeInventoryItem(item.itemName, true, 1);
                }
                catch(System.Exception ex)
                {
                    Debug.LogError($"Error using '{item.itemName}' out of battle: {ex.Message}");
                }
            }
            
            if(success)
            {
                menu.UpdateParty();
                AudioManager.Instance.PlaySoundEffect("Save",1);
                UpdateInventoryImages(GameManager.Instance.inventory);
            }
        }
        

        void ShowItemDescription(InventoryItem item)
        {
            if(item.description != "")
            {
                itemDescriptionText.text = item.description;
            }
            else
            {
                itemDescriptionText.text = "Nothing is known of this item.";
            }
        }

        void HideItemDescription(InventoryItem item)
        {
            itemDescriptionText.text = "";
        }
    }
}
