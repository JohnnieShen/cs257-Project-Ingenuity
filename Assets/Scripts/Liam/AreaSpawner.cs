using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using System;
using System.Linq;

public class AreaSpawner : MonoBehaviour
{
    /*
    Author: Liam
    Summary: This script spawns enemies in a square region around the spawn point with side length 2 * range.
    */
    [SerializeField] private Terrain terrain;
    public static event Action OnSpawnCompleted;
    [SerializeField] float range;
    [SerializeField] GameObject[] prefabs;
    [SerializeField] int numberEnemies;
    bool spawned = false;

    /*
    When the scene is loaded, a number of enemies are spawned within a square region centered at the spawn point.
    */
    private void OnTriggerEnter(Collider other)
    {
        // Check if trigger is player block rather than enemy block
        if (!other.CompareTag("Block")) return;
        if (spawned) return;

        // Spawn enemies
        for (int i = 0; i < numberEnemies; i++)
        {
            var position = transform.position + new Vector3(UnityEngine.Random.Range(-range, range), 0, UnityEngine.Random.Range(-range, range));
            var terrain = Terrain.activeTerrain;

            if (terrain != null && terrain.terrainData != null)
            {
                float terrainHeight = terrain.SampleHeight(position) + terrain.transform.position.y;
                position.y = terrainHeight + 5f;
            }
            else
            {
                Debug.LogWarning("No valid active terrain — skipping height sample.");
                position.y = 10f;
            }

            Instantiate(prefabs[UnityEngine.Random.Range(0, prefabs.Length)], position, Quaternion.identity);
        }

        // Set spawned to true to prevent triggering more than once
        spawned = true;
        OnSpawnCompleted?.Invoke();
    }
}
