using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

[Serializable]
public class EnemyTroop
{
    public List<GameObject> enemyPrefabs;
    public int startingLevel;
    public int endingLevel;
}
public class RandomEncounter : MonoBehaviour
{
    public List<EnemyTroop> possibleEncounters;
    public float encounterChance = 10f; //Percentage chance (0-100) of an encounter occurring
    public int gracePeriod = 0; //5 squares
    public void TriggerRandomEncounter(Transform position)
    {
        if(gracePeriod > 0)
        {
            gracePeriod--;
            return;
        }
        if (UnityEngine.Random.Range(0f, 100f) > encounterChance) return;
         List<EnemyTroop> validEncounters = new List<EnemyTroop>();
        foreach(var encounter in possibleEncounters)
        {
            int partyAvgLevel = 0;
            foreach(var memberName in YourParty.instance.partyMembers)
            {
                var member = YourParty.instance.GetPartyMember(memberName);
                partyAvgLevel += member.level;
            }
            partyAvgLevel /= YourParty.instance.partyMembers.Count;
            if(partyAvgLevel >= encounter.startingLevel && partyAvgLevel <= encounter.endingLevel)
            {
                validEncounters.Add(encounter);
            }
        }
        if(validEncounters.Count == 0) return;
        var thisEncounter = validEncounters[UnityEngine.Random.Range(0, validEncounters.Count)];
        YourParty.instance.StartEncounter(thisEncounter.enemyPrefabs, position);
        gracePeriod = 10;
        StartCoroutine(DisableTemporarily());
    }

    IEnumerator DisableTemporarily()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        player.SetActive(false);
        while(FindObjectsByType<BattleManager>(FindObjectsSortMode.None).Length > 0)
        {
            yield return null;
        }
        player.SetActive(true);
    }
}
