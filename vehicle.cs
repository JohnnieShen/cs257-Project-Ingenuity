using System.Collections;
using System.Collections.Generic;
using System; // TODO: just import Tuple instead of System
using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField] Vehicle vehicle;
    public int connectionStrength; // TODO: implement without public
    [SerializeField] Vector3Int[] connectionsArray; // used to instantiate connections since sets cannot be serialized
    public HashSet<Vector3Int> connections; // TODO: implement without public 
    public Vector3Int position; // TODO: implement without public
    int health;

    public bool callCheckConnection = false; // true if a joint has broken and CheckConnections should be called at the end of this frame

    void Awake()
    {
        connections = new HashSet<Vector3Int>(connectionsArray);
    }

    public void Initialize(Vehicle vehicle, Vector3Int position)
    {
        this.vehicle = vehicle;
        this.position = position;
    }

    void Remove()
    {
        vehicle.Remove(this);
        position = Vector3Int.zero;
        vehicle = null;
        // TODO: reparent to environment
    }

    HashSet<Block> Component()
    {
        HashSet<Block> component = new HashSet<Block>();
        Queue<Block> queue = new Queue<Block>();
        queue.Enqueue(this);
        while (queue.Count > 0)
        {
            Block block = queue.Dequeue();
            if (!component.Contains(block))
            {
                component.Add(block);
                foreach (FixedJoint joint in block.GetComponents<FixedJoint>())
                {
                    queue.Enqueue(joint.connectedBody.gameObject.GetComponent<Block>());
                }
            }
        }
        return component;
    }

    void OnJointBreak()
    {
        callCheckConnection = true;
    }

    void LateUpdate()
    {
        if (callCheckConnection) // TODO: if Update() is not used, we can disable/enable the whole script rather than toggling the boolean callCheckConnection
        {
            CheckConnection();
        }
    }

    void CheckConnection()
    {
        HashSet<Block> component = Component();
        bool connected = component.Contains(vehicle.GetCommandModule());
        foreach (Block block in component)
        {
            block.callCheckConnection = false;
            if (!connected)
            {
                block.Remove();
            }
        }
    }

    // void Update()
    // {
    //     // Maybe this should be done in the wheel and gun scripts which have a reference to the command module which stores the move and attack data
    //     if (vehicle)
    //     {
    //         if (wheel)
    //         {
    //             wheel.Move(vehicle.moveTarget);
    //         }
    //         if (gun)
    //         {
    //             gun.Shoot(vehicle.shootTarget);
    //         }
    //     }
    // }

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
    // TODO: put these in command module block
    // public Vector3 shootTarget;
    // public int turn; // 0 for left, 1 for straight, 2 for right
    // public int throttle; // 0 for brake, 1 for none, 2 for throttle up
    [SerializeField] int size;
    Block[, ,] blocks;
    Block commandModule;

    void Awake()
    {
        blocks = new Block[2 * size + 1, 2 * size + 1, 2 * size + 1]; // command module should be in the center at (size, size, size)
        Transform commandModuleTransform = commandModule.gameObject.transform;
    }

    void EnablePhysics()
    {
        foreach (Block block in blocks) // TODO: implement this more efficiently without iterating through the entire array which is mostly null
        {
            if (!block)
            {
                continue;
            }
            foreach (Block neighbor in Neighbors(block))
            {
                FixedJoint joint = block.gameObject.AddComponent<FixedJoint>();
                joint.connectedBody = neighbor.gameObject.GetComponent<Rigidbody>();
                joint.breakForce = block.connectionStrength + neighbor.connectionStrength;
            }
        }
        foreach (Block block in blocks)
        {
            block.gameObject.GetComponent<Rigidbody>().isKinematic = false; // TODO: move this into the above loop if there are no issues with blocks falling during joint construction
        }
    }

    void DisablePhysics()
    {
        foreach (Block block in blocks)
        {
            foreach (FixedJoint joint in block.GetComponents<FixedJoint>())
            {
                Destroy(joint);
            }
            block.gameObject.GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    void Add(GameObject prefab, Block block, Vector3Int direction)
    {
        if (!block.connections.Contains(direction))
        {
            Debug.LogError("Blocks must be attached to a connection point.");
        }
        Vector3Int position = block.position + direction;
        if (position.x < 0 || position.x >= blocks.GetLength(0) || position.y < 0 || position.y >= blocks.GetLength(1) || position.z < 0 || position.z >= blocks.GetLength(2))
        {
            Debug.LogError("Blocks cannot be placed outside of the vehicle's build area.");
        }
        Transform blockTransform = block.gameObject.transform;
        Block newBlock = Instantiate(prefab, blockTransform.TransformPoint(direction), blockTransform.rotation, transform).GetComponent<Block>();
        newBlock.Initialize(this, position);
        blocks[position.x, position.y, position.z] = newBlock;
    }

    public void Remove(Block block)
    {
        blocks[block.position.x, block.position.y, block.position.z] = null;
    }

    HashSet<Block> Neighbors(Block block)
    {
        HashSet<Block> neighbors = new HashSet<Block>(); // TODO: should neighbors contain self?
        foreach (Vector3Int connection in block.connections)
        {
            Vector3Int otherPosition = block.position + connection;
            Block otherBlock = blocks[otherPosition.x, otherPosition.y, otherPosition.z];
            if (otherBlock && otherBlock.connections.Contains(-connection))
            {
                neighbors.Add(otherBlock);
            }
        }
        return neighbors;
    }

    public Block GetCommandModule()
    {
        return commandModule;
    }
}
