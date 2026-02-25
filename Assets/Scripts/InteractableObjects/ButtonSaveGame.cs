using System;
using UnityEngine;
using System.Collections;   

public class ButtonSaveGame : InteractableObject
{
    private string originalPrompt = "Save voyage log";

    void Start()
    {
        interactPrompt = originalPrompt + $" (Slot {SaveManager.saveSlotIndex + 1})";
    }

    public override void Interact()
    {
        SaveManager.Save();
        UIController.Instance.AddNewInformation($"Game saved to slot {SaveManager.saveSlotIndex + 1}!");
    }
 }