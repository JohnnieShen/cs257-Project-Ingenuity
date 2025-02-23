using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hull : MonoBehaviour
{
    void Start()
    {
        if (BlockManager.instance != null)
        {
            Vector3Int gridPos = Vector3Int.RoundToInt(transform.localPosition);
            Rigidbody rb = GetComponent<Rigidbody>();
            if(rb != null)
            {
                BlockManager.instance.AddBlock(gridPos, rb);
            }
            else
            {
                Debug.LogError("Hull has no Rigidbody component!", gameObject);
            }
        }
    }

    void OnDestroy()
    {
        if (BlockManager.instance != null)
        {
            Vector3Int gridPos = Vector3Int.RoundToInt(transform.localPosition);
            BlockManager.instance.RemoveBlock(gridPos);
        }
    }
}
