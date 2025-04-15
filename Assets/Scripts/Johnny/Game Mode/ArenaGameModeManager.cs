using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaGameManager : MonoBehaviour
{
    /* 
    * Author: Johnny
    * Summary: This script manages the arena game mode, including spawning enemies and blocks, handling waves, and managing game state.
    * It allows players to switch between driving and building modes, and it handles the timing of waves and intermissions.
    * The script uses a coroutine to wait for the player to press Enter before starting the first wave.
    * It also manages the spawning of enemies and blocks, and it checks if all enemies are destroyed before ending the wave.
    * The script uses a list to keep track of the current wave's enemies and handles the intermission between waves.
    * The script is attached to a GameObject in the scene and requires references to enemy and block prefabs, as well as a spawn center and radius.
    * The script also uses the ModeSwitcher class to switch between driving and building modes.

    * CURRENTLY DEPRECATED
    */
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

    /* Start is called before the first frame update.
    * It disables manually switching modes for the player and starts the coroutine to wait for the player to press Enter.
    * The coroutine will then start the first wave of enemies.
    */
    private void Start()
    {
        ModeSwitcher.instance.canManuallySwitchMode = false;
        StartCoroutine(waitForKeyPress());
        
    }

    /* Coroutine that waits for the player to press Enter before starting the first wave.
    * It uses a while loop to check for the key press and yields until it is detected.
    * Once Enter is pressed, it starts the wave routine.
    */
    private IEnumerator waitForKeyPress()
    {
        while (!Input.GetKeyDown(KeyCode.Return))
        {
            yield return null;
        }
        
        StartCoroutine(StartWaveRoutine());
    }

    /* Update is called once per frame.
    * It checks if a wave is active and updates the wave timer.
    * If all enemies are destroyed or the timer reaches zero, it ends the wave.
    */
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

    /* Coroutine that starts the wave routine.
    * It sets the game mode to driving, clears the current wave enemies list, and spawns a number of enemies.
    * It also sets the wave timer and marks the wave as active.
    * The coroutine yields once to allow for the wave to start.
    */
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

    /* Spawns a random enemy from the list of enemy prefabs.
    * It selects a random prefab from the list and instantiates it at a random position within the spawn radius.
    * It also gets the EnemyAI component from the instantiated enemy and adds it to the current wave enemies list.
    */
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

    /* Ends the current wave and spawns reward blocks.
    * It sets the wave active flag to false, spawns a number of reward blocks, and starts the intermission coroutine.
    * The reward blocks are spawned at random positions within the spawn radius.
    */
    private void EndWave()
    {
        waveActive = false;

        for (int i = 0; i < blockCount; i++)
        {
            SpawnRandomRewardBlock();
        }

        StartCoroutine(IntermissionBeforeNextWave());
    }

    /* Spawns a random reward block from the list of block prefabs.
    * It selects a random prefab from the list and instantiates it at a random position within the spawn radius.
    * The block prefab is used to reward the player for completing the wave.
    */
    private void SpawnRandomRewardBlock()
    {
        if (blockPrefabs.Count == 0) return;

        GameObject blockPrefab = blockPrefabs[Random.Range(0, blockPrefabs.Count)];
        Vector3 spawnPos = GetRandomPositionWithinRadius();
        Instantiate(blockPrefab, spawnPos, Quaternion.identity);
    }

    /* Coroutine that handles the intermission between waves.
    * It sets the game mode to building, waits for the intermission time or until the player presses Enter.
    * Once the intermission is over, it starts the next wave routine.
    */
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

    /* Gets a random position within the spawn radius around the spawn center.
    * It uses Random.insideUnitCircle to get a random point within a circle and adds it to the spawn center position.
    * The Y coordinate is set to zero.
    * Returns a Vector3 representing the random position.
    */
    private Vector3 GetRandomPositionWithinRadius()
    {
        if (spawnCenter == null)
        {
            return Vector3.zero;
        }

        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        return spawnCenter.position + new Vector3(randomCircle.x, 0, randomCircle.y);
    }

    /* Checks if all enemies in the current wave are destroyed.
    * It iterates through the current wave enemies list and removes any null references.
    * If the list is empty, it returns true, indicating that all enemies are destroyed.
    * Otherwise, it returns false.
    */
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
