using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ConvertToAI : MonoBehaviour
{
    private void Start()
    {
        Time.timeScale = 0;
        var commandModule = transform.Find("CommandModule");

        List<Transform> children = new List<Transform>();
        List<Wheel> wheels = new List<Wheel>();
        List<Turret> turrets = new List<Turret>();

        foreach (Transform child in transform)
        {
            // Used to track children so they can be reparented
            children.Add(child);

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
                turrets.Add(turret);
            }
        }

        // Set up HealthSystem script
        HealthSystem healthSystem = commandModule.AddComponent<HealthSystem>();

        // Set up EnemyMovement script
        EnemyMovement enemyMovement = commandModule.AddComponent<EnemyMovement>();
        enemyMovement.wheels = wheels.ToArray();
        enemyMovement.rb = commandModule.GetComponent<Rigidbody>();

        // Set up EnemyAI script
        EnemyAI enemyAI = commandModule.AddComponent<EnemyAI>();
        Transform aimTransform = new GameObject("AimTarget").transform;
        aimTransform.parent = transform;
        enemyAI.aimTransform = aimTransform;
        enemyAI.enemyTurrets = turrets;
        enemyAI.enemyMovement = enemyMovement;
        enemyAI.healthSystem = healthSystem;

        // Remove AimSphere and VehicleResourceManager scripts which are only used by the player vehicle
        Destroy(commandModule.GetComponent<AimSphere>());
        Destroy(commandModule.GetComponent<VehicleResourceManager>());

        // Reset transforms
        Transform center = new GameObject().transform;
        center.parent = transform;
        center.localPosition = commandModule.transform.localPosition;
        center.localRotation = commandModule.transform.localRotation;
        foreach (Transform child in children)
        {
            child.transform.parent = center.transform;
        }
        center.localPosition = Vector3.zero;
        center.localRotation = Quaternion.identity;
        foreach (Transform child in children)
        {
            child.transform.parent = transform;
        }
        Destroy(center.gameObject);
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
    }
}
