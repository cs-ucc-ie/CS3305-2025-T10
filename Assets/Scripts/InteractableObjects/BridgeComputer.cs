using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PartMatch
{
    public KeyItem unusedPart;
    public KeyItem usedPart;
    public GameObject lightObject;
    public GameObject gearSlotObject;
    public GameObject doorObject;
}

public class BridgeComputer : InteractableObject
{
    [Header("Part References")]
    [SerializeField] private List<PartMatch> partMatches = new List<PartMatch>();

    [Header("Materials")]
    [SerializeField] private Material redLightMaterial;   // 红灯材质
    [SerializeField] private Material greenLightMaterial; // 绿灯材质

    private bool allPartsInstalled = false;



    private void Start()
    {
        interactPrompt = "Install Communication Part";

        UpdateLightsAndGearsAndDoors();

    }

    private void Update()
    {
    }

    public override void Interact()
    {
        if (allPartsInstalled)
        {
            UIController.Instance.AddNewInformation("Not yet implemented.");
            return;
        }

        foreach (var partMatch in partMatches)
        {
            if (InventoryManager.Instance.HasItem(partMatch.unusedPart, 1))
            {
                ActivatePart(partMatch.unusedPart, partMatch.lightObject, partMatch.gearSlotObject, partMatch.usedPart);
                return;
            }
        }

        UIController.Instance.AddNewInformation("No unused parts available to install.");
    }

    private void ActivatePart(KeyItem part, GameObject light, GameObject gearSlot, KeyItem usedVersion)
    {
        do { } while (InventoryManager.Instance.RemoveItem(part, 1)); // Ensure the item is removed before proceeding
        InventoryManager.Instance.AddItem(usedVersion);

        UpdateLightsAndGearsAndDoors();
        UIController.Instance.AddNewInformation($"{part.itemName} installed!");
        CheckAllPartsInstalled();
    }

    private void CheckAllPartsInstalled()
    {
        foreach (var partMatch in partMatches)
        {
            if (!InventoryManager.Instance.HasItem(partMatch.usedPart, 1))
            {
                return; // If any used part is missing, exit the method
            }
        }
        UIController.Instance.AddNewInformation("All parts installed. System fully activated.");
        interactPrompt = "Send Distress Signal";
        allPartsInstalled = true;
    }

    private void UpdateLightsAndGearsAndDoors()
    {
        foreach (var partMatch in partMatches)
        {
            bool isUsed = InventoryManager.Instance.HasItem(partMatch.usedPart, 1);
            UpdateSingleLight(partMatch.lightObject, partMatch.gearSlotObject, isUsed);
            if (partMatch.doorObject != null)
                UpdateSingleDoor(partMatch.doorObject, isUsed);
        }
    }

    private void UpdateSingleLight(GameObject light, GameObject gearSlot, bool isUsed)
    {
        Debug.Log($"Updating light and gear for {(isUsed ? "used" : "unused")} part. Light: {light.name}, GearSlot: {gearSlot.name}");
        if (light != null)
        {
            Renderer lightRenderer = light.GetComponent<Renderer>();
            if (lightRenderer != null)
            {
                if (isUsed && greenLightMaterial != null)
                {
                    lightRenderer.material = greenLightMaterial;
                }
                else if (!isUsed && redLightMaterial != null)
                {
                    lightRenderer.material = redLightMaterial;
                }
            }
        }

        if (gearSlot != null)
        {
            Debug.Log($"Setting gear slot '{gearSlot.name}' active state to {isUsed}");

            gearSlot.SetActive(isUsed);
        }
    }

    private void UpdateSingleDoor(GameObject door, bool enabled)
    {
        Debug.Log($"Updating door '{door.name}' to {(enabled ? "enabled" : "disabled")} state.");
        if (door != null)
        {
            InteractableDoorSwitchScene doorComponent = door.GetComponent<InteractableDoorSwitchScene>();
            Debug.Log($"Door component found: {doorComponent != null}");
            Debug.Log($"Setting door '{door.name}' enabled state to {enabled}");
            if (doorComponent != null)
            {
                doorComponent.enabled = enabled;
            }
        }
    }
}
