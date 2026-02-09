using System;
using UnityEngine;

public class CheckInteractable : MonoBehaviour
{
    public float interactDistance = 2f;
    private InteractableObject interactableObject;
    public static Action<string> onInteractableObjectFound;
    public static Action onNoInteractableObject;
    private bool lastTimeInteractable = false;

    void Start()
    {
        InputManager.OnInteractPressed += TryInteract;
    }

    void OnDisable()
    {
        InputManager.OnInteractPressed -= TryInteract;
    }

    public bool CanInteract()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            interactableObject = hit.collider.gameObject.GetComponent<InteractableObject>();
            if (interactableObject != null) return true;
        }
        return false;
    }

    void Update()
    {
        if (CanInteract() && !lastTimeInteractable)
        {
            onInteractableObjectFound?.Invoke(interactableObject.interactPrompt);
            lastTimeInteractable = true;
        }
        else if (!CanInteract() && lastTimeInteractable)
        {
            onNoInteractableObject?.Invoke();
            lastTimeInteractable = false;
        }
    }

    public void TryInteract()
    {
        if (CanInteract())
        {
            interactableObject.Interact();
        }
    }
}
