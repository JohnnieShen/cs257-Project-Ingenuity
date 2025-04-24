using System.Linq;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System.Text;

public class ChecklistEvaluator : MonoBehaviour
{
    [SerializeField] private BlockChecklist checklist;
    public TextMeshProUGUI checklistText;

    public bool IsComplete => checklist.requirements
                                       .All(r => BlockManager.instance.GetMountedCount(r.block) >= r.requiredCount);

    private void OnEnable()
    {
        VehicleBuildEvents.OnBlockAdded += Recheck;
        VehicleBuildEvents.OnBlockRemoved += Recheck;
        UpdateUI();
    }
    private void OnDisable()
    {
        VehicleBuildEvents.OnBlockAdded -= Recheck;
        VehicleBuildEvents.OnBlockRemoved -= Recheck;
    }

    private void Recheck(Block _, Vector3Int __)
    {
        UpdateUI();
        if (IsComplete)
            GameManager.Instance.WinGame();
    }
    private void UpdateUI()
    {
        if (checklistText == null || checklist == null) return;

        StringBuilder sb = new StringBuilder();
        foreach (var req in checklist.requirements)
        {
            int have = BlockManager.instance.GetMountedCount(req.block);
            sb.AppendLine($"{req.block.BlockName}: {have} / {req.requiredCount}");
        }
        checklistText.text = sb.ToString();
    }
}
