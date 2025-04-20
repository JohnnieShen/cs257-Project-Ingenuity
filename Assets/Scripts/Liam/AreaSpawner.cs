using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AreaSpawner : MonoBehaviour
{
    /*
    Author: Liam
    Summary: This script spawns enemies in a square region around the spawn point with side length 2 * range.
    */
    [SerializeField] float range;
    [SerializeField] GameObject prefab;
    [SerializeField] int numberEnemies;

    /*
    When the scene is loaded, a number of enemies are spawned within a square region centered at the spawn point.
    */
    private void Awake()
    {
        for (int i = 0; i < numberEnemies; i++)
        {
            var position = transform.position + new Vector3(Random.Range(-range, range), 0, Random.Range(-range, range));
            position.y = Terrain.activeTerrain.SampleHeight(position) + 5;
            Instantiate(prefab, position, Quaternion.identity);
        }
    }
}
