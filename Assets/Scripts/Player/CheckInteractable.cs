using UnityEngine;

public class CheckInteractable : MonoBehaviour
{
    public float interactDistance = 2f;
    private InteractableObject interactableObject;

    void Start()
    {
        InputManager.OnInteractPressed += TryInteract;
    }

    void OnDisable()
    {
        InputManager.OnInteractPressed -= TryInteract;
    }

    public void TryInteract()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            interactableObject = hit.collider.gameObject.GetComponent<InteractableObject>();
            if (interactableObject != null) interactableObject.Interact();
            else Debug.Log("Nothing to interact.");
        }
    }
}
