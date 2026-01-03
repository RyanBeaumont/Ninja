using TMPro;
using UnityEngine;
using UnityEngine.UI;  // if using UI Text

public class PlayerInteractor : MonoBehaviour
{
    public float interactRange = 2f;
    public KeyCode interactKey = KeyCode.E;
    public GameObject promptPrefab;  // assign a prefab in Inspector

    private ChainedInteractable currentTarget;
    private GameObject currentPrompt;
    GameObject promptInstance;
    TMP_Text promptText;

    void Start()
    {
        // Create the prompt once at the beginning
        promptInstance = Instantiate(promptPrefab);
        promptText = promptInstance.GetComponentInChildren<TMPro.TMP_Text>();
        promptInstance.SetActive(false); // hidden at start
    }

    void Update()
    {
        FindInteractable();

        if (currentTarget != null && CanInteract())
        {
            // Update prompt position & text
            promptInstance.SetActive(true);
            promptInstance.transform.position =
                ((MonoBehaviour)currentTarget).transform.position + Vector3.up * 0f;
            promptText.text = $"[E] {currentTarget.GetPromptMessage()}";

            // Interaction
            if (Input.GetKeyDown(interactKey))
            {
                currentTarget.Interact();
            }
        }
        else
        {
            promptInstance.SetActive(false);
        }
    }

    bool CanInteract()
    {
        DialogBox d = FindFirstObjectByType<DialogBox>();
        if(GameManager.Instance.GetGameplayState() != GameplayState.FreeMovement) return false;
        if (d != null && d.GetComponent<Canvas>().enabled) return false;
        return true;
    }

    void FindInteractable()
    {
        currentTarget = null;

        Collider[] hits = Physics.OverlapSphere(transform.position, interactRange);
        float closest = Mathf.Infinity;

        foreach (var hit in hits)
        {
            var interactable = hit.GetComponent<ChainedInteractable>();
            if (interactable != null && interactable.active && interactable.GetComponent<TriggerInteractable>() == null)
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < closest)
                {
                    closest = dist;
                    currentTarget = interactable;
                }
            }
        }
    }
}
