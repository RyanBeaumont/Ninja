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
    RandomEnemies,
    Self,
    Any
}

public class Card
{
    [Header("Display")]
    public string cardName;
    [TextArea(2, 4)]
    public string description;
    public string artwork;

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

    public Dictionary<string,string> itemDescriptions = new Dictionary<string,string>()
    {
        {"Coke", "Good for your HEALTH... You think. Can be used in combat"},
        {"Bang", "Throw at an enemy to deal damage. Does not consume a turn."},
        {"Lockpick", "You made this out of a hairpin! Perks of having long, luscious locks"}
    };

    public Card GetCardByName(string name)
    {
        return allCards.Find(card => card.cardName == name);
    }

    public List<Card> GetCardsByClass(CardClass cardClass, int level)
    {
        return allCards.FindAll(card => (card.cardClass == cardClass) && card.level <= level);
    }

    public List<Card> BuildDeckByClass(CardClass mainClass, CardClass subClass, int level)
    {
        print("AllCards contains " + allCards.Count + " cards.");
        List<Card> deck = new List<Card>();
        deck.AddRange(GetCardsByClass(mainClass, level));
        deck.AddRange(GetCardsByClass(subClass, level - 4));
        deck.AddRange(GetCardsByClass(CardClass.None, level)); //neutral cards
        return deck;
    }

    public List<Card> GetNewCardsForLevel(CardClass mainClass, CardClass subClass, int level)
    {
        List<Card> newCards = new List<Card>();
        newCards.AddRange(allCards.FindAll(card => card.cardClass == mainClass && card.level == level));
        newCards.AddRange(allCards.FindAll(card => card.cardClass == subClass && card.level == level-4));
        newCards.AddRange(allCards.FindAll(card => card.cardClass == CardClass.None && card.level == level));
        return newCards;
    }

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

