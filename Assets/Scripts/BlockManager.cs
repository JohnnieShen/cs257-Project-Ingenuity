using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockManager : MonoBehaviour
{
    public static BlockManager instance;
    public List<Hull> blocks;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Debug.LogError("Duplicated BlockManager", gameObject);
    }

    public void AddBlock(Hull block)
    {
        blocks.Add(block);
    }

    public void RemoveBlock(Hull block)
    {
        blocks.Remove(block);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            foreach (Hull block in blocks)
            {
                block.GetComponent<Rigidbody>().isKinematic = false;
            }
        }
    }
}
