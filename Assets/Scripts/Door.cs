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
        if(!active) return;
        if(spawnPointIndex == -1) spawnPointIndex = GameManager.Instance.currentSpawnPointIndex;
        if(sceneVariant == -1) sceneVariant = GameManager.Instance.sceneVariant;
        GameManager.Instance.SetGameplayState(GameplayState.FreeMovement);
        GameManager.Instance.StartSceneTransition(sceneName, spawnPointIndex, sceneVariant, cameraTarget, skybox);
        CallNext();
    }
}
