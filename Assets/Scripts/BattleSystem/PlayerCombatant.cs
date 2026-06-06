using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class PlayerCombatant : Combatant
{
    public List<Card> deck = new List<Card>();
    public List<Card> hand = new List<Card>();
    public List<Card> discard = new List<Card>();
    [HideInInspector] public int tp; //TERROR points
    [HideInInspector] public bool hidden = false;

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

    public Card[] Scry(int amount)
    {
        if (deck.Count < amount)
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
        //return the top 3 cards of the deck without deleting
        return deck.Take(amount).ToArray();
    }

    public override void Update()
    {
        base.Update();
        if (hidden)
        {
            var playerCharactersRemaining = BattleManager.Instance.combatants.FindAll(c => c is PlayerCombatant && c.alive).Count;
            if (Input.GetKeyDown(KeyCode.E) || playerCharactersRemaining == 1)
            {
                hidden = false;
                surprise = true;
                initiative = 100f;
                BattleManager.Instance.AddCombatant(this);
                PlayAnimation("ArmsCrossed");
                SetTargetPosition(transform.position + Vector3.up*4f);
                AudioManager.Instance.PlaySoundEffect("Teleport");
                RemoveStatusEffect("");
                GameManager.Instance.ShowMessage($"{combatantName} emerges from the shadows to play next!");
                if(playerCharactersRemaining == 1)
                {
                    GameManager.Instance.ShowMessage($"{combatantName} is no longer hidden since they are the last alive");
                }
            }
        }
    }

    public bool PlayCard(Card card)
    {
        var cost = card.cost;
        if(card.tempCost != 0){cost = card.tempCost; card.tempCost = 0;}
        if (hand.Contains(card) && mp >= cost && tp >= card.tpCost && BattleManager.Instance.discardPower >= card.discardCost)
        {
            if (card.effects.Any(e => e is SuplexDamageAction || e is GrappleDamageAction))
            {
                var success = false;
                foreach(EnemyCombatant e in FindObjectsByType<EnemyCombatant>(FindObjectsSortMode.None))
                {
                    if (e.alive && (e.HasStatusEffect("Off-Balance") != null || e.HasStatusEffect("Prone") != null))
                    {
                        success = true; break;
                    }
                }
                if(success == false)
                {
                    AudioManager.Instance.PlaySoundEffect("Negative",1f);
                    GameManager.Instance.ShowMessage("Can only be used on an Off-Balance or Prone opponent");
                    return false;
                } 
            }
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
            AudioManager.Instance.PlaySoundEffect("Negative",1f);
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

    public void Revive()
    {
        hp = maxHp/2f;
        alive = true;
        BattleManager.Instance.combatants.Add(this);
        YourParty.instance.GetPartyMember(combatantName).alive = true;
        PlayAnimation("Drink");
        GameManager.Instance.ShowMessage($"{combatantName} is risen!");
    }

    public override bool StartTurn()
    {
        var se = HasStatusEffect("Discard");
        if(HasStatusEffect("Discard") != null)
        {
            for(var i=0; i<se.amount; i++)
            {
                //discard random card
                if(hand.Count > 0){
                var random = Random.Range(0,hand.Count);
                hand.RemoveAt(random);
                GameManager.Instance.ShowMessage($"{combatantName} discarded a card");
                }
            }
        }
        if(HasStatusEffect("Keg Backpack") != null)
        {
            GameManager.Instance.ShowMessage($"{combatantName} drank from the Coca-Cola Keg restoring 10HP");
            Heal(10);
        }
       
        
        RemoveStatusEffect("Discard");
        if (base.StartTurn())
        {
            DrawCards(1);
            maxMp = (int)(psychic * 4);
            mp += EvaluateStatFormula("PSY");
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
