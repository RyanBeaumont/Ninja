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

        //BASIC CARDS

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
            cardName = "Powerslash",
            description = "A powerful slash attack.",
            cost = 20,
            level = 1,
            artwork = "IconSlash",
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "30",
                    animation = "SwordHeavy",
                    damageType = DamageType.Slashing,
                    targetType = TargetType.SingleEnemy,
                    hits = 1
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
            cardName = "Tornado Butt Kick",
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


        /*
        ---------------------NINJA--------------------------------------------------------------------------------
        */

        allCards.Add(new Card()
        {
            cardName = "Quick Strike",
            description = "Apply 2 poison and play again",
            cost = 10,
            level = 1,
            artwork = "IconFist",
            cardClass = CardClass.Ninja,
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "10",
                    animation = "KnifeBackhand",
                    damageType = DamageType.Slashing,
                    targetType = TargetType.SingleEnemy,
                    statusEffect = new StatusEffect
                    {
                        name = "Poisoned",
                        amount = 2,
                        additive = true,
                        duration = -1
                    },
                    hits = 1,
                    bonusActions = 1,
                }
            }
        });
        allCards.Add(new Card()
        {
            cardName = "Dual Blades",
            description = "Throw 2 poisoned knives at random",
            cost = 0,
            level = 2,
            artwork = "IconDoubleKnife",
            cardClass = CardClass.Ninja,
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "10",
                    animation = "ThrowKnife",
                    damageType = DamageType.Slashing,
                    targetType = TargetType.None,
                    statusEffect = new StatusEffect()
                    {
                        name = "Poisoned",
                        amount = 1,
                        duration = -1,
                    },
                    hits = 2,
                    loopAnimation = true
                }
            }
        });
        allCards.Add(new Card()
        {
            cardName = "Stabby Stab",
            description = "Poisoned blade strikes for each hit you've dealt including items",
            cost = 10,
            level = 3,
            artwork = "IconStab",
            cardClass = CardClass.Ninja,
            effects = new List<GameAction>()
            {
                new StabbyStabAction()
                {
                    damage = "10",
                    animation = "SwordBackhand",
                    damageType = DamageType.Slashing,
                    targetType = TargetType.SingleEnemy,
                    statusEffect = new StatusEffect
                    {
                        name = "Poisoned",
                        amount = 1,
                        additive = true,
                        duration = -1
                    },
                    hits = 1,
                    loopAnimation = true
                }
            }
        });
        allCards.Add(new Card()
        {
            cardName = "It Begins",
            description = "Play 2 extra cards this turn",
            tpCost = 50,
            level = 4,
            artwork = "IconSuperSaiyan",
            cardClass = CardClass.Ninja,
            effects = new List<GameAction>()
            {
                new UltimateAction()
                {
                    targetType = TargetType.None,
                    animation = "BoTwirl",  
                    bonusActions = 2,
                },
            }
        });
         allCards.Add(new Card()
        {
            cardName = "Get Poison'd",
            description = "Until the end of your next turn, all your attacks apply poison",
            cost = 20,
            level = 4,
            artwork = "IconDeath",
            cardClass = CardClass.Ninja,
            effects = new List<GameAction>()
            {
                new StatusEffectAction()
                {
                    animation = "Unsheath",
                    targetType = TargetType.Self,
                    statusEffect = new StatusEffect()
                    {
                        name = "Poisoner",
                        amount = 1,
                        duration = 2
                    }
                }
            }
        });
         allCards.Add(new Card()
        {
            cardName = "Tactical Reload",
            cardClass = CardClass.Ninja,
            description = "Discard your hand and draw cards based on your PSY",
            artwork = "IconSuperSaiyan",
            cost = 10,
            level = 5,
            effects = new List<GameAction>()
            {
                new DrawUntilAction()
                {
                    cardCount = 4,
                    animation = "GatherChi",
                    targetType = TargetType.Self
                }
            }
        });
        allCards.Add(new Card()
        {
            cardName = "Exploit Weakness",
            description = "Punch someone. If they are debuffed, play again",
            cost = 10,
            level = 6,
            artwork = "IconFist",
            cardClass = CardClass.Ninja,
            effects = new List<GameAction>()
            {
                new ExploitWeaknessAction()
                {
                    damage = "20",
                    animation = "Jab",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.SingleEnemy,
                    hits = 1,
                }
            }
        });
        
        allCards.Add(new Card()
        {
            cardName = "Disappear",
            description = "Vanish. Re-enter on anyone's turn by pressing E",
            cost = 30,
            level = 7,
            artwork = "IconDeath",
            cardClass = CardClass.Ninja,
            effects = new List<GameAction>()
            {
                new VanishAction()
                {
                    animation = "ArmsCrossed"
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Chain Kill",
            description = "Execute an enemy below 50 HP, and gain this card back",
            cost = 40,
            level = 7,
            artwork = "IconFist",
            cardClass = CardClass.Ninja,
            effects = new List<GameAction>()
            {
                new ChainKillAction()
                {
                    animation = "SwordHeavy",
                    targetType = TargetType.SingleEnemy,
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "The Perfect Tool",
            description = "Look at an ally's top 3 cards. You may discard any of them",
            cost = 10,
            level = 6,
            artwork = "IconSuperSaiyan",
            cardClass = CardClass.Ninja,
            effects = new List<GameAction>()
            {
                new ScryAction()
                 {
                    animation = "GatherChi",
                    targetType = TargetType.SingleAlly,
                    scryAmount = 3
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Seven Knives",
            description = "Costs 50 TP. Throw 7 poisoned knives at random",
            cost = 0,
            tpCost = 50,
            level = 8,
            artwork = "IconDoubleKnife",
            cardClass = CardClass.Ninja,
            effects = new List<GameAction>()
            {
                new UltimateAction()
                {
                    animation = "BoTwirl",  
                    targetType = TargetType.None,
                },
                new DamageAction()
                {
                    damage = "6",
                    animation = "ThrowKnife",
                    damageType = DamageType.Slashing,
                    targetType = TargetType.None,
                    statusEffect = new StatusEffect
                    {
                        name = "Poisoned",
                        amount = 1,
                        additive = true,
                        duration = -1
                    },
                    hits = 7,
                    loopAnimation = true
                }
            }
        });

        /*
        -----------------------------------------------PSYCHIC-----------------------------------------------------------------
        */
        allCards.Add(new Card()
        {
            cardName = "Mind Strike",
            description = "Deals mental damage that ignores status effects",
            artwork = "IconPsychic",
            cardClass = CardClass.Psychic,
            level = 1,
            cost = 15,
            effects = new List<GameAction>()
            {
                new ChiBladeAction()
                {
                    damage = "35",
                    animation = "PsychicLift",
                    damageType = DamageType.Psychic,
                    targetType = TargetType.SingleEnemy,
                    hits = 1
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Flirty Wink",
            description = "Steal MP from an enemy or give it to an ally",
            artwork = "IconSuperSaiyan",
            cardClass = CardClass.Psychic,
            level = 2,
            cost = 0,
            effects = new List<GameAction>()
            {
                new EnergySuckAction()
                {
                    targetType = TargetType.Any,
                    mpAmount = "PSY",
                    animation = "Sassy"
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "E-S-Pow",
            description = "Your opponent questions everything and attacks an ally or themselves",
            tpCost = 50,
            cardClass = CardClass.Psychic,
            artwork = "IconPsychic",
            level = 3,
            effects = new List<GameAction>()
            {
                new UltimateAction()
                {
                    animation = "Levitate",  
                    targetType = TargetType.None,
                },
                new StatusEffectAction()
                {
                    animation = "ArmsCrossed",
                    targetType = TargetType.SingleEnemy,
                    statusEffect = new StatusEffect()
                    {
                        name = "E-S-Pow",
                        amount = 1,
                        duration = 2 //permanent
                    },
                }
            }
        });
        
        allCards.Add(new Card()
        {
            cardName = "Chi Blade",
            description = "Drain your MP to deal equivalent slashing damage",
            artwork = "IconSlash",
            cardClass = CardClass.Psychic,
            level = 3,
            cost = 0,
            effects = new List<GameAction>()
            {
                new ChiBladeAction()
                {
                    damage = "MP * 1.25",
                    animation = "Slash",
                    damageType = DamageType.Slashing,
                    targetType = TargetType.SingleEnemy,
                    hits = 1
                }
            }
        });
        allCards.Add(new Card()
        {
            cardName = "Rage Storm",
            description = "Deal massive psychic damage to all enemies",
            artwork = "IconSuperSaiyan",
            cardClass = CardClass.Psychic,
            level = 4,
            cost = 80,
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "60",
                    animation = "CombatBurst",
                    damageType = DamageType.Psychic,
                    targetType = TargetType.AllEnemies,
                    hits = 1
                }
            }
        });
        
        allCards.Add(new Card()
        {
            cardName = "Linking Strike",
            description = "Target shares status effects with their team for 3 turns",
            artwork = "IconPsychic",
            cardClass = CardClass.Psychic,
            level = 5,
            cost = 30,
            effects = new List<GameAction>()
            {
                new DamageAction
                {
                    targetType = TargetType.SingleEnemy,
                    animation = "KnifeBackhand",
                    damage = "40",
                    damageType = DamageType.Psychic,
                    statusEffect = new StatusEffect()
                    {
                        name = "Linked",
                        duration = 3,
                    }
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Slap Some Sense",
            description = "Slap any target to remove status effects",
            cost = 10,
            level = 6,
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
            cardName = "Lock In",
            description = "Affected ally doubles their PSY but weakens their DEF. Play again",
            artwork = "IconPsychic",
            cardClass = CardClass.Psychic,
            level = 6,
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
            cardName = "Shopping Spree",
            description = "An attack that reduces the cost of all cards in hand. Play again",
            artwork = "IconSuperSaiyan",
            cardClass = CardClass.Psychic,
            level = 7,
            cost = 15,
            effects = new List<GameAction>()
            {
                new ReduceCostAction
                {
                    animation = "Slash",
                    damage = "15",
                    damageType = DamageType.Slashing,
                    targetType = TargetType.SingleEnemy,
                    hits = 1,
                    bonusActions = 1,
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Battle of Wills",
            description = "Deals damage based on the difference in MP",
            artwork = "IconPsychic",
            cardClass = CardClass.Psychic,
            level = 7,
            cost = 10,
            effects = new List<GameAction>()
            {
                new BattleOfWillsAction()
                {
                    damage = "MP",
                    animation = "CombatBurst",
                    damageType = DamageType.Psychic,
                    targetType = TargetType.SingleEnemy,
                    hits = 1
                }
            }
        });
        
        /*
        ---------------------------------WARRIOR----------------------------------------------------------
        */

        allCards.Add(new Card()
        {
            cardName = "Uppercut",
            description = "Knock the enemy off-balance, weakening their DEF",
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
                        name = "Off-Balance",
                        stat = "DEF",
                        amount = .25f,
                        duration = -1,
                        removeOnHit = true
                    },
                    hits = 1
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "C-C-Combo",
            cardClass = CardClass.Warrior,
            description = "3 Hits",
            cost = 25,
            level = 2,
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
            cardName = "SPARTA! KICK",
            description = "Ultimate attack deals massive damage. Must discard 2 cards",
            tpCost = 50,
            discardCost = 2,
            cardClass = CardClass.Warrior,
            artwork = "IconSpartaKick",
            level = 3,
            effects = new List<GameAction>()
            {
                new UltimateAction()
                {
                    animation = "CraneKick", 
                    targetType = TargetType.None, 
                },
                new DamageAction()
                {
                    damage = "70",
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
            cardName = "Channel Rage",
            description = "A small permanent attack boost",
            cost = 20,
            cardClass = CardClass.Warrior,
            artwork = "IconSuperSaiyan",
            level = 4,
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
            cardName = "Uberslash",
            description = "A powerful slash attack.",
            cost = 40,
            level = 5,
            discardCost = 1,
            cardClass = CardClass.Warrior,
            artwork = "IconSlash",
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "50",
                    animation = "LongswordBlast",
                    damageType = DamageType.Slashing,
                    targetType = TargetType.SingleEnemy,
                    hits = 1
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Wild Swing",
            description = "Play the top card of your deck for free",
            cost = 0,
            level = 6,
            artwork = "IconSuperSaiyan",
            cardClass = CardClass.Warrior,
            effects = new List<GameAction>()
            {
                new WildSwingAction()
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Card Exchange",
            description = "Draw a card plus an extra for each card discarded",
            cost = 0,
            level = 7,
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
            cardName = "Here It Comes!",
            description = "Double your STRENGTH for your next turn",
            cost = 20,
            level = 8,
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
        

        /*
        ---------------------GRAPPLER------------------------------------------------------
        */

                allCards.Add(new Card()
        {
            cardName = "Headbutt",
            description = "Deal damage WITH YOUR HEAD. Knock the enemy Off-Balance",
            cost = 0,
            level = 1,
            cardClass = CardClass.Grappler,
            artwork = "IconHeadbutt",
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
                        amount = .25f,
                        duration = -1,
                        removeOnHit = true
                    },
                    hits = 1
                }
            }
        });


        allCards.Add(new Card()
        {
            cardName = "Rock Solid",
            description = "Increase your team's DEF. Counterattacks trigger Off-Balance",
            cost = 10,
            level = 2,
            artwork = "IconShield",
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
                        amount = -0.5f,
                        additive = true,
                        duration = 2
                    }
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Y.E.E.T.",
            description = "Throw an off-balance or prone enemy for massive damage",
            cost = 25,
            level = 2,
            cardClass = CardClass.Grappler,
            artwork = "IconGrab",
            effects = new List<GameAction>()
            {
                new SuplexDamageAction()
                {
                    damage = "20",
                    animation = "SlamAttacker",
                    receivingAnimation = "SlamVictim",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.SingleEnemy,
                    hits = 1
                }
            }
        });

            allCards.Add(new Card()
        {
            cardName = "Chain of Pain",
            description = "Re-use your last played card",
            artwork = "IconGrab",
            cardClass = CardClass.Grappler,
            level = 6,
            cost = 25,
            effects = new List<GameAction>()
            {
                new ChainOfPainAction()
                {
                    targetType = TargetType.Self,
                    animation = "ArmsCrossed"
                }
            }
        });
        
        allCards.Add(new Card()
        {
            cardName = "Identify Weakness",
            description = "Hitting the enemy's weakness inflicts Off-Balance",
            cost = 15,
            level = 4,
            cardClass = CardClass.Grappler,
            artwork = "IconPsychic",
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

        allCards.Add(new Card()
        {
            cardName = "Grand Slam",
            description = "Stun an off-balance or prone enemy. Knock yourself off balance",
            tpCost = 50,
            level = 5,
            cardClass = CardClass.Grappler,
            artwork = "IconGrab",
            effects = new List<GameAction>()
            {
                new UltimateAction()
                {
                    animation = "Rage",  
                    targetType = TargetType.None,
                },
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
            cardName = "Omnisweep",
            description = "All off-balance or prone enemies take heavy damage",
            cost = 20,
            level = 7,
            cardClass = CardClass.Grappler,
            artwork = "IconKick",
            effects = new List<GameAction>()
            {
                new OmnisweepDamageAction()
                {
                    damage = "50",
                    animation = "Sweep",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.AllEnemies,
                    statusEffect = new StatusEffect
                    {
                        name = "Prone",
                        amount = 1,
                        duration = 2,
                    },
                    hits = 1
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Quick Slap",
            description = "Knock the enemy Off-Balance and play again",
            cost = 10,
            level = 8,
            cardClass = CardClass.Grappler,
            artwork = "IconFist",
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "20",
                    animation = "Slap",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.SingleEnemy,
                    statusEffect = new StatusEffect()
                    {
                        name = "Off-Balance",
                        stat = "DEF",
                        amount = .25f,
                        duration = -1,
                        removeOnHit = true
                    },
                    hits = 1,
                    bonusActions = 1
                }
            }
        });

       

        allCards.Add(new Card()
        {
            cardName = "Nardbuster",
            description = "Stun the enemy, knock them prone, and enable party lifesteal",
            tpCost = 50,
            level = 8,
            cardClass = CardClass.Grappler,
            artwork = "IconGroin",
            effects = new List<GameAction>()
            {
                new NardbusterDamageAction()
                {
                    damage = "40",
                    animation = "Uppercut",
                    receivingAnimation = "Launcher",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.SingleEnemy,
                    lifesteal = true,
                    hits = 1
                }
            }
        });

        

    }
}