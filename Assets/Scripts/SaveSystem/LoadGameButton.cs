using UnityEngine;

public class LoadGameButton : MonoBehaviour
{
    SaveData saveData;
    public void SetSaveData(SaveData data)
    {
        saveData = data;
        GetComponentInChildren<TMPro.TMP_Text>().text = data.sceneName + " - " + (data.playTime/60).ToString("F2") + "m";
    }

    public void OnClick()
    { 
        if (saveData != null)
        {
            var GameManager = Object.Instantiate(Resources.Load<GameObject>("GameManager"));
            GameManager.GetComponent<GameManager>().SpawnPlayer(0);
            var YourParty = Object.Instantiate(Resources.Load<GameObject>("YourParty"));
            YourParty.GetComponent<YourParty>().LoadGame(saveData);
        }
        else
        {
            Debug.Log("No save data found.");
        }
    }
}
