using UnityEngine;

public class CalendarInteractable : ChainedInteractable
{
    bool imactive = false;
    float timer = -1f;
    GameObject calendar;
    bool dayAdvanced = false;
    public override void Interact()
    {
        calendar = Instantiate(Resources.Load<GameObject>("Calendar"));
        calendar.GetComponent<Calendar>().dayAdvanced = dayAdvanced;
        imactive = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        GameManager.Instance.SetGameplayState(GameplayState.Dialog);
    }

    void Update()
    {
        if(imactive && Input.GetButtonUp("Interact"))
        {
            timer = 2f;
            dayAdvanced = true;
            imactive = false;
        }
        if(timer > 0f)
        {
            timer -= Time.unscaledDeltaTime;
            if(timer <= 0f)
            {
                if(calendar != null)Destroy(calendar);
                YourParty.instance.day ++;
                timer = -1f;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                GameManager.Instance.SetGameplayState(GameplayState.FreeMovement);
                CallNext();
            }
        }
    }
}
