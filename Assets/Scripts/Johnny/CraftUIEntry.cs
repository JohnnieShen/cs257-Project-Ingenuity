using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftUIEntry : MonoBehaviour
{
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

    public void Setup(OnCraftClicked craftDelegate, OnRecycleClicked recycleDelegate, Block blockData)
    {
        this.blockData = blockData;
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
}
