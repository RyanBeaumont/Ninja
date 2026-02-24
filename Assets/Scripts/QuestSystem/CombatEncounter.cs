using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;

public class CombatEncounter : ChainedInteractable
{
    public List<GameObject> enemies;
    public List<GameObject> hideObjects;
    public AudioClip battleMusic;

    GameObject player;

    void StartBattle()
    {
        YourParty.instance.StartEncounter(enemies, transform, player);
        AudioManager.Instance.PlaySoundEffect("Battle");
        AudioManager.Instance.PlayMusic(battleMusic);
        if(transform.childCount > 0)
        transform.GetChild(0).gameObject.SetActive(false);
        GameManager.Instance.SetGameplayState(GameplayState.Combat);
        BattleManager.Instance.onWin += onWin; 
    }

    void onWin()
    {
        BattleManager.Instance.onWin -= onWin;
        GameManager.Instance.SetGameplayState(GameplayState.FreeMovement);
        if(transform.childCount > 0)
        transform.GetChild(0).gameObject.SetActive(true);
        player.SetActive(true);
        AudioManager.Instance.PlayMainTheme();
        if(GetComponent<CapsuleCollider>() != null)
        GetComponent<CapsuleCollider>().enabled = true;
            foreach(var obj in hideObjects)
            {
                obj.SetActive(true);
                if(obj.GetComponent<DefaultPose>() != null)
                {
                    obj.GetComponent<DefaultPose>().PlayDefault();
                }
            }
        CallNext();  
    }



    public override void Interact()
    {
        if(active){
            //SaveSystem.SaveGame(YourParty.instance.currentSaveFileName);
            print("Encounter Interact");
            player = GameObject.FindGameObjectWithTag("Player");
            player.SetActive(false);
                foreach(var obj in hideObjects)
                {
                    obj.SetActive(false);
                }
            StartBattle();
        }
    }

}
