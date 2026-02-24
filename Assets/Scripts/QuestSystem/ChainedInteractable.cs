using UnityEngine;

public abstract class ChainedInteractable : PersistentObject, IInteractable
{
    [Tooltip("If true, interaction stops here.")]
    [HideInInspector] public bool blockInteraction;
    [HideInInspector] public IInteractable next;
    [HideInInspector] public string failBranch;

    public abstract void Interact();
    public virtual string GetPromptMessage()
    {
        // Default: return GameObject name
        return $"{FormatName(gameObject.name)}";
    }

    public void CallNext()
    {
        if (!blockInteraction && active && (GameManager.Instance.GetGameplayState() == GameplayState.FreeMovement || GameManager.Instance.GetGameplayState() == GameplayState.Dialog))
        {
            if (next != null){next.Interact(); return;}

        }

        //If no further interactions
        GameManager.Instance.SetGameplayState(GameplayState.FreeMovement);
        if(this is not Door)
            GameManager.Instance.DestroyCamera();
    }

    public void Fail()
    {
        foreach(Branch branch in GetComponents<Branch>())
        {
            if (branch.branchName.Equals(failBranch))
            {
                branch.CallNext();
                return;
            }
        }
        //No branch found
        GameManager.Instance.SetGameplayState(GameplayState.FreeMovement);
        if(this is not Door)
            GameManager.Instance.DestroyCamera();
        
    }

    protected override void Awake()
    {
        base.Awake();
        // auto-find the next interactable in chain
        var interactables = GetComponents<IInteractable>();
        int thisIndex = System.Array.IndexOf(interactables, this);
        if (thisIndex >= 0 && thisIndex < interactables.Length - 1)
            next = interactables[thisIndex + 1];
    }

    string FormatName(string raw)
{
    if (string.IsNullOrEmpty(raw)) return "";

    System.Text.StringBuilder sb = new System.Text.StringBuilder();
    sb.Append(char.ToUpper(raw[0]));

    for (int i = 1; i < raw.Length; i++)
    {
        char c = raw[i];
        // insert a space before capital letters or numbers following letters
        if ((char.IsUpper(c) && !char.IsWhiteSpace(raw[i - 1])) ||
            (char.IsDigit(c) && char.IsLetter(raw[i - 1])))
        {
            sb.Append(' ');
        }
        sb.Append(c);
    }

    return sb.ToString();
}
}