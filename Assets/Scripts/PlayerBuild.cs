using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBuild : MonoBehaviour
{
    [SerializeField] Transform cameraTransform;
    [SerializeField] GameObject[] inventory;
    int index;
    public void OnBuild()
    {
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, LayerMask.GetMask("Default", "Block")))
        {
            // Log a warning if the player did not select a block
            if (hit.transform.gameObject.layer != LayerMask.NameToLayer("Block"))
            {
                Debug.LogWarning("No block selected");
                return;
            }

            // Get a reference to the block that was selected
            Block block = hit.collider.gameObject.GetComponent<Block>();

            // Log a warning if the player does not have the authority to add to this command module
            if (!block.commandModule || gameObject != block.commandModule.authority)
            {
                Debug.LogWarning("No authority");
                return;
            }

            // Calculate coordinates of the new block
            Vector3 localNormal = block.transform.InverseTransformDirection(hit.normal);
            Vector3Int coordinates = block.coordinates + new Vector3Int(Mathf.RoundToInt(localNormal.x), Mathf.RoundToInt(localNormal.y), Mathf.RoundToInt(localNormal.z));

            // Add the new block
            block.commandModule.Add(coordinates, inventory[index]);
        }
    }

    public void OnCollect()
    {
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, LayerMask.GetMask("Default", "Block")))
        {
            // Log a warning if the player did not select a block
            if (hit.transform.gameObject.layer != LayerMask.NameToLayer("Block"))
            {
                Debug.LogWarning("No block selected");
                return;
            }

            // Get a reference to the block that was selected
            Block block = hit.collider.gameObject.GetComponent<Block>();

            // Log a warning if the player does not have the authority to collect this block or if this block is a command module
            if ((block.commandModule && gameObject != block.commandModule.authority) || block.gameObject.GetComponent<CommandModule>())
            {
                Debug.LogWarning("No authority");
                return;
            }

            // Collect block
            AddToInventory(block);
        }
    }

    void AddToInventory(Block block)
    {
        // TODO
        if (block.commandModule)
        {
            block.commandModule.blocks[block.coordinates.x, block.coordinates.y, block.coordinates.z] = null;
        }
        Destroy(block.gameObject);
    }

    int mod(int x, int y)
    {
        while (x < 0)
        {
            x += y;
        }
        while (x >= y)
        {
            x -= y;
        }
        return x;
    }

    public void OnSwitch(InputValue value)
    {
        index = mod(index + (int) value.Get<Vector2>().y, inventory.Length);
    }
}
