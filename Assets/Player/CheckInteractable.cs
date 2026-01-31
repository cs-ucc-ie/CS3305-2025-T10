using UnityEngine;

public class CheckInteractable : MonoBehaviour
{
    /* 
        This script checks for interactable objects in front of the player.
        If the player presses the "E" key while looking at an interactable object within a certain distance,
        it calls the Interact method on that object.
    */
    public float interactDistance = 2f;
    private InteractableObject interactableObject;

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            interactableObject = hit.collider.gameObject.GetComponent<InteractableObject>();
            if (interactableObject != null)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    interactableObject.Interact();
                }
            }
        }
    }
}