        allCards.Add(new Card()
        {
            cardName = "Basic Strike",
            description = "A basic physical attack.",
            cost = 0,
            level = 0,
            artwork = "IconFist",
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
            artwork = "IconFist",
            cardClass = CardClass.Ninja,
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "15",
                    animation = "KnifeBackhand",
                    damageType = DamageType.Slashing,
                    targetType = TargetType.SingleEnemy,
                    hits = 1,
                    bonusActions = 1,
                }
            }
        });
        allCards.Add(new Card()
        {
            cardName = "Stabby Stab",
            description = "Hits an extra time for each strike you've dealt this turn",
            cost = 10,
            level = 3,
            artwork = "IconFist",
            cardClass = CardClass.Ninja,
            effects = new List<GameAction>()
            {
                new StabbyStabAction()
                {
                    damage = "15",
                    animation = "SwordThrust",
                    damageType = DamageType.Slashing,
                    targetType = TargetType.SingleEnemy,
                    hits = 1,
                    loopAnimation = true
                }
            }
        });
        allCards.Add(new Card()
        {
            cardName = "It Begins",
            description = "Play 2 extra cards this turn",
            cost = 20,
            level = 4,
            artwork = "IconFist",
            cardClass = CardClass.Ninja,
            effects = new List<GameAction>()
            {
                new GameAction()
                {
                    animation = "Burst",
                    targetType = TargetType.None,
                    bonusActions = 2
                }
            }
        });
         allCards.Add(new Card()
        {
            cardName = "Tactical Reload",
            cardClass = CardClass.Ninja,
            description = "Draw until you have 4 cards",
            artwork = "IconSuperSaiyan",
            cost = 10,
            level = 1,
            effects = new List<GameAction>()
            {
                new DrawUntilAction()
                {
                    cardCount = 3,
                    animation = "GatherChi",
                    targetType = TargetType.Self
                }
            }
        });
        allCards.Add(new Card()
        {
            cardName = "Seven Knives",
            description = "Costs 50 TP. Throw 7 knives at random",
            cost = 0,
            tpCost = 50,
            level = 2,
            artwork = "IconFist",
            cardClass = CardClass.Ninja,
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "12",
                    animation = "ThrowKnife",
                    damageType = DamageType.Slashing,
                    targetType = TargetType.None,
                    hits = 7,
                    loopAnimation = true
                }
            }
        });
         allCards.Add(new Card()
        {
            cardName = "Dual Blades",
            description = "Throw 2 knives at random",
            cost = 0,
            level = 2,
            artwork = "IconFist",
            cardClass = CardClass.Ninja,
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "10",
                    animation = "ThrowKnife",
                    damageType = DamageType.Slashing,
                    targetType = TargetType.None,
                    hits = 2,
                    loopAnimation = true
                }
            }
        });
        allCards.Add(new Card()
        {
            cardName = "Sneak Attack!",
            description = "Increase your STRENGTH until your next turn ends",
            cost = 20,
            level = 4,
            artwork = "IconSuperSaiyan",
            cardClass = CardClass.Ninja,
            effects = new List<GameAction>()
            {
                new StatusEffectAction()
                {
                    animation = "GatherChi",
                    targetType = TargetType.Self,
                    statusEffect = new StatusEffect()
                    {
                        name = "Rage",
                        stat = "STR",
                        amount = 10,
                        duration = 2
                    }
                }
            }
        });
        allCards.Add(new Card()
        {
            cardName = "Smart Strike",
            description = "Draw a card",
            cost = 10,
            artwork = "IconFist",
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
            cost = 25,
            level = 3,
            artwork = "IconMultiFist",
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "15",
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
            artwork = "IconKick",
            cost = 10,
            level = 1,
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "25",
                    animation = "SpinKick",
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
            artwork = "IconSlash",
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
            description = "Ultimate attack, costs 50 TP",
            tpCost = 50,
            cardClass = CardClass.Warrior,
            artwork = "IconSpartaKick",
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
            artwork = "IconKick",
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
            artwork = "IconSlash",
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
            cardName = "Uberslash",
            description = "A powerful slash attack.",
            cost = 40,
            level = 4,
            cardClass = CardClass.Warrior,
            artwork = "IconSlash",
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "70",
                    animation = "LongswordBlast",
                    damageType = DamageType.Slashing,
                    targetType = TargetType.SingleEnemy,
                    hits = 1
                }
            }
        });

         allCards.Add(new Card()
        {
            cardName = "Gather Chi",
            description = "Gain MP and draw 1 card",
            artwork = "IconSuperSaiyan",
            cost = 0,
            level = 1,
            effects = new List<GameAction>()
            {
                new GainMPAction()
                {
                    mpAmount = "PSY",
                    animation = "GatherChi",
                    targetType = TargetType.Self
                }
                ,new DrawCardsAction()
                {
                    cardCount = 1,
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
            artwork = "IconSuperSaiyan",
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
            artwork = "IconFist",
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
                        amount = -8,
                        duration = -1,
                        removeOnHit = true
                    },
                    hits = 1
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Here It Comes!",
            description = "Double your STRENGTH for your next turn",
            cost = 20,
            level = 4,
            artwork = "IconSuperSaiyan",
            cardClass = CardClass.Warrior,
            effects = new List<GameAction>()
            {
                new StatusEffectAction()
                {
                    animation = "GatherChi",
                    targetType = TargetType.Self,
                    statusEffect = new StatusEffect()
                    {
                        name = "DoubleDamage",
                        stat = "STR",
                        amount = 2,
                        additive = false,
                        duration = 2
                    }
                }
            }
        });
        allCards.Add(new Card()
        {
            cardName = "Wild Swing",
            description = "Play the top card of your deck for free",
            cost = 0,
            level = 4,
            artwork = "IconSuperSaiyan",
            cardClass = CardClass.Grappler,
            effects = new List<GameAction>()
            {
                new WildSwingAction()
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Omnislash",
            description = "Damages all opponents",
            artwork = "IconSword",
            cost = 30,
            level = 2,
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "40",
                    animation = "SwordBackhand",
                    damageType = DamageType.Slashing,
                    targetType = TargetType.AllEnemies,
                    hits = 1
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Casual Lean",
            description = "Increase your team's DEF and Counter Damage until the end of your next turn",
            cost = 20,
            level = 4,
            artwork = "IconSuperSaiyan",
            cardClass = CardClass.Grappler,
            effects = new List<GameAction>()
            {
                new StatusEffectAction()
                {
                    animation = "CasualLean",
                    targetType = TargetType.AllAllies,
                    statusEffect = new StatusEffect()
                    {
                        name = "CasualLean",
                        stat = "DEF",
                        amount = 10,
                        additive = true,
                        duration = 2
                    }
                }
            }
        });

    }
}