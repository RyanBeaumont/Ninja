using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;
using TMPro;
using System.Linq;
using UnityEngine.UI;
using System.Collections;

public class PointerHoverHandler : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    ISelectHandler,
    IDeselectHandler
{
    public Action onEnter;
    public Action onExit;
    public Action onSelect;
    public Action onDeselect;

    public void OnPointerEnter(PointerEventData eventData)
    {
        onEnter?.Invoke();
        AudioManager.Instance.PlaySoundEffect("MenuHover");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onExit?.Invoke();
    }

    public void OnSelect(BaseEventData eventData)
    {
        onSelect?.Invoke();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        onDeselect?.Invoke();
    }
}

public class Inventory : MonoBehaviour
{
    public Transform itemContainer;
    public TMP_Text itemDescriptionText;
    public void UpdateInventoryImages(List<InventoryItem> inventory)
    {
        if(gameObject.activeInHierarchy){
        StartCoroutine(GameManager.Instance.SelectDefault());
        }
        int selectedIndex = 0;
        if (GameManager.Instance.controllerMode && EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
        {
            var currentSelected = EventSystem.current.currentSelectedGameObject.transform;
            while (currentSelected != null && currentSelected.parent != itemContainer)
            {
                currentSelected = currentSelected.parent;
            }
            if (currentSelected != null && currentSelected.parent == itemContainer)
            {
                selectedIndex = currentSelected.GetSiblingIndex();
            }
        }

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
            var rootImage = itemGO.GetComponent<Image>();
            if (rootImage != null && item is Equipment)
            {
                rootImage.color = new Color(0.75f, 0.88f, 1f, 1f);
            }
            var itemText = itemGO.transform.Find("Count").GetComponent<TMP_Text>();
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
            var extraInfo = itemGO.transform.Find("ExtraInfo").GetComponent<TMP_Text>();
            if(item.mpCost > 0){extraInfo.text = $"{item.mpCost}mp";}
            if(item is Equipment e){
                extraInfo.text = e.type;
            }
            var itemButton = itemGO.GetComponent<UnityEngine.UI.Button>();
            itemButton.onClick.AddListener(() => {
                UseItem(item);
            });

            var hover = itemGO.AddComponent<PointerHoverHandler>();
            hover.onEnter = () => { ShowItemDescription(item); };
            hover.onExit = () => { HideItemDescription(item); };
            hover.onSelect = () => { ShowItemDescription(item); };
            hover.onDeselect = () => { HideItemDescription(item); };
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
                    menu.ShowCharacterMenu(menu.currentCharacter);
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
                if (battleManager.activePlayer == null || battleManager.activePlayer.mp < item.mpCost)
                {
                    GameManager.Instance.ShowMessage("Not enough MP!");
                    AudioManager.Instance.PlaySoundEffect("Negative");
                    return;
                }
                Debug.Log("Trying to use item in battle");
                item.gameAction.caller = battleManager.activeCombatant;
                battleManager.pattern = item.gameAction.pattern;
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
                           caller = item.gameAction.caller,
                           inventoryItemName = item.itemName,
                           inventoryItemMPCost = item.mpCost
                       };
                       battleManager.actionQueue.Add(targetAction);
                       battleManager.HideInventory();
                   }
                   else
                   {
                       // Direct execution for non-targeted actions
                       battleManager.actionQueue.Add(item.gameAction);
                       GameManager.Instance.ConsumeInventoryItem(item.itemName, true, 1);
                       battleManager.activePlayer.GainMP(-item.mpCost);
                       UpdateInventoryImages(GameManager.Instance.inventory);
                       battleManager.HideInventory();
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
                if(menu.deckContainer.gameObject.activeInHierarchy && !(item is Equipment)){
                    menu.deckContainer.gameObject.SetActive(false);
                    menu.characterContainer.gameObject.SetActive(true);
                    menu.currentCharacter = "";
                }
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
