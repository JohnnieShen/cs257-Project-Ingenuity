using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class Inventory : MonoBehaviour
{
    public GameObject[] prefabs;
    int index;

    public void Add(Block block)
    {
        // TODO
        if (block.vehicle)
        {
            block.vehicle.blocks.Remove(block.coordinates);
        }
        Destroy(block.gameObject);
    }

    public void OnSwitch(InputValue value)
    {
        index = Helper.Mod(index + 1, prefabs.Length);
    }

    public GameObject GetPrefab()
    {
        return prefabs[index];
    }
}
