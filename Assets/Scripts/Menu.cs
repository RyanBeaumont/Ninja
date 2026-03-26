using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public Transform characterContainer;
    public Transform deckContainer;
    public Transform cardReserve;
    public Transform deck;
    public Transform itemContainer;
    public Transform entireMenu;
    public GameObject cardPrefab;
    public TMP_Text deckText;
    public Transform characterList;
    public Transform tutorialUI;
    public TMP_Text statsText;
    public TMP_Text nameText;
    public Image portrait;
    public Transform settingsContainer;
    public Transform equipmentContainer;
    public TMP_Text descriptionText;
    public string currentCharacter = "";

    void Start()
    {
        deckContainer.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        tutorialUI.gameObject.SetActive(false);
        entireMenu.gameObject.SetActive(false);
        settingsContainer.gameObject.SetActive(false);
    }

    public void ShowSettingsMenu()
    {
        settingsContainer.gameObject.SetActive(true);
        characterContainer.gameObject.SetActive(false);
    }

    public void ShowCharacterMenu(string character)
    {
        currentCharacter = character;
        PartyMember p = YourParty.instance.GetPartyMember(character);
        if(p != null)
        {
            nameText.text = p.memberName;
            YourParty.instance.GetStats(p,out float attack, out float maxHp, out float speed, out float psychic);
            portrait.sprite = Resources.Load<Sprite>($"Sprites/{p.memberName}");
            statsText.text = $"Attack: {attack}  \n HP: {p.hpPercentage * maxHp}/{maxHp}  \n MP/Turn: {psychic}  \n Speed: {speed}";
            print("Party member deck contains " + p.deck.Count + " cards.");
            deckContainer.gameObject.SetActive(true);
            characterContainer.gameObject.SetActive(false);
            //remove existing card prefabs
            foreach(Transform child in cardReserve){Destroy(child.gameObject);}
            foreach(Transform child in deck){Destroy(child.gameObject);}
            foreach(Card card in p.deck)
            {
                var thisCardPrefab = Instantiate(cardPrefab,deck);
                thisCardPrefab.GetComponent<MenuCardDisplay>().SetData(card);
                thisCardPrefab.GetComponentInChildren<MenuCardDisplay>().onPointerDown = () => RemoveCardFromDeck(card);
            }
            var allCards = CardDatabase.Instance.BuildDeckByClass(p.mainClass, p.subClass, p.level);
            foreach(Card card in allCards)
            {
                if(p.deck.Contains(card)) continue; //Don't show cards already in deck
                var thisCardPrefab = Instantiate(cardPrefab,cardReserve);
                thisCardPrefab.GetComponent<MenuCardDisplay>().SetData(card);
                thisCardPrefab.GetComponentInChildren<MenuCardDisplay>().onPointerDown = () => MoveCardToDeck(card);
            }
            deckText.text = $"Your Deck ({p.deck.Count})                Available Cards";

            //Equipment container
            //clear
            foreach(Transform child in equipmentContainer) Destroy(child.gameObject);
            //populate
            foreach(InventoryItem item in p.equipment)
            {
                var itemGO = Instantiate(Resources.Load<GameObject>("InventoryItem"), equipmentContainer);
                var itemText = itemGO.GetComponentInChildren<TMPro.TMP_Text>();
                if(itemText != null)
                {
                    itemText.text = $"{item.itemName}";
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
                    UnequipItem(item);
                });
                //mouse enter to show description
                var hover = itemGO.AddComponent<PointerHoverHandler>();

                hover.onEnter = () =>{ShowItemDescription(item);};
            }
        }
    }

    public bool EquipItem(InventoryItem item)
    {
        var p = YourParty.instance.GetPartyMember(currentCharacter);
        if(p != null)
        {
            // Check if item is already equipped
            if(p.equipment.Contains(item))
            {
                UnequipItem(item);
                return false; // Don't consume when unequipping
            }
            
            //Iterate backward to remove all equipment that shares a type
            for(int i = p.equipment.Count - 1; i >= 0; i--)
            {
                if(p.equipment[i] is Equipment e && item is Equipment itemEq && e.type == itemEq.type)
                {
                    UnequipItem(p.equipment[i]);
                }
            }
            AudioManager.Instance.PlaySoundEffect("MenuEquip");
            p.equipment.Add(item);
            GameManager.Instance.ConsumeInventoryItem(item.itemName,true, 1);
            FindFirstObjectByType<Inventory>().UpdateInventoryImages(GameManager.Instance.inventory);
            ShowCharacterMenu(currentCharacter);
            return true; // Consume when equipping
        }
        return false;
    }
    public void UnequipItem(InventoryItem item)
    {
        AudioManager.Instance.PlaySoundEffect("MenuEquip");
        var p = YourParty.instance.GetPartyMember(currentCharacter);
        if(p != null)
        {
            p.equipment.Remove(item);
            GameManager.Instance.AddInventoryItem(item.itemName, 1);
            ShowCharacterMenu(currentCharacter);
        }
        FindFirstObjectByType<Inventory>().UpdateInventoryImages(GameManager.Instance.inventory);
    }

    void ShowItemDescription(InventoryItem item)
    {
        descriptionText.text = item.description;

    }

    public void ShowTutorialMessage(string tutorialMessage){
        if(tutorialMessage != "" && tutorialMessage != null)
        {
            tutorialUI.gameObject.SetActive(true);
            tutorialUI.GetComponent<TMP_Text>().text = tutorialMessage;
        }else
            tutorialUI.gameObject.SetActive(false);
    }

    public void MoveCardToDeck(Card card)
    {
        
        print("Clicked");
        var p = YourParty.instance.GetPartyMember(currentCharacter);
        if(p != null)
        {
            p.deck.Add(card);
            ShowCharacterMenu(currentCharacter);
        }
    }

    public void RemoveCardFromDeck(Card card)
    {
        print("Clicked");
        var p = YourParty.instance.GetPartyMember(currentCharacter);
        if(p != null)
        {
            p.deck.Remove(card);
            ShowCharacterMenu(currentCharacter);
        }
    }

    public void UpdateParty()
    {
        foreach(Transform child in characterList) Destroy(child.gameObject);
        foreach(string p in YourParty.instance.partyMembers)
        {
            var thisCharacter = Instantiate(Resources.Load<GameObject>("CharacterUI"),characterList);
            var partyMember = YourParty.instance.GetPartyMember(p);
            if(partyMember != null)
            {
                thisCharacter.transform.Find("CharacterName").GetComponent<TMP_Text>().text = p;
                thisCharacter.transform.Find("Subheading").GetComponent<TMP_Text>().text = $"Lv. {partyMember.level} {partyMember.mainClass} {partyMember.subClass}";
                YourParty.instance.GetStats(partyMember,out var attack, out var tempHP, out var speed, out var psychic);
                if(partyMember.alive){
                thisCharacter.transform.Find("Health/HP").GetComponent<TMP_Text>().text = $"{partyMember.hpPercentage * tempHP}/{tempHP}";
                thisCharacter.transform.Find("Health").GetComponent<Slider>().value = partyMember.hpPercentage;
                thisCharacter.transform.Find("Portrait").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Sprites/{partyMember.memberName}");
                }
                else
                {
                    thisCharacter.transform.Find("Health/HP").GetComponent<TMP_Text>().text = $"DEAD";
                    thisCharacter.transform.Find("Health").GetComponent<Slider>().value = 0;
                    thisCharacter.transform.Find("Portrait").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Sprites/Cards/IconDeath");
                }
                
                thisCharacter.GetComponentInChildren<Button>().onClick.AddListener(() => ShowCharacterMenu(p));
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
         if(tutorialUI.gameObject.activeInHierarchy && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.LeftShift)))
        {
            tutorialUI.gameObject.SetActive(false);
            Time.timeScale = 1f;
        }

        if(FindFirstObjectByType<BattleManager>() != null || GameObject.Find("ShopUI") != null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape) && GameManager.Instance.GetGameplayState() == GameplayState.FreeMovement)
        {
            AudioManager.Instance.PlaySoundEffect("MenuClose");
            if(entireMenu.gameObject.activeInHierarchy){
                if(deckContainer.gameObject.activeInHierarchy){
                    deckContainer.gameObject.SetActive(false);
                    characterContainer.gameObject.SetActive(true);
                    currentCharacter = "";
                }
                else if (settingsContainer.gameObject.activeInHierarchy)
                {
                    settingsContainer.gameObject.SetActive(false);
                characterContainer.gameObject.SetActive(true);
                }
                else
                {
                    entireMenu.gameObject.SetActive(false);
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    Time.timeScale = 1f;
                }
            }
        else
            {
                entireMenu.gameObject.SetActive(true);
                UpdateParty();
                itemContainer.GetComponent<Inventory>().UpdateInventoryImages(GameManager.Instance.inventory);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Time.timeScale = 0f;
            }
        
        }   

       
    }
}
