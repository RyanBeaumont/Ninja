using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;
using TMPro;

public class PointerHoverHandler : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    public Action onEnter;
    public Action onExit;

    public void OnPointerEnter(PointerEventData eventData)
    {
        onEnter?.Invoke();
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
        foreach(var item in inventory)
        {
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
                UseItem(item.itemName);
            });
            //mouse enter to show description
            var hover = itemGO.AddComponent<PointerHoverHandler>();

            hover.onEnter = () =>{ShowItemDescription(item);};

            hover.onExit = () =>{HideItemDescription(item);};
        }

        void UseItem(string itemName)
        {
            var menu = FindFirstObjectByType<Menu>();
            var battleManager = FindFirstObjectByType<BattleManager>();
            bool success = false;
            switch(itemName)
            {
                case "Coke":
                    //heal player
                    if(battleManager != null)
                    {
                        battleManager.UseCoke();
                        success = true;
                    }
                    else
                    {
                        var p = YourParty.instance.GetPartyMember(menu.currentCharacter);
                        if(p != null)
                        {
                            p.hpPercentage += 0.5f;
                            if(p.hpPercentage > 1f) p.hpPercentage = 1f;
                            success = true;
                        }
                    }
                    break;
                case "BANG":
                    //damage enemy
                    break;
                default:
                    break;
            }
                if(success)
                {
                    GameManager.Instance.ConsumeInventoryItem(itemName, true, 1);
                    UpdateInventoryImages(GameManager.Instance.inventory);
                }
            
        }

        void ShowItemDescription(InventoryItem item)
        {
            string description = "";
            if(CardDatabase.Instance.itemDescriptions.TryGetValue(item.itemName, out description))
            {
                itemDescriptionText.text = description;
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
