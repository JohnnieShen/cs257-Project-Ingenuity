using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Block : MonoBehaviour
{
    public CommandModule commandModule;
    public Vector3Int coordinates;

    public void Initialize(CommandModule commandModule, Vector3Int coordinates)
    {
        this.commandModule = commandModule;
        this.coordinates = coordinates;
        commandModule.blocks[coordinates.x, coordinates.y, coordinates.z] = this;
    }
    //public int connectionStrength; // TODO: implement without public
    //[SerializeField] Vector3Int[] connectionsArray; // used to instantiate connections since sets cannot be serialized
    //public HashSet<Vector3Int> connections; // TODO: implement without public 

    //int health;

    //public bool callCheckConnection = false; // true if a joint has broken and CheckConnections should be called at the end of this frame

    //void Awake()
    //{
    //    connections = new HashSet<Vector3Int>(connectionsArray);
    //}

    //public void Initialize(Vector3Int position)
    //{
    //    this.position = position;
    //}

    //void Remove()
    //{
    //    vehicle.Remove(this);
    //    position = Vector3Int.zero;
    //    vehicle = null;
    //    // TODO: reparent to environment
    //}

    //HashSet<Block> ConnectedBlocks()
    //{
    //    HashSet<Block> component = new HashSet<Block>();
    //    Queue<Block> queue = new Queue<Block>();
    //    queue.Enqueue(this);
    //    while (queue.Count > 0)
    //    {
    //        Block block = queue.Dequeue();
    //        if (!component.Contains(block))
    //        {
    //            component.Add(block);
    //            foreach (FixedJoint joint in block.GetComponents<FixedJoint>())
    //            {
    //                queue.Enqueue(joint.connectedBody.gameObject.GetComponent<Block>());
    //            }
    //        }
    //    }
    //    return component;
    //}

    //void OnJointBreak()
    //{
    //    callCheckConnection = true;
    //}

    //void LateUpdate()
    //{
    //    if (callCheckConnection) // TODO: if Update() is not used, we can disable/enable the whole script rather than toggling the boolean callCheckConnection
    //    {
    //        CheckConnection();
    //    }
    //}

    //void CheckConnection()
    //{
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
    //}
}