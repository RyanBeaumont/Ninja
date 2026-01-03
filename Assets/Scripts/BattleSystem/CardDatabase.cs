using UnityEngine;
using System.Collections.Generic;

public enum CardClass
{
    None, Warrior, Ninja, Psychic, Grappler
}

public enum TargetType
{
    None,           // e.g. self buffs
    SingleEnemy,
    SingleAlly,
    AllEnemies,
    AllAllies,
    Self,
    Any
}

public class Card
{
    [Header("Display")]
    public string cardName;
    [TextArea(2, 4)]
    public string description;
    public Sprite artwork;

    [Header("Rules")]
    public CardClass cardClass;
    public int cost;
    public int tpCost = 0;
    public int level = 0;

    [Header("Effects")]
    public List<GameAction> effects = new();
}


public class CardDatabase : MonoBehaviour
{
    public static CardDatabase Instance;
    public List<Card> allCards = new List<Card>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Card GetCardByName(string name)
    {
        return allCards.Find(card => card.cardName == name);
    }

    public List<Card> GetCardsByClass(CardClass cardClass, int level)
    {
        return allCards.FindAll(card => (card.cardClass == cardClass || card.cardClass == CardClass.None) && card.level <= level);
    }

    public List<Card> BuildDeckByClass(CardClass mainClass, CardClass subClass, int level)
    {
        List<Card> deck = new List<Card>();
        deck.AddRange(GetCardsByClass(mainClass, level));
        deck.AddRange(GetCardsByClass(mainClass, level));
        deck.AddRange(GetCardsByClass(subClass, level - 4));
        deck.AddRange(GetCardsByClass(subClass, level - 4));
        return deck;
    }

    void Start()
    {
        allCards.Add(new Card()
        {
            cardName = "Basic Strike",
            description = "A basic physical attack.",
            cost = 0,
            level = 0,
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "15",
                    animation = "Jab",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.SingleEnemy,
                    hits = 1
                }
            }
        });
        allCards.Add(new Card()
        {
            cardName = "Quick Strike",
            description = "Play again",
            cost = 10,
            cardClass = CardClass.Ninja,
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "15",
                    animation = "Jab",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.SingleEnemy,
                    hits = 1,
                    bonusActions = 1,
                }
            }
        });
        allCards.Add(new Card()
        {
            cardName = "Smart Strike",
            description = "Draw a card",
            cost = 10,
            cardClass = CardClass.Grappler,
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "15",
                    animation = "Jab",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.SingleEnemy,
                    hits = 1,
                },
                new DrawCardsAction()
                {
                    cardCount = 1,
                    animation = "",
                    targetType = TargetType.Self
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "C-C-Combo",
            cardClass = CardClass.Warrior,
            description = "3 Hits",
            cost = 20,
            level = 3,
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "10",
                    animation = "PunchCombo",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.SingleEnemy,
                    hits = 3
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Tornado Ass Kick",
            description = "Damages all opponents",
            cost = 10,
            level = 1,
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "25",
                    animation = "SwordWhirlwind",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.AllEnemies,
                    hits = 1
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Chi Blade",
            description = "Deals damage based on your MP",
            cardClass = CardClass.Psychic,
            level = 1,
            cost = 10,
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "MP * 0.75",
                    animation = "Slash",
                    damageType = DamageType.Slashing,
                    targetType = TargetType.SingleEnemy,
                    hits = 1
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "SPARTA! KICK",
            description = "Ultimate attack",
            tpCost = 50,
            cardClass = CardClass.Warrior,
            level = 2,
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "80",
                    animation = "Kick",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.SingleEnemy,
                    hits = 1
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Lifestrike",
            description = "Heals equal to damage dealt",
            cardClass = CardClass.Warrior,
            level = 3,
            cost = 30,
            effects = new List<GameAction>()
            {
                new LifestrikeAction()
                {
                    damage = "30",
                    animation = "Kick",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.SingleEnemy,
                    hits = 1
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Powerslash",
            description = "A powerful slash attack.",
            cost = 20,
            level = 1,
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "40",
                    animation = "SwordHeavy",
                    damageType = DamageType.Slashing,
                    targetType = TargetType.SingleEnemy,
                    hits = 1
                }
            }
        });

         allCards.Add(new Card()
        {
            cardName = "Gather Chi",
            description = "Gain MP and draw 2 cards",
            cost = 0,
            level = 1,
            effects = new List<GameAction>()
            {
                new GainMPAction()
                {
                    mpAmount = "PSY * 2",
                    animation = "",
                    targetType = TargetType.Self
                }
                ,new DrawCardsAction()
                {
                    cardCount = 2,
                    animation = "",
                    targetType = TargetType.Self
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Channel Rage",
            description = "A small permanent attack boost",
            cost = 20,
            cardClass = CardClass.Warrior,
            level = 2,
            effects = new List<GameAction>()
            {
                new StatusEffectAction()
                {
                    animation = "Burst",
                    targetType = TargetType.Self,
                    statusEffect = new StatusEffect()
                    {
                        name = "Rage",
                        stat = "ATK",
                        amount = 3,
                        duration = -1 //permanent
                    },
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Uppercut",
            description = "Concuss the enemy, increasing your next hit against them",
            cost = 10,
            level = 1,
            cardClass = CardClass.Warrior,
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "15",
                    animation = "Uppercut",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.SingleEnemy,
                    statusEffect = new StatusEffect()
                    {
                        name = "Concussed",
                        stat = "DEF",
                        amount = -4,
                        duration = -1,
                        removeOnHit = true
                    },
                    hits = 1
                }
            }
        });
    }
}