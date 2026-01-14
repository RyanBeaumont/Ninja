using TMPro;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public RectTransform savePanel;
    public Transform saves;

    void Start()
    {
        savePanel.gameObject.SetActive(false);
    }

    public void StartNewGame()
    {
        var GameManager = Object.Instantiate(Resources.Load<GameObject>("GameManager"));
        var YourParty = Object.Instantiate(Resources.Load<GameObject>("YourParty"));
        YourParty.GetComponent<YourParty>().BuildStartingDeck();
        GameManager.GetComponent<GameManager>().ChangeScene("DojoInterior",0,0);
    }

    public void ShowSavePanel()
    {
        savePanel.gameObject.SetActive(true);
        foreach (Transform child in saves){Destroy(child.gameObject);}
        foreach (var save in SaveSystem.GetAllSaves())
        {
            GameObject loadButton = Instantiate(Resources.Load<GameObject>("LoadButton"), saves);
            loadButton.GetComponent<LoadGameButton>().SetSaveData(save);
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            savePanel.gameObject.SetActive(false);
        }
    }

    public void LoadGame(SaveData data)
    {
        
        if (data != null)
        {
            var GameManager = Object.Instantiate(Resources.Load<GameObject>("GameManager"));
            GameManager.GetComponent<GameManager>().SpawnPlayer(0);
            var YourParty = Object.Instantiate(Resources.Load<GameObject>("YourParty"));
            YourParty.GetComponent<YourParty>().LoadGame(data);
        }
        else
        {
            Debug.Log("No save data found.");
        }
    }
}
