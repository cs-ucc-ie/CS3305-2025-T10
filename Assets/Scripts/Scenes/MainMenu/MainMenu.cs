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
        StartNewGame(1);
    }

    public void NewGameSlot2()
    {
        StartNewGame(2);
    }

    public void NewGameSlot3()
    {
        StartNewGame(3);
    }

    public void LoadGameSlot1()
    {
        LoadGame(1);
    }

    public void LoadGameSlot2()
    {
        LoadGame(2);
    }

    public void LoadGameSlot3()
    {
        LoadGame(3);
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
        SaveManager.Load();
        UnityEngine.SceneManagement.SceneManager.LoadScene("IntroScene");
    }

    public void QuitGame()
    {
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
