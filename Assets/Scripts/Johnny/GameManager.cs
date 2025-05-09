using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject initialCore;
    public GameObject player;

    public GameObject corePrefab;
    public Transform coreParent;
    public GameObject camerHolder;

    private GameObject _currentCore;
    public GameObject visualWidget;
    public Vector3Int blockParentPos;
    [Header("Win-screen")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private bool pauseOnWin = true;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioSource winAudioSource;
    [SerializeField, Min(0.1f)] private float fadeDuration = 1.5f;
    [SerializeField] private CanvasGroup fadeOutOverlay;
    [SerializeField] private float fadeOutDuration = 1.5f;
    [SerializeField] private Block unlockableBlock;
    void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        //DontDestroyOnLoad(gameObject); // breaks respawning
    }

    void Start()
    {
        // If you started the scene with a core already in place, register it now:
        if (initialCore != null)
        {
            _currentCore = initialCore;
        }
        else
        {
            Debug.LogWarning("GameManager: no initialCore assigned; will only spawn on first death.");
        }
        CheckBossStatus();
    }

    void Update()
    {
        if (_currentCore == null){
            SceneManager.LoadScene("Main Menu"); // respawning breaks game, so just send player back to main menu
            //SpawnCore();
            //if (coreParent == null) return;
            //coreParent.position = blockParentPos;
            //if (ModeSwitcher.instance != null)
            //{
            //    ModeSwitcher.instance.SetMode(ModeSwitcher.Mode.Build);
            //}
        }
    }

    private void SpawnCore()
    {
        if (corePrefab == null || coreParent == null) return;

        _currentCore = Instantiate(corePrefab, coreParent);
        _currentCore.transform.localPosition = Vector3.zero;
        _currentCore.transform.localRotation = Quaternion.identity;
        _currentCore.transform.localScale = Vector3.one;

        _currentCore.name = corePrefab.name;

        RegisterCore(_currentCore);
    }

    private void RegisterCore(GameObject core) {
        if (BlockManager.instance != null)
        {
            BlockManager.instance.gridOrigin = core.transform;
        }
        PlayerMove playerMove = player.GetComponent<PlayerMove>();
        if (playerMove != null)
        {
            playerMove.moveCenter = core.transform;
        }
        BuildSystem buildSystem = player.GetComponent<BuildSystem>();
        if (buildSystem != null)
        {
            buildSystem.commandModule = core.transform;
        }
        Minimap minimap = camerHolder.GetComponentsInChildren<Minimap>(true)[0];
        if (minimap != null)
        {
            minimap.player = core.transform;
        }
        FreeCameraLook cameraLook = camerHolder.GetComponentInChildren<FreeCameraLook>(true);
        if (cameraLook != null)
        {
            cameraLook.SetTarget(core.transform);
            cameraLook.ResetView();
        }
        visualWidget = null;
        foreach (var t in core.GetComponentsInChildren<Transform>(true))
        {
            if (t.CompareTag("BuildVisualWidget"))
            {
                visualWidget = t.gameObject;
                break;
            }
        }
        ModeSwitcher.instance.buildArrow = visualWidget;
        RebuildPlayerBlockManager(core);
        UpdateAllEnemyAITargets(core.transform);
        EnablePickUpForChildHulls(coreParent.gameObject);
    }
    private void RebuildPlayerBlockManager(GameObject core)
    {
        var bm = BlockManager.instance;
        bm.blocks.Clear();
        bm.blockConnections.Clear();

        var hulls = core.GetComponentsInChildren<Hull>();
        foreach (var h in hulls)
        {
            var rb = h.GetComponent<Rigidbody>();
            var localPos = Vector3Int.RoundToInt(
                core.transform.InverseTransformPoint(h.transform.position)
            );
            bm.AddBlock(localPos, rb);
        }

        StartCoroutine(bm.DelayedRecalculateConnections());
    }
    private void UpdateAllEnemyAITargets(Transform coreTransform)
    {
        if (EnemyBlockManager.instance != null)
        {
            foreach (var enemy in EnemyBlockManager.instance.GetEnemyVehicles())
            {
                if (enemy != null)
                    enemy.playerTarget = coreTransform;
            }
        }
        else
        {
            foreach (var enemy in FindObjectsOfType<EnemyAI>())
            {
                enemy.playerTarget = coreTransform;
            }
        }
    }
    public void WinGame()
    {
        Debug.Log("WinGame() called");
        if (winPanel != null)
        {
            winPanel.SetActive(true);

            CanvasGroup cg = winPanel.GetComponent<CanvasGroup>();
            if (cg == null) cg = winPanel.AddComponent<CanvasGroup>();

            cg.alpha = 0f;
            
            StartCoroutine(FadeCanvasGroup(cg, 0f, 1f, fadeDuration));
        }
        PlayWinSound();
        if (pauseOnWin)
            Time.timeScale = 0f;
        StartCoroutine(HandleWinDelay());
    }
    private IEnumerator HandleWinDelay()
    {
        float delay = 5f;

        if (pauseOnWin)
            yield return new WaitForSecondsRealtime(delay);
        else
            yield return new WaitForSeconds(delay);

        Time.timeScale = 1f;
        ModeSwitcher.instance.hideAllUI();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("Main Menu");
    }
    private IEnumerator FadeCanvasGroup(CanvasGroup cg,
                                        float from, float to, float duration)
    {
        float elapsed = 0f;
        cg.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        cg.alpha = to;
    }
    private void PlayWinSound()
    {
        if (winSound == null) return;
        winAudioSource.clip = winSound;
        winAudioSource.Play();
    }
    public void ResumeGame()
    {
        Time.timeScale = 1f;
    }
    private void EnablePickUpForChildHulls(GameObject parent)
    {
        foreach (var hull in parent.GetComponentsInChildren<Hull>(true))
        {
            if (!hull.CompareTag("Core"))
            {
                hull.canPickup = true;
            }
        }
    }
    private void CheckBossStatus()
    {
        bool bossAlive = FindObjectOfType<BossHealthBarToggle>() != null;

        if (unlockableBlock == null)
        {
            Debug.LogWarning("GameManager: unlockableBlock is not assigned.");
            return;
        }

        unlockableBlock.isCraftable = !bossAlive;

        Debug.Log($"Boss {(bossAlive ? "alive" : "dead")} at scene start - " +
                  $"unlockable block \"{unlockableBlock.BlockName}\" craftable = {unlockableBlock.isCraftable}");
    }
}
