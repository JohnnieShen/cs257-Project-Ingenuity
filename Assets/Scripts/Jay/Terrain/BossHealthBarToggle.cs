using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHealthBarToggle : MonoBehaviour
{
    /*
    Author: Jay
    Summary: Activates the boss's health bar when the command module of the
    player is within a certain distance of the boss. Simple UI activation.
    */
    [Header("References")]
    public Transform player;
    public GameObject healthBarUI;

    [Header("Settings")]
    public float visibilityDistance = 30f;
    [SerializeField] private Block unlockableBlock;
    public float distance = 0f;
    private static readonly WaitForSeconds WaitHalfSecond = new WaitForSeconds(0.5f);
    private Coroutine _distanceRoutine;

    /*
    Checks periodically whether the player is within a certain distance of
    the boss' transform, if so, the UI health bar will be activated on
    the upper middle portion of the player's screen.
    */
    private void OnEnable()
    {
        // Start the distance-checking loop when the object becomes active
        _distanceRoutine = StartCoroutine(CheckDistanceRoutine());
    }

    private void OnDisable()
    {
        // Stop the loop when the object (or its parent) is disabled
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
            }

            // Wait for 0.5 s before checking again
            yield return WaitHalfSecond;
        }
    }
}
