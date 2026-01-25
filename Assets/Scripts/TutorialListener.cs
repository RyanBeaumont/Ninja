using System.Collections.Generic;
using UnityEngine;

public class TutorialListener : AnimationListener
{
    List<string> completedMessages = new List<string>();
    public override void SlowMo(string message)
    {
        base.SlowMo(message);
        if(message != "" && !completedMessages.Contains(message)){
        FindFirstObjectByType<Menu>().ShowTutorialMessage(message);
        Time.timeScale = 0.0125f;
        completedMessages.Add(message);
        }
    }
    
}
