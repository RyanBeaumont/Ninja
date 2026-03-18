using UnityEngine;
using UnityEngine.UI;

public class CombinationLock : ChainedInteractable
{
    CombinationLockDigit[] digits; // Assign 4 in inspector
    public int[] correctCode = new int[4]; // e.g. 1,2,3,4
    GameObject ui;

    public override void Interact()
    {
        ui = Instantiate(Resources.Load<GameObject>("CombinationLock"));
        digits = ui.GetComponentsInChildren<CombinationLockDigit>();
        var submitButton = ui.transform.Find("Submit").GetComponent<UnityEngine.UI.Button>();
        submitButton.onClick.AddListener(CheckCode);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        GameManager.Instance.SetGameplayState(GameplayState.Dialog);
        Time.timeScale = 0f;
    }

    public void CheckCode()
    {
        for (int i = 0; i < digits.Length; i++)
        {
            if (digits[i].Value != correctCode[i])
            {
                Cleanup();
                AudioManager.Instance.PlaySoundEffect("Negative");
                Fail();
                return;
            }
        }

        Cleanup();
        AudioManager.Instance.PlaySoundEffect("ChaChing");
        CallNext();
    }

    void Cleanup()
    {
        Time.timeScale = 1f;
        if (ui != null) Destroy(ui);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        GameManager.Instance.SetGameplayState(GameplayState.FreeMovement);
    }
}