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
    public string currentCharacter = "";

    void Start()
    {
        deckContainer.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        entireMenu.gameObject.SetActive(false);
    }

    public void ShowCharacterMenu(string character)
    {
        currentCharacter = character;
        PartyMember p = YourParty.instance.GetPartyMember(character);
        if(p != null)
        {
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
        }
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
                var tempHP = YourParty.instance.hpPerLevel * partyMember.level + 100f;
                thisCharacter.transform.Find("Health/HP").GetComponent<TMP_Text>().text = $"{partyMember.hpPercentage * tempHP}/{tempHP}";
                thisCharacter.transform.Find("Health").GetComponent<Slider>().value = partyMember.hpPercentage;
                thisCharacter.GetComponentInChildren<Button>().onClick.AddListener(() => ShowCharacterMenu(p));
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && GameManager.Instance.GetGameplayState() == GameplayState.FreeMovement)
        {
            if(entireMenu.gameObject.activeInHierarchy){
                if(deckContainer.gameObject.activeInHierarchy){
                    deckContainer.gameObject.SetActive(false);
                    characterContainer.gameObject.SetActive(true);
                    currentCharacter = "";
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
