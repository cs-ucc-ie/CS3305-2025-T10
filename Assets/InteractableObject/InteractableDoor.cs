using UnityEngine;
using System.Collections;

public class InteractableDoor : InteractableObject
{
    /* 
        This script makes the door move up when interacted with,
        waits for a specified duration, and then moves back down.
        It requires a key item in the inventory to operate.
        */
    private bool isBusy = false;
    public float moveDistance = 2.5f;
    public float moveDuration = 1f;
    public float waitDuration = 3f;
    public Item key;
    private Vector3 startPos;
    public override void Interact()
    {
        if (!isBusy && InventoryManager.Instance.HasItem(key, 1))
        {
            isBusy = true;
            InventoryManager.Instance.RemoveItem(key, 1);
            startPos = transform.position;
            StartCoroutine(MoveRoutine());
        }
    }

    IEnumerator MoveRoutine()
    {
        yield return StartCoroutine(MoveTo(startPos + Vector3.up * moveDistance, moveDuration));
        // yield return new WaitForSeconds(waitDuration);
        // yield return StartCoroutine(MoveTo(startPos, moveDuration));
        // isBusy = false;
    }

    IEnumerator MoveTo(Vector3 target, float duration)
    {
        Vector3 from = transform.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.position = Vector3.Lerp(from, target, t);
            yield return null;
        }

        transform.position = target;
    }
}
