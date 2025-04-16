using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeSpawner : MonoBehaviour
{
    public Terrain terrain;
    public GameObject breakableTreePrefab;
    public Transform player;
    public float activationDistance = 30f;

    private Dictionary<Vector3, GameObject> activeTrees = new Dictionary<Vector3, GameObject>();
    private Vector3 lastCheckedPosition = Vector3.zero;
    private float checkInterval = 5f;

    void Start() {
        //Debug.Log("TreeSpawner script started");
    }

    void Update()
    {
        if (Vector3.Distance(player.position, lastCheckedPosition) > checkInterval)
        {
            UpdateTreeSpawns();
            lastCheckedPosition = player.position;
        }
    }

    void UpdateTreeSpawns()
    {
        TreeInstance[] instances = terrain.terrainData.treeInstances;
        Vector3 terrainPos = terrain.transform.position;

        HashSet<Vector3> nearbyTreePositions = new HashSet<Vector3>();

        foreach (TreeInstance tree in instances)
        {
            Vector3 worldPos = Vector3.Scale(tree.position, terrain.terrainData.size) + terrainPos;

            if (Vector3.Distance(player.position, worldPos) <= activationDistance)
            {
                nearbyTreePositions.Add(worldPos);
                if (!activeTrees.ContainsKey(worldPos))
                {
                    GameObject newTree = Instantiate(breakableTreePrefab, worldPos, Quaternion.identity);
                    activeTrees.Add(worldPos, newTree);
                    //Debug.Log($"Spawned breakable tree at {worldPos}");
                }
            }
        }

        var positionsToRemove = new List<Vector3>();
        foreach (var kvp in activeTrees)
        {
            if (!nearbyTreePositions.Contains(kvp.Key))
            {
                //Debug.Log($"Removed breakable tree at {kvp.Key}");
                Destroy(kvp.Value);
                positionsToRemove.Add(kvp.Key);
            }
        }
        foreach (var pos in positionsToRemove) activeTrees.Remove(pos);
    }

    void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(player.position, activationDistance);
        }
    }
    
}