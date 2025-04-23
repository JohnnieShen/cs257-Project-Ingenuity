using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Projectile : MonoBehaviour
{

    /* 
    * Author: Johnny
    * Summary: This script handles the projectile behavior in the game. It manages the projectile's speed, damage, and impact effects.
    * The projectile can be set to be an enemy projectile or not, and it will destroy itself after a specified time.
    * When the projectile collides with a block or core, it applies damage and triggers a visual effect.
    * The script also includes a jitter effect for the block upon impact, which can be customized in terms of duration and magnitude.
    */

    public bool IsEnemyProjectile = false;
    public float speed;
    public float ballisticDamage = 10f;
    public float energyDamage = 10f;
    public float timeToDestroy = 10f;
    public ParticleSystem impactEffect;
    public float jitterDuration = 0.3f;
    public float jitterMagnitude = 0.1f;
    public bool isEnergy = false;
    public Vector3 velocity;
    float spawnTime;
    /* Start is called before the first frame update.
    * It destroys the projectile after a specified time to prevent it from existing indefinitely in the game world.
    */
    void Start()
    {
        Destroy(gameObject, timeToDestroy);
        spawnTime = Time.time;
    }

    /* Update is called once per frame.
    * It moves the projectile forward at a specified speed.
    */
    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    /* SetDamage is a public method that allows other scripts to set the damage values for the projectile.
    * It takes two float parameters: ballisticDmg and energyDmg, which represent the damage values for ballistic and energy damage respectively.
    * Param 1: ballisticDmg - The damage value for ballistic damage.
    * Param 2: energyDmg - The damage value for energy damage.
    */
    public void SetDamage(float ballisticDmg, float energyDmg)
    {
        ballisticDamage = ballisticDmg;
        energyDamage = energyDmg;
    }

    /* OnTriggerEnter is called when the collider other enters the trigger collider attached to the object where this script is attached.
    * It checks if the projectile is an enemy projectile and applies damage to the block or core it collides with.
    * It also triggers a visual effect at the collision point and applies a jitter effect to the block or core.
    * The method uses a coroutine to handle the jitter effect, which can be customized in terms of duration and magnitude.
    * Param 1: other - The collider that the projectile collides with.
    */
    void OnTriggerEnter(Collider other)
    {
        if (Time.time - spawnTime < 0.05f) return;
        // Debug.Log("Projectile hit: " + other.name + " " + other.tag);
        BlockHealth blockHealth = null;
        if (IsEnemyProjectile)
        {
            if (other.CompareTag("Block") || other.CompareTag("Core"))
            {
                blockHealth = other.GetComponent<BlockHealth>();
            }
            else if (other.CompareTag("ConnectionPoint"))
            {
                if (other.transform.parent != null && other.transform.parent.parent != null)
                {
                    if (other.transform.parent.parent.CompareTag("Block") || other.transform.parent.parent.CompareTag("Core"))
                    {
                        blockHealth = other.transform.parent.parent.GetComponent<BlockHealth>();
                    }
                }
            }
        }
        else
        {
            if (other.CompareTag("EnemyBlock"))
            {
                blockHealth = other.GetComponent<BlockHealth>();
            }
            else if (other.CompareTag("ConnectionPoint"))
            {
                if (other.transform.parent != null && other.transform.parent.parent != null)
                {
                    if (other.transform.parent.parent.CompareTag("EnemyBlock"))
                    {
                        blockHealth = other.transform.parent.parent.GetComponent<BlockHealth>();
                    }
                }
            }
        }
        // if (other.CompareTag("EnemyBlock"))
        // {
        //     blockHealth = other.GetComponent<BlockHealth>();
        // }
        // else if (other.CompareTag("ConnectionPoint"))
        // {
        //     if (other.transform.parent != null && other.transform.parent.parent != null)
        //     {
        //         blockHealth = other.transform.parent.parent.GetComponent<BlockHealth>();
        //     }
        // }

        if (blockHealth != null)
        {
            Vector3 collisionPoint = other.ClosestPoint(transform.position);
            if (impactEffect != null)
            {
                ParticleSystem effect = Instantiate(impactEffect, collisionPoint, Quaternion.identity);
                Destroy(effect.gameObject, effect.main.duration);
            }

            Hull hull = blockHealth.GetComponent<Hull>();
            MonoBehaviour mb = blockHealth as MonoBehaviour;
            if (hull != null && mb != null)
            {
                // Start the coroutine that jitters only the MeshRenderers in hull.childMeshRenderers.
                mb.StartCoroutine(JitterVisualMeshes(hull, jitterDuration, jitterMagnitude));
            }
            else if (mb != null)
            {
                // Fallback jitter: jitter the entire block transform.
                mb.StartCoroutine(JitterBlock(blockHealth.transform, jitterDuration, jitterMagnitude));
            }

            blockHealth.TakeDamage(ballisticDamage);

            Destroy(gameObject);
        }
    }

    /* JitterBlock is a coroutine that applies a jitter effect to the block transform.
    * It takes the block transform, duration, and magnitude as parameters.
    * The coroutine computes the block's position relative to a reference transform and applies a random jitter to it.
    * After the jitter effect, it restores the block's position to its original state.
    * Param 1: blockTransform - The transform of the block to be jittered.
    * Param 2: duration - The duration of the jitter effect.
    * Param 3: magnitude - The magnitude of the jitter effect.
    */
    private IEnumerator JitterBlock(Transform blockTransform, float duration, float magnitude)
    {
        // Try to get a Hull component and use its coreTransform if available.
        Hull hull = blockTransform.GetComponent<Hull>();
        Transform reference = null;
        if (hull != null && hull.coreTransform != null)
        {
            reference = hull.coreTransform;
        }
        else if (blockTransform.parent != null)
        {
            reference = blockTransform.parent;
        }
        else
        {
            // Fallback: just use blockTransform itself.
            reference = blockTransform;
        }

        // Record the block's current position relative to the chosen reference.
        Vector3 initialOffset = blockTransform.position - reference.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            // Compute the updated base position from the reference transform.
            Vector3 baseWorldPos = reference.position + initialOffset;
            // Apply a random jitter that will follow the reference.
            blockTransform.position = baseWorldPos + Random.insideUnitSphere * magnitude;
            elapsed += Time.deltaTime;
            yield return null;
        }
        // After jitter, restore the block's position relative to the updated reference.
        blockTransform.position = reference.position + initialOffset;
    }

    /* JitterVisualMeshes is a coroutine that applies a jitter effect to the visual meshes of the hull.
    * It takes the hull, duration, and magnitude as parameters.
    * The coroutine computes the position of each MeshRenderer relative to a reference transform and applies a random jitter to it.
    * After the jitter effect, it restores the position of each MeshRenderer to its original state.
    * Param 1: hull - The Hull component containing the visual meshes to be jittered.
    * Param 2: duration - The duration of the jitter effect.
    * Param 3: magnitude - The magnitude of the jitter effect.
    */
    private IEnumerator JitterVisualMeshes(Hull hull, float duration, float magnitude)
    {
        // Determine the reference transform.
        Transform reference = null;
        if (hull.coreTransform != null)
        {
            reference = hull.coreTransform;
        }
        else if (hull.transform.parent != null)
        {
            reference = hull.transform.parent;
        }
        else
        {
            reference = hull.transform;
        }
        
        // Store each MeshRenderer's original local position relative to the reference.
        Dictionary<Transform, Vector3> originalLocalPositions = new Dictionary<Transform, Vector3>();
        foreach (MeshRenderer mr in hull.childMeshRenderers)
        {
            if (mr != null)
            {
                originalLocalPositions[mr.transform] = reference.InverseTransformPoint(mr.transform.position);
            }
        }
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            // For each mesh, compute its base world position using the stored local position and add random jitter.
            foreach (MeshRenderer mr in hull.childMeshRenderers)
            {
                if (mr != null && originalLocalPositions.ContainsKey(mr.transform))
                {
                    if (mr.transform == null) continue;
                    Vector3 baseWorldPos = reference.TransformPoint(originalLocalPositions[mr.transform]);
                    mr.transform.position = baseWorldPos + Random.insideUnitSphere * magnitude;
                }
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Restore each MeshRenderer's position exactly.
        foreach (MeshRenderer mr in hull.childMeshRenderers)
        {
            if (mr != null && originalLocalPositions.ContainsKey(mr.transform))
            {
                mr.transform.position = reference.TransformPoint(originalLocalPositions[mr.transform]);
            }
        }
    }
}