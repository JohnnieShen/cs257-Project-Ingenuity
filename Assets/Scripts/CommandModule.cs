using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class CommandModule : MonoBehaviour
{
    public GameObject authority;
    public Block[,,] blocks;
    [SerializeField] public Transform buildTransform; // Make sure the transform lies in the corner of the build area such that it extends towards the positive octant

    void Awake()
    {
        // Instantiate blocks
        BoxCollider buildArea = buildTransform.gameObject.GetComponent<BoxCollider>();
        blocks = new Block[(int) buildArea.size.x, (int) buildArea.size.y, (int) buildArea.size.z];

        // Put command module in the center
        gameObject.GetComponent<Block>().Initialize(this, FloatToInt(buildTransform.InverseTransformPoint(transform.position)));
    }

    public void Add(Vector3Int coordinates, GameObject prefab)
    {
        // Check if coordinates are out of bounds
        if (coordinates.x < 0 || coordinates.x >= blocks.GetLength(0) || coordinates.y < 0 || coordinates.y >= blocks.GetLength(1) || coordinates.z < 0 || coordinates.z >= blocks.GetLength(2))
        {
            Debug.LogWarning("Exceeded build area");
            return;
        }

        // Instantiate block
        Block block = Instantiate(prefab, buildTransform.TransformPoint(coordinates), buildTransform.rotation).GetComponent<Block>();
        block.Initialize(this, coordinates);
    }

    public Vector3 IntToFloat(Vector3Int v)
    {
        return new Vector3(v.x, v.y, v.z);
    }

    public Vector3Int FloatToInt(Vector3 v)
    {
        return new Vector3Int((int) v.x, (int) v.y, (int) v.z);
    }

    //// TODO: implement these
    //Vector3 shootTarget; // in world space
    //int turn; // 0 for left, 1 for straight, 2 for right
    //int throttle; // 0 for brake, 1 for none, 2 for throttle up

public void OnToggleMode()
{
    EnablePhysics();
}

public void EnablePhysics()
{
    // Compute joints
    foreach (Block block in blocks)
    {
        // Skip if empty
        if (!block)
        {
            continue;
        }
        
        // Find adjacent blocks with compatible connections
        HashSet<Block> connectedBlocks = new HashSet<Block>();
        foreach (Vector3Int connection in block.connections)
        {
            Vector3Int otherCoordinates = block.coordinates + connection;
            if (otherCoordinates.x < 0 || otherCoordinates.x >= blocks.GetLength(0) || otherCoordinates.y < 0 || otherCoordinates.y >= blocks.GetLength(1) || otherCoordinates.z < 0 || otherCoordinates.z >= blocks.GetLength(2))
            {
                continue;
            }
            Block otherBlock = blocks[otherCoordinates.x, otherCoordinates.y, otherCoordinates.z];
            if (otherBlock && otherBlock.connections.Contains(-connection))
            {
                connectedBlocks.Add(otherBlock);
            }
        }

        // Add joint to each with break force equal to the sum of the connection strength parameters
        foreach (Block connectedBlock in connectedBlocks)
        {
            FixedJoint joint = block.gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = connectedBlock.gameObject.GetComponent<Rigidbody>();
            joint.breakForce = block.connectionStrength + connectedBlock.connectionStrength;
        }
    }

    // Enable physics
    foreach (Block block in blocks)
    {
        if (block)
        {
            block.gameObject.GetComponent<Rigidbody>().isKinematic = false; // TODO: move this into the above loop if there are no issues with blocks falling during joint construction
        }
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
    //    foreach (Vector3Int connection in block.connections)
    //    {
    //        Vector3Int otherPosition = block.position + connection;
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