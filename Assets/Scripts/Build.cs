using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem;

public class Build : MonoBehaviour
{
    [SerializeField] Transform cameraTransform;
    [SerializeField] Inventory inventory;
    [SerializeField] Vehicle vehicle;
   
    public void OnBuild()
    {
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, LayerMask.GetMask("Default", "Block")))
        {
            // Get a reference to the block that was selected
            Block block = hit.collider.gameObject.GetComponent<Block>();

            // Check if block is null
            if (!block)
            {
                Debug.LogWarning("No block selected");
                return;
            }

            // Check if block is attached to player's vehicle
            if (vehicle != block.vehicle)
            {
                Debug.LogWarning("No authority");
                return;
            }

            // Calculate connection
            Vector3 connection = vehicle.commandModule.transform.InverseTransformDirection(hit.normal);

            // Check if block has that connection
            if (!block.connections.Contains(connection))
            {
                Debug.LogWarning("No connection");
                return;
            }

            // Add the new block
            block.vehicle.Add(block.coordinates + connection, connection, inventory.GetPrefab());
        }
    }

    public void OnMode()
    {
        vehicle.EnablePhysics();
    }
}
