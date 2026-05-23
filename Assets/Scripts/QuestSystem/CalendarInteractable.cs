using UnityEngine;

public class CalendarInteractable : ChainedInteractable
{
    bool imactive = false;
    float timer = -1f;
    GameObject calendar;
    public override void Interact()
    {
        calendar = Instantiate(Resources.Load<GameObject>("Calendar"));
        imactive = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        if(imactive && Input.GetKeyDown(KeyCode.Mouse0))
        {
            timer = 2f;
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
                CallNext();
            }
        }
    }
}
