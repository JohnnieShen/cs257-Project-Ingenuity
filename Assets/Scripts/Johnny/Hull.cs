using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hull : MonoBehaviour
{
    // public List<ConnectionPoint> connectionPoints = new List<ConnectionPoint>();
    public List<Vector3Int> validConnectionOffsets = new List<Vector3Int>();
    private EnemyAI parentAI;
    private bool isAIVehicle = false;
    public bool canPickup = false;
    public Block sourceBlock;
    public bool isPreview = false;
    private Transform commandModule;
    void Start()
    {
        if(isPreview)
        {
            return;
        }
        commandModule = transform.parent.Find("CommandModule");
        if (commandModule == null)
        {
            Debug.LogError("CommandModule sibling not found!", gameObject);
        }
        isAIVehicle = transform.parent.GetComponentInChildren<EnemyMovement>() != null;
        if (!isAIVehicle && BlockManager.instance != null)
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
        parentAI = transform.parent.GetComponentInChildren<EnemyAI>();
        if (parentAI != null)
        {
            Transform referenceTransform = (commandModule != null) ? commandModule : parentAI.transform;
            Vector3Int localPos = Vector3Int.RoundToInt(referenceTransform.InverseTransformPoint(transform.position));
            EnemyBlockManager.instance.RegisterBlock(parentAI, localPos, GetComponent<Rigidbody>());
        }
        // connectionPoints.AddRange(GetComponentsInChildren<ConnectionPoint>());
        // Rigidbody myRb = GetComponent<Rigidbody>();
        // if (myRb != null)
        // {
        //     foreach (ConnectionPoint cp in connectionPoints)
        //     {
        //         cp.body = myRb;
        //     }
        // }
    }
    
    void OnDestroy()
    {
        if (isPreview)
        {
            return;
        }
        if (!isAIVehicle && BlockManager.instance != null)
        {
            // BlockManager.instance.ValidateStructure();
            Vector3Int gridPos = Vector3Int.RoundToInt(transform.localPosition);
            BlockManager.instance.RemoveBlock(gridPos);
            BlockManager.instance.RemoveConnections(gridPos);
            // BlockManager.instance.CleanupBrokenJoints();
            // BlockManager.instance.recalculateConnections();
            
            BlockManager.instance.StartCoroutine(BlockManager.instance.DelayedRecalculateConnections());
        }
        if (parentAI != null)
        {
            Transform referenceTransform = (commandModule != null) ? commandModule : parentAI.transform;
            Vector3Int localPos = Vector3Int.RoundToInt(referenceTransform.InverseTransformPoint(transform.position));
            EnemyBlockManager.instance.RemoveBlock(parentAI, localPos);
            EnemyBlockManager.instance.RemoveConnections(parentAI, localPos);
            EnemyBlockManager.instance.ValidateStructure(parentAI);
        }
    }

    void OnJointBreak(float breakForce)
    {
        if (isPreview)
        {
            return;
        }
        StartCoroutine(DelayedRecalculate());   
    }

    private IEnumerator DelayedRecalculate()
    {
        yield return null;
        if (isAIVehicle)
        {
            transform.parent.GetComponentInChildren<EnemyAI>().BuildConnectionGraph();
        }
        else
        {
            BlockManager.instance.recalculateConnections();
        }
    }
}
