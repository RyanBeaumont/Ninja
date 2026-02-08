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
    public int tempCost = 0;
    public int tpCost = 0;
    public int level = 0;
    public int discardCost = 0;

    [Header("Effects")]
    public List<GameAction> effects = new();
}

//Ninja: DISAPPEAR - untargetable ally
//Share Buff
//NULLIFY
//Reduce all cards' cost?
//Ultimate: Gun - Kill a non-boss character
//Ultimate: Dominate Mind - You choose an enemy's next target

/* Enemies
    Jade - Picks her target each turn for heavy damage. Counterattacks each turn.
    Chrome Dome - Reveals weakness when charging major attacks
    Faceless - Spawns minions and consumes them for buff
    Grappler - Stuns you
*/

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
                    damage = "10",
                    animation = "SwordBackhand",
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
            cardClass = CardClass.None,
            level = 3,
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
            cardName = "Battle of Wills",
            description = "Deals damage based on the difference in MP",
            artwork = "IconSlash",
            cardClass = CardClass.Psychic,
            level = 2,
            cost = 10,
            effects = new List<GameAction>()
            {
                new BattleOfWillsAction()
                {
                    damage = "MP * 0.75",
                    animation = "Slash",
                    damageType = DamageType.Psychic,
                    targetType = TargetType.SingleEnemy,
                    hits = 1
                }
            }
        });
        allCards.Add(new Card()
        {
            cardName = "Chi Blade",
            description = "Drain your MP to deal equivalent slashing damage",
            artwork = "IconSlash",
            cardClass = CardClass.Psychic,
            level = 1,
            cost = 0,
            effects = new List<GameAction>()
            {
                new ChiBladeAction()
                {
                    damage = "MP * 0.9",
                    animation = "Slash",
                    damageType = DamageType.Slashing,
                    targetType = TargetType.SingleEnemy,
                    hits = 1
                }
            }
        });
        allCards.Add(new Card()
        {
            cardName = "Mind Strike",
            description = "Deals mental damage that ignores status effects",
            artwork = "IconSlash",
            cardClass = CardClass.Psychic,
            level = 1,
            cost = 15,
            effects = new List<GameAction>()
            {
                new ChiBladeAction()
                {
                    damage = "30",
                    animation = "headbutt",
                    damageType = DamageType.Psychic,
                    targetType = TargetType.SingleEnemy,
                    hits = 1
                }
            }
        });
        allCards.Add(new Card()
        {
            cardName = "Vampire Energy Suck",
            description = "Steal MP from an enemy or give it to an ally",
            artwork = "IconSuperSaiyan",
            cardClass = CardClass.Psychic,
            level = 1,
            cost = 0,
            effects = new List<GameAction>()
            {
                new EnergySuckAction()
                {
                    targetType = TargetType.Any,
                    mpAmount = "PSY",
                    animation = "Choke"
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Tactical Reload",
            description = "Recover an ally's last played card at reduced MP cost",
            artwork = "IconSuperSaiyan",
            cardClass = CardClass.Psychic,
            level = 1,
            cost = 20,
            effects = new List<GameAction>()
            {
                new ReloadAction()
                {
                    targetType = TargetType.SingleAlly,
                    animation = "Objection"
                }
            }
        });

        

        allCards.Add(new Card()
        {
            cardName = "Linking Strike",
            description = "Affected target shares next status effect with their team",
            artwork = "IconSuperSaiyan",
            cardClass = CardClass.Psychic,
            level = 4,
            cost = 30,
            effects = new List<GameAction>()
            {
                new DamageAction
                {
                    targetType = TargetType.SingleEnemy,
                    animation = "KnifeBackhand",
                    damage = "30",
                    damageType = DamageType.Psychic,
                    statusEffect = new StatusEffect()
                    {
                        name = "Linked",
                        duration = -1,
                        removeOnHit = true
                    }
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Lock In",
            description = "Affected ally doubles their PSY but weakens their DEF. Play again",
            artwork = "IconSuperSaiyan",
            cardClass = CardClass.Psychic,
            level = 4,
            cost = 30,
            effects = new List<GameAction>()
            {
                new LockInAction
                {
                    targetType = TargetType.SingleAlly,
                    animation = "GatherChi",
                    bonusActions = 1,
                }
            }
        });

         allCards.Add(new Card()
        {
            cardName = "Flirty Wink",
            description = "Give an ally a health boost based on your PSY, and all your TP",
            artwork = "IconSuperSaiyan",
            cardClass = CardClass.Psychic,
            level = 2,
            tpCost = 10,
            effects = new List<GameAction>()
            {
                new ShareTPAction
                {
                    targetType = TargetType.SingleAlly,
                    healAmount = "PSY * 2",
                    animation = "GatherChi"
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "SPARTA! KICK",
            description = "Ultimate attack, costs 50 TP",
            tpCost = 50,
            discardCost = 2,
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
            discardCost = 1,
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
            cardName = "Card Exchange",
            description = "Draw a card plus an extra for each card discarded",
            cost = 0,
            level = 4,
            discardCost = 0,
            cardClass = CardClass.Warrior,
            artwork = "IconSlash",
            effects = new List<GameAction>()
            {
                new CardExchangeAction()
                {
                    animation = "GatherChi"
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
            level = 5,
            artwork = "IconSuperSaiyan",
            cardClass = CardClass.Warrior,
            effects = new List<GameAction>()
            {
                new WildSwingAction()
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Rock Solid",
            description = "Increase your team's DEF. Counterattacks trigger Off-Balance",
            cost = 20,
            level = 4,
            artwork = "IconSuperSaiyan",
            cardClass = CardClass.Grappler,
            effects = new List<GameAction>()
            {
                new StatusEffectAction()
                {
                    animation = "ArmsCrossed",
                    targetType = TargetType.AllAllies,
                    statusEffect = new StatusEffect()
                    {
                        name = "Rock Solid",
                        stat = "DEF",
                        amount = 10,
                        additive = true,
                        duration = 2
                    }
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Headbutt",
            description = "Deal damage WITH YOUR HEAD. Knock the enemy Off-Balance",
            cost = 10,
            level = 1,
            cardClass = CardClass.Grappler,
            artwork = "IconKick",
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "15",
                    animation = "headbutt",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.SingleEnemy,
                    statusEffect = new StatusEffect()
                    {
                        name = "Off-Balance",
                        stat = "DEF",
                        amount = -4,
                        duration = -1,
                        removeOnHit = true
                    },
                    hits = 1
                }
            }
        });


        allCards.Add(new Card()
        {
            cardName = "Suplex",
            description = "Deal heavy damage to an off-balance enemy. Knock yourself off balance",
            cost = 25,
            level = 3,
            cardClass = CardClass.Grappler,
            artwork = "IconKick",
            effects = new List<GameAction>()
            {
                new SuplexDamageAction()
                {
                    damage = "20",
                    animation = "ShoulderThrowAttacker",
                    receivingAnimation = "ShoulderThrowVictim",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.SingleEnemy,
                    hits = 1
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Knock it Off",
            description = "Slap an enemy. If they're off-balance, they lose all status effects",
            cost = 25,
            level = 3,
            cardClass = CardClass.Grappler,
            artwork = "IconFist",
            effects = new List<GameAction>()
            {
                new NullifyDamageAction()
                {
                    damage = "15",
                    animation = "Slap",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.SingleEnemy,
                    hits = 1
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Slap Some Sense",
            description = "Slap any target to remove status effects",
            cost = 10,
            level = 5,
            cardClass = CardClass.Psychic,
            artwork = "IconFist",
            effects = new List<GameAction>()
            {
                new NullifyDamageAction2()
                {
                    damage = "15",
                    animation = "Slap",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.Any,
                    hits = 1
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Pin",
            description = "Stun an off-balance enemy. Knock yourself off balance",
            cost = 30,
            level = 3,
            cardClass = CardClass.Grappler,
            artwork = "IconFist",
            effects = new List<GameAction>()
            {
                new GrappleDamageAction()
                {
                    damage = "40",
                    animation = "ShoulderThrowAttacker",
                    receivingAnimation = "ShoulderThrowVictim",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.SingleEnemy,
                    hits = 1
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Tag Team",
            description = "Stun an off-balance enemy. Enable party lifesteal",
            tpCost = 50,
            level = 2,
            cardClass = CardClass.Grappler,
            artwork = "IconFist",
            effects = new List<GameAction>()
            {
                new GrappleDamageAction()
                {
                    damage = "40",
                    animation = "ShoulderThrowAttacker",
                    receivingAnimation = "ShoulderThrowVictim",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.SingleEnemy,
                    lifesteal = true,
                    hits = 1
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Chokehold",
            description = "Continuously grapple an off-balance opponent until you take damage",
            cost = 30,
            level = 4,
            cardClass = CardClass.Grappler,
            artwork = "IconFist",
            effects = new List<GameAction>()
            {
                new StatusEffectAction()
                {
                    animation = "Choke",
                    targetType = TargetType.SingleEnemy,
                    statusEffect = new StatusEffect()
                    {
                        name = "Choked",
                        amount = 1,
                        additive = true,
                        duration = 3
                    }
                },
                new StatusEffectAction()
                {
                    targetType = TargetType.Self,
                    statusEffect = new StatusEffect()
                    {
                        name = "Choking",
                        amount = 1,
                        additive = true,
                        duration = 3
                    }
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Identify Weakness",
            description = "Hitting the enemy's weakness inflicts Off-Balance",
            cost = 25,
            level = 3,
            cardClass = CardClass.Grappler,
            artwork = "IconSuperSaiyan",
            effects = new List<GameAction>()
            {
                new StatusEffectAction()
                {
                    animation = "Objection",
                    targetType = TargetType.SingleEnemy,
                    statusEffect = new StatusEffect()
                    {
                        name = "Exposed",
                        amount = 1,
                        additive = true,
                        duration = 2
                    }
                }
            }
        });

        

    }
}