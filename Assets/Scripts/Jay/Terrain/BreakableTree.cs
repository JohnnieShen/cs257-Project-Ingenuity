using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableTree : MonoBehaviour
{
    /*
    Author: Jay
    Summary: A script to allow for the tree obstacles to be removed once the
    player collides with the box collider. Both the tree prefab (physical game object)
    as well as the matching tree instance in the terrain itself are deleted together.
    */
    public Terrain terrain;                 // Assigndynamically
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