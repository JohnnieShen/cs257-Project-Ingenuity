using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class TerrainBlockSpawner : MonoBehaviour
{
    public List<Block> blockPrefabs;
    public int numberOfBlocks = 10;

    public Terrain targetTerrain;
    public float heightOffset = 1f;

    public int maxSpawnAttempts = 50;

    public Transform exclusionCenter;
    public float exclusionRadius = 0f;
    private Vector3 terrainPos;
    private Vector3 terrainSize;

    void Start()
    {
        if (targetTerrain == null)
            targetTerrain = Terrain.activeTerrain;

        if (targetTerrain == null)
        {
            Debug.LogError("TerrainBlockSpawner: No terrain assigned or found in scene.");
            return;
        }

        terrainPos = targetTerrain.GetPosition();
        terrainSize = targetTerrain.terrainData.size;

        SpawnBlocks();
    }

    private void SpawnBlocks()
    {
        if (blockPrefabs == null || blockPrefabs.Count == 0)
        {
            Debug.LogWarning("TerrainBlockSpawner: No block prefabs assigned.");
            return;
        }

        int spawned = 0;
        int attempts = 0;

        while (spawned < numberOfBlocks && attempts < numberOfBlocks * maxSpawnAttempts)
        {
            attempts++;
            float x = Random.Range(terrainPos.x, terrainPos.x + terrainSize.x);
            float z = Random.Range(terrainPos.z, terrainPos.z + terrainSize.z);
            float y = targetTerrain.SampleHeight(new Vector3(x, 0, z)) + terrainPos.y + heightOffset;

            Vector3 candidate = new Vector3(x, y, z);

            if (exclusionCenter != null && exclusionRadius > 0f)
            {
                float dist = Vector3.Distance(candidate, exclusionCenter.position);
                if (dist < exclusionRadius)
                    continue;
            }

            GameObject prefab = blockPrefabs[Random.Range(0, blockPrefabs.Count)].BlockObject;
            GameObject spawnedBlock = Instantiate(prefab, candidate, Quaternion.identity);
            var hull = spawnedBlock.GetComponentInChildren<Hull>();
            if (hull != null)
            {
                hull.canPickup = true;
            }
            spawned++;
        }

        //Debug.Log($"TerrainBlockSpawner: Spawned {spawned}/{numberOfBlocks} blocks after {attempts} attempts.");
    }
}
