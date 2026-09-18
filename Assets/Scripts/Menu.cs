
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

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
    public Transform universalUI;
    public TMP_Text descriptionText;
    public string currentCharacter = "";
    public Transform locationName;
    public Transform bossHP;
    public GameObject paused;
    public TMP_Text cashText;

    public float lastMenuOpenTime = -10f;


    void Start()
    {
        deckContainer.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        tutorialUI.gameObject.SetActive(false);
        bossHP.gameObject.SetActive(false);
        entireMenu.gameObject.SetActive(false);
        universalUI.gameObject.SetActive(false);
        settingsContainer.gameObject.SetActive(false);
        paused.SetActive(false);
        var audioStart = GameObject.FindAnyObjectByType<StartMusic>();
        if(audioStart != null)
        {
            locationName.GetComponentInChildren<TMP_Text>().text = audioStart.locationName;
            StartCoroutine(FadeOutLocationName());
        }
    }

    IEnumerator FadeOutLocationName()
    {
        var text = locationName.GetComponentInChildren<TMP_Text>();
        var image = locationName.GetComponentInChildren<Image>();
        float duration = 2f; // Duration of the fade-out
        float elapsedTime = 0f;
        yield return new WaitForSeconds(2f);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(.5f, 0f, elapsedTime / duration);
            text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
            image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
            yield return null;
        }

        text.gameObject.SetActive(false); // Hide the text after fading out
    }

    public void ShowSettingsMenu()
    {
        if(settingsContainer.gameObject.activeInHierarchy)
        {
            settingsContainer.gameObject.SetActive(false);
            characterContainer.gameObject.SetActive(true);
            SelectFirstCharacterEntry();
        }
        else
        {
            settingsContainer.gameObject.SetActive(true);
            //set first setting selected
            StartCoroutine(GameManager.Instance.SelectDefault());
            characterContainer.gameObject.SetActive(false);
        }
    }

    public void QuitToMainMenu()
    {
        //Stop audio
        AudioManager.Instance.StopAllAudio();
        GameManager.Reset();
    }

    public void ShowCharacterMenu(string character)
    {

        // record when the menu is opened so quick clicks can be ignored
        bool openingDeckView = !deckContainer.gameObject.activeInHierarchy;
        if (openingDeckView)
        {
            lastMenuOpenTime = Time.unscaledTime;
        }

        currentCharacter = character;
        PartyMember p = YourParty.instance.GetPartyMember(character);
        if(p != null)
        {
            nameText.text = p.memberName;
            YourParty.instance.GetStats(p,out float attack, out float maxHp, out float speed, out float psychic);
            portrait.sprite = Resources.Load<Sprite>($"Sprites/{p.memberName}");
            statsText.text = $"Attack: {attack}  \n HP: {p.hpPercentage * maxHp}/{maxHp}  \n PSY: {psychic} ({Mathf.Round(psychic/10)}MP)  \n Speed: {speed}";
            print("Party member deck contains " + p.deck.Count + " cards.");
            deckContainer.gameObject.SetActive(true);
            characterContainer.gameObject.SetActive(false);
            //remove existing card prefabs
            foreach(Transform child in cardReserve){Destroy(child.gameObject);}
            foreach(Transform child in deck){Destroy(child.gameObject);}
            foreach (var cardGroup in GroupCards(p.deck))
            {
                CreateMenuCard(deck, cardGroup.Value.card, cardGroup.Value.count, () => RemoveCardFromDeck(cardGroup.Value.card));
            }
            var allCards = CardDatabase.Instance.BuildDeckByClass(p.mainClass, p.subClass, p.level);
            allCards.AddRange(YourParty.instance.GetEarnedClasslessCards());
            foreach (Card card in p.deck)
            {
                allCards.Remove(card);
            }
            foreach (var cardGroup in GroupCards(allCards))
            {
                CreateMenuCard(cardReserve, cardGroup.Value.card, cardGroup.Value.count, () => MoveCardToDeck(cardGroup.Value.card));
            }
            deckText.text = $"Your Deck ({p.deck.Count})";
            if(p.deck.Count == CardDatabase.Instance.deckMin)
            deckText.text = $"Your Deck: MINIMUM SIZE {CardDatabase.Instance.deckMin}";
            if(p.deck.Count == CardDatabase.Instance.deckMax)
            deckText.text = $"Your Deck: MINIMUM SIZE {CardDatabase.Instance.deckMax}";
            

            //Equipment container
            //clear
            foreach(Transform child in equipmentContainer) Destroy(child.gameObject);
            //populate
            string[] equipmentTypes = { "Head", "Body", "Drip" };
            foreach(string equipmentType in equipmentTypes)
            {
                var item = p.equipment.Find(equipment =>
                    equipment is Equipment equipped && equipped.type == equipmentType);
                var itemGO = Instantiate(Resources.Load<GameObject>("InventoryItem"), equipmentContainer);
                var rootImage = itemGO.GetComponent<Image>();
                if (rootImage != null && item is Equipment)
                {
                    rootImage.color = new Color(0.75f, 0.88f, 1f, 1f);
                }
                var itemText = itemGO.GetComponentInChildren<TMPro.TMP_Text>();
                
                if(itemText != null)
                {
                    itemText.text = item != null ? item.itemName : $"None ({equipmentType})";
                    var itemImage = itemGO.transform.Find("Image").GetComponent<UnityEngine.UI.Image>();
                    //Disable the item image
                    itemImage.enabled = false;
                }
                if(item != null)
                {
                    var itemImage = itemGO.transform.Find("Image").GetComponent<UnityEngine.UI.Image>();
                    if(itemImage != null)
                    {
                        itemImage.enabled = true;
                        var sprite = Resources.Load<Sprite>($"Items/{item.itemName}");
                        if(sprite != null)
                        {
                            itemImage.sprite = sprite;
                        }
                        var extraInfo = itemGO.transform.Find("ExtraInfo").GetComponent<TMP_Text>();
                        if(item is Equipment e){
                            extraInfo.text = e.type;
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

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        StartCoroutine(GameManager.Instance.SelectDefault());
    }

    Dictionary<string, (Card card, int count)> GroupCards(List<Card> cards)
    {
        var groupedCards = new Dictionary<string, (Card card, int count)>();
        foreach (Card card in cards)
        {
            if (groupedCards.TryGetValue(card.cardName, out var cardGroup))
            {
                groupedCards[card.cardName] = (cardGroup.card, cardGroup.count + 1);
            }
            else
            {
                groupedCards.Add(card.cardName, (card, 1));
            }
        }

        var orderedCards = new Dictionary<string, (Card card, int count)>();
        foreach (Card card in CardDatabase.Instance.allCards)
        {
            if (groupedCards.TryGetValue(card.cardName, out var cardGroup))
            {
                orderedCards.Add(card.cardName, cardGroup);
            }
        }

        foreach (var cardGroup in groupedCards)
        {
            if (!orderedCards.ContainsKey(cardGroup.Key))
            {
                orderedCards.Add(cardGroup.Key, cardGroup.Value);
            }
        }

        return orderedCards;
    }

    void CreateMenuCard(Transform parent, Card card, int count, System.Action onSubmit)
    {
        var cardObject = Instantiate(cardPrefab, parent);
        cardObject.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        var cardDisplay = cardObject.GetComponent<CardDisplay>();
        cardDisplay.displayMode = true;
        cardDisplay.SetData(card);
        if (cardDisplay.count != null)
        {
            cardDisplay.count.text = count > 1 ? $"x{count}" : "";
        }
        cardDisplay.onSubmitAction = onSubmit;
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

    private void SelectFirstCharacterEntry()
    {
        ClearSelection();
        StartCoroutine(GameManager.Instance.SelectDefault());
    }

    private void ClearSelection()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
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
        // ignore clicks that occur immediately after opening the menu
        float grace = 0.15f;
        if (Time.unscaledTime - lastMenuOpenTime < grace)
            return;
        print("Clicked");
        var p = YourParty.instance.GetPartyMember(currentCharacter);
        if(p != null)
        {
            if(p.deck.Count >= CardDatabase.Instance.deckMax)
            {
                GameManager.Instance.ShowMessage($"Can't have more than {CardDatabase.Instance.deckMax} cards");
                AudioManager.Instance.PlaySoundEffect("Negative");
            }
            else
            {
                p.deck.Add(card);
                ShowCharacterMenu(currentCharacter);
                AudioManager.Instance.PlaySoundEffect("MenuEquip");
            }
            
        }
    }

    public void RemoveCardFromDeck(Card card)
    {
        print("Clicked");
        var p = YourParty.instance.GetPartyMember(currentCharacter);
        if(p != null)
        {
            if(p.deck.Count <= CardDatabase.Instance.deckMin)
            {
                GameManager.Instance.ShowMessage($"Can't have less than {CardDatabase.Instance.deckMin} cards");
                AudioManager.Instance.PlaySoundEffect("Negative");
            }
            else
            {
                AudioManager.Instance.PlaySoundEffect("MenuEquip");
                p.deck.Remove(card);
                ShowCharacterMenu(currentCharacter);
            }
            
        }
    }

    public void UpdateParty()
    {
        cashText.text = $"Cash Money: {YourParty.instance.gold}";
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
                int xpThreshold = 100 + partyMember.level * 25;
                thisCharacter.transform.Find("XP").GetComponent<Slider>().value = (float)partyMember.xp / xpThreshold;
                thisCharacter.transform.Find("XP/XP").GetComponent<TMP_Text>().text = $"{partyMember.xp}/{xpThreshold} XP";
                }
                else
                {
                    thisCharacter.transform.Find("Health/HP").GetComponent<TMP_Text>().text = $"DEAD";
                    thisCharacter.transform.Find("Health").GetComponent<Slider>().value = 0;
                    thisCharacter.transform.Find("XP").GetComponent<Slider>().value = 0;
                    thisCharacter.transform.Find("Portrait").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Sprites/MarkedForDeath");
                }
                
                thisCharacter.GetComponentInChildren<Button>().onClick.AddListener(() => ShowCharacterMenu(p));
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Detect WASD or controller input
         if(tutorialUI.gameObject.activeInHierarchy && (Input.GetButtonDown("Jump") || Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0))
        {
            tutorialUI.gameObject.SetActive(false);
            Time.timeScale = 1f;
        }

        if(GameObject.Find("ShopUI") != null || GameObject.Find("Saves(Clone)") != null || GameManager.Instance.GetGameplayState() == GameplayState.Dialog)
        {
            return;
        }

        if (Input.GetButtonDown("Cancel") && GameManager.Instance.GetGameplayState() == GameplayState.FreeMovement)
        {
            AudioManager.Instance.PlaySoundEffect("MenuClose");
            if(entireMenu.gameObject.activeInHierarchy){
                if(deckContainer.gameObject.activeInHierarchy){
                    deckContainer.gameObject.SetActive(false);
                    characterContainer.gameObject.SetActive(true);
                    currentCharacter = "";
                    SelectFirstCharacterEntry();
                }
                else if (settingsContainer.gameObject.activeInHierarchy)
                {
                    settingsContainer.gameObject.SetActive(false);
                    characterContainer.gameObject.SetActive(true);
                    SelectFirstCharacterEntry();
                }
                else
                {
                    ClearSelection();
                    entireMenu.gameObject.SetActive(false);
                    universalUI.gameObject.SetActive(false);
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    Time.timeScale = 1f;
                }
            }
            else
            {
                ClearSelection();
                entireMenu.gameObject.SetActive(true);
                universalUI.gameObject.SetActive(true);
                UpdateParty();
                SelectFirstCharacterEntry();
                itemContainer.GetComponent<Inventory>().UpdateInventoryImages(GameManager.Instance.inventory);

                
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Time.timeScale = 0f;
            }

        }
        else if(Input.GetButtonDown("Cancel") && FindFirstObjectByType<BattleManager>() != null && BattleManager.Instance.dontPause == false)
        {
            if(universalUI.gameObject.activeInHierarchy)
                {
                    paused.SetActive(false);
                    universalUI.gameObject.SetActive(false);
                    settingsContainer.gameObject.SetActive(false);
                    Time.timeScale = 1f;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    //Make this GUI layer in front of everything
                    GetComponent<Canvas>().sortingOrder = -100;
                }
            else if(GameObject.FindFirstObjectByType<Targeter>() == null)
            {
                universalUI.gameObject.SetActive(true);
                paused.SetActive(true);
                SelectFirstCharacterEntry();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Time.timeScale = 0f;
                GetComponent<Canvas>().sortingOrder = 100;
            }
        }   

       
    }
}
