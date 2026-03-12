using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;

namespace TowerDefense.Editor
{
    public class QuickSetup : EditorWindow
{
    [MenuItem("Tower Defense/Quick Setup")]
    public static void ShowWindow()
    {
        GetWindow<QuickSetup>("Tower Defense Quick Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("Tower Defense Quick Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Create Game Manager"))
        {
            CreateGameManager();
        }

        if (GUILayout.Button("Create Enemy Path"))
        {
            CreateEnemyPath();
        }

        if (GUILayout.Button("Create Tower Placement"))
        {
            CreateTowerPlacement();
        }

        GUILayout.Space(10);
        GUILayout.Label("Note: You still need to configure the components", EditorStyles.helpBox);
    }

    private void CreateGameManager()
    {
        GameObject gm = new GameObject("GameManager");
        gm.AddComponent<GameManager>();
        gm.AddComponent<CurrencyManager>();
        gm.AddComponent<WaveManager>();

        // Set default spawn point
        GameObject spawnPoint = new GameObject("SpawnPoint");
        spawnPoint.transform.SetParent(gm.transform);
        spawnPoint.transform.position = Vector3.zero;

        WaveManager waveManager = gm.GetComponent<WaveManager>();
        var spawnPointField = typeof(WaveManager).GetField("spawnPoint", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (spawnPointField != null)
        {
            spawnPointField.SetValue(waveManager, spawnPoint.transform);
        }

        Selection.activeGameObject = gm;
        Debug.Log("GameManager created! Don't forget to configure it.");
    }

    private void CreateEnemyPath()
    {
        GameObject path = new GameObject("EnemyPath");
        path.AddComponent<EnemyPath>();

        EnemyPath enemyPath = path.GetComponent<EnemyPath>();
        
        // Add default waypoints (simple path)
        var waypointsField = typeof(EnemyPath).GetField("waypoints",
            BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (waypointsField != null)
        {
            var waypoints = new List<Vector2>();
            waypoints.Add(new Vector2(0, 0));
            waypoints.Add(new Vector2(5, 0));
            waypoints.Add(new Vector2(5, 5));
            waypoints.Add(new Vector2(0, 5));
            waypointsField.SetValue(enemyPath, waypoints);
        }

        Selection.activeGameObject = path;
        Debug.Log("EnemyPath created! Adjust waypoints in the Inspector.");
    }

    private void CreateTowerPlacement()
    {
        GameObject placement = new GameObject("TowerPlacement");
        placement.AddComponent<TowerPlacement>();

        Selection.activeGameObject = placement;
        Debug.Log("TowerPlacement created! Configure layers in the Inspector.");
    }
    }
}
