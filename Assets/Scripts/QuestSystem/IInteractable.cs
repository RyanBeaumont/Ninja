using UnityEngine;
public interface IInteractable
{
    string GetPromptMessage();   // e.g. "Press E to Interact"
    void Interact(); // player passes itself in
}