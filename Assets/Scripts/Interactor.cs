using TMPro;
using UnityEngine;
using UnityEngine.UI;  // if using UI Text

public class PlayerInteractor : MonoBehaviour
{
    public float interactRange = 2f;
    public KeyCode interactKey = KeyCode.E;
    public GameObject promptPrefab;  // assign a prefab in Inspector
    public LayerMask lineOfSightMask;

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

    Vector3 origin = transform.position + Vector3.up * 1f;

    foreach (var hit in hits)
    {
        var interactable = hit.GetComponent<ChainedInteractable>();
        if (interactable == null) continue;
        if (!interactable.active) continue;
        if (interactable.GetComponentInParent<TriggerInteractable>() != null) continue;

        Vector3 targetPoint = hit.bounds.center;
        Vector3 dir = (targetPoint - origin).normalized;
        float dist = Vector3.Distance(origin, targetPoint);

        // Raycast for line of sight
        if (Physics.Raycast(origin, dir, out RaycastHit rayHit, dist, lineOfSightMask, QueryTriggerInteraction.Ignore))
        {
            // Only valid if we hit THIS interactable
            if (rayHit.collider != hit){
                print($"Blocked by {rayHit.collider.gameObject.name}");
                continue;
            }
        }

        if (dist < closest)
        {
            closest = dist;
            currentTarget = interactable;
        }
    }
}
}
