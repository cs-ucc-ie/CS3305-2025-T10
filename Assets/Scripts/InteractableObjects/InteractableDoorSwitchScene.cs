using System;
using UnityEngine;

public class InteractableDoorSwitchScene : InteractableObject
{
    [SerializeField] private String sceneName;

    public override void Interact()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
