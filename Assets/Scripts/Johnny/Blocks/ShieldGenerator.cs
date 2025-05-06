using UnityEngine;

public class ShieldGenerator : MonoBehaviour
{
    /*
    * Author: Johnny
    * Summary: This script is attached to a shield generator block in the game. It manages the shield's health and regeneration.
    * The shield absorbs damage from enemy projectiles and regenerates over time.
    * The shield is represented by a sphere collider and a mesh renderer.
    */
    public float maxShieldHealth = 100f;
    public float currentShieldHealth = 100f;
    public float shieldRegenRate = 5f;
    public float shieldRegenDelay = 3f;
    public float shieldRadius = 5f;
    
    public Material shieldMaterial;
    public Material aiShieldMaterial;
    public bool isAI = false;
    
    public SphereCollider shieldCollider;
    public MeshRenderer shieldRenderer;
    private float lastDamageTime;
    private float regenerationTimer;
    private bool isRegenerating;
    // private bool isShieldActive = true;

    /* Awake is called when the script instance is being loaded.
    * It initializes the shield and sets up the collider and renderer.
    */
    private void Awake()
    {
        InitializeShield();
    }

    /* Helper for initializing the shield.
    * It creates a new GameObject for the shield, adds a MeshRenderer and SphereCollider to it,
    * and sets the material and properties for the shield.
    */
    private void InitializeShield()
    {
        GameObject shieldObject = new GameObject("Shield");
        shieldObject.transform.SetParent(transform);
        shieldObject.transform.localPosition = Vector3.zero;
        shieldObject.transform.localScale = Vector3.one * shieldRadius;

        shieldRenderer = shieldObject.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = shieldObject.AddComponent<MeshFilter>();
        meshFilter.mesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");

        shieldCollider = shieldObject.AddComponent<SphereCollider>();
        shieldCollider.radius = 1f;
        shieldCollider.isTrigger = true;
        shieldRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        shieldRenderer.receiveShadows = false;

        shieldObject.layer = LayerMask.NameToLayer("Shield");
        shieldRenderer.material = shieldMaterial;
        if (transform.parent == null)
        {
            return;
        }
        EnemyAI enemyAI = transform.parent.GetComponentInChildren<EnemyAI>();
        if (enemyAI != null)
        {
            isAI = true;
        }
        if (isAI && aiShieldMaterial != null)
        {
            shieldRenderer.material = aiShieldMaterial;
        }
    }

    /* Updates shield regeneration and visuals every frame.
    * It checks if the shield has been damaged and if enough time has passed to regenerate it.
    * It also updates the shield's visual representation based on its current health.
    */
    private void Update()
    {
        if (!isRegenerating)
        {
            regenerationTimer += Time.deltaTime;
        }

        if (regenerationTimer >= shieldRegenDelay && currentShieldHealth < maxShieldHealth)
        {
            isRegenerating = true;
            RegenerateShield();
        }

        if(currentShieldHealth > 0 && !shieldRenderer.enabled)
        {
            ReactivateShield();
        }

        UpdateShieldVisuals();
    }

    /* Regenerates the shield's health over time.
    * It increases the current shield health by the regeneration rate multiplied by the time since the last frame.
    * It ensures that the current shield health does not exceed the maximum shield health.
    */
    private void RegenerateShield()
    {
        currentShieldHealth = Mathf.Min(currentShieldHealth + shieldRegenRate * Time.deltaTime, maxShieldHealth);
        
        if (currentShieldHealth >= maxShieldHealth)
        {
            isRegenerating = false;
        }
    }

    /* Updates the shield's visual representation based on its current health.
    * It sets the shield material's intensity and center based on the current shield health.
    * The intensity is a value between 0 and 1, representing the shield's health percentage.
    */
    private void UpdateShieldVisuals()
    {
        if(shieldRenderer != null)
        {
            float intensity = currentShieldHealth / maxShieldHealth;
            shieldMaterial.SetFloat("_ShieldIntensity", intensity);
            shieldMaterial.SetVector("_ShieldCenter", transform.position);
        }
    }

    /* OnTriggerEnter is called when another collider enters the trigger collider attached to the shield.
    * It checks if the collider belongs to an enemy projectile and applies damage to the shield.
    * If the shield's health reaches zero, it deactivates the shield.
    * It also destroys the projectile that hit the shield.
    */
    private void OnTriggerEnter(Collider other)
    {
        Projectile projectile = other.GetComponent<Projectile>();
        if (projectile != null)
        {
            bool validProjectile = isAI ? !projectile.IsEnemyProjectile : projectile.IsEnemyProjectile;
            if (validProjectile)
            {
                if (projectile.energyDamage > 0)
                {
                    currentShieldHealth = Mathf.Max(currentShieldHealth - projectile.energyDamage, 0);
                    Debug.Log("Shield took damage: " + projectile.energyDamage + " Current Shield Health: " + currentShieldHealth);
                    regenerationTimer = 0f;
                    isRegenerating = false;
                    lastDamageTime = Time.time;
                    if (currentShieldHealth <= 0)
                    {
                        DeactivateShield();
                        return;
                    }
                    if (projectile.isEnergy){
                        Destroy(projectile.gameObject);
                    }
                    else {
                        DeflectProjectile(projectile, other);
                    }
                }
            }
        }
    }

    /* Deactivates the shield and sets its health to zero.
    * It disables the shield's collider and renderer, and invokes a method to reactivate the shield after a delay.
    */
    private void DeactivateShield()
    {
        // isShieldActive = false;
        shieldRenderer.enabled = false;
        shieldCollider.enabled = false;
    }

    /* Reactivates the shield and resets its health to maximum.
    * It enables the shield's collider and renderer, and sets the current shield health to the maximum value.
    */
    private void ReactivateShield()
    {
        if(currentShieldHealth > 0)
        {
            shieldRenderer.enabled = true;
            shieldCollider.enabled = true;
        }
    }

    /* BoostShield is called to increase the shield's health by a specified amount.
    * It ensures that the current shield health does not exceed the maximum shield health.
    * Param1: amount - The amount to increase the shield's health by.
    * Currently not used.
    */
    public void BoostShield(float amount)
    {
        currentShieldHealth = Mathf.Min(currentShieldHealth + amount, maxShieldHealth);
    }
    public void SetAI(bool ai)
    {
        isAI = ai;
        if (ai && aiShieldMaterial != null)
        {
            shieldRenderer.material = aiShieldMaterial;
        }
        else
        {
            shieldRenderer.material = shieldMaterial;
        }
    }
    private void DeflectProjectile(Projectile projectile, Collider projectileCollider)
    {
        Vector3 shieldCenter = transform.position;
        Vector3 collisionPoint = projectileCollider.ClosestPoint(shieldCenter);
        Vector3 normal = (collisionPoint - shieldCenter).normalized;

        Vector3 incomingDirection = projectile.transform.forward;
        Vector3 reflectedDirection = Vector3.Reflect(incomingDirection, normal).normalized;

        float spreadAngle = 30f;
        reflectedDirection = Quaternion.Euler(
            Random.Range(-spreadAngle, spreadAngle),
            Random.Range(-spreadAngle, spreadAngle),
            0
        ) * reflectedDirection;

        projectile.transform.forward = reflectedDirection;
    }
}