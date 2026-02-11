using System;
using UnityEngine;

public class InteractableButtonSwitchScene : InteractableObject
{
    [SerializeField] private String sceneName;

    public override void Interact()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
