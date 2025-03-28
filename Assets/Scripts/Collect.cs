using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collect : MonoBehaviour
{
    [SerializeField] Transform cameraTransform;
    [SerializeField] Inventory inventory;
    [SerializeField] Vehicle vehicle;
    [SerializeField] Block commandModule;
    public void OnCollect()
    {
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, LayerMask.GetMask("Default", "Block")))
        {
            // Check that a block was selecteed
            if (hit.transform.gameObject.layer != LayerMask.NameToLayer("Block"))
            {
                Debug.LogWarning("No block selected");
                return;
            }

            // Get a reference to the block that was selected
            Block block = hit.collider.gameObject.GetComponent<Block>();

            // Log a warning if the player does not have the authority to collect this block or if this block is a command module
            if (block.vehicle != vehicle)
            {
                Debug.LogWarning("No authority");
                return;
            }
            
            // Check if block is command module
            if (block == commandModule)
            {
                Debug.LogWarning("Cannot remove command module");
                return;
            }

            // Collect block
            inventory.Add(block);
        }
    }
}
