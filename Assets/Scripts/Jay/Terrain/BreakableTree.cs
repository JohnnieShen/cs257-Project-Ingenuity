using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableTree : MonoBehaviour
{
    public Terrain terrain;                 // Assign in inspector or dynamically
    public float removalRadius = 1f;        // Radius to find matching terrain tree

    void Start()
{
    if (terrain == null)
    {
        terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            Debug.LogError("No terrain found!");
        }
    }
}
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Core") || other.CompareTag("Block"))
        {
            RemoveMatchingTerrainTree();
            Destroy(gameObject); // Destroy the prefab
        }
    }

    void RemoveMatchingTerrainTree()
    {
        if (terrain == null) return;

        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPos = terrain.transform.position;
        TreeInstance[] trees = terrainData.treeInstances;
        System.Collections.Generic.List<TreeInstance> updatedTrees = new();

        Vector3 thisPos = transform.position;

        bool treeRemoved = false;

        foreach (TreeInstance tree in trees)
        {
            Vector3 worldTreePos = Vector3.Scale(tree.position, terrainData.size) + terrainPos;

            if (!treeRemoved && Vector3.Distance(worldTreePos, thisPos) <= removalRadius)
            {
                // Skip this tree (effectively removing it)
                treeRemoved = true;
                continue;
            }

            updatedTrees.Add(tree);
        }

        terrainData.treeInstances = updatedTrees.ToArray();
    }
}