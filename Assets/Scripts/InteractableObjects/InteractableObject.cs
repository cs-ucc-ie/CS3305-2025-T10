using UnityEngine;

public abstract class InteractableObject : MonoBehaviour
{
    public string interactPrompt = "You must replace this prompt in child classes.";
    public abstract void Interact();
}
