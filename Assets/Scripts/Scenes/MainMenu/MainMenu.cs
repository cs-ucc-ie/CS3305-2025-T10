using UnityEngine;
using UnityEngine.Playables;

public class MainMenu : MonoBehaviour
{  
    public float rotationSpeedUp;
    public float rotationSpeedForward;
    public float rotationSpeedLeft;
    public GameObject newGamePanel;
    public GameObject loadGamePanel;
    public GameObject aboutPanel;

    private Camera mainCamera;

    public GameObject gameManager;
    public GameObject gameManagerFromOtherScene;

     void OnEnable()
    {
        gameManager.SetActive(false);
    }

    void OnDisable()
    {
        gameManager.SetActive(true);
    }

    void Start()
    {
        mainCamera = Camera.main;
        newGamePanel.SetActive(false);
        loadGamePanel.SetActive(false);
        aboutPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        gameManagerFromOtherScene = GameObject.Find("GameManager");
        if (gameManagerFromOtherScene != null)        {
            gameManagerFromOtherScene.SetActive(false);
        }
    }

    void Update()
    {
        mainCamera.transform.Rotate(Vector3.left, rotationSpeedLeft * Time.deltaTime);
        mainCamera.transform.Rotate(Vector3.up, rotationSpeedUp * Time.deltaTime);
        mainCamera.transform.Rotate(Vector3.forward, rotationSpeedForward * Time.deltaTime);
    }

    public void ShowNewGamePanel()
    {
        newGamePanel.SetActive(true);
        loadGamePanel.SetActive(false);
        aboutPanel.SetActive(false);
    }

    public void ShowLoadGamePanel()
    {
        newGamePanel.SetActive(false);
        loadGamePanel.SetActive(true);
        aboutPanel.SetActive(false);
    }

    public void ShowAboutPanel()
    {
        newGamePanel.SetActive(false);
        loadGamePanel.SetActive(false);
        aboutPanel.SetActive(true);
    }

    public void NewGameSlot1()
    {
        StartNewGame(0);
    }

    public void NewGameSlot2()
    {
        StartNewGame(1);
    }

    public void NewGameSlot3()
    {
        StartNewGame(2);
    }

    public void LoadGameSlot1()
    {
        LoadGame(0);
    }

    public void LoadGameSlot2()
    {
        LoadGame(1);
    }

    public void LoadGameSlot3()
    {
        LoadGame(2);
    }
    public void StartNewGame(int slot)
    {
        SaveManager.saveSlotIndex = slot;
        SaveManager.Save();
        UnityEngine.SceneManagement.SceneManager.LoadScene("IntroScene");
    }

    public void LoadGame(int slot)
    {
        SaveManager.saveSlotIndex = slot;
        if (gameManagerFromOtherScene != null)
        {
            gameManagerFromOtherScene.SetActive(true);
        }
        SaveManager.Load();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Bridge");
    }

    public void QuitGame()
    {
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
