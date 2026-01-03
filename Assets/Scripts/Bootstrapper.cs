using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class Bootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void EnsureGameController()
    {
        if (GameManager.Instance == null)
        {
            
            var GameManager = Object.Instantiate(Resources.Load<GameObject>("GameManager"));
            var YourParty = Object.Instantiate(Resources.Load<GameObject>("YourParty"));
            Debug.Log("GameManager instantiated by Bootstrapper.");
            GameManager.GetComponent<GameManager>().SpawnPlayer(0);
        }
        
    }
}