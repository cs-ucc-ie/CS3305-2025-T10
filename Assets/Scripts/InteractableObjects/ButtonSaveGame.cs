using System;
using UnityEngine;
using System.Collections;   

public class ButtonSaveGame : InteractableObject
{
    private string originalPrompt = "Save voyage log";
    private string saveCompletePrompt = "Save Complete";

    void Start()
    {
        interactPrompt = originalPrompt;
    }

    public override void Interact()
    {
        SaveManager.Save();
        StartCoroutine(ShowSaveComplete());
    }

    private IEnumerator ShowSaveComplete()
    {
        // this.enabled = false;
        // this.enabled = true;
        interactPrompt = saveCompletePrompt;
        yield return new WaitForSeconds(5f); 
        interactPrompt = originalPrompt; 
}
 }