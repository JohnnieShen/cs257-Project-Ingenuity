using UnityEngine;
using TMPro;

public class CraftUIManager : MonoBehaviour
{
    public GameObject craftUIEntryPrefab;
    public Transform entriesParent;
    public TextMeshProUGUI ScrapText;

    private void Start()
    {
        PopulateCraftUI();
        if (VehicleResourceManager.Instance != null)
        {
            VehicleResourceManager.Instance.onScrapChanged.AddListener(UpdateScrapText);
            UpdateScrapText(VehicleResourceManager.Instance.scrapCount);
        }
    }

    private void PopulateCraftUI()
    {
        var invManager = BlockInventoryManager.instance;
        if (invManager == null || invManager.availableBuildingBlocks == null)
        {
            return;
        }

        foreach (var blockInv in invManager.availableBuildingBlocks)
        {
            if (blockInv != null && blockInv.Block != null && blockInv.Block.isCraftable)
            {
                GameObject entryObj = Instantiate(craftUIEntryPrefab, entriesParent);
                CraftUIEntry entry = entryObj.GetComponent<CraftUIEntry>();
                if (entry != null)
                {
                    entry.blockData = blockInv.Block;
                    entry.Setup(HandleCraftClicked, HandleRecycleClicked, blockInv.Block);
                }
            }
        }
    }

    private void HandleCraftClicked(Block block)
    {
        Debug.Log("Craft clicked for: " + block.BlockName);

        if (VehicleResourceManager.Instance.TryUseScrap(block.cost))
        {
            BlockInventoryManager.instance.AddBlock(block, 1);
            Debug.Log("Crafted: " + block.BlockName);
        }
        else
        {
            Debug.Log("Not enough scrap to craft: " + block.BlockName);
        }
    }

    private void HandleRecycleClicked(Block block)
    {
        Debug.Log("Recycle clicked for: " + block.BlockName);

        bool recycled = BlockInventoryManager.instance.TryConsumeBlock(block, 1);
        if (recycled)
        {
            int bonus = block.recycleBonus;
            VehicleResourceManager.Instance.AddScrap(bonus);
            Debug.Log("Recycled: " + block.BlockName + ", bonus: " + bonus);
        }
        else
        {
            Debug.Log("No block available to recycle: " + block.BlockName);
        }
    }
    private void UpdateScrapText(int newValue)
    {
        // Just update the text so the UI shows the current scrapCount
        if (ScrapText != null)
        {
            ScrapText.text = newValue.ToString();
        }
    }
}
