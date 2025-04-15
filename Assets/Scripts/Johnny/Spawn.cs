using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Spawn : MonoBehaviour
{
    /* 
    * Author: Liam
    * Summary: This script is responsible for spawning enemy objects in a specified area of the terrain.
    * It uses the Unity Terrain system to determine the terrain's position and size, and it spawns a specified number of enemies at random positions.
    * The script also includes an exclusion zone to prevent spawning enemies too close to a specified center point.
    * The script is attached to a GameObject in the scene and requires references to the enemy prefab, the number of enemies to spawn, and the exclusion center and radius.
    */
    public List<GameObject> aiPrefabs;
    public int numberEnemies;
    public Transform exclusionCenter;
    public float exclusionRadius = 10f;
    
    /* Awake is called when the script instance is being loaded.
    * It initializes the terrain and checks if it is active. If not, it logs an error message.
    * It then generates random spawn positions for the specified number of enemies within the terrain's bounds.
    * It also checks if the spawn position is within the exclusion zone and retries if necessary.
    * If a valid position is found, it instantiates the enemy prefab at that position.
    * If no valid position is found after 100 attempts, it logs a warning message.
    */
    private void Awake()
    {
        Terrain terrain = Terrain.activeTerrain;
        if(terrain == null)
        {
            Debug.LogError("No active Terrain found!");
            return;
        }
        Vector3 terrainPosition = terrain.transform.position;
        Vector3 terrainSize = terrain.terrainData.size;
        
        for (int i = 0; i < numberEnemies; i++)
        {
            Vector3 randomPos = Vector3.zero;
            int attempt = 0;
            bool validPosition = false;
            while (!validPosition && attempt < 100)
            {
                randomPos.x = terrainPosition.x + Random.Range(0, terrainSize.x);
                randomPos.z = terrainPosition.z + Random.Range(0, terrainSize.z);
                randomPos.y = terrain.SampleHeight(randomPos) + 5;

                if (exclusionCenter != null && exclusionRadius > 0)
                {
                    if (Vector3.Distance(randomPos, exclusionCenter.position) < exclusionRadius)
                    {
                        attempt++;
                        continue;
                    }
                }
                validPosition = true;
            }

            if (aiPrefabs != null && aiPrefabs.Count > 0)
            {
                GameObject chosenPrefab = aiPrefabs[Random.Range(0, aiPrefabs.Count)];
                Instantiate(chosenPrefab, randomPos, Quaternion.identity);
            }
            else
            {
                Debug.LogError("No AI prefabs assigned in the inspector!");
            }
        }
    }
}
