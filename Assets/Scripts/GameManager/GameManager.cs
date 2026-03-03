using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Bootstrapping")]
    [SerializeField] private GameObject playerPrefab;

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
        // Covers the first scene when the game starts
        EnsurePlayerExists();
        MovePlayerToSpawn(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsurePlayerExists();
        MovePlayerToSpawn(scene.name);
    }

    private void EnsurePlayerExists()
    {
        GameObject existingPlayer = GameObject.FindWithTag("Player");
        if (existingPlayer != null) return;

        if (playerPrefab == null)
        {
            Debug.LogError("GameManager: playerPrefab is not assigned.");
            return;
        }

        Instantiate(playerPrefab);
        Debug.Log("GameManager: Spawned Player (persistent).");
    }

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

        Transform spawn = spawnObj.transform;
        player.transform.SetPositionAndRotation(spawn.position, spawn.rotation);

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}