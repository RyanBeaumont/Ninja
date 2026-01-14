using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;

public class CombatEncounter : ChainedInteractable
{
    public List<GameObject> enemies;
    public AudioClip battleMusic;

    GameObject player;

    IEnumerator StartBattle()
    {
        YourParty.instance.StartEncounter(enemies, transform);
        AudioManager.Instance.PlaySoundEffect("Gong");
        AudioManager.Instance.PlayMusic(battleMusic);
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
        AudioManager.Instance.PlayMainTheme();
        GetComponent<CapsuleCollider>().enabled = true;
        CallNext();  
    }

    public override void Interact()
    {
        if(active){
            SaveSystem.SaveGame(YourParty.instance.currentSaveFileName);
            print("Encounter Interact");
            player = GameObject.FindGameObjectWithTag("Player");
            player.SetActive(false);
            StartCoroutine(StartBattle());
        }
    }

}
