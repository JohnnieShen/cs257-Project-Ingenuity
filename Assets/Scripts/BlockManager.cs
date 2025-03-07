using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockManager : MonoBehaviour
{
    public static BlockManager instance;
    public Dictionary<Vector3Int, Rigidbody> blocks = new Dictionary<Vector3Int, Rigidbody>();
    public Dictionary<Vector3Int, List<Vector3Int>> blockConnections = new Dictionary<Vector3Int, List<Vector3Int>>();
    private bool isValidating = false;
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
            //MIGHT NOT BE NECESSARY
            blocks.Remove(localPos);
            RemoveConnections(localPos);
        }
    }
    public void AddConnection(Vector3Int position1, Vector3Int position2)
    {
        AddConnectionDirection(position1, position2);
        AddConnectionDirection(position2, position1);
    }

    private void AddConnectionDirection(Vector3Int from, Vector3Int to)
    {
        if (!blockConnections.ContainsKey(from))
            blockConnections[from] = new List<Vector3Int>();
            
        if (!blockConnections[from].Contains(to))
            blockConnections[from].Add(to);
    }
    public void RemoveConnections(Vector3Int position)
    {
        if (blockConnections.ContainsKey(position))
        {
            foreach (var connection in blockConnections[position])
            {
                if (blockConnections.ContainsKey(connection))
                    blockConnections[connection].Remove(position);
            }
            blockConnections.Remove(position);
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
    public HashSet<Vector3Int> CoreConnectionCheck()
    {
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        Vector3Int corePosition = Vector3Int.zero;
        
        queue.Enqueue(corePosition);
        visited.Add(corePosition);

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();
            
            if (blockConnections.ContainsKey(current))
            {
                foreach (Vector3Int neighbor in blockConnections[current])
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }
        
        return visited;
    }
    public void ValidateStructure()
    {
        if (isValidating)
            return;
        
        isValidating = true;
        HashSet<Vector3Int> connected = CoreConnectionCheck();
        List<Vector3Int> allPositions = new List<Vector3Int>(blocks.Keys);
        
        foreach (Vector3Int pos in allPositions)
        {
            if (!connected.Contains(pos))
            {
                DetachBlock(pos);
            }
        }
        isValidating = false;
    }
    private void DetachBlock(Vector3Int pos)
    {
        if (blocks.TryGetValue(pos, out Rigidbody rb))
        {
            rb.isKinematic = false;
            FixedJoint[] joints = rb.GetComponents<FixedJoint>();
            foreach (FixedJoint joint in joints)
            {
                Destroy(joint);
            }
            Hull hull = rb.GetComponent<Hull>();
            if (hull != null)
            {
                hull.canPickup = true;
                Wheel wheel = rb.GetComponent<Wheel>();
                if (wheel != null)
                {
                    wheel.enabled = false;
                    wheel.driveInput = 0f;
                    wheel.currentSteerAngle = 0f;
                }

                Turret turret = rb.GetComponent<Turret>();
                if (turret != null)
                {
                    turret.enabled = false;
                    if (turret.blockedLine != null)
                        turret.blockedLine.enabled = false;
                }
            }
        }
        blocks.Remove(pos);
        RemoveConnections(pos);
    }

    public void recalculateConnections()
    {
        blockConnections.Clear();

        foreach (var blockEntry in blocks)
        {
            Rigidbody rb = blockEntry.Value;
            FixedJoint[] joints = rb.GetComponents<FixedJoint>();
            
            foreach (FixedJoint joint in joints)
            {
                if (joint.connectedBody != null)
                {
                    Vector3Int connectedPos = Vector3Int.RoundToInt(
                        transform.InverseTransformPoint(joint.connectedBody.transform.position)
                    );
                    AddConnection(blockEntry.Key, connectedPos);
                }
            }
        }

        ValidateStructure();
    }
}
