using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockManager : MonoBehaviour
{
    public static BlockManager instance;
    public Dictionary<Vector3Int, Rigidbody> blocks = new Dictionary<Vector3Int, Rigidbody>(); // Directory for storing the blocks and their positions
    // ^^^ Key is the position of the block in local space as relative to the core block, in Vector3Int as x, y, z coordinates aligned to the grid.
    // ^^^ Value is the Rigidbody component of the block.
    public Dictionary<Vector3Int, List<Vector3Int>> blockConnections = new Dictionary<Vector3Int, List<Vector3Int>>(); // Directory for storing the connections between blocks
    // ^^^ Key is the position of the block in local space as relative to the core block, in Vector3Int as x, y, z coordinates aligned to the grid.
    // ^^^ Value is a list of Vector3Int positions of the connected blocks. Each entry in the list is a connection from the key block to the connected block.
    private bool isValidating = false;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Debug.LogError("Duplicated BlockManager", gameObject);
    }

    // Add a block to the blocks directory
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
    // Check if a block exists at a given position
    public bool TryGetBlockAt(Vector3Int localPos, out Rigidbody blockRb)
    {
        return blocks.TryGetValue(localPos, out blockRb);
    }

    // Remove a block from the blocks directory
    public void RemoveBlock(Vector3Int localPos)
    {
        if (blocks.ContainsKey(localPos))
        {
            //MIGHT NOT BE NECESSARY
            blocks.Remove(localPos);
            RemoveConnections(localPos);
        }
    }

    // ^^^ These functions are mainly used in the build logic, but also used in e.g. the block destruction logic.

    // Add a connection between two blocks
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

    // Remove a connection between two blocks
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

    // Enable or disable the physics of all blocks in the vehicle
    // Only effects the player vehicle blocks, not AI since those are stored in a different manager.
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

    // Check if the core block is connected to all other blocks in the vehicle by running a BFS on the connections as edges
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

    // Validate the structure of the vehicle by checking if all blocks are connected to the core block.
    public void ValidateStructure()
    {
        if (isValidating)
            return;
        
        isValidating = true;
        HashSet<Vector3Int> connected = CoreConnectionCheck();
        List<Vector3Int> allPositions = new List<Vector3Int>(blocks.Keys); // Creating a copy of the keys (aka the block positions) to avoid modifying the dictionary while iterating over it.
        
        foreach (Vector3Int pos in allPositions)
        {
            if (!connected.Contains(pos)) // If we cannot reach a certain block from the core block, detach it.
            {
                DetachBlock(pos);
            }
        }
        isValidating = false;
    }
    private void DetachBlock(Vector3Int pos)
    {
        if (blocks.TryGetValue(pos, out Rigidbody rb)) // If the block is in the blocks directory
        {
            rb.isKinematic = false; // Enable physics on the block
            FixedJoint[] joints = rb.GetComponents<FixedJoint>();
            foreach (FixedJoint joint in joints)
            {
                Destroy(joint); // Destroy all fixed joints on the block
            }
            Hull hull = rb.GetComponent<Hull>();
            if (hull != null)
            {
                hull.canPickup = true; // Set the block to be pickable, aka in this scenerio it is detached
                // (Really need better naming for this variable, canPickup is not very descriptive, will fix later)
                Wheel wheel = rb.GetComponent<Wheel>();
                if (wheel != null)
                {
                    wheel.enabled = false;
                    wheel.driveInput = 0f;
                    wheel.currentSteerAngle = 0f;
                    // ^^^ Disable the wheel script and reset the drive input and steering angle
                }

                Turret turret = rb.GetComponent<Turret>();
                if (turret != null)
                {
                    turret.enabled = false;
                    if (turret.blockedLine != null)
                        turret.blockedLine.enabled = false;
                    // ^^^ Disable the turret script and the blocked line renderer
                }
            }
        }
        blocks.Remove(pos); // Remove the block from the blocks directory
        RemoveConnections(pos); // Remove the connections of the block
    }

    // Recalculate the connections between blocks
    public void recalculateConnections()
    {
        // First clear the current connections
        blockConnections.Clear();

        // For each block, check if it has any joint connections and if so add them to the connections directory
        foreach (var blockEntry in blocks)
        {
            // Debug.Log("Checking block at " + blockEntry.Key);
            Rigidbody rb = blockEntry.Value;
            FixedJoint[] joints = rb.GetComponents<FixedJoint>();
            // Debug.Log("Found " + joints.Length + " connections");
            foreach (FixedJoint joint in joints)
            {
                if (joint == null!= null && joint.connectedBody != null)
                {
                    Vector3Int connectedPos = Vector3Int.RoundToInt(
                        transform.InverseTransformPoint(joint.connectedBody.transform.position) // Get the connected block's position in local space
                    );
                    // Debug.Log("Connected from "+blockEntry.Key+ " to " + connectedPos);
                    AddConnection(blockEntry.Key, connectedPos); // Add the connection to the directory
                }
            }
        }

        ValidateStructure();
    }
}
