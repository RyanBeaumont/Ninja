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
    public bool exile = false;
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
    public int deckMin = 12;
    public int deckMax = 40;

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
            se.particleEffect = so.statusEffect.particleEffect;
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
        //Double main-class cards
        deck.AddRange(GetCardsByClass(mainClass, level));

        return deck;
    }

    public List<Card> GetNewCardsForLevel(CardClass mainClass, CardClass subClass, int level)
    {
        List<Card> newCards = new List<Card>();
        newCards.AddRange(allCards.FindAll(card => card.cardClass == mainClass && card.level == level));
        newCards.AddRange(allCards.FindAll(card => card.cardClass == subClass && card.level == level-4));

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
            description = "A basic bludgeoning attack.",
            cost = 2,
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
            cardName = "Basic Slice",
            description = "A basic slashing attack.",
            cost = 2,
            level = 0,
            artwork = "IconKnife",
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "15 + 15*LOW",
                    animation = "SwordBackhand",
                    damageType = DamageType.Slashing,
                    targetType = TargetType.SingleEnemy,
                    hits = 1,
                    pattern = "1"
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "B. Slap",
            description = "The AUDACITY!",
            cost = 1,
            level = 1,
            artwork = "IconSlap",
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "8 + 8*MED",
                    animation = "Slap",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.SingleEnemy,
                    hits = 1,
                    pattern = "1"
                }
            }
        });

                allCards.Add(new Card()
        {
            cardName = "Critical Hit",
            description = "Gain 2 MP",
            cost = 1,
            level = 100,
            exile = true,
            artwork = "IconSuperSaiyan",
            effects = new List<GameAction>()
            {
                new GainMPAction()
                {
                    mpAmount = "2",
                    animation = "GatherChi",
                    targetType = TargetType.None,
                    pattern = ""
                }
            }
        });

        

         allCards.Add(new Card()
        {
            cardName = "Powerslash",
            description = "A powerful slash attack.",
            cost = 3,
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
            cardName = "Tornado Butt Kick",
            description = "Damages all opponents",
            artwork = "IconKick",
            cost = 3,
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
            description = "Draw 2 cards",
            artwork = "IconCard",
            cost = 2,
            level = 1,
            effects = new List<GameAction>()
            {
            new DrawCardsAction()
                {
                    cardCount = 2,
                    animation = "GatherChi",
                    targetType = TargetType.Self
                }
            }
        });
        

        //---------------MONSTER LOOT----------------


        allCards.Add(new Card()
        {
            cardName = "Poison Vial",
            description = "Apply 2 poison",
            cost = 2,
            level = 2,
            artwork = "IconDeath2",
            cardClass = CardClass.None,
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "10*LOW",
                    animation = "Throw",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.SingleEnemy,
                    statusEffect = getStatusEffect("Poisoned", 2, -1),
                    hits = 1,
                    pattern = "22"
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Band-Aid",
            description = "Heal an ally based on your PSY",
            artwork = "IconHeal",
            
            cardClass = CardClass.None,
            level = 5,
            cost = 1,
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
            cost = 1,
            cardClass = CardClass.None,
            artwork = "IconShield",
            level = 2,
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
            cardName = "Combat E-S-Pow",
            description = "3 turns: Draw a card when hitting a weakness",
            cost = 4,
            cardClass = CardClass.None,
            artwork = "IconCard",
            level = 2,
            effects = new List<GameAction>()
            {
                new StatusEffectAction()
                {
                    animation = "Levitate",
                    targetType = TargetType.Self,

                    statusEffect = getStatusEffect("DrawOnCrit")
                },

            }
        });

        allCards.Add(new Card()
        {
            cardName = "Open Chakra",
            description = "3 turns: Gain 1 MP when hitting a weakness",
            cost = 4,
            cardClass = CardClass.None,
            artwork = "IconCard",
            level = 2,
            effects = new List<GameAction>()
            {
                new StatusEffectAction()
                {
                    animation = "Levitate",
                    targetType = TargetType.Self,

                    statusEffect = getStatusEffect("GainMPOnCrit")
                },

            }
        });

        allCards.Add(new Card()
        {
            cardName = "Intro to ESP",
            description = "Mental attack scaled heavily by your PSY power",
            cost = 3,
            level = 4,
            artwork = "IconPsychic",
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "10 + 10*HIGHPSY",
                    animation = "PsychicLift",
                    damageType = DamageType.Psychic,
                    targetType = TargetType.SingleEnemy,
                    hits = 1,
                    pattern = "3"
                }
            }
        });

         allCards.Add(new Card()
        {
            cardName = "Smart Strike",
            description = "Draw a card",
            cost = 2,
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
            cardName = "Psychoslash",
            description = "A powerful slash attack.",
            cost = 4,
            level = 6,
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
            cardName = "Go to Sweep",
            description = "Knock ALL opponents off-balance",
            artwork = "IconKick",
            cost = 3,
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

        allCards.Add(new Card()
        {
            cardName = "America Punch",
            description = "Stun the opponent",
            artwork = "IconFist",
            cost = 4,
            cardClass = CardClass.Grappler,
            level = 9,
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "40 + 40*MED",
                    animation = "Punch",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.SingleEnemy,
                    statusEffect = getStatusEffect("Stunned"),
                    hits = 1,
                    pattern = "2 3"
                }
            }
        });

        /*
        ---------------------NINJA--------------------------------------------------------------------------------
        */


        
        allCards.Add(new Card()
        {
            cardName = "Sugar Rush",
            description = "Snort raw sugar. Gain 1 MP",
            cost = 0,
            cardClass = CardClass.Ninja,
            artwork = "IconSuperSaiyan",
            level = 6,
            effects = new List<GameAction>()
            {
                new GainMPAction()
                {
                    animation = "Burst",
                    mpAmount = "1"
                },

            }
        });
         allCards.Add(new Card()
        {
            cardName = "Quick Strike",
            description = "Gain 2 MP",
            cost = 2,
            level = 3,
            artwork = "IconFist",
            cardClass = CardClass.Ninja,
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "10+10*LOW",
                    animation = "KnifeBackhand",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.SingleEnemy,
                    hits = 1,
                    bonusActions = 1,
                    pattern = "22"
                },
                new GainMPAction()
                {
                    mpAmount = "2",
                    animation = "GatherChi",
                    targetType = TargetType.None
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Poison Strike",
            description = "Apply 3 poison",
            cost = 1,
            level = 1,
            artwork = "IconDeath",
            cardClass = CardClass.Ninja,
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "10*LOW",
                    animation = "KnifeBackhand",
                    damageType = DamageType.Slashing,
                    targetType = TargetType.SingleEnemy,
                    statusEffect = getStatusEffect("Poisoned", 3, -1),
                    hits = 1,
                    bonusActions = 1,
                    pattern = "22"
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Flurry of Blows",
            description = "3 turns: Strike again for each hit against a weakness",
            cost = 4,
            cardClass = CardClass.Ninja,
            artwork = "IconNinja",
            level = 4,
            effects = new List<GameAction>()
            {
                new StatusEffectAction()
                {
                    animation = "BoTwirl",
                    targetType = TargetType.Self,

                    statusEffect = getStatusEffect("Flurry")
                },

            }
        });

        allCards.Add(new Card()
        {
            cardName = "Dual Blades",
            description = "Throw 2 poisoned knives at random",
            cost = 3,
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
            description = "Strikes for each hit you've dealt including items",
            cost = 2,
            level = 3,
            artwork = "IconKnife",
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
            cost = 1,
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
            description = "Your hits apply Poison for 2 turns",
            cost = 2,
            level = 4,
            artwork = "IconDeath2",
            cardClass = CardClass.Ninja,
            effects = new List<GameAction>()
            {
                new StatusEffectAction()
                {
                    animation = "Unsheath",
                    targetType = TargetType.Self,
                    statusEffect = getStatusEffect("Poisoner", 1, 2)
                }
            }
        });
         allCards.Add(new Card()
        {
            cardName = "Tactical Reload",
            cardClass = CardClass.Ninja,
            description = "Discard your hand and draw cards based on your PSY",
            artwork = "IconCard",
            cost = 3,
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
            cost = 3,
            level = 6,
            artwork = "IconBomb",
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
            cardName = "Shopping Spree",
            description = "An attack that reduces the cost of all cards in hand.",
            artwork = "IconSuperSaiyan",
            cardClass = CardClass.Ninja,
            level = 10,
            cost = 4,
            effects = new List<GameAction>()
            {
                new ReduceCostAction
                {
                    animation = "Slash",
                    damage = "15 + 15*MEDPSY",
                    damageType = DamageType.Slashing,
                    targetType = TargetType.SingleEnemy,
                    hits = 1,
                    pattern = "1 3"
                }
            }
        });
        /*
        allCards.Add(new Card()
        {
            cardName = "Speed Boost",
            description = "Choose an ally to gain increased speed",
            cost = 3,
            level = 99,
            artwork = "IconNinja",
            cardClass = CardClass.Ninja,
            effects = new List<GameAction>()
            {
                new StatusEffectAction()
                {
                    animation = "NinjaIdle",
                    targetType = TargetType.SingleAlly,
                    statusEffect = getStatusEffect("Speed Boost", 20, 3)
                },
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
        */

        allCards.Add(new Card()
        {
            cardName = "It Was HIM!",
            description = "Enemies can only target the chosen ally. Play again",
            cost = 1,
            level = 3,
            artwork = "IconTarget",
            cardClass = CardClass.Ninja,
            effects = new List<GameAction>()
            {
                new StatusEffectAction()
                {
                    animation = "Objection",
                    targetType = TargetType.SingleAlly,
                    statusEffect = getStatusEffect("Taunt", 1, -1),
                    bonusActions = 1
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Seven Knives",
            description = "Costs 50 TP. Throw 7 knives at random",
            cost = 1,
            tpCost = 50,
            level = 9,
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
                    damage = "20 + 10*LOW",
                    animation = "ThrowKnife",
                    damageType = DamageType.Slashing,
                    targetType = TargetType.None,
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
            cost = 3,
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "30 + 30*HIGHPSY",
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
            cardName = "See the Future",
            description = "Look at an ally's top 3 cards. You may discard any of them",
            cost = 2,
            level = 6,
            artwork = "IconCard",
            cardClass = CardClass.Psychic,
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
            cardName = "Gather Intel",
            description = "Deal 1 hit of each damage type",
            cost = 2,
            level = 7,
            cardClass = CardClass.Psychic,
            artwork = "IconMultiFist",
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    multiDamageType = true,
                    damage = "10 + 10*LOW",
                    animation = "Jab",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.SingleEnemy,
                    hits = 3,
                    loopAnimation = true,
                    pattern = "22"
                },

            }
        });

        allCards.Add(new Card()
        {
            cardName = "Flirty Wink",
            description = "Give 2 MP to an ally",
            artwork = "IconWink",
            cardClass = CardClass.Psychic,
            level = 2,
            cost = 2,
            effects = new List<GameAction>()
            {
                new EnergySuckAction()
                {
                    targetType = TargetType.SingleAlly,
                    mpAmount = "2",
                    animation = "Sass"
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Vampire Energy Suck",
            description = "Steal MP from an enemy equal to your PSY",
            artwork = "IconPsychic",
            cardClass = CardClass.Psychic,
            level = 4,
            cost = 2,
            effects = new List<GameAction>()
            {
                new EnergySuckAction()
                {
                    targetType = TargetType.SingleEnemy,
                    mpAmount = "PSY/10",
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
            artwork = "IconSuperSaiyan",
            level = 4,
            cost = 1,
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
            cardName = "Lightning",
            description = "Attack all enemies each turn until you take damage",
            cost = 3,
            level = 10,
            artwork = "IconShield",
            cardClass = CardClass.Grappler,
            effects = new List<GameAction>()
            {
                new StatusEffectAction()
                {
                    animation = "Rage",
                    targetType = TargetType.Self,
                    statusEffect = getStatusEffect("Lightning", 1, -1)
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "E-S-Pow",
            description = "Make the enemy loco. They hit themselves or an ally",
            tpCost = 50,
            cost = 1,
            cardClass = CardClass.Psychic,
            artwork = "IconPsychic",
            level = 9,
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
                    animation = "Objection",
                    targetType = TargetType.SingleEnemy,
                    statusEffect = getStatusEffect("E-S-Pow", 1, 2),
                }
            }
        });
        
        allCards.Add(new Card()
        {
            cardName = "Mind War",
            description = "Drain your MP to deal equivalent psychic damage",
            artwork = "IconPsychic",
            cardClass = CardClass.Psychic,

            level = 5,
            cost = 0,
            effects = new List<GameAction>()
            {
                new ChiBladeAction()
                {
                    damage = "MP*10 + MP*10*MEDPSY",
                    animation = "Slash",
                    damageType = DamageType.Psychic,
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
            cost = 6,
            effects = new List<GameAction>()
            {
                new DamageAction()
                {
                    damage = "50 + 50*MEDPSY",
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
            level = 6,
            cost = 3,
            effects = new List<GameAction>()
            {
                new DamageAction
                {
                    targetType = TargetType.SingleEnemy,
                    animation = "KnifeBackhand",
                    damage = "40 + 40*LOWPSY",
                    damageType = DamageType.Psychic,
                    statusEffect = getStatusEffect("Linked", 0, 2),
                    pattern = "3 3"
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Slap Some Sense",
            description = "Slap any target to heal them and remove stun or status effects",
            cost = 1,
            level = 7,
            cardClass = CardClass.Psychic,
            artwork = "IconSlap",
            effects = new List<GameAction>()
            {
                new PurgeDebuffsAction()
                {
                    animation = "Slap",
                    targetType = TargetType.SingleAlly,
                    pattern = "1"
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Lock In",
            description = "Affected ally gains an additional MP each turn but weakens their DEF. Play again",
            artwork = "IconPsychic",
            cardClass = CardClass.Psychic,
            level = 8,
            cost = 3,
            effects = new List<GameAction>()
            {
                new LockInAction
                {
                    targetType = TargetType.SingleAlly,
                    animation = "GatherChi",
                }
            }
        });

        

        allCards.Add(new Card()
        {
            cardName = "Bounty",
            description = "Choose an enemy. The first person to hit them gains 2 MP",
            artwork = "IconSuperSaiyan",
            cost = 3,
            cardClass = CardClass.Ninja,
            level = 9,
            effects = new List<GameAction>()
            {
                new StatusEffectAction()
                {
                   statusEffect = getStatusEffect("Bounty"),
                   targetType = TargetType.SingleEnemy,
                   animation = "Objection",
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Advanced Band-Aid",
            description = "Heal an ally",
            artwork = "IconHeal",
            
            cardClass = CardClass.Psychic,
            level = 9,
            cost = 2,
            effects = new List<GameAction>()
            {
                new HealAction()
                {
                    targetType = TargetType.SingleAlly,
                    healAmount = "PSY*3",
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
            cost = 2,
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
            cost = 3,
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
            cardName = "Dropkick",
            description = "Deal heavy damage based on your MP and use it up",
            artwork = "IconSpartaKick",
            cardClass = CardClass.Warrior,

            level = 5,
            cost = 0,
            effects = new List<GameAction>()
            {
                new ChiBladeAction()
                {
                    damage = "8 + MP*15*MED",
                    animation = "FlyingAxeKick",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.SingleEnemy,
                    hits = 1,
                    pattern = "222"
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Adrenaline",
            cardClass = CardClass.Warrior,
            description = "Gain 2 MP. Must discard 2 cards",
            cost = 0,
            discardCost = 2,
            level = 6,
            artwork = "IconSuperSaiyan",
            effects = new List<GameAction>()
            {
                new GainMPAction()
                {
                    animation = "Burst",
                    mpAmount = "2"
                }
            }
        });


        allCards.Add(new Card()
        {
            cardName = "SPARTA! KICK",
            description = "Ultimate attack deals massive damage. Must discard 2 cards",
            tpCost = 50,
            cost = 1,
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
                    damage = "70 + 70*MED",
                    animation = "Kick",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.SingleEnemy,
                    hits = 1,
                    pattern = "1 1  2"
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Berzerk!",
            description = "Wild Swing 3 times. Exile this card",
            tpCost = 50,
            cost = 1,
            exile = true,
            cardClass = CardClass.Warrior,
            artwork = "IconBomb",
            level = 8,
            effects = new List<GameAction>()
            {
                new CutAction(){},
                new UltimateAction()
                {
                    animation = "CraneKick", 
                    targetType = TargetType.None, 
                },
                new WildSwingAction(),
                new WildSwingAction(),
                new WildSwingAction()
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Lifestrike",
            description = "Heals equal to damage dealt",
            cardClass = CardClass.Warrior,
            artwork = "IconHeal",
            level = 1,
            cost = 3,
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
            cost = 1,
            cardClass = CardClass.Warrior,
            artwork = "IconSuperSaiyan",
            level = 3,
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
            cost = 4,
            level = 5,
            discardCost = 1,
            cardClass = CardClass.Warrior,
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
                    pattern = "1  21  2"
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Heavy Blow",
            description = "Deal heavy damage. Gain 2 fewer MP next turn",
            cost = 2,
            level = 6,
            artwork = "IconSlash",
            effects = new List<GameAction>()
            {
                
                new StatusEffectAction()
                {
                    statusEffect = getStatusEffect("Exhausted",2,2),
                    targetType = TargetType.Self,
                    animation = "IdleDrunk"
                },
                new DamageAction()
                {
                    damage = "40 + 40*MED",
                    animation = "SwordHeavy",
                    damageType = DamageType.Slashing,
                    targetType = TargetType.SingleEnemy,
                    hits = 1,
                    pattern = "1  222"
                },
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Wild Swing",
            description = "Play the top card of your deck for free. Exile this card",
            cost = 2,
            level = 6,
            exile = true,
            artwork = "IconBomb",
            cardClass = CardClass.Warrior,
            effects = new List<GameAction>()
            {
                new WildSwingAction()
            }
        });

        allCards.Add(new Card()
        {
            cardName = "One Two Punch",
            description = "Gain 2 MP if the opponent is debuffed",
            artwork = "IconMultiFist",
            cost = 2,
            level = 2,
            effects = new List<GameAction>()
            {
                new ExploitWeaknessAction()
                {
                    damage = "30 + 30*MED",
                    animation = "Uppercut",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.SingleEnemy,
                    hits = 1,
                    pattern = "2 2"
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Whirlwind",
            cardClass = CardClass.Warrior,
            description = "Deal 3 hits to all enemies. Gain 1 less MP next turn",
            cost = 3,
            level = 7,
            artwork = "IconKick",
            effects = new List<GameAction>()
            {
                new StatusEffectAction()
                {
                    statusEffect = getStatusEffect("Exhausted",1,2),
                    targetType = TargetType.Self,
                    animation = "IdleDrunk"
                },
               new DamageAction()
                {
                    damage = "25 + 25*LOW",
                    animation = "SpinKick",
                    loopAnimation = true,
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.AllEnemies,
                    hits = 3,
                    pattern = "2 2 2"
                },
                
            }
        });

        allCards.Add(new Card()
        {
            cardName = "The Closer",
            description = "Discard your hand to deal damage per card",
            cost = 4,
            level = 8,
            cardClass = CardClass.Warrior,
            artwork = "IconDeath2",
            effects = new List<GameAction>()
            {
                new CloserAction()
                {
                    animation = "LongswordBlast",
                    damageType = DamageType.Slashing,
                    targetType = TargetType.SingleEnemy,
                    hits = 1,
                    pattern = "22222"
                },
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Here It Comes!",
            description = "Double your STRENGTH for your next turn",
            cost = 4,
            level = 9,
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

         allCards.Add(new Card()
        {
            cardName = "Execution",
            description = "Deal damage based on missing HP",
            cost = 3,
            discardCost = 1,
            level = 10,
            artwork = "IconDeath",
            effects = new List<GameAction>()
            {
                new ExecuteAction()
                {
                    animation = "LongswordBlast",
                    damageType = DamageType.Slashing,
                    targetType = TargetType.SingleEnemy,
                    hits = 1,
                    pattern = "1  222"
                },
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Double Draw",
            description = "Draw an extra card each turn",
            cost = 3,
            cardClass = CardClass.Warrior,
            artwork = "IconCard",
            level = 10,
            effects = new List<GameAction>()
            {
                new StatusEffectAction()
                {
                    animation = "Levitate",
                    targetType = TargetType.Self,

                    statusEffect = getStatusEffect("Draw")
                },

            }
        });

        
        

        /*
        ---------------------GRAPPLER------------------------------------------------------
        */

                allCards.Add(new Card()
        {
            cardName = "Headbutt",
            description = "Deal damage WITH YOUR HEAD. Knock the enemy Prone",
            cost = 2,
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
            cardName = "Re-Concuss",
            description = "Hit a prone or off-balance enemy to knock them prone again",
            cost = 3,
            level = 5,
            cardClass = CardClass.Grappler,
            artwork = "IconFist",
            effects = new List<GameAction>()
            {
                new ReconcussDamageAction()
                {
                    damage = "15 + 20*MED",
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
            cardName = "Shoulder Charge",
            cardClass = CardClass.Grappler,
            description = "Stun the opponent and yourself",
            cost = 2,
            level = 4,
            artwork = "IconSuperSaiyan",
            effects = new List<GameAction>()
            {
                 new StatusEffectAction()
                {
                    statusEffect = getStatusEffect("Stunned",1,1),
                    targetType = TargetType.Self,
                    animation = "IdleDrunk"
                },
               new DamageAction()
                {
                    damage = "40 + 20*MED",
                    animation = "headbutt",
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.SingleEnemy,
                    hits = 1,
                    pattern = "2 2 2"
                }
               
            }
        });


        allCards.Add(new Card()
        {
            cardName = "Rock Solid",
            description = "Increase your team's DEF. Counterattacks trigger Off-Balance",
            cost = 3,
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
            cardName = "Back Stance",
            description = "3 turns: Increase your counter damage",
            cost = 2,
            level = 6,
            artwork = "IconShield",
            cardClass = CardClass.Grappler,
            effects = new List<GameAction>()
            {
                new StatusEffectAction()
                {
                    animation = "Rage",
                    targetType = TargetType.AllAllies,
                    statusEffect = getStatusEffect("IncreasedCounter", 0, 2)
                }
            }
        });

        allCards.Add(new Card()
        {
            cardName = "Y.E.E.T.",
            description = "Throw an off-balance or prone enemy for massive damage",
            cost = 3,
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
            cost = 2,
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
            cardName = "Un-Untouchable",
            description = "All allies will automatically block 2 hits",
            cost = 5,
            cardClass = CardClass.Grappler,
            artwork = "IconShield",
            level = 11,
            effects = new List<GameAction>()
            {
                new StatusEffectAction()
                {
                    animation = "Rage",
                    targetType = TargetType.AllAllies,
                    statusEffect = getStatusEffect("Block"),
                },
                new StatusEffectAction()
                {
                    animation = "Rage",
                    targetType = TargetType.AllAllies,
                    statusEffect = getStatusEffect("Block"),
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
            cost = 3,
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
            description = "3 turns: Hitting the enemy's weakness inflicts Off-Balance",
            cost = 2,
            level = 4,
            cardClass = CardClass.Grappler,
            artwork = "IconTarget",
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
            cost = 1,
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
            cost = 2,
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
            cost = 1,
            level = 8,
            cardClass = CardClass.Grappler,
            artwork = "IconSlap",
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
            level = 10,
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