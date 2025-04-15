using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ConvertToAI : MonoBehaviour
{
    private void Start()
    {
        var commandModule = transform.Find("CommandModule");

        List<Wheel> wheels = new List<Wheel>();

        foreach (Transform child in transform)
        {
            // Convert blocks from Player tag and layer to Enemy tag and layer
            child.tag = "EnemyBlock";
            child.gameObject.layer = LayerMask.NameToLayer("EnemyBlock");

            // Obtain reference to wheel
            Wheel wheel = child.GetComponentInChildren<Wheel>();
            if (wheel != null )
            {
                wheel.isAI = true;
                wheels.Add(wheel);
            }

            // Obtain reference to turret
            Turret turret = child.GetComponent<Turret>();
            if (turret != null)
            {
                turret.isAI = true;
            }
        }

        // Set up HealthSystem script
        HealthSystem healthSystem = commandModule.AddComponent<HealthSystem>();

        // Set up EnemyMovement script
        EnemyMovement enemyMovement = commandModule.AddComponent<EnemyMovement>();
        enemyMovement.wheels = wheels.ToArray();

        // Set up EnemyAI script
        EnemyAI enemyAI = commandModule.AddComponent<EnemyAI>();
        enemyAI.enemyMovement = enemyMovement;
        enemyAI.healthSystem = healthSystem;

        // Remove AimSphere and VehicleResourceManager scripts which are only used by the player vehicle
    }
}
