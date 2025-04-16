using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHealthBarToggle : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameObject healthBarUI;

    [Header("Settings")]
    public float visibilityDistance = 30f;

    private void Update()
    {
        if (player == null || healthBarUI == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= visibilityDistance)
        {
            if (!healthBarUI.activeSelf)
                healthBarUI.SetActive(true);
        }
        else
        {
            if (healthBarUI.activeSelf)
                healthBarUI.SetActive(false);
        }
    }
}
