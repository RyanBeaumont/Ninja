using UnityEngine;

public class Door : ChainedInteractable
{
    public string sceneName;
    public int spawnPointIndex;
    public int sceneVariant = 0;
    public Material skybox;
    public override void Interact()
    {
        if(active)
        GameManager.Instance.StartSceneTransition(sceneName, spawnPointIndex, sceneVariant);
        if(skybox != null)
        {
            Camera.main.GetComponent<Skybox>().material = skybox;
        }
    }
}
