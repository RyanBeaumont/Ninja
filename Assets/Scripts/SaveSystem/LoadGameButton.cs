
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadGameButton : MonoBehaviour
{
    public SaveData saveData;
    public bool save = false; //load is true
    public void SetSaveData(SaveData data, bool saveInsteadOfLoad = false)
    {
        saveData = data;
        save = saveInsteadOfLoad;
        //Show playtime in hours and minutes rounded
        GetComponentInChildren<TMPro.TMP_Text>().text = $"{data.locationName} - {Mathf.Round(data.playTime / 3600f)}h {Mathf.Round(data.playTime % 3600 / 60f)}m";
        foreach(var partyMember in data.playersInParty)
        {
           var portrait = Resources.Load<Sprite>($"Sprites/{partyMember}");
           if(portrait != null)
           {
            var uiObject = Instantiate(Resources.Load<GameObject>("TurnIcon"), transform);
            uiObject.GetComponent<Image>().sprite = portrait;
            uiObject.transform.localScale = Vector3.one * 2f;
            uiObject.transform.Find("Initial").GetComponent<TMP_Text>().text = "";
           }
        }
    }

    public void OnClick()
    { 
        if(save == false){
            if (saveData != null)
            {
                var GameManager = Object.Instantiate(Resources.Load<GameObject>("GameManager"));
                //GameManager.GetComponent<GameManager>().SpawnPlayer(0);
                var YourParty = Object.Instantiate(Resources.Load<GameObject>("YourParty"));
                YourParty.GetComponent<YourParty>().LoadGame(saveData);
            }
            else
            {
                Debug.Log("No save data found.");
            }
        }
        else
        {
            //YourParty.instance.currentSaveFileName = saveData.saveFileName;
        }
        
    }
}
