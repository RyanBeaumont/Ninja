using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteractor : MonoBehaviour
{
    public float interactRange = 2f;
    public GameObject promptPrefab;
    public LayerMask lineOfSightMask;

    private ChainedInteractable currentTarget;
    private GameObject promptInstance;
    private TMP_Text promptText;
    private Image promptIconImage;
    private string currentIconsPath;

    void Start()
    {
        promptInstance = Instantiate(promptPrefab);
        promptText = promptInstance.GetComponentInChildren<TMP_Text>();

        Transform iconTransform = promptInstance.transform.Find("key");
        if (iconTransform != null)
        {
            promptIconImage = iconTransform.GetComponent<Image>();
        }

        if (promptIconImage == null)
        {
            promptIconImage = promptInstance.GetComponentInChildren<Image>();
        }

        promptInstance.SetActive(false);
        RefreshPromptIcon();
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.inputIconsChanged += RefreshPromptIcon;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.inputIconsChanged -= RefreshPromptIcon;
        }
    }

    void Update()
    {
        FindInteractable();

        if (currentTarget != null && CanInteract() && GameManager.Instance.GetGameplayState() == GameplayState.FreeMovement)
        {
            promptInstance.SetActive(true);
            promptInstance.transform.position = ((MonoBehaviour)currentTarget).transform.position + Vector3.up * 0f;
            promptText.text = currentTarget.GetPromptMessage();

            if (Input.GetButtonDown("Interact"))
            {
                currentTarget.Interact();
                promptInstance.SetActive(false);
            }
        }
        else
        {
            promptInstance.SetActive(false);
        }
    }


    private void RefreshPromptIcon()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        string iconsPath = GameManager.Instance.inputIconsPath;
        if (string.IsNullOrEmpty(iconsPath) || iconsPath == currentIconsPath)
        {
            return;
        }

        currentIconsPath = iconsPath;
        if (promptIconImage == null)
        {
            return;
        }

        promptIconImage.sprite = Resources.Load<Sprite>($"{iconsPath}/Select");
    }

    bool CanInteract()
    {
        DialogBox d = FindFirstObjectByType<DialogBox>();
        if (GameManager.Instance.GetGameplayState() != GameplayState.FreeMovement) return false;
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

            if (Physics.Raycast(origin, dir, out RaycastHit rayHit, dist, lineOfSightMask, QueryTriggerInteraction.Ignore))
            {
                if (rayHit.collider != hit)
                {
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
