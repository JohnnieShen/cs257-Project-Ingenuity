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

    /*
    Checks periodically whether the player is within a certain distance of
    the boss' transform, if so, the UI health bar will be activated on
    the upper middle portion of the player's screen.
    */
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
