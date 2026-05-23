using UnityEngine;
using System.Collections.Generic;
public class EnemySoulContainer : EnemyCombatant
{
    public string trappedCharacter;

    public override void Start(){
        base.Start();
        if(combatantName != "")
        {
            trappedCharacter = combatantName;
            var trappedPartyMember = YourParty.instance.GetPartyMember(trappedCharacter);
            if(trappedPartyMember == null)
            {
                print("Party member is null");
                return;
            }
            var model = Instantiate(Resources.Load<GameObject>($"Characters/{trappedPartyMember.modelName}"),transform);
            YourParty.instance.GetStats(trappedPartyMember,out float attack1, out float maxHp1, out float speed1, out float psychic1);
            maxHp = maxHp1;
            hp = maxHp;
        }
    }

    public override void OnHit(string direction)
    {
        base.OnHit(direction);
        if(HasStatusEffect("Stunned") != null) {OnDeath();}
    }

    public override void OnDeath()
    {
        base.OnDeath();
        GameManager.Instance.ShowMessage($"{trappedCharacter} is freed!");
        //Spawn the player back
        var BattleSetup = GameObject.Find("BattleSetup");
        var playerSpawn = BattleSetup.transform.Find("PlayerSpawn");
        var partyMember = YourParty.instance.GetPartyMember(trappedCharacter);
            var combatantObject = Instantiate(Resources.Load<GameObject>("PlayerCombatant"), playerSpawn);
            var model = Instantiate(Resources.Load<GameObject>($"Characters/{partyMember.modelName}"), combatantObject.transform);

            var healthbar = Instantiate(Resources.Load<GameObject>("Health"), combatantObject.transform);

            //give cards
            var doubleDeck = new List<Card>(partyMember.deck);
            doubleDeck.AddRange(partyMember.deck);
            combatantObject.GetComponent<PlayerCombatant>().deck = doubleDeck;
            combatantObject.GetComponent<PlayerCombatant>().ShuffleDeck();
            combatantObject.GetComponent<PlayerCombatant>().DrawCards(4);

            //spread out combatants centered around spawn point
            var spacing = 1.5f;
            combatantObject.transform.localPosition = new Vector3((-0.5f*spacing*(BattleManager.Instance.combatants.Count+1)) + ((BattleManager.Instance.combatants.Count+1) * spacing), 0f, 0f);
            var combatant = combatantObject.GetComponent<Combatant>();
            BattleManager.Instance.AddCombatant(combatant);

            combatant.combatantName = partyMember.memberName;

            //print($"{combatant.combatantName} HP: {combatant.hp}/{combatant.maxHp} HP PERCENT {partyMember.hpPercentage}");
            YourParty.instance.GetStats(partyMember, out var attack, out var maxHp, out var speed, out var psychic);
            //check for equipment with other stats
            foreach(Equipment e in partyMember.equipment)
            {
                foreach(StatusEffect s in e.statusEffects){
                    
                    if(s.stat != "ATK" && s.stat != "DEF" && s.stat != "MAXHP" && s.stat != "SPD" && s.stat != "PSY")
                    {
                        combatant.statusEffects.Add(s);
                    }
                }
            }
                
            combatant.attack = attack; combatant.maxHp = maxHp; combatant.speed = speed; combatant.psychic = psychic;
            combatant.hp = Mathf.Max(hp,1);
            combatant.maxMp = combatant.psychic * 4;
            combatant.level = partyMember.level;
            combatant.defense = 1f;
            if(YourParty.instance.gameDifficulty.value == 2) combatant.defense = 0.8f;
            if(YourParty.instance.gameDifficulty.value == 0) combatant.defense = 1.2f;
            
            
            combatant.enabled = true;
            partyMember.alive = true; combatant.alive = true;
            combatant.GetComponentInChildren<Animator>().enabled = true;    
    }


    public override void DefaultAttack()
    {
        //
        GameManager.Instance.ShowMessage($"{trappedCharacter} is trapped!");
    }
}
