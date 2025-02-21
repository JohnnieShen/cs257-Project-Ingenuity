using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hull : MonoBehaviour
{
    void Start()
    {
        BlockManager.instance.AddBlock(this);
    }

    void OnDestroy()
    {
        BlockManager.instance.RemoveBlock(this);
    }
}
