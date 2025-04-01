using UnityEngine;

public class ShieldGenerator : MonoBehaviour
{
    public float maxShieldHealth = 100f;
    public float currentShieldHealth = 100f;
    public float shieldRegenRate = 5f;
    public float shieldRegenDelay = 3f;
    public float shieldRadius = 5f;
    
    public Material shieldMaterial;
    
    private SphereCollider shieldCollider;
    private MeshRenderer shieldRenderer;
    private float lastDamageTime;
    // private bool isShieldActive = true;

    private void Awake()
    {
        InitializeShield();
    }

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
        shieldCollider.radius = shieldRadius;
        shieldCollider.isTrigger = true;

        shieldRenderer.material = shieldMaterial;
        shieldRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        shieldRenderer.receiveShadows = false;

        shieldObject.layer = LayerMask.NameToLayer("Shield");
    }

    private void Update()
    {
        if(Time.time > lastDamageTime + shieldRegenDelay && currentShieldHealth < maxShieldHealth)
        {
            RegenerateShield();
        }

        UpdateShieldVisuals();
    }

    private void RegenerateShield()
    {
        currentShieldHealth = Mathf.Min(currentShieldHealth + shieldRegenRate * Time.deltaTime, maxShieldHealth);
    }

    private void UpdateShieldVisuals()
    {
        if(shieldRenderer != null)
        {
            float intensity = currentShieldHealth / maxShieldHealth;
            shieldMaterial.SetFloat("_ShieldIntensity", intensity);
            shieldMaterial.SetVector("_ShieldCenter", transform.position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Debug.Log("Shield hit: " + other.name + " " + other.tag);
        Projectile projectile = other.GetComponent<Projectile>();
        if(projectile != null && projectile.IsEnemyProjectile)
        {
            // Debug.Log("Shield hit by enemy projectile");
            currentShieldHealth = Mathf.Max(currentShieldHealth - projectile.energyDamage, 0);
            lastDamageTime = Time.time;
            
            Destroy(projectile.gameObject);
            
            if(currentShieldHealth <= 0)
            {
                DeactivateShield();
            }
        }
    }

    private void DeactivateShield()
    {
        // isShieldActive = false;
        shieldRenderer.enabled = false;
        shieldCollider.enabled = false;

        Invoke("ReactivateShield", 5f);
    }

    private void ReactivateShield()
    {
        currentShieldHealth = maxShieldHealth;
        // isShieldActive = true;
        shieldRenderer.enabled = true;
        shieldCollider.enabled = true;
    }

    public void BoostShield(float amount)
    {
        currentShieldHealth = Mathf.Min(currentShieldHealth + amount, maxShieldHealth);
    }
}