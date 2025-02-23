using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockManager : MonoBehaviour
{
    public static BlockManager instance;
    public Dictionary<Vector3Int, Rigidbody> blocks = new Dictionary<Vector3Int, Rigidbody>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Debug.LogError("Duplicated BlockManager", gameObject);
    }

    public void AddBlock(Vector3Int localPos, Rigidbody blockRb)
    {
        // foreach (var block in blocks)
        // {
        //     Debug.Log("Block at " + block.Key + " with Rigidbody " + block.Value);
        // }
        if (!blocks.ContainsKey(localPos))
        {
            blocks.Add(localPos, blockRb);
        }
    }

    public bool TryGetBlockAt(Vector3Int localPos, out Rigidbody blockRb)
    {
        return blocks.TryGetValue(localPos, out blockRb);
    }

    public void RemoveBlock(Vector3Int localPos)
    {
        if (blocks.ContainsKey(localPos))
        {
            blocks.Remove(localPos);
        }
    }
    public void EnableVehiclePhysics()
    {
        foreach (Rigidbody rb in blocks.Values)
        {
            if (rb != null)
            {
                rb.isKinematic = false;
            }
        }
    }

    public void DisableVehiclePhysics()
    {
        foreach (Rigidbody rb in blocks.Values)
        {
            if (rb != null)
            {
                rb.isKinematic = true;
            }
        }
    }
    // void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.Return))
    //     {
    //         foreach (Hull block in blocks)
    //         {
    //             block.GetComponent<Rigidbody>().isKinematic = false;
    //         }
    //     }
    // }
}
