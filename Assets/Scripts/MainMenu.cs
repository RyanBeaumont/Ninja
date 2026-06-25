using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
        GameManager.GetComponent<GameManager>().ChangeScene("Overworld",99,9);
    }

    public void StartChapter2()
    {
        var GameManager = Object.Instantiate(Resources.Load<GameObject>("GameManager"));
        var YourParty = Object.Instantiate(Resources.Load<GameObject>("YourParty"));
        YourParty.GetComponent<YourParty>().BuildStartingDeck();
        var gm = GameManager.GetComponent<GameManager>();
        gm.AddInventoryItem("Coke",4);
        gm.AddInventoryItem("Bang",1);
       gm.ChangeScene("Cabin",0,0);
    }

      public void StartChapter3()
    {
        var GameManager = Object.Instantiate(Resources.Load<GameObject>("GameManager"));
        var YourParty = Object.Instantiate(Resources.Load<GameObject>("YourParty"));
        YourParty.GetComponent<YourParty>().BuildStartingDeck();
        var gm = GameManager.GetComponent<GameManager>();
        gm.AddInventoryItem("Coke",4);
        gm.AddInventoryItem("DrPepper",1);
        gm.AddInventoryItem("Bicycle Helmet",1);
        gm.AddInventoryItem("Brass Knuckles",1);
        gm.AddInventoryItem("Bang",1);
        gm.AddInventoryItem("Coffee",1);
       gm.ChangeScene("StormHouse",0,0);
    }

     public void StartChapter4()
    {
        var GameManager = Object.Instantiate(Resources.Load<GameObject>("GameManager"));
        var YourParty = Object.Instantiate(Resources.Load<GameObject>("YourParty"));
        YourParty.GetComponent<YourParty>().BuildStartingDeck();
        var gm = GameManager.GetComponent<GameManager>();
        gm.AddInventoryItem("Coke",4);
        gm.AddInventoryItem("DrPepper",1);
        gm.AddInventoryItem("Bicycle Helmet",1);
        gm.AddInventoryItem("Brass Knuckles",1);
        gm.AddInventoryItem("Shirt of High Ab Visibility",1);
        gm.AddInventoryItem("Dark Black Clothes",1);
       gm.ChangeScene("SpartanDojo",2,1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ShowSavePanel()
    {
        Debug.Log("Show Save Panel");
        savePanel.gameObject.SetActive(true);
        foreach (Transform child in saves){Destroy(child.gameObject);}
        var allSaves = SaveSystem.GetAllSaves();
        for(int i = 0; i < allSaves.Count; i++)
        {
            var save = allSaves[i];
            GameObject loadButton = Instantiate(Resources.Load<GameObject>("LoadButton"), saves);
            loadButton.GetComponent<LoadGameButton>().SetSaveData(save);
            //set button selected if it's the first - select via Selectable to ensure Submit works
            if(i == 0)
            {
                var btnSelectable = loadButton.GetComponent<Selectable>();
                var es = EventSystem.current ?? FindFirstObjectByType<EventSystem>();
                if(es != null)
                {
                    es.SetSelectedGameObject(null);
                    if(btnSelectable != null) btnSelectable.Select();
                    es.SetSelectedGameObject(loadButton);
                }
            }
        }
    }

    void Update()
    {
        if(Input.GetButtonDown("Cancel") && savePanel.gameObject.activeSelf)
        {
            Debug.Log("Hide Save Panel");
            //Select the first button in the main menu when closing the save panel
            var firstButton = GetComponentInChildren<Button>();
            if(firstButton != null)
            {
                var btnSelectable = firstButton.GetComponent<Selectable>();
                var es = EventSystem.current ?? FindFirstObjectByType<EventSystem>();
                if(es != null)
                {
                    es.SetSelectedGameObject(null);
                    if(btnSelectable != null) btnSelectable.Select();
                    es.SetSelectedGameObject(firstButton.gameObject);
                }
            }
            savePanel.gameObject.SetActive(false);
        }
    }

   
}
