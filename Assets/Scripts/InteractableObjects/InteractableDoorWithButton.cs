using UnityEngine;
using System.Collections;

public class InteractableDoorWithButton : InteractableObject
{
    /* 
        This script makes the door move up when interacted with,
        waits for a specified duration, and then moves back down.
        */
    private bool isBusy = false;
    [SerializeField] float moveDistance = 2.5f;
    [SerializeField] float moveDuration = 0.5f;
    [SerializeField] float waitDuration = 0.5f;
    [SerializeField] private GameObject doorObject;
    // public Item key;
    private Vector3 startPos;
    
    public override void Interact()
    {
        if (!isBusy /*&& InventoryManager.Instance.HasItem(key, 1)*/)
        {
            isBusy = true;
            //InventoryManager.Instance.RemoveItem(key, 1);
            startPos = doorObject.transform.position;
            StartCoroutine(MoveRoutine());
        }
    }

    IEnumerator MoveRoutine()
    {
        yield return StartCoroutine(MoveTo(startPos + Vector3.up * moveDistance, moveDuration));
        yield return new WaitForSeconds(waitDuration);
        yield return StartCoroutine(MoveTo(startPos, moveDuration));
        isBusy = false;
    }

    IEnumerator MoveTo(Vector3 target, float duration)
    {
        Vector3 from = doorObject.transform.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            doorObject.transform.position = Vector3.Lerp(from, target, t);
            yield return null;
        }

        doorObject.transform.position = target;
    }
}
