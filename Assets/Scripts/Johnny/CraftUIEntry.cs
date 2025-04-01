using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class CraftUIEntry : MonoBehaviour
{
    public BlockInventoryManager blockInventoryManager;
    public delegate void OnCraftClicked(Block block);
    public OnCraftClicked onCraftClicked;
    public delegate void OnRecycleClicked(Block block);
    public OnRecycleClicked onRecycleClicked;
    public TextMeshProUGUI blockNameText;
    public TextMeshProUGUI blockRecycleBonusText;
    public TextMeshProUGUI blockCraftCostText;
    public Image iconImage;
    public Button blockCraftButton;
    public Button blockRecycleButton;
    public Block blockData;
    void Start()
    {
        if (blockNameText != null)
            blockNameText.text = blockData.BlockName;
        if (iconImage != null)
            iconImage.sprite = blockData.uiSprite;
    }

    void Update()
    {
        // if (craftButton != null)
        // {
        //     craftButton.onClick.RemoveAllListeners();
        //     craftButton.onClick.AddListener(() => onCraftClicked?.Invoke(block));
        // }
    }
}
