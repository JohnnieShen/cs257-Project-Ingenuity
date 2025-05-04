using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Hull : MonoBehaviour
{
    /*
    * Author: Johnny
    * Summary: This script is attached to a hull block in the game. It manages the hull's connection points and its interaction with the BlockManager.
    * The hull can be part of a vehicle and can be connected to other blocks. It also handles the destruction of the hull and its connections.
    * The script uses a coroutine to delay the recalculation of connections when a joint breaks.
    */

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

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip audioClip;

    /* Start is called before the first frame update.
    * It checks if the hull is a preview or an AI vehicle. If it's not, it registers the block with the BlockManager or EnemyBlockManager.
    * It also finds the core transform and sets up the connection points.
    * It also sets up the visual renderers for the childrens.
    */
    void Start()
    {
        if(isPreview)
        {
            return;
        }
        if (transform.parent == null)
        {
            return;
        }
        isAIVehicle = transform.parent.GetComponentInChildren<EnemyMovement>() != null;
        if (!gameObject.CompareTag("EnemyBlock"))
        {
            coreTransform = transform.parent.GetComponentInChildren<VehicleResourceManager>().transform;
        }
        if (!isAIVehicle && BlockManager.instance != null)
        {
            Vector3 worldPos = transform.position;
            Vector3 localPos = coreTransform.InverseTransformPoint(worldPos);
            Vector3Int gridPos = Vector3Int.RoundToInt(localPos);

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
    
    /* OnDestroy is called when the MonoBehaviour will be destroyed.
    * It checks if the hull is a preview or an AI vehicle. If it's not, it removes the block from the BlockManager or EnemyBlockManager.
    * It also removes the connections and validates the structure of the vehicle.
    * It also starts a coroutine to delay the recalculation of connections.
    */
    void OnDestroy()
    {
        if (isPreview)
        {
            return;
        }
        if (!isAIVehicle && BlockManager.instance != null)
        {
            // use coreTransform to compute the local grid position
            if (transform == null) {
                return;
            }
            Vector3 worldPos = transform.position;
            Vector3 localPos = coreTransform.InverseTransformPoint(worldPos);
            Vector3Int gridPos = Vector3Int.RoundToInt(localPos);

            BlockManager.instance.RemoveBlock(gridPos);
            BlockManager.instance.RemoveConnections(gridPos);
            BlockManager.instance.StartCoroutine(
                BlockManager.instance.DelayedRecalculateConnections()
            );
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

    /* OnJointBreak is called when a joint attached to the hull breaks.
    * It checks if the hull is a preview. If it's not, it starts a coroutine to delay the recalculation of connections.
    * It also validates the structure of the vehicle.
    * Param1: breakForce - The force at which the joint broke.
    */
    void OnJointBreak(float breakForce)
    {
        if (isPreview)
        {
            return;
        }
        StartCoroutine(DelayedRecalculate());

        audioSource.PlayOneShot(audioClip);
    }

    /* DelayedRecalculate is a coroutine that waits for one frame and then recalculates the connections of the vehicle.
    * It checks if the hull is an AI vehicle. If it is, it calls the BuildConnectionGraph method on the EnemyAI component.
    * It also validates the structure of the vehicle using the EnemyBlockManager.
    * If the hull is not an AI vehicle, it calls the recalculateConnections method on the BlockManager.
    */
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
