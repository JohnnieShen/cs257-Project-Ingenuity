using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
public class CraftUIManager : MonoBehaviour
{
    /* 
    * Author: Johnny
    * Summary: This script manages the crafting UI in the game. It populates the UI with available blocks, handles crafting and recycling actions,
    * and updates the scrap count displayed in the UI. It uses a dictionary to keep track of the UI entries for each block and updates them accordingly.
    * The script also listens for changes in the scrap count and updates the UI text accordingly.
    */
    public static CraftUIManager Instance { get; private set; }
    public GameObject craftUIEntryPrefab;
    public Transform entriesParent;
    public TextMeshProUGUI ScrapText;
    private BlockInventoryMatrix inventoryMatrix => BlockInventoryManager.instance.inventoryMatrix;
    private readonly Dictionary<Block, CraftUIEntry> entryLookup = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /* Start is called before the first frame update.
    * It populates the crafting UI with available blocks and sets up listeners for scrap count changes.
    * It also updates the scrap text to reflect the current scrap count.
    */
    private void Start()
    {
        PopulateCraftUI();
        if (VehicleResourceManager.Instance != null)
        {
            VehicleResourceManager.Instance.onScrapChanged.AddListener(UpdateScrapText);
            UpdateScrapText(VehicleResourceManager.Instance.scrapCount);
        }
    }

    private void OnEnable()
    {
        // PopulateCraftUI();
        if (VehicleResourceManager.Instance != null)
        {
            VehicleResourceManager.Instance.onScrapChanged.AddListener(UpdateScrapText);
            UpdateScrapText(VehicleResourceManager.Instance.scrapCount);
        }
    }

    /* It goes through the available building blocks in the BlockInventoryManager and creates a UI entry for each craftable block.
    * It sets up the UI entry with the block data and assigns the crafting and recycling actions to the buttons.
    * It also refreshes the entry to show the current block count.
    */
    private void PopulateCraftUI()
    {
        if (inventoryMatrix == null) return;

        for (int r = 0; r < inventoryMatrix.rowsCount; r++)
        {
            var row = inventoryMatrix.rows[r];
            if (row?.columns == null) continue;

            for (int c = 0; c < inventoryMatrix.columnsCount; c++)
            {
                var bi = row.columns[c];
                if (bi == null || bi.Block == null || !bi.Block.isCraftable) 
                    continue;

                var go = Instantiate(craftUIEntryPrefab, entriesParent);
                var entry = go.GetComponent<CraftUIEntry>();
                entry.blockData = bi.Block;
                entry.Setup(HandleCraftClicked, HandleRecycleClicked, bi.Block);

                entryLookup[bi.Block] = entry;
                RefreshEntry(bi.Block);
            }
        }
        StartCoroutine(RebuildNextFrame());
    }

    /* It handles the crafting action when the craft button is clicked.
    * It checks if there are enough resources to craft the block and updates the UI accordingly.
    * It also shows a popup message to inform the player about the crafting result.
    * Param 1: block - The block to be crafted.
    */
    private void HandleCraftClicked(Block block)
    {
        string message;
        Debug.Log("Craft clicked for: " + block.BlockName);

        if (VehicleResourceManager.Instance.TryUseScrap(block.cost))
        {
            BlockInventoryManager.instance.AddBlock(block, 1);
            message = $"Successfully crafted {block.BlockName}";
            Debug.Log(message);
            RefreshEntry(block);
        }
        else
        {
            message = $"Not enough scrap to craft {block.BlockName}";
            Debug.LogWarning(message);
        }

        UIManager.Instance.ShowPopup(message, 2f);
    }

    /* It handles the recycling action when the recycle button is clicked.
    * It checks if there are enough blocks to recycle and updates the UI accordingly.
    * It also shows a popup message to inform the player about the recycling result.
    * Param 1: block - The block to be recycled.
    */
    private void HandleRecycleClicked(Block block)
    {
        string message;
        Debug.Log("Recycle clicked for: " + block.BlockName);

        bool recycled = BlockInventoryManager.instance.TryConsumeBlock(block, 1);
        if (recycled)
        {
            int bonus = block.recycleBonus;
            VehicleResourceManager.Instance.AddScrap(bonus);
            message = $"Recycled {block.BlockName}, bonus: {bonus}";
            Debug.Log(message);
            RefreshEntry(block);
        }
        else
        {
            message = $"No {block.BlockName} available to recycle";
            Debug.LogWarning(message);
        }

        UIManager.Instance.ShowPopup(message, 2f);
    }

    /* It updates the scrap text displayed in the UI.
    * It takes in the new scrap count and updates the text accordingly.
    * Param 1: newValue - The new scrap count to be displayed.
    */
    private void UpdateScrapText(int newValue)
    {
        // Just update the text so the UI shows the current scrapCount
        if (ScrapText != null)
        {
            ScrapText.text = newValue.ToString();
        }
    }

    /* It refreshes the entry for a specific block in the crafting UI.
    * It checks if the entry exists in the dictionary and updates the block count displayed in the UI.
    * Param 1: block - The block whose entry needs to be refreshed.
    */
    private void RefreshEntry(Block block)
    {
        if (entryLookup.TryGetValue(block, out var entry))
        {
            int newCount = BlockInventoryManager.instance.GetBlockCount(block);
            entry.UpdateCount(newCount);
        }
    }
    public void TryAddCraftableBlock(Block block)
    {
        if (block == null || !block.isCraftable) return;
        if (entryLookup.ContainsKey(block))
            return;

        var go = Instantiate(craftUIEntryPrefab, entriesParent);
        var entry = go.GetComponent<CraftUIEntry>();

        entry.blockData = block;
        entry.Setup(HandleCraftClicked, HandleRecycleClicked, block);

        entryLookup[block] = entry;
        RefreshEntry(block);
        StartCoroutine(RebuildNextFrame());
    }
    IEnumerator RebuildNextFrame()
    {
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)entriesParent);
    }
}
