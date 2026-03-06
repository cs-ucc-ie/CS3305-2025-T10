using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Bootstrapping")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] GameObject uiObj;
    [SerializeField] GameObject weaponTestDriverObj;
    [SerializeField] GameObject inputManagerObj;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void SetActiveObj(bool isActive)
    {
        if (uiObj != null) uiObj.SetActive(isActive);
        if (weaponTestDriverObj != null) weaponTestDriverObj.SetActive(isActive);
        if (inputManagerObj != null) inputManagerObj.SetActive(isActive);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        CheckSceneAndSpawnPlayerAndUI();
    }

    private void CheckSceneAndSpawnPlayerAndUI()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        // destroy player and disable some manager in sudden scenes
        if (currentScene.name == "MainMenu" || currentScene.name == "IntroScene")
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null){
                Destroy(playerObj);
            }
            SetActiveObj(false);
        }
        else
        {
            Debug.Log($"GameManager: Checking scene {currentScene.name} for player spawning and UI activation.");
            // spawn player if not exists
            GameObject existingPlayer = GameObject.FindWithTag("Player");
            if (existingPlayer == null)
            {
                Instantiate(playerPrefab);
                SetActiveObj(true);
                var weaponMountPoint = GameObject.Find("WeaponMountPoint").transform;
                Debug.Log("weaponMountPoint.childCount" + weaponMountPoint.childCount);
                if(weaponMountPoint.childCount == 0){
                    weaponTestDriverObj.GetComponent<WeaponTestDriver>().Start();
                }
            }
            
            MovePlayerToSpawn(currentScene.name);
            SetActiveObj(true);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"GameManager: Scene loaded: {scene.name}");
        CheckSceneAndSpawnPlayerAndUI();
    }

    // private void EnsurePlayerExists()
    // {
    //     GameObject existingPlayer = GameObject.FindWithTag("Player");
    //     if (existingPlayer != null) return;

    //     if (playerPrefab == null)
    //     {
    //         Debug.LogError("GameManager: playerPrefab is not assigned.");
    //         return;
    //     }

    //     Instantiate(playerPrefab);
    //     Debug.Log("GameManager: Spawned Player (persistent).");
    // }

    private void MovePlayerToSpawn(string sceneName)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError($"GameManager: No Player found in scene {sceneName}.");
            return;
        }

        GameObject spawnObj = GameObject.FindWithTag("PlayerSpawn");
        if (spawnObj == null)
        {
            Debug.LogWarning($"GameManager: No PlayerSpawn found in scene {sceneName}. Player not moved.");
            return;
        }

        Debug.Log($"GameManager: Moving Player to spawn point in scene {sceneName}.");
        Transform spawn = spawnObj.transform;

        Debug.Log($"GameManager: Found spawn point in scene {sceneName}. Position: {spawn.position}, Rotation: {spawn.rotation}.");

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.transform.SetPositionAndRotation(spawn.position, spawn.rotation);

        if (cc != null) cc.enabled = true;

        Debug.Log($"GameManager: Player moved to spawn point in scene {sceneName}.");
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}