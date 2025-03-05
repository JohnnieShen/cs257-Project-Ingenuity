using UnityEngine;
using UnityEngine.Animations.Rigging;
using System.Collections;

public class Turret : MonoBehaviour
{
    [SerializeField] private MultiAimConstraint baseAim;
    [SerializeField] private MultiAimConstraint topArmAim;
    [SerializeField] private MultiAimConstraint gunAim;

    private Transform aimTarget;
    [Header("Shooting Settings")]
    [SerializeField] private Transform shootPoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float reloadTime = 2f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private int magazineSize = 5;
    [Header("Obstacle Check Settings")]
    [SerializeField] private float checkDistance = 100f;
    public LineRenderer blockedLine;

    private int currentAmmo;
    private float nextFireTime = 0f;
    private bool isReloading = false;
    public bool isBlocked = false;

    void Start()
    {
        aimTarget = FreeCameraLook.instance?.aimTarget;
        if (aimTarget == null)
        {
            Debug.LogWarning("Turret target not assigned in FreeCameraLook or FreeCameraLook instance is missing.");
            return;
        }

        SetConstraintTarget(baseAim, aimTarget);
        SetConstraintTarget(topArmAim, aimTarget);
        SetConstraintTarget(gunAim, aimTarget);

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

    void Update()
    {
        if (aimTarget == null)
            return;
        CheckIfBlocked();
    }
    void OnEnable()
    {
        FreeCameraLook.OnFire += HandleFireEvent;
    }

    void OnDisable()
    {
        FreeCameraLook.OnFire -= HandleFireEvent;
    }

    public void HandleFireEvent()
    {
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
                projScript.SetDamage(damage);
            }
        }
        else
        {
            Debug.LogWarning("Projectile prefab or shoot point is not assigned.");
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

        isBlocked = false;
        if (blockedLine != null)
            blockedLine.enabled = false;
    }
}
