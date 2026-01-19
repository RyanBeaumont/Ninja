using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;

public class CombatEncounter : ChainedInteractable
{
    public List<GameObject> enemies;
    public AudioClip battleMusic;

    GameObject player;

    void StartBattle()
    {
        YourParty.instance.StartEncounter(enemies, transform, player);
        AudioManager.Instance.PlaySoundEffect("Battle");
        AudioManager.Instance.PlayMusic(battleMusic);
        transform.GetChild(0).gameObject.SetActive(false);
        GameManager.Instance.SetGameplayState(GameplayState.Combat);
        BattleManager.Instance.onWin += onWin; 
    }

    void onWin()
    {
        BattleManager.Instance.onWin -= onWin;
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
            //SaveSystem.SaveGame(YourParty.instance.currentSaveFileName);
            print("Encounter Interact");
            player = GameObject.FindGameObjectWithTag("Player");
            player.SetActive(false);
            StartBattle();
        }
    }

}
