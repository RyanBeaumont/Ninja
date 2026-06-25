using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsInteractable : ChainedInteractable
{
    public override void Interact()
    {
        Instantiate(Resources.Load<GameObject>("Credits"));
        Invoke(nameof(ReturnToMainMenu), 30f);
    }

    public void ReturnToMainMenu()
    {
        //Quit the game
        SceneManager.LoadScene("TitleScene");
    }

}
