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
    //public string pattern = "";

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
        public int deckMin = 5;
    public int deckMax = 12;

    public StatusEffect getStatusEffect(string name, float amount = 0, int duration = 0)
    {
        StatusEffectData so = Resources.Load<StatusEffectData>($"StatusEffects/{name}");
        StatusEffect se = new StatusEffect();
        if(so != null)
        {
            se.name = so.statusEffect.name;
            se.amount = so.statusEffect.amount;
            se.description = so.statusEffect.description;
            se.sprite = so.statusEffect.sprite;
            se.duration = so.statusEffect.duration;
            se.stat = so.statusEffect.stat;
            se.additive = so.statusEffect.additive;
            se.removeOnHit = so.statusEffect.removeOnHit;
            return se;
        }
        else
        {
            se.name = name;
        }
        if(amount != 0) se.amount = amount;
        if(duration != 0) se.duration = duration;
        return se;
    }

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
                    damage = "15 + 15*LOW",
                    animation = "Jab",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.SingleEnemy,
                    hits = 1,
                    pattern = "1"
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
                    damage = "30 + 30*MED",
                    animation = "SwordHeavy",
                    damageType = DamageType.Slashing,
                    targetType = TargetType.SingleEnemy,
                    hits = 1,
                    pattern = " 2"
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
                    damage = "15 + 15*LOW",
                    animation = "Jab",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.SingleEnemy,
                    hits = 1,
                    pattern = ""
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
            cost = 15,
            level = 1,
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "25 + 25*LOW",
                    animation = "SpinKick",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.AllEnemies,
                    hits = 1,
                    pattern = "1 1 1"
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
            cardName = "Advanced Strike",
            description = "Now leveled up to use both fists at once",
            cost = 0,
            level = 4,
            artwork = "IconFist",
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "15 + 15*LOW",
                    animation = "Jab",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.SingleEnemy,
                    hits = 2,
                    loopAnimation = true,
                    pattern = "11"
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Psychoslash",
            description = "A powerful slash attack.",
            cost = 60,
            level = 5,
            artwork = "IconSlash",
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "50 + 50*MED",
                    animation = "LongswordBlast",
                    damageType = DamageType.Slashing,
                    targetType = TargetType.SingleEnemy,
                    hits = 1,
                    pattern = " 222"
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Band-Aid",
            description = "Heal an ally based on your PSY",
            artwork = "IconPsychic",
            
            cardClass = CardClass.None,
            level = 6,
            cost = 10,
            effects = new List<GameAction>()
            {
                new HealAction()
                {
                    targetType = TargetType.SingleAlly,
                    healAmount = "PSY",
                    pattern = "11",
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "M-M-Mystery Block",
            description = "Take half incoming damage",
            cost = 10,
            cardClass = CardClass.None,
            artwork = "IconShield",
            level = 3,
            effects = new List<GameAction>()
            {
                new StatusEffectAction()
                {
                    animation = "BlockSuccess",
                    targetType = TargetType.Self,
                    statusEffect = getStatusEffect("M-M-Mystery Block")
                },

            }
        });

        allCards.Add(new Card()
        {
            cardName = "Sugar Rush",
            description = "Snort raw sugar. Play again TWICE",
            cost = 15,
            cardClass = CardClass.None,
            artwork = "IconSuperSaiyan",
            level = 7,
            effects = new List<GameAction>()
            {
                new GameAction()
                {
                    animation = "Burst",
                    bonusActions = 2,
                },

            }
        });

        allCards.Add(new Card()
        {
            cardName = "Go to Sweep",
            description = "Knock ALL opponents off-balance",
            artwork = "IconKick",
            cost = 30,
            level = 8,
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "40 + 40*LOW",
                    animation = "Sweep",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.AllEnemies,
                    statusEffect = getStatusEffect("Off-Balance"),
                    hits = 1,
                    pattern = "1 1   2"
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
                    damage = "10+10*LOW",
                    animation = "KnifeBackhand",
                    damageType = DamageType.Slashing,
                    targetType = TargetType.SingleEnemy,
                    statusEffect = getStatusEffect("Poisoned", 2, -1),
                    hits = 1,
                    bonusActions = 1,
                    pattern = "22"
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
                    damage = "10 + 10*MED",
                    animation = "ThrowKnife",
                    damageType = DamageType.Slashing,
                    targetType = TargetType.None,
                    statusEffect = getStatusEffect("Poisoned", 1, -1),
                    hits = 2,
                    loopAnimation = true,
                    pattern = "11"
                }
            }
        });
        allCards.Add(new Card()
        {
            cardName = "Stabby Stab",
            description = "Poisoned blade strikes for each hit you've dealt including items",
            cost = 15,
            level = 3,
            artwork = "IconStab",
            cardClass = CardClass.Ninja,
            effects = new List<GameAction>()
            {
                new StabbyStabAction()
                {
                    damage = "15 + 15*MED",
                    animation = "SwordBackhand",
                    damageType = DamageType.Slashing,
                    targetType = TargetType.SingleEnemy,
                    statusEffect = getStatusEffect("Poisoned", 1, -1),
                    hits = 1,
                    loopAnimation = true,
                    pattern = "11  2"
                }
            }
        });
        allCards.Add(new Card()
        {
            cardName = "It Begins",
            description = "Open a wound for one turn that makes the enemy weak to all damage",
            tpCost = 50,
            level = 4,
            artwork = "IconSuperSaiyan",
            cardClass = CardClass.Ninja,
            effects = new List<GameAction>()
            {
                new CutAction(){},
                new DamageAction()
                {
                    targetType = TargetType.SingleEnemy,
                    damageType = DamageType.Slashing,
                    animation = "SwordWhirlwind",  
                    damage = "30 + 30*MED",
                    statusEffect = getStatusEffect("Weak", 1, 1),
                    pattern = "2 2"
                },
            }
        });
         allCards.Add(new Card()
        {
            cardName = "Get Poison'd",
            description = "Your teammates apply Poison for as long as they take no damage",
            cost = 20,
            level = 4,
            artwork = "IconDeath",
            cardClass = CardClass.Ninja,
            effects = new List<GameAction>()
            {
                new StatusEffectAction()
                {
                    animation = "Unsheath",
                    targetType = TargetType.AllAllies,
                    statusEffect = getStatusEffect("Poisoner", 1, -1)
                }
            }
        });
         allCards.Add(new Card()
        {
            cardName = "Tactical Reload",
            cardClass = CardClass.Ninja,
            description = "Discard your hand and draw cards based on your PSY",
            artwork = "IconSuperSaiyan",
            cost = 15,
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
            cardName = "Death Bomb",
            description = "Attach a sticky bomb. When they Die, they Explode",
            cost = 20,
            level = 6,
            artwork = "IconFist",
            cardClass = CardClass.Ninja,
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "1",
                    animation = "Throw",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.Any,
                    statusEffect = getStatusEffect("Death Bomb", 1, -1),
                    hits = 1,
                    pattern = "2"
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
            cardName = "Deploy Cat",
            description = "Deploy Assault Kitten to the battlefield",
            cost = 20,
            artwork = "IconSuperSaiyan",
            cardClass = CardClass.Ninja,
            level = 99,
            effects = new List<GameAction>()
            {
                new SummonAction()
                {
                    enemy = false,
                    summon = Resources.Load<GameObject>("Enemies/EnemySpartan")
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "It Was HIM!",
            description = "Enemies can only target the chosen ally until your next turn",
            cost = 15,
            level = 3,
            artwork = "IconDeath",
            cardClass = CardClass.Ninja,
            effects = new List<GameAction>()
            {
                new StatusEffectAction()
                {
                    animation = "Objection",
                    targetType = TargetType.SingleAlly,
                    statusEffect = getStatusEffect("Taunt", 1, -1)
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
                new CutAction(){},
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
                    statusEffect = getStatusEffect("Poisoned", 1, -1),
                    hits = 7,
                    loopAnimation = true,
                    pattern = "1 111 111"
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
            cost = 20,
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "30 + 30*HIGH",
                    animation = "PsychicLift",
                    damageType = DamageType.Psychic,
                    targetType = TargetType.SingleEnemy,
                    hits = 1,
                    pattern = "3 3"
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
                    animation = "Sass"
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Insanity",
            description = "Go insane for 2 turns. Your cards get duplicated but target randomly",
            tpCost = 50,
            
            cardClass = CardClass.Psychic,
            artwork = "IconPsychic",
            level = 4,
            effects = new List<GameAction>()
            {
                new UltimateAction()
                {
                    animation = "Levitate",  
                    targetType = TargetType.None,
                },
                new StatusEffectAction()
                {
                    pattern = "3 3 3",
                    animation = "Rage",
                    targetType = TargetType.Self,
                    statusEffect = getStatusEffect("Insanity"),
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
            level = 10,
            effects = new List<GameAction>()
            {
                new CutAction(){},
                new UltimateAction()
                {
                    animation = "Levitate",  
                    targetType = TargetType.None,
                },
                new StatusEffectAction()
                {
                    animation = "ArmsCrossed",
                    targetType = TargetType.SingleEnemy,
                    statusEffect = getStatusEffect("E-S-Pow", 1, 2),
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
                    damage = "MP + MP*HIGH",
                    animation = "Slash",
                    damageType = DamageType.Slashing,
                    targetType = TargetType.SingleEnemy,
                    hits = 1,
                    pattern = "2111"
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
                    damage = "60 + 60*HIGH",
                    animation = "CombatBurst",
                    damageType = DamageType.Psychic,
                    targetType = TargetType.AllEnemies,
                    hits = 1,
                    pattern = "2 22"
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
                    damage = "40 + 40*LOW",
                    damageType = DamageType.Psychic,
                    statusEffect = getStatusEffect("Linked", 0, 2),
                    pattern = "3 3"
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Slap Some Sense",
            description = "Slap any target to remove status effects including stun",
            cost = 10,
            level = 6,
            cardClass = CardClass.Psychic,
            artwork = "IconFist",
            effects = new List<GameAction>()
            {
                new NullifyDamageAction2()
                {
                    damage = "10",
                    animation = "Slap",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.Any,
                    hits = 1,
                    pattern = "1"
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
                    damage = "15 + 15*MED",
                    damageType = DamageType.Slashing,
                    targetType = TargetType.SingleEnemy,
                    hits = 1,
                    bonusActions = 1,
                    pattern = "1 3"
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Advanced Band-Aid",
            description = "Heal an ally and play again",
            artwork = "IconPsychic",
            
            cardClass = CardClass.Psychic,
            level = 7,
            cost = 15,
            effects = new List<GameAction>()
            {
                new HealAction()
                {
                    targetType = TargetType.SingleAlly,
                    healAmount = "PSY*2",
                    bonusActions = 1,
                    pattern = "111",
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
                    damage = "15 + 15*MED",
                    animation = "Uppercut",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.SingleEnemy,
                    statusEffect = getStatusEffect("Off-Balance"),
                    hits = 1,
                    pattern = "1 1"
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
                    damage = "15 + 15*LOW",
                    animation = "PunchCombo",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.SingleEnemy,
                    hits = 3,
                    pattern = " 111"
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
            level = 4,
            effects = new List<GameAction>()
            {
                new CutAction(){},
                new UltimateAction()
                {
                    animation = "CraneKick", 
                    targetType = TargetType.None, 
                },
                new DamageAction()
                {
                    damage = "70 + 70*HIGH",
                    animation = "Kick",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.SingleEnemy,
                    hits = 1,
                    pattern = "1 1 2"
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
                    damage = "30 + 30*MED",
                    animation = "Kick",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.SingleEnemy,
                    hits = 1,
                    pattern = " 2"
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
                    statusEffect = getStatusEffect("Rage",2,-1)
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
                    damage = "50 + 50*HIGH",
                    animation = "LongswordBlast",
                    damageType = DamageType.Slashing,
                    targetType = TargetType.SingleEnemy,
                    hits = 1,
                    pattern = "1  21  2"
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
            cardName = "The Closer",
            description = "Discard cards to this attack for extra damage",
            cost = 20,
            level = 7,
            discardCost = 0,
            cardClass = CardClass.Warrior,
            artwork = "IconSlash",
            effects = new List<GameAction>()
            {
                new CloserAction()
                {
                    animation = "FlyingAxeKick",
                    pattern = "2 2 2"
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
                    statusEffect = getStatusEffect("DoubleDamage", 2, 2)
                }
            }
        });
        

        /*
        ---------------------GRAPPLER------------------------------------------------------
        */

                allCards.Add(new Card()
        {
            cardName = "Headbutt",
            description = "Deal damage WITH YOUR HEAD. Knock the enemy Prone",
            cost = 0,
            level = 1,
            cardClass = CardClass.Grappler,
            artwork = "IconHeadbutt",
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "15 + 15*MED",
                    animation = "headbutt",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.SingleEnemy,
                    statusEffect = getStatusEffect("Prone"),
                    hits = 1,
                    pattern = "2"
                }
            }
        });


        allCards.Add(new Card()
        {
            cardName = "Rock Solid",
            description = "Increase your team's DEF. Counterattacks trigger Off-Balance",
            cost = 10,
            level = 3,
            artwork = "IconShield",
            cardClass = CardClass.Grappler,
            effects = new List<GameAction>()
            {
                new StatusEffectAction()
                {
                    animation = "ArmsCrossed",
                    targetType = TargetType.AllAllies,
                    statusEffect = getStatusEffect("Rock Solid", 0, 2)
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
                    damage = "60 + 60*HIGH",
                    animation = "SlamAttacker",
                    receivingAnimation = "SlamVictim",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.SingleEnemy,
                    hits = 1,
                    pattern = "1 11  2"
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Untouchable",
            description = "You or an ally will automatically block 1 hit",
            cost = 10,
            cardClass = CardClass.Grappler,
            artwork = "IconShield",
            level = 3,
            effects = new List<GameAction>()
            {
                new StatusEffectAction()
                {
                    animation = "BlockSuccess",
                    targetType = TargetType.SingleAlly,
                    statusEffect = getStatusEffect("Block")
                },

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
            cost = 10,
            level = 4,
            cardClass = CardClass.Grappler,
            artwork = "IconPsychic",
            effects = new List<GameAction>()
            {
                new StatusEffectAction()
                {
                    animation = "Objection",
                    targetType = TargetType.SingleEnemy,
                    statusEffect = getStatusEffect("Exposed",0,2)
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
                new CutAction(){},
                new UltimateAction()
                {
                    animation = "Rage",  
                    targetType = TargetType.None,
                },
                new GrappleDamageAction()
                {
                    damage = "40 + 40*MED",
                    animation = "ShoulderThrowAttacker",
                    receivingAnimation = "ShoulderThrowVictim",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.SingleEnemy,
                    hits = 1,
                    pattern = "2 2 2"
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
                    damage = "50 + 50*MED",
                    animation = "Sweep",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.AllEnemies,
                    statusEffect = getStatusEffect("Prone"),
                    hits = 1,
                    pattern = "1111"
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
                    damage = "20 + 20*LOW",
                    animation = "Slap",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.SingleEnemy,
                    statusEffect = getStatusEffect("Off-Balance"),
                    hits = 1,
                    bonusActions = 1,
                    pattern = "11"
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
                    damage = "40 + 40*MED",
                    animation = "Uppercut",
                    receivingAnimation = "Launcher",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.SingleEnemy,
                    lifesteal = true,
                    hits = 1,
                    pattern = "2 2 3"
                }
            }
        });

        

    }
}