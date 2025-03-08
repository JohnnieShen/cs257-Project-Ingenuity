using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class CommandModule : MonoBehaviour
{
    [SerializeField] int radius; // Determines the build area using Manhattan distance
    public GameObject authority;
    public Block[,,] blocks;

    void Awake()
    {
        // Instantiate blocks
        blocks = new Block[2 * radius + 1, 2 * radius + 1, 2 * radius + 1];

        // Put command module in the center
        gameObject.GetComponent<Block>().Initialize(this, new Vector3Int(radius, radius, radius));
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
        Block block = Instantiate(prefab, transform.TransformPoint(coordinates - new Vector3Int(radius, radius, radius)), transform.rotation, transform).GetComponent<Block>();
        block.Initialize(this, coordinates);
    }

    //// TODO: implement these
    //Vector3 shootTarget; // in world space
    //int turn; // 0 for left, 1 for straight, 2 for right
    //int throttle; // 0 for brake, 1 for none, 2 for throttle up

    //void EnablePhysics()
    //{
    //    foreach (Block block in blocks)
    //    {
    //        if (!block)
    //        {
    //            continue;
    //        }
    //        foreach (Block neighbor in Neighbors(block))
    //        {
    //            FixedJoint joint = block.gameObject.AddComponent<FixedJoint>();
    //            joint.connectedBody = neighbor.gameObject.GetComponent<Rigidbody>();
    //            joint.breakForce = block.connectionStrength + neighbor.connectionStrength;
    //        }
    //    }
    //    foreach (Block block in blocks)
    //    {
    //        block.gameObject.GetComponent<Rigidbody>().isKinematic = false; // TODO: move this into the above loop if there are no issues with blocks falling during joint construction
    //    }
    //}

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
}