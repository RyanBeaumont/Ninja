using UnityEngine;

public class Door : ChainedInteractable
{
    public string sceneName;
    public int spawnPointIndex;
    public int sceneVariant = 0;
    public Material skybox;
    public Transform cameraTarget;
    public override void Interact()
    {
        if(active)
        GameManager.Instance.StartSceneTransition(sceneName, spawnPointIndex, sceneVariant, cameraTarget);
        if(skybox != null)
        {
            Camera.main.GetComponent<Skybox>().material = skybox;
        }
    }
}
