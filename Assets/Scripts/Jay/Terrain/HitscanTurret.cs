using UnityEngine;
using UnityEngine.Animations.Rigging;
using System.Collections;
using System.Collections.Generic;

public class HitscanTurret : MonoBehaviour
{
    /*
    Author: Johnny / Jay
    Summary: This is a copy of the original script (outdated copy) for the hitscan gun, please refer
    to Johnny's folder for full documentation. This script was for testing purposes
    for particle effects.
    */
    [SerializeField] private ParticleSystem muzzleSpark;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private List<MultiAimConstraint> aimConstraints = new List<MultiAimConstraint>();

    private Transform aimTarget;
    [Header("Shooting Settings")]
    [SerializeField] private Transform shootPoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float reloadTime = 2f;
    [SerializeField] private float ballisticDamage = 10f;
    [SerializeField] private float energyDamage = 10f;
    [SerializeField] private int magazineSize = 5;
    [Header("Obstacle Check Settings")]
    [SerializeField] private float checkDistance = 100f;
    public LineRenderer blockedLine;

    private int currentAmmo;
    private float nextFireTime = 0f;
    private bool isReloading = false;
    public bool isBlocked = false;
    public bool isAI = false;
    // public Transform aimTransform;

    void Start()
    {
        // aimTarget = FreeCameraLook.instance?.aimTarget;
        EnemyAI enemyAI = GetComponentInParent<EnemyAI>();
         if (enemyAI != null && enemyAI.aimTransform != null)
        {
            aimTarget = enemyAI.aimTransform;
            isAI = true;
        }
        else
        {
            aimTarget = FreeCameraLook.instance?.aimTarget;
            isAI = false;
        }
        if (aimTarget == null)
        {
            Debug.LogWarning("Turret target not assigned in FreeCameraLook or FreeCameraLook instance is missing.");
            return;
        }

        foreach (MultiAimConstraint constraint in aimConstraints)
        {
            SetConstraintTarget(constraint, aimTarget);
        }

        RigBuilder rigs = GetComponent<RigBuilder>();
        if (rigs != null)
            rigs.Build();

        currentAmmo = magazineSize;
        if (blockedLine != null)
        {
            blockedLine.startWidth = 0.05f;
            blockedLine.endWidth = 0.05f;
            blockedLine.enabled = false;
        }
    }
    private void DisableAimConstraints()
    {
        foreach (MultiAimConstraint constraint in aimConstraints)
        {
            SetConstraintWeight(constraint, 0f);
        }
        RigBuilder rigs = GetComponent<RigBuilder>();
        if (rigs != null)
            rigs.Build();
    }

    private void SetConstraintWeight(MultiAimConstraint constraint, float weight)
    {
        if (constraint != null)
            constraint.weight = weight;
    }
    void Update()
    {
        Hull hull = GetComponent<Hull>();
        if (hull != null && hull.canPickup)
        {
            if (blockedLine != null)
                blockedLine.enabled = false;
            DisableAimConstraints();
            return;
        }
        if (aimTarget == null)
            return;
        CheckIfBlocked();
    }
    void OnEnable()
    {
        if (isAI)
            return;
        FreeCameraLook.OnFire += HandleFireEvent;
    }

    void OnDisable()
    {
        if (isAI)
            return;
        FreeCameraLook.OnFire -= HandleFireEvent;
    }

    public void HandleFireEvent()
    {
        Hull hull = GetComponent<Hull>();
        if (hull != null && hull.canPickup)
            return;
        if (isReloading)
            return;

        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }
        if (isBlocked)
        {
            return;
        }

        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + (1f / fireRate);
        }
    }

    private void Shoot()
    {
        if (projectilePrefab != null && shootPoint != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);
            Projectile projScript = projectile.GetComponent<Projectile>();
            if (projScript != null)
            {
                projScript.SetDamage(ballisticDamage, energyDamage);
            }
        }
        else
        {
            Debug.LogWarning("Projectile prefab or shoot point is not assigned.");
        }
        if (muzzleFlash != null) {
            muzzleFlash.transform.position = shootPoint.position;
            muzzleFlash.transform.rotation = shootPoint.rotation;
            muzzleFlash.Play();
        }
        if (muzzleSpark != null) {
            muzzleSpark.transform.position = shootPoint.position;
            muzzleSpark.transform.rotation = shootPoint.rotation;
            muzzleSpark.Play();
        }

        currentAmmo--;
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reloading...");
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = magazineSize;
        isReloading = false;
        Debug.Log("Reload complete.");
    }

    private void SetConstraintTarget(MultiAimConstraint constraint, Transform newTarget)
    {
        if (constraint == null || newTarget == null)
            return;

        var data = constraint.data;
        var sources = data.sourceObjects;

        if (sources.Count == 0)
        {
            sources.Add(new WeightedTransform(newTarget, 1f));
        }
        else
        {
            sources.SetTransform(0, newTarget);
        }

        constraint.weight = 1f;
        data.sourceObjects = sources;
        constraint.data = data;
    }

    private void CheckIfBlocked()
    {
        if (shootPoint == null) 
        {
            isBlocked = false;
            return;
        }

        Ray ray = new Ray(shootPoint.position, shootPoint.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, checkDistance))
        {
            if (!isAI) {
                // Debug.Log("Hit object: " + hit.collider.name);
                Debug.DrawRay(ray.origin, ray.direction * checkDistance, Color.red);
                if (hit.collider.CompareTag("Block")||hit.collider.CompareTag("Core"))
                {
                    if (blockedLine != null)
                    {
                        blockedLine.enabled = true;
                        blockedLine.positionCount = 2;
                        blockedLine.SetPosition(0, shootPoint.position);
                        blockedLine.SetPosition(1, hit.point);
                    }
                    isBlocked = true;
                    return;
                }
            }
            else
            {
                if (hit.collider.CompareTag("EnemyBlock"))
                {
                    isBlocked = true;
                    return;
                }
            }
        }

        isBlocked = false;
        if (blockedLine != null)
            blockedLine.enabled = false;
    }
}
