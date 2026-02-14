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
                        var pm = YourParty.instance.GetPartyMember(menu.currentCharacter);
                        if(pm != null)
                        {
                            var multiplier = 10f; if(pm.subClass == CardClass.Grappler) multiplier = 20f; if(pm.mainClass == CardClass.Grappler) multiplier = 30f;
                            var maxHp = pm.level * multiplier + 50f;
                            float healAmount = 50f;
                            pm.hpPercentage += healAmount / maxHp;
                            if(pm.hpPercentage > 1f) pm.hpPercentage = 1f;
                            success = true;
                        }
                        
                    }
                    break;
                case "Coca-Cola Keg":
                    //heal player
                    if(battleManager != null)
                    {
                        battleManager.UseCokeKeg();
                        success = true;
                    }
                    else
                    {
                        foreach(PartyMember pm in YourParty.instance.reserve)
                        {
                            var multiplier = 10f; if(pm.subClass == CardClass.Grappler) multiplier = 20f; if(pm.mainClass == CardClass.Grappler) multiplier = 30f;
                            var maxHp = pm.level * multiplier + 50f;
                            float healAmount = 50f;
                            pm.hpPercentage += healAmount / maxHp;
                            if(pm.hpPercentage > 1f) pm.hpPercentage = 1f;
                            
                        }
                        success = true;
                    }
                    break;
                
                case "Bang":
                    if(battleManager != null)
                    {
                        battleManager.UseBang();
                        success = true;
                    }
                    break;
                case "DrPepper":
                    if(battleManager != null)
                    {
                        battleManager.UseDrPepper();
                        success = true;
                    }
                    break;
                case "Coffee":
                    if(battleManager != null)
                    {
                        battleManager.UseCoffee();
                        success = true;
                    }
                    break;
                default:
                    break;
            }
                if(success)
                {
                    GameManager.Instance.ConsumeInventoryItem(itemName, true, 1);
                    menu.UpdateParty();
                    AudioManager.Instance.PlaySoundEffect("Save",1);
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
