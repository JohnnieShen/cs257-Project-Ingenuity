using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hull : MonoBehaviour
{
    // public List<ConnectionPoint> connectionPoints = new List<ConnectionPoint>();
    public List<Vector3Int> validConnectionOffsets = new List<Vector3Int>();
    public List<MeshRenderer> childMeshRenderers = new List<MeshRenderer>();
    public Transform coreTransform;
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
        else {
            commandModule = transform.parent.GetComponentInChildren<EnemyAI>().gameObject.transform;
            if (commandModule == null)
            {
                Debug.LogError("CommandModule sibling not found!", gameObject);
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
        Transform[] siblings = transform.parent.GetComponentsInChildren<Transform>();
        foreach (Transform sibling in siblings)
        {
            if (!isAIVehicle && sibling.CompareTag("Core"))
            {
                coreTransform = sibling;
                break;
            }
            else if (isAIVehicle && sibling.GetComponent<EnemyAI>() != null)
            {
                coreTransform = sibling;
                break;
            }
        }
        if (coreTransform == null)
        {
            Debug.LogWarning("Core transform not found for Hull: " + gameObject.name);
        }
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        childMeshRenderers = new List<MeshRenderer>(renderers);
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
            // Debug.Log("Removing block from AI vehicle: " + parentAI.name);
            Transform referenceTransform = (commandModule != null) ? commandModule : parentAI.transform;
            // Debug.Log("command module is null "+(commandModule == null));
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
            var ai = transform.parent.GetComponentInChildren<EnemyAI>();
            ai.BuildConnectionGraph();
            EnemyBlockManager.instance.ValidateStructure(ai);
        }
        else
        {
            BlockManager.instance.recalculateConnections();
        }
    }
}
