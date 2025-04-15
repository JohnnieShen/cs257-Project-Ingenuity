using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Spawn : MonoBehaviour
{
    public GameObject prefab;
    public int numberEnemies;
    public Transform exclusionCenter;
    public float exclusionRadius = 10f;
    
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

            if (!validPosition)
            {
                Debug.LogWarning($"Could not find a valid spawn position after {attempt} attempts.");
                continue;
            }
            Instantiate(prefab, randomPos, Quaternion.identity);
        }
    }
}
