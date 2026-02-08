using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerCombatant : Combatant
{
    public List<Card> deck = new List<Card>();
    public List<Card> hand = new List<Card>();
    public List<Card> discard = new List<Card>();
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

    public Card Scry()
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
        if (deck.Count == 0) return null; //No cards to draw
        return deck[0];
    }

    public bool PlayCard(Card card)
    {
        var cost = card.cost;
        if(card.tempCost != 0){cost = card.tempCost; card.tempCost = 0;}
        if (hand.Contains(card) && mp >= cost && tp >= card.tpCost && BattleManager.Instance.discardPower >= card.discardCost)
        {
            hand.Remove(card);
            discard.Add(card);
            mp -= cost;
            tp -= card.tpCost;
            BattleManager.Instance.UpdateDiscardPower(BattleManager.Instance.discardPower - card.discardCost);
            BattleManager.Instance.ExecuteCard(card, this);
            return true;
        }
        else
        {
            if(mp < card.cost)
                GameManager.Instance.ShowMessage("Not enough MP!");
            else if(tp < card.tpCost)
                GameManager.Instance.ShowMessage("Not enough TP!");
            else if(BattleManager.Instance.discardPower < card.discardCost)
                GameManager.Instance.ShowMessage($"You must first discard { card.discardCost-BattleManager.Instance.discardPower} more cards with right-click");
        }
        return false;
    }

    public void DiscardCard(Card card)
    {
        hand.Remove(card);
        discard.Add(card);
        BattleManager.Instance.UpdateDiscardPower(BattleManager.Instance.discardPower + 1);
    }

    public void OnHit()
    {
        BattleManager.Instance.PlayerHit();
    }

    void Awake()
    {
    
    }

    public void GainTP(int amount)
    {
        tp += amount;
        var damageNumber = Instantiate(Resources.Load<GameObject>("DamageNumber"), transform.position, Quaternion.identity);
        var damageText = damageNumber.GetComponentInChildren<TMP_Text>();
        damageText.text = $"{Mathf.RoundToInt(amount)}";
        damageText.color = Color.magenta;
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

    public override bool StartTurn()
    {
        var se = HasStatusEffect("Discard");
        if(HasStatusEffect("Discard") != null)
        {
            for(var i=0; i<se.amount; i++)
            {
                //discard random card
                var random = Random.Range(0,hand.Count);
                hand.RemoveAt(random);
                GameManager.Instance.ShowMessage($"{combatantName} discarded a card");
            }
        }
       
        
        RemoveStatusEffect("Discard");
        if (base.StartTurn())
        {
            DrawCards(1);
            maxMp = (int)(psychic * 10);
            mp += (int)psychic;
            if (mp > maxMp) mp = maxMp;
            var HandManager = FindFirstObjectByType<HandManager>();
            HandManager.InitializeHand(hand);
            HandManager.SetHandActive(true);
            ShowStats();
        }

        return true;
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
