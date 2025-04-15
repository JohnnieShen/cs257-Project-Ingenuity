using System.Collections.Generic;
using UnityEngine;


/* BlockInventory tracks the inventory of blocks in the game.
 * It contains a Block reference, the current count of that block, and the maximum stack size.
 */
[System.Serializable]
public class BlockInventory
{
    public Block Block;
    public int CurrentCount;
    public int MaxStack = 99;
}

public class BlockInventoryManager : MonoBehaviour
{
    /* 
    * Author: Johnny
    * Summary: This script manages the inventory of blocks in the game. It allows for adding, consuming, and checking the count of blocks.
    * It also provides functionality to initialize the inventory and check if a block is present in the inventory.
    * The script uses a singleton pattern to ensure that only one instance of the BlockInventoryManager exists in the game.
    */

    public static BlockInventoryManager instance;

    [Header("Block Inventory Setup")]
    public BlockInventory[] availableBuildingBlocks;
    
    private Dictionary<Block, BlockInventory> blockInventory = new Dictionary<Block, BlockInventory>();

    private void Awake()
    {
        if (instance == null) instance = this;
        else Debug.LogError("Duplicated BlockInventoryManager!", gameObject);

        InitializeInventory();
    }

    private void InitializeInventory()
    {
        blockInventory.Clear();
        if (availableBuildingBlocks == null) return;

        foreach (BlockInventory bi in availableBuildingBlocks)
        {
            if (bi != null && bi.Block != null)
            {
                blockInventory[bi.Block] = bi;
            }
        }
    }

    public bool ContainsBlock(Block block)
    {
        return block != null && blockInventory.ContainsKey(block);
    }

    public int GetBlockCount(Block block)
    {
        if (block != null && blockInventory.TryGetValue(block, out var inventoryEntry))
        {
            return inventoryEntry.CurrentCount;
        }
        return 0;
    }

    public bool TryConsumeBlock(Block block, int amount)
    {
        if (block == null || amount <= 0) return false;

        if (blockInventory.TryGetValue(block, out var entry))
        {
            if (entry.CurrentCount >= amount)
            {
                entry.CurrentCount -= amount;
                blockInventory[block] = entry;
                return true;
            }
        }
        return false;
    }

    public void AddBlock(Block block, int amount)
    {
        if (block == null || amount <= 0) return;

        if (blockInventory.TryGetValue(block, out var entry))
        {
            // Debug.Log($"Adding {amount} of {block.BlockName} to inventory. Current: {entry.CurrentCount}, Max: {entry.MaxStack}");
            entry.CurrentCount = Mathf.Min(entry.CurrentCount + amount, entry.MaxStack);
            blockInventory[block] = entry; 
        }
        else
        {
            // Debug.Log($"Adding new block {block.BlockName} to inventory with amount {amount}.");
            BlockInventory newEntry = new BlockInventory()
            {
                Block = block,
                CurrentCount = amount,
                MaxStack = 99 // or a default
            };
            blockInventory.Add(block, newEntry);
        }
    }

    public string GetDisplayString(Block block)
    {
        if (block == null) return "None";
        if (blockInventory.TryGetValue(block, out var entry))
        {
            return $"{block.BlockName} ({entry.CurrentCount}/{entry.MaxStack})";
        }
        return block.BlockName + " (not in inventory)";
    }
}
