using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerCombatant : Combatant
{
    public List<Card> deck = new List<Card>();
    public List<Card> hand = new List<Card>();
    List<Card> discard = new List<Card>();
    [HideInInspector] public int tp; //TERROR points

    public void DrawCards(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (deck.Count == 0)
            {
                //Reshuffle discard into deck
                deck.AddRange(discard);
                discard.Clear();
                //shuffle deck
                for (int j = 0; j < deck.Count; j++)
                {
                    Card temp = deck[j];
                    int randomIndex = Random.Range(j, deck.Count);
                    deck[j] = deck[randomIndex];
                    deck[randomIndex] = temp;
                }
            }
            if (deck.Count == 0) break; //No cards to draw
            Card drawnCard = deck[0];
            deck.RemoveAt(0);
            hand.Add(drawnCard);
        }
    }

    public bool PlayCard(Card card)
    {
        if (hand.Contains(card) && mp >= card.cost && tp >= card.tpCost)
        {
            hand.Remove(card);
            discard.Add(card);
            mp -= card.cost;
            tp -= card.tpCost;
            BattleManager.Instance.ExecuteCard(card, this);
            return true;
        }
        else
        {
            if(mp < card.cost)
                GameManager.Instance.ShowMessage("Not enough MP!");
            else if(tp < card.tpCost)
                GameManager.Instance.ShowMessage("Not enough TP!");
        }
        return false;
    }

    public void OnHit()
    {
        BattleManager.Instance.PlayerHit();
    }

    void Awake()
    {
    
    }

    public void ShuffleDeck()
    {
        for (int j = 0; j < deck.Count; j++)
        {
            Card temp = deck[j];
            int randomIndex = Random.Range(j, deck.Count);
            deck[j] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    public override void StartTurn()
    {
        base.StartTurn();
        DrawCards(1);
        maxMp = (int)(psychic * 10);
        mp += (int)psychic;
        if (mp > maxMp) mp = maxMp;
        var HandManager = FindFirstObjectByType<HandManager>();
        HandManager.InitializeHand(hand);
        HandManager.SetHandActive(true);
        ShowStats();
    }

    public void BonusTurn()
    {
        ReturnToStartPosition();
         var HandManager = FindFirstObjectByType<HandManager>();
        HandManager.InitializeHand(hand);
        HandManager.SetHandActive(true);
        ShowStats();
    }

    void ShowStats()
    {
        //show player stats UI
        var battleManager = FindFirstObjectByType<BattleManager>();
        battleManager.playerStats.gameObject.SetActive(true);
        battleManager.playerStats.Find("Name").GetComponent<TMP_Text>().text = combatantName;
        battleManager.playerStats.Find("Level").GetComponent<TMP_Text>().text = $"Level {level}";
        battleManager.playerStats.Find("MP").GetComponent<TMP_Text>().text = $"MP {mp}/{maxMp}";
        battleManager.playerStats.Find("TP").GetComponent<TMP_Text>().text = $"TP {tp}";
    }
}
