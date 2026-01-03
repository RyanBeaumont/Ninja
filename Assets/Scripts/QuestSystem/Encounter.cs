using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;

public class Encounter : ChainedInteractable
{
    public List<GameObject> enemies;
    TMP_Text battleTitle;

    GameObject player;

    IEnumerator StartBattle()
    {
        print("Starting Battle");
        GetComponentInChildren<Animator>().Play("Bow");
        player.GetComponentInChildren<Animator>().Play("Bow");
        GameManager.Instance.SetGameplayState(GameplayState.Combat);
        battleTitle = GameObject.Find("MainCanvas/BattleHUD/BattleTitle").GetComponent<TMP_Text>();
        battleTitle.text = "GET READY";
        yield return new WaitForSeconds(0.3f);
        GameManager.Instance.SetGameplayState(GameplayState.Combat);
        GameManager.Instance.Freeze(1.7f);
        battleTitle.text = "FOR";
        yield return new WaitForSeconds(1.7f);
        
        transform.GetChild(0).gameObject.SetActive(false);
        GetComponent<CapsuleCollider>().enabled = false;
        battleTitle.text = "BATTLE!";
        for(int i=0; i<enemies.Count; i++)
        {
            var thisEnemy = Instantiate(enemies[i],transform.position + new Vector3(i*1f,0f,0f),transform.rotation);
        }
        yield return new WaitForSeconds(2f);
        battleTitle.text = "";
        while(GameObject.FindGameObjectsWithTag("Enemy").Length > 0)
        {
            yield return null;
        }
        GameManager.Instance.SetGameplayState(GameplayState.FreeMovement);
        if(next != null) {
            transform.GetChild(0).gameObject.SetActive(true);
            GetComponent<CapsuleCollider>().enabled = true;
            CallNext();
        }
        else
        {
            Destroy(gameObject);
        }
        
    }

    public override void Interact()
    {
        if(active){
            print("Encounter Interact");
            player = GameObject.FindGameObjectWithTag("Player");
            StartCoroutine(StartBattle());
        }
    }

}
