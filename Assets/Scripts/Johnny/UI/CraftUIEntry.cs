using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftUIEntry : MonoBehaviour
{
    /*
    * Author: Johnny
    * Summary: This script is responsible for managing the UI entry for crafting and recycling blocks in the game.
    * It provides functionality to set up the UI with block data, update the block count, and handle button clicks for crafting and recycling.
    */

    public delegate void OnCraftClicked(Block block);
    public OnCraftClicked onCraftClicked;
    public delegate void OnRecycleClicked(Block block);
    public OnRecycleClicked onRecycleClicked;

    public TextMeshProUGUI blockNameText;
    public TextMeshProUGUI blockCraftCostText;
    public TextMeshProUGUI blockRecycleBonusText;
    public Image iconImage;
    public Button blockCraftButton;
    public Button blockRecycleButton;
    
    public Block blockData;

    /* Setup method initializes the UI entry with block data and sets up button click listeners.
    * It takes in delegates for crafting and recycling actions, as well as the block data to be displayed.
    * It updates the block name, icon, craft cost, and recycle bonus text based on the provided block data.
    * Param 1: craftDelegate - Delegate for crafting action.
    * Param 2: recycleDelegate - Delegate for recycling action.
    * Param 3: blockData - Block data to be displayed in the UI entry.
    */
    public void Setup(OnCraftClicked craftDelegate, OnRecycleClicked recycleDelegate, Block blockData)
    {
        this.blockData = blockData;
        int current = BlockInventoryManager.instance.GetBlockCount(blockData);
        UpdateCount(current);
        if (blockNameText != null)
            blockNameText.text = blockData.BlockName;
        if (iconImage != null)
            iconImage.sprite = blockData.uiSprite;
        if (blockCraftCostText != null)
            blockCraftCostText.text = "-" + blockData.cost.ToString();
        if (blockRecycleBonusText != null)
            blockRecycleBonusText.text = "+" + blockData.recycleBonus.ToString();
        
        onCraftClicked = craftDelegate;
        onRecycleClicked = recycleDelegate;
        
        if (blockCraftButton != null)
        {
            blockCraftButton.onClick.RemoveAllListeners();
            blockCraftButton.onClick.AddListener(() => onCraftClicked?.Invoke(blockData));
        }
        if (blockRecycleButton != null)
        {
            blockRecycleButton.onClick.RemoveAllListeners();
            blockRecycleButton.onClick.AddListener(() => onRecycleClicked?.Invoke(blockData));
        }
    }

    /* UpdateCount method updates the block count displayed in the UI entry.
    * It takes in the new count and updates the block name text to reflect the current count.
    * Param 1: count - The new count of the block to be displayed.
    */
    public void UpdateCount(int count)
    {
        if (blockNameText != null && blockData != null)
            blockNameText.text = $"{blockData.BlockName} ({count})";
    }
}
