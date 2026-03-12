using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Wave
{
    public List<EnemySpawnData> enemies = new List<EnemySpawnData>();
    public float timeBetweenSpawns = 1f;
    public float timeBeforeNextWave = 5f;
}

[System.Serializable]
public class EnemySpawnData
{
    public EnemyData enemyData;
    public int count = 1;
    [Tooltip("Path index to use (-1 = use PathManager selection mode, 0+ = specific path index)")]
    public int pathIndex = -1; // -1 means use PathManager's selection mode
}

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Wave Settings")]
    [SerializeField] private List<Wave> waves = new List<Wave>();
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private bool autoStartWaves = true;
    [SerializeField] private float timeBeforeFirstWave = 3f;
    
    [Header("Multi-Path Settings")]
    [Tooltip("If true, enemies will spawn at their assigned path's first waypoint instead of the spawn point")]
    [SerializeField] private bool spawnAtPathStart = false;

    private int currentWaveIndex = 0;
    private bool isSpawning = false;
    private List<Enemy> activeEnemies = new List<Enemy>();

    public event System.Action<int> OnWaveStarted;
    public event System.Action<int> OnWaveCompleted;
    public event System.Action OnAllWavesCompleted;

    public int CurrentWave => Mathf.Min(currentWaveIndex + 1, waves.Count); // Cap at total waves to prevent overflow
    public int TotalWaves => waves.Count;
    public int RemainingEnemies => activeEnemies.Count;
    public bool IsSpawning => isSpawning;
    public bool HasMoreWaves => currentWaveIndex < waves.Count;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    /// <summary>
    /// When a level scene unloads, clear Instance so the next level's WaveManager can become the active one.
    /// Fixes "Start Wave button still says All Waves Complete" when going to another stage after completing one.
    /// </summary>
    private void OnSceneUnloaded(Scene scene)
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        // Reset wave state when scene loads
        ResetWaveState();
        
        if (autoStartWaves)
        {
            StartCoroutine(StartWavesCoroutine());
        }
    }
    
    /// <summary>
    /// Resets wave state for a new level/scene.
    /// </summary>
    private void ResetWaveState()
    {
        currentWaveIndex = 0;
        isSpawning = false;
        activeEnemies.Clear();
    }

    private IEnumerator StartWavesCoroutine()
    {
        yield return new WaitForSeconds(timeBeforeFirstWave);
        StartNextWave();
    }

    public void StartNextWave()
    {
        if (currentWaveIndex >= waves.Count)
        {
            AllWavesCompleted();
            return;
        }

        if (isSpawning) return;

        Wave wave = waves[currentWaveIndex];
        int totalSpawns = 0;
        if (wave.enemies != null)
            foreach (var es in wave.enemies) totalSpawns += es.count;
        StartCoroutine(SpawnWaveCoroutine(wave));
    }

    private IEnumerator SpawnWaveCoroutine(Wave wave)
    {
        isSpawning = true;
        OnWaveStarted?.Invoke(CurrentWave);

        if (wave.enemies == null || wave.enemies.Count == 0)
        {
            Debug.LogWarning("[WaveManager] Wave has no enemy entries! Add enemies to the wave in the Inspector.");
            isSpawning = false;
            yield break;
        }

        foreach (var enemySpawn in wave.enemies)
        {
            for (int i = 0; i < enemySpawn.count; i++)
            {
                SpawnEnemy(enemySpawn.enemyData, enemySpawn.pathIndex);
                yield return new WaitForSeconds(wave.timeBetweenSpawns);
            }
        }

        // Wait for all enemies to be defeated (keep isSpawning true so Start Wave button stays disabled)
        yield return new WaitUntil(() => activeEnemies.Count == 0);

        isSpawning = false;
        OnWaveCompleted?.Invoke(CurrentWave);
        currentWaveIndex++;

        if (currentWaveIndex < waves.Count)
        {
            yield return new WaitForSeconds(wave.timeBeforeNextWave);
            // Only auto-start next wave if autoStartWaves is enabled
            if (autoStartWaves)
            {
                StartNextWave();
            }
        }
        else
        {
            AllWavesCompleted();
        }
    }

    private void SpawnEnemy(EnemyData enemyData)
    {
        SpawnEnemy(enemyData, -1);
    }
    
    private void SpawnEnemy(EnemyData enemyData, int pathIndex)
    {
        // Determine spawn position
        Vector3 spawnPosition;
        EnemyPath assignedPath = null;
        
        // Get the path for this enemy
        if (PathManager.Instance != null)
        {
            if (pathIndex >= 0)
            {
                // Use specific path
                assignedPath = PathManager.Instance.GetPath(pathIndex);
            }
            else
            {
                // Use PathManager's selection mode
                assignedPath = PathManager.Instance.GetPath();
            }
        }
        else
        {
            assignedPath = FindObjectOfType<EnemyPath>();
            if (assignedPath == null)
                Debug.LogWarning("[WaveManager] PathManager.Instance is null and no EnemyPath in scene - enemy may not move!");
        }

        if (assignedPath != null && assignedPath.Waypoints.Count == 0)
            Debug.LogWarning("[WaveManager] Assigned path '" + assignedPath.gameObject.name + "' has 0 waypoints!");
        
        // Determine spawn position
        if (spawnAtPathStart && assignedPath != null && assignedPath.Waypoints.Count > 0)
        {
            // Spawn at the first waypoint of the assigned path
            spawnPosition = assignedPath.GetWorldWaypoint(0);
        }
        else
        {
            // Use the spawn point (or path start if spawn point is null)
            if (spawnPoint != null)
            {
                spawnPosition = spawnPoint.position;
            }
            else if (assignedPath != null && assignedPath.Waypoints.Count > 0)
            {
                spawnPosition = assignedPath.GetWorldWaypoint(0);
            }
            else
            {
                Debug.LogError("[WaveManager] No spawn point or path available! Assign spawn point or add EnemyPath with waypoints.");
                return;
            }
        }

        if (enemyData == null)
        {
            Debug.LogError("[WaveManager] SpawnEnemy: enemyData is null! Assign EnemyData in the wave.");
            return;
        }
        if (enemyData.enemyPrefab == null)
        {
            Debug.LogError("[WaveManager] SpawnEnemy: enemyPrefab is null on '" + enemyData.name + "'!");
            return;
        }

        GameObject enemyObj = Instantiate(enemyData.enemyPrefab, spawnPosition, Quaternion.identity);
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        
        if (enemy != null)
        {
            enemy.Initialize(enemyData, assignedPath);
            enemy.OnEnemyDestroyed += RemoveEnemy;
            activeEnemies.Add(enemy);
        }
        else
            Debug.LogWarning("[WaveManager] Spawned prefab has no Enemy component!");
    }

    private void RemoveEnemy(Enemy enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
        }
    }

    private void AllWavesCompleted()
    {
        OnAllWavesCompleted?.Invoke();
        GameManager.Instance?.Victory();
    }

    public void AddWave(Wave wave)
    {
        waves.Add(wave);
    }
}
