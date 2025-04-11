using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class Vehicle : MonoBehaviour
{
    public Dictionary<Vector3, Block> blocks;
    public Block commandModule;

    void Awake()
    {
        // Instantiate blocks
        blocks = new Dictionary<Vector3, Block>();
        blocks.Add(Vector3.zero, commandModule);
    }

    public void Add(Vector3 coordinates, Vector3 orientation, GameObject prefab)
    {
        //// Check if coordinates are out of bounds
        //if (coordinates.x < 0 || coordinates.x >= blocks.GetLength(0) || coordinates.y < 0 || coordinates.y >= blocks.GetLength(1) || coordinates.z < 0 || coordinates.z >= blocks.GetLength(2))
        //{
        //    Debug.LogWarning("Exceeded build area");
        //    return;
        //}

        // Instantiate block
        Block block = Instantiate(prefab, commandModule.transform.TransformPoint(coordinates), commandModule.transform.rotation, transform).GetComponent<Block>();
        block.vehicle = this;
        block.coordinates = coordinates;
        blocks.Add(coordinates, block);
    }

    //// TODO: implement these
    //Vector3 shootTarget; // in world space
    //int turn; // 0 for left, 1 for straight, 2 for right
    //int throttle; // 0 for brake, 1 for none, 2 for throttle up

    public void EnablePhysics()
    {
        // Compute joints
        foreach ((_, Block block) in blocks)
        {
            // Find adjacent blocks with compatible connections
            foreach (Vector3Int connection in block.connections)
            {
                if (blocks.TryGetValue(block.coordinates + connection, out Block otherBlock) && otherBlock.connections.Contains(-connection))
                {
                    // Add joint to each with break force equal to the sum of the connection strength parameters
                    FixedJoint joint = block.gameObject.AddComponent<FixedJoint>();
                    joint.connectedBody = otherBlock.gameObject.GetComponent<Rigidbody>();
                    joint.breakForce = block.connectionStrength + otherBlock.connectionStrength;
                }
            }

            // Enable physics
            block.gameObject.GetComponent<Rigidbody>().isKinematic = false ;// TODO: move this into its own loop if there are issues
        }
    }

    //void DisablePhysics()
    //{
    //    foreach (Block block in blocks)
    //    {
    //        foreach (FixedJoint joint in block.GetComponents<FixedJoint>())
    //        {
    //            Destroy(joint); // TODO: does this trigger the OnJointBreak event?
    //        }
    //    }
    //    foreach (Block block in blocks)
    //    {
    //        block.gameObject.GetComponent<Rigidbody>().isKinematic = true; // TODO: move this into the above loop if there are no issues with blocks falling during joint destruction
    //    }
    //}

    //HashSet<Block> Neighbors(Block block)
    //{
    //    HashSet<Block> neighbors = new HashSet<Block>(); // TODO: should neighbors contain self?
    //    foreach (Vector3 connection in block.connections)
    //    {
    //        Vector3 otherPosition = block.position + connection;
    //        Block otherBlock = blocks[otherPosition.x, otherPosition.y, otherPosition.z];
    //        if (otherBlock && otherBlock.connections.Contains(-connection))
    //        {
    //            neighbors.Add(otherBlock);
    //        }
    //    }
    //    return neighbors;
    //}

    // HashSet<Block> ConnectedBlocks()
    // {
    //    HashSet<Block> connectedBlocks = new HashSet<Block>();
    //    Queue<Block> queue = new Queue<Block>();
    //    queue.Enqueue(this);
    //    while (queue.Count > 0)
    //    {
    //        Block block = queue.Dequeue();
    //        if (!connectedBlocks.Contains(block))
    //        {
    //            connectedBlocks.Add(block);
    //            foreach (FixedJoint joint in block.GetComponents<FixedJoint>())
    //            {
    //                queue.Enqueue(joint.connectedBody.gameObject.GetComponent<Block>());
    //            }
    //        }
    //    }
    //    return component;
    // }

    // void CheckConnections()
    // {
    //    HashSet<Block> component = Component();
    //    bool connected = component.Contains(vehicle.GetCommandModule());
    //    foreach (Block block in component)
    //    {
    //        block.callCheckConnection = false;
    //        if (!connected)
    //        {
    //            block.Remove();
    //        }
    //    }
    // }

    //void LateUpdate()
    //{
    //    if (callCheckConnection) // TODO: if Update() is not used, we can disable/enable the whole script rather than toggling the boolean callCheckConnection
    //    {
    //        CheckConnection();
    //    }
    //}
}