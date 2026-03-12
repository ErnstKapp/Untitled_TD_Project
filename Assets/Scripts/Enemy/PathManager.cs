using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages multiple enemy paths in a level.
/// Provides methods to get paths by index, randomly, or round-robin.
/// </summary>
public class PathManager : MonoBehaviour
{
    public static PathManager Instance { get; private set; }

    [Header("Path Settings")]
    [SerializeField] private List<EnemyPath> paths = new List<EnemyPath>();
    [SerializeField] private PathSelectionMode selectionMode = PathSelectionMode.RoundRobin;
    
    [Header("Debug")]
    [SerializeField] private bool showPathNumbers = true;

    private int roundRobinIndex = 0;

    public enum PathSelectionMode
    {
        RoundRobin,      // Cycle through paths in order
        Random,          // Randomly select a path
        FirstOnly,       // Always use the first path (backward compatibility)
        Specific         // Manually assign paths (use GetPath(index))
    }

    public int PathCount => paths.Count;
    public PathSelectionMode SelectionMode => selectionMode;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("[PathManager] Multiple PathManager instances found! Destroying duplicate component only.");
            Destroy(this);
            return;
        }

        // Populate paths in Awake so they're ready before any spawning (fixes enemies stuck at spawn when returning to scene)
        EnsurePathsPopulated();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        EnsurePathsPopulated();
    }

    private void EnsurePathsPopulated()
    {
        if (paths.Count > 0)
        {
            paths.RemoveAll(path => path == null);
            return;
        }
        EnemyPath[] foundPaths = FindObjectsOfType<EnemyPath>(true);
        paths.AddRange(foundPaths);
        paths.RemoveAll(path => path == null);
        if (paths.Count == 0)
            Debug.LogWarning("[PathManager] No EnemyPath objects found in scene! Enemies will not be able to move.");
    }

    /// <summary>
    /// Gets a path based on the current selection mode.
    /// </summary>
    public EnemyPath GetPath()
    {
        if (paths.Count == 0)
        {
            EnsurePathsPopulated();
            if (paths.Count == 0)
            {
                Debug.LogError("[PathManager] No paths available!");
                return null;
            }
        }

        switch (selectionMode)
        {
            case PathSelectionMode.RoundRobin:
                EnemyPath path = paths[roundRobinIndex];
                roundRobinIndex = (roundRobinIndex + 1) % paths.Count;
                return path;

            case PathSelectionMode.Random:
                return paths[Random.Range(0, paths.Count)];

            case PathSelectionMode.FirstOnly:
                return paths[0];

            case PathSelectionMode.Specific:
                // For specific mode, default to first path
                return paths[0];

            default:
                return paths[0];
        }
    }

    /// <summary>
    /// Gets a specific path by index.
    /// </summary>
    /// <param name="index">Path index (0-based)</param>
    /// <returns>The EnemyPath at the specified index, or null if invalid</returns>
    public EnemyPath GetPath(int index)
    {
        if (paths.Count == 0)
            EnsurePathsPopulated();
        if (index < 0 || index >= paths.Count)
        {
            Debug.LogWarning($"[PathManager] Invalid path index {index}. Returning first path.");
            return paths.Count > 0 ? paths[0] : null;
        }
        return paths[index];
    }

    /// <summary>
    /// Gets a random path.
    /// </summary>
    public EnemyPath GetRandomPath()
    {
        if (paths.Count == 0)
        {
            return null;
        }
        return paths[Random.Range(0, paths.Count)];
    }

    /// <summary>
    /// Registers a path (useful if paths are created at runtime).
    /// </summary>
    public void RegisterPath(EnemyPath path)
    {
        if (path != null && !paths.Contains(path))
        {
            paths.Add(path);
        }
    }

    /// <summary>
    /// Unregisters a path.
    /// </summary>
    public void UnregisterPath(EnemyPath path)
    {
        paths.Remove(path);
    }

    /// <summary>
    /// Resets the round-robin counter.
    /// </summary>
    public void ResetRoundRobin()
    {
        roundRobinIndex = 0;
    }

    private void OnDrawGizmos()
    {
        if (!showPathNumbers || paths.Count == 0) return;

        // Draw path numbers above each path's first waypoint
        for (int i = 0; i < paths.Count; i++)
        {
            if (paths[i] == null) continue;
            
            if (paths[i].Waypoints.Count > 0)
            {
                Vector3 firstWaypoint = paths[i].GetWorldWaypoint(0);
                firstWaypoint.y += 0.5f; // Offset above waypoint
                
                #if UNITY_EDITOR
                UnityEditor.Handles.Label(firstWaypoint, $"Path {i + 1}");
                #endif
            }
        }
    }
}
