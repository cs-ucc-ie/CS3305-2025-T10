using UnityEngine;
using System.Collections;

public class ClosedDoor : InteractableObject 
{
    
    [Header("Door Settings")]
    public GameObject doorObject;
    public float moveDistance = 3.0f;
    public float moveDuration = 1.0f;
    public bool openPermanently;
    public float waitTime = 2.0f;

    public CrushZone bottomCrushZone;

    private bool isMoving = false;
    private bool isOpen = false;

    public override void Interact() 
    {
        if ( !isMoving && !isOpen) 
        {
            StartCoroutine(AnimateDoor());
        }
    }

    private IEnumerator AnimateDoor() 
    {
        isMoving = true;
        interactPrompt = "Opening....";

        Vector3 closedPos = doorObject.transform.position;
        Vector3 openPos = closedPos + (Vector3.up * moveDistance);

        // Move up
        bottomCrushZone.isActive = false;
        yield return StartCoroutine(MoveToPosition(doorObject.transform, openPos));
        isOpen = true;
        interactPrompt = openPermanently ? "Open" : "About to close";

        if (!openPermanently) 
        {
            yield return new WaitForSeconds(waitTime);
            bottomCrushZone.isActive = true;
            yield return StartCoroutine(MoveToPosition(doorObject.transform, closedPos));
            isOpen = false;
            bottomCrushZone.isActive = false;
            interactPrompt = "Open";
        }
        isMoving = false;
    }

    private IEnumerator MoveToPosition(Transform target, Vector3 endPos) 
    {
        float elapsed = 0;
        Vector3 startPos = target.position;
        while (elapsed < moveDuration) 
        {
            target.position = Vector3.Lerp(startPos, endPos, elapsed/moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        target.position = endPos;
    }
}
