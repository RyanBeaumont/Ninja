using UnityEngine;
using UnityEngine.UI;

public class ComputerTerminalInteractible : ChainedInteractable
{
    bool isActive = false;
    GameObject prefab;
    public override void Interact()
    {
        isActive = true;
        prefab = Instantiate(Resources.Load<GameObject>("Computer"));
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
        GameManager.Instance.SetGameplayState(GameplayState.Dialog);
        var exitButton = prefab.transform.Find("Terminal/Exit").GetComponent<Button>();
        exitButton.onClick.AddListener(Exit);
    }

    void Exit()
    {
        Destroy(prefab.gameObject);
        isActive = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
        GameManager.Instance.SetGameplayState(GameplayState.FreeMovement);
        CallNext();
    }
}
