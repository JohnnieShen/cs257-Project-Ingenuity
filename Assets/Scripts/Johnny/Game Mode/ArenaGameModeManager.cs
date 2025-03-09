using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaGameManager : MonoBehaviour
{
    public float waveDuration = 60f;
    public float intermissionTime = 5f;
    public int numberOfEnemiesToSpawn = 3;
    
    public Transform spawnCenter;
    public float spawnRadius = 20f;

    public List<GameObject> enemyPrefabs;
    public List<GameObject> blockPrefabs;
    public int blockCount = 3;

    private float waveTimer;
    private bool waveActive = false;
    private List<EnemyAI> currentWaveEnemies = new List<EnemyAI>();

    private void Start()
    {
        ModeSwitcher.instance.canManuallySwitchMode = false;
        StartCoroutine(waitForKeyPress());
        
    }
    private IEnumerator waitForKeyPress()
    {
        while (!Input.GetKeyDown(KeyCode.Return))
        {
            yield return null;
        }
        
        StartCoroutine(StartWaveRoutine());
    }
    private void Update()
    {
        if (waveActive)
        {
            waveTimer -= Time.deltaTime;

            if (AllEnemiesDestroyed() || waveTimer <= 0f)
            {
                EndWave();
            }
        }
    }

    private IEnumerator StartWaveRoutine()
    {
        ModeSwitcher.instance.SetMode(ModeSwitcher.Mode.Drive);
        currentWaveEnemies.Clear();
        for (int i = 0; i < numberOfEnemiesToSpawn; i++)
        {
            SpawnRandomEnemy();
        }
        waveTimer = waveDuration;
        waveActive = true;

        yield return null; 
    }

    private void SpawnRandomEnemy()
    {
        if (enemyPrefabs.Count == 0) return;

        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
        Vector3 spawnPos = GetRandomPositionWithinRadius();
        
        GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        EnemyAI enemyAI = enemyObj.GetComponentInChildren<EnemyAI>();
        
        if (enemyAI)
        {
            currentWaveEnemies.Add(enemyAI);
        }
    }

    private void EndWave()
    {
        waveActive = false;

        for (int i = 0; i < blockCount; i++)
        {
            SpawnRandomRewardBlock();
        }

        StartCoroutine(IntermissionBeforeNextWave());
    }

    private void SpawnRandomRewardBlock()
    {
        if (blockPrefabs.Count == 0) return;

        GameObject blockPrefab = blockPrefabs[Random.Range(0, blockPrefabs.Count)];
        Vector3 spawnPos = GetRandomPositionWithinRadius();
        Instantiate(blockPrefab, spawnPos, Quaternion.identity);
    }
    private IEnumerator IntermissionBeforeNextWave()
    {
        ModeSwitcher.instance.SetMode(ModeSwitcher.Mode.Build);
        float elapsed = 0f;
        while (elapsed < intermissionTime)
        {
            if (Input.GetKeyDown(KeyCode.Return))
                break;
            elapsed += Time.deltaTime;
            yield return null;
        }
        StartCoroutine(StartWaveRoutine());
    }
    private Vector3 GetRandomPositionWithinRadius()
    {
        if (spawnCenter == null)
        {
            return Vector3.zero;
        }

        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        return spawnCenter.position + new Vector3(randomCircle.x, 0, randomCircle.y);
    }

    private bool AllEnemiesDestroyed()
    {
        for (int i = currentWaveEnemies.Count - 1; i >= 0; i--)
        {
            if (currentWaveEnemies[i] == null)
            {
                currentWaveEnemies.RemoveAt(i);
            }
        }

        return currentWaveEnemies.Count == 0;
    }
}
