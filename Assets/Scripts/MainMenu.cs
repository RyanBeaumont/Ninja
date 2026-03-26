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

   
}
