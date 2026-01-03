using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;

public class CombatEncounter : ChainedInteractable
{
    public List<GameObject> enemies;
    TMP_Text battleTitle;

    GameObject player;

    IEnumerator StartBattle()
    {
        YourParty.instance.StartEncounter(enemies, transform);
        transform.GetChild(0).gameObject.SetActive(false);
        GameManager.Instance.SetGameplayState(GameplayState.Combat);
        yield return new WaitForSeconds(2f);


        while(FindObjectsByType<BattleManager>(FindObjectsSortMode.None).Length > 0)
        {
            yield return null;
        }

        GameManager.Instance.SetGameplayState(GameplayState.FreeMovement);
        transform.GetChild(0).gameObject.SetActive(true);
        player.SetActive(true);
        GetComponent<CapsuleCollider>().enabled = true;
        CallNext();  
    }

    public override void Interact()
    {
        if(active){
            print("Encounter Interact");
            player = GameObject.FindGameObjectWithTag("Player");
            player.SetActive(false);
            StartCoroutine(StartBattle());
        }
    }

}
