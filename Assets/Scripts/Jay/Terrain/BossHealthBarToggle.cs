using System.Collections;
using UnityEngine;

public class BossHealthBarToggle : MonoBehaviour
{
    /*
    Author: Jay
    Summary: Activates the boss's health bar and its child UI elements when the
    player is within a certain distance of the boss; deactivates all when out of range.
    */

    [Header("References")]
    public Transform player;
    public GameObject healthBarUI;

    [Header("Settings")]
    public float visibilityDistance = 30f;
    [SerializeField] private Block unlockableBlock;
    private static readonly WaitForSeconds WaitHalfSecond = new WaitForSeconds(0.5f);
    private Coroutine _distanceRoutine;

    private void OnEnable()
    {
        _distanceRoutine = StartCoroutine(CheckDistanceRoutine());
    }

    private void OnDisable()
    {
        if (_distanceRoutine != null)
            StopCoroutine(_distanceRoutine);
    }
    
    private void OnDestroy()
    {
        if (unlockableBlock == null) return;

        unlockableBlock.isCraftable = true;
        UIManager.Instance?.ShowPopup("New Craftable Block Unlocked: " + unlockableBlock.BlockName, 3f);
        if (CraftUIManager.Instance != null && CraftUIManager.Instance.isActiveAndEnabled)
            CraftUIManager.Instance.TryAddCraftableBlock(unlockableBlock);
    }

    private IEnumerator CheckDistanceRoutine()
    {
        while (true)
        {
            if (player != null && healthBarUI != null)
            {
                float dist = Vector3.Distance(transform.position, player.position);
                bool shouldShow = dist <= visibilityDistance;

                if (shouldShow != healthBarUI.activeSelf)
                {
                    healthBarUI.SetActive(shouldShow);
                    Debug.Log($"Health bar {(shouldShow ? "activated" : "deactivated")} (dist = {dist:F1} m)");
                }

                foreach (Transform child in healthBarUI.transform)
                {
                    if (child.gameObject.activeSelf != healthBarUI.activeSelf)
                        child.gameObject.SetActive(healthBarUI.activeSelf);
                }
            }

            yield return WaitHalfSecond;
        }
    }
}
