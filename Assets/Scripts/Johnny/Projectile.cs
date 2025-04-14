using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Projectile : MonoBehaviour
{
    public bool IsEnemyProjectile = false;
    public float speed;
    public float ballisticDamage = 10f;
    public float energyDamage = 10f;
    public float timeToDestroy = 10f;
    public ParticleSystem impactEffect;
    public float jitterDuration = 0.3f;
    public float jitterMagnitude = 0.1f;

    void Start()
    {
        Destroy(gameObject, timeToDestroy);
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    public void SetDamage(float ballisticDmg, float energyDmg)
    {
        ballisticDamage = ballisticDmg;
        energyDamage = energyDmg;
    }

    void OnTriggerEnter(Collider other)
    {
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