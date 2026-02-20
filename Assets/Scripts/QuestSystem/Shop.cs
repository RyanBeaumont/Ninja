using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable] public class ShopItem
{
    public InventoryItem item;
    public int cost;
}

public class Shop : ChainedInteractable
{
    public List<ShopItem> items;
    bool shopActive = false;
    GameObject shopUI;
    TMP_Text descriptionText;
    GameObject container;

    public override void Interact()
    {
        shopActive = true;
        shopUI = Instantiate(Resources.Load<GameObject>("Shop"));
        shopUI.name = "Shop";
        container = shopUI.transform.Find("Scroll View/Viewport/Content").gameObject;
        descriptionText = shopUI.transform.Find("Description/Text (TMP)").GetComponent<TMP_Text>();
        //unlock cursor
        AudioManager.Instance.PlaySoundEffect("ChaChing");
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        UpdateShop();
    }

    public void UpdateShop()
    {
        shopUI.transform.Find("Title").GetComponent<TMP_Text>().text = $"SHOP: You have ${YourParty.instance.gold}";

        //Clear container
        foreach(Transform child in container.transform){Destroy(child.gameObject);}
        foreach(ShopItem item in items)
        {
            if(item.item.quantity <= 0) continue;
            var itemGO = Instantiate(Resources.Load<GameObject>("InventoryItem"), container.transform);
            var itemText = itemGO.GetComponentInChildren<TMPro.TMP_Text>();
            if(itemText != null)
            {
                itemText.text = $"{item.item.itemName} X{item.item.quantity} - ${item.cost}";
            }
            var itemImage = itemGO.transform.Find("Image").GetComponent<UnityEngine.UI.Image>();
            if(itemImage != null)
            {
                var sprite = Resources.Load<Sprite>($"Items/{item.item.itemName}");
                if(sprite != null)
                {
                    itemImage.sprite = sprite;
                }
            }
            var itemButton = itemGO.GetComponent<UnityEngine.UI.Button>();
            itemButton.onClick.AddListener(() => {
                BuyItem(item);
            });
            //mouse enter to show description
            var hover = itemGO.AddComponent<PointerHoverHandler>();

            hover.onEnter = () =>{ShowItemDescription(item.item);};
        }
    }

    void ShowItemDescription(InventoryItem item)
    {

      descriptionText.text = item.description;
    }

    void BuyItem(ShopItem item)
    {
        if(YourParty.instance.gold >= item.cost)
        {
            AudioManager.Instance.PlaySoundEffect("ChaChing");
            YourParty.instance.gold -= item.cost;
            GameManager.Instance.AddInventoryItem(item.item.itemName, 1);
            item.item.quantity -= 1;
            UpdateShop();
        }
        else
        {
            AudioManager.Instance.PlaySoundEffect("Negative");
            GameManager.Instance.ShowMessage("Too poor");
        }
    }

    void Update()
    {
        if(shopActive && Input.GetKeyDown(KeyCode.Escape))
        {
            //set cursor locked
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            shopActive = false;
            Destroy(shopUI);
            Time.timeScale = 1f;
            CallNext();
        }
    }
}


