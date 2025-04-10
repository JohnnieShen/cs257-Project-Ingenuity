using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Spawn : MonoBehaviour
{
    [SerializeField] float range;
    [SerializeField] GameObject prefab;
    [SerializeField] int numberEnemies;
    private void Awake()
    {
        for (int i = 0; i < numberEnemies; i++)
        {
            var position = new Vector3(Random.Range(-range, range), 100, Random.Range(-range, range));
            position.y = Terrain.activeTerrain.SampleHeight(position) + 5;
            Instantiate(prefab, position, Quaternion.identity);
        }
    }
}
