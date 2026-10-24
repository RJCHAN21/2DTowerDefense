using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaveManager : MonoBehaviour
{
#region Unity Inspector Fields
    [Header("Wave Setup")]
    [SerializeField] private WaveDefinition[] allWaves;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private TextMeshProUGUI waveNumberTMP;

    [Tooltip("Cooldown before the next wave can be started manually.")]
    [SerializeField, Min(0f)] private float timeBetweenWaves = 3f;

    [Header("Wave Control")]
    [SerializeField] private Button waveControlBtn;
    [SerializeField] private Image waveControlIcon;
    [SerializeField] private Image waveControlBackground;
    [SerializeField] private Sprite playBackground;
    [SerializeField] private Sprite playIcon;
    [SerializeField] private Sprite fastForwardBackground;
    [SerializeField] private Sprite fastForwardIcon;
    [SerializeField, Min(1f)] private float fastForwardSpeed = 2f;
    [SerializeField] private TextMeshProUGUI gameControlTMP;

    [Header("Wave Completion Reward")]
    [SerializeField] private Currency currency;
    [SerializeField, Min(0)] private int baseCompletionReward = 20;
    [SerializeField, Min(0)] private int rewardIncreasePerWave = 5;

    [Header("Wave Number Animation")]
    [SerializeField, Min(0.01f)] private float waveAnimationDuration = 0.5f;
    [SerializeField] private float waveRiseDistance = 20f;
    [SerializeField, Min(0f)] private float wavePulseAmount = 0.15f;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip waveStartSFX;
    [SerializeField] private AudioClip waveCompleteSFX;
    [SerializeField] private AudioClip speedChangeSFX;

    [Header("Music")]
    [SerializeField] private AudioClip finalWaveMusic;

    [Header("Gameplay Hints")]
    [SerializeField] private TextMeshProUGUI gameplayHint;
    [SerializeField, Min(0.01f)] private float hintFadeDuration = 0.2f;
    [SerializeField, TextArea] private string[] gameplayHints =
    {
        "Place towers to defend your base against rogue soldiers, then press START WAVE.",
        "Place towers near bends to keep enemies within range for longer.",
        "Elite Soldiers move quickly. Cover more than one part of the path.",
        "Suicide Bombers attack nearby towers. Avoid placing all your defenses together.",
        "Flamethrowers overheat. Support them with towers that can fire while they recharge.",
        "Missile explosions can damage several nearby enemies at once.",
        "Basic Turrets can upgrade into either Snipers or Shotgunners.",
        "Choose a tower's focus to target the first, last, or strongest enemy in range.",
        "Set high-damage towers to Strongest focus to prioritize the Tank over its weaker escorts.",
        "Damaged towers recover health after going without damage for a while.",
        "Selling returns 75% of the coins invested in a tower and its upgrades.",
        "Completing a wave awards extra coins. Use intermissions to improve your defenses.",
        "The final Tank must be destroyed. Letting it reach your base ends the game."
    };
#endregion

#region Public Properties
    public event Action AllWavesCompleted;
    public bool IsWaveRunning => isActiveAndEnabled && _state == WaveState.Running;
#endregion

#region Private Properties
    private enum WaveState
    {
        Waiting,
        Running,
        Cooldown,
        Finished,
        Stopped
    }

    private WaveState _state = WaveState.Stopped;
    private int currentWaveIndex;
    private bool _isFastForward;

    private Vector2 _wavePosition;
    private Vector3 _waveScale;
    private float _waveAlpha;
    private Coroutine _waveAnimation;
    private bool _visualReady;
#endregion

#region Unity Life Cycle
    private void Awake()
    {
        if (gameplayHint != null)
        {
            gameplayHint.alpha = 0f;
            gameplayHint.raycastTarget = false;
        }
    }

    private void OnEnable()
    {
        GameManager.GameEnded += StopWaves;
    }

    private void Start()
    {
        if (enemySpawner == null ||
            currency == null ||
            waveNumberTMP == null ||
            waveControlBtn == null ||
            waveControlIcon == null ||
            playIcon == null ||
            fastForwardIcon == null ||
            allWaves == null ||
            waveControlBackground == null ||
            playBackground == null ||
            fastForwardBackground == null ||
            allWaves.Length == 0)
        {
            Debug.LogError("Assign all WaveManager references and waves.", this);
            enabled = false;
            return;
        }

        foreach (WaveDefinition wave in allWaves)
        {
            if (wave == null || wave.Bursts == null || wave.Bursts.Length == 0)
            {
                Debug.LogError(
                    "Every wave needs an assigned definition with at least one burst.",
                    this);

                enabled = false;
                return;
            }

            foreach (WaveSpawnBurst burst in wave.Bursts)
            {
                if (burst.DelayBeforeBurst < 0f ||
                    float.IsNaN(burst.DelayBeforeBurst) ||
                    float.IsInfinity(burst.DelayBeforeBurst))
                {
                    Debug.LogError("Burst delays must be finite and nonnegative.", wave);
                    enabled = false;
                    return;
                }
            }
        }

        _wavePosition = waveNumberTMP.rectTransform.anchoredPosition;
        _waveScale = waveNumberTMP.rectTransform.localScale;
        _waveAlpha = waveNumberTMP.alpha;
        _visualReady = true;

        waveControlBtn.onClick.AddListener(OnWaveControlClicked);

        Time.timeScale = 1f;
        _state = WaveState.Waiting;

        waveNumberTMP.text = $"WAVE 1 / {allWaves.Length}";
        ShowGameplayHint();
        RefreshControlButton();
    }

    private void Update()
    {
        if (gameplayHint == null)
            return;

        bool showHint =
            (_state == WaveState.Waiting || _state == WaveState.Cooldown) &&
            !string.IsNullOrWhiteSpace(gameplayHint.text);

        gameplayHint.alpha = Mathf.MoveTowards(
            gameplayHint.alpha,
            showHint ? 1f : 0f,
            Time.unscaledDeltaTime / hintFadeDuration);
    }

    private void OnDisable()
    {
        GameManager.GameEnded -= StopWaves;
        StopWaves();

        if (gameplayHint != null)
            gameplayHint.alpha = 0f;
    }

    private void OnDestroy()
    {
        if (waveControlBtn != null)
            waveControlBtn.onClick.RemoveListener(OnWaveControlClicked);
    }
#endregion

#region Wave Control
    private void OnWaveControlClicked()
    {
        if (Time.timeScale == 0f)
            return;

        if (_state == WaveState.Waiting)
        {
            StartNextWave();
        }
        else if (_state == WaveState.Running)
        {
            _isFastForward = !_isFastForward;

            Time.timeScale = _isFastForward
                ? fastForwardSpeed
                : 1f;

            SoundManager.Instance?.PlaySFX(speedChangeSFX);
            RefreshControlButton();
        }
    }

    private void StartNextWave()
    {
        _state = WaveState.Running;
        _isFastForward = false;
        Time.timeScale = 1f;

        if (currentWaveIndex == allWaves.Length - 1)
            SoundManager.Instance?.PlayMusic(finalWaveMusic);
            
        waveNumberTMP.text =
            $"WAVE {currentWaveIndex + 1} / {allWaves.Length}";

        if (_waveAnimation != null)
            StopCoroutine(_waveAnimation);

        _waveAnimation = StartCoroutine(AnimateWaveNumber());

        RefreshControlButton();
        SoundManager.Instance?.PlaySFX(waveStartSFX);
        StartCoroutine(RunWave(allWaves[currentWaveIndex]));
    }

    private void RefreshControlButton()
    {
        if (waveControlIcon != null)
        {
            waveControlIcon.sprite = _isFastForward
                ? fastForwardIcon
                : playIcon;
        }

        if (waveControlBackground != null)
        {
            waveControlBackground.sprite = _isFastForward
                ? fastForwardBackground
                : playBackground;
        }

        if (waveControlBtn != null)
        {
            waveControlBtn.interactable =
                _state == WaveState.Waiting ||
                _state == WaveState.Running;
        }

        if (gameControlTMP != null)
        {
            switch (_state)
            {
                case WaveState.Waiting:
                    gameControlTMP.text = "START WAVE";
                    break;

                case WaveState.Running:
                    gameControlTMP.text = "FAST-FORWARD";
                    break;

                case WaveState.Cooldown:
                    gameControlTMP.text = "INTERMISSION";
                    break;

                default:
                    gameControlTMP.text = "";
                    break;
            }
        }
    }
#endregion

#region Wave Sequence
    private IEnumerator RunWave(WaveDefinition wave)
    {
        foreach (WaveSpawnBurst burst in wave.Bursts)
        {
            if (burst.WaitForEnemiesToClear)
                yield return WaitForEnemiesToClear();

            if (burst.DelayBeforeBurst > 0f)
                yield return new WaitForSeconds(burst.DelayBeforeBurst);

            bool started = enemySpawner.TryConfigureAndStartSpawner(
                burst.EnemyPrefabIndex,
                burst.EnemyHealth,
                burst.EnemySpeed,
                burst.Count,
                burst.SpawnInterval,
                burst.CoinReward,
                overrideHealth: burst.OverrideHealth,
                overrideSpeed: burst.OverrideSpeed,
                overrideReward: burst.OverrideReward);

            if (!started)
            {
                StopWaves();
                yield break;
            }

            while (enemySpawner.IsSpawning)
                yield return null;

            if (enemySpawner.SpawnFailed ||
                !enemySpawner.isActiveAndEnabled)
            {
                Debug.LogError("Wave stopped because spawning was interrupted.", this);
                StopWaves();
                yield break;
            }
        }

        yield return WaitForEnemiesToClear();

        int completedWaveNumber = currentWaveIndex + 1;

        int completionReward = baseCompletionReward
            + rewardIncreasePerWave * completedWaveNumber;

        if (!currency.TryDeposit(completionReward))
        {
            Debug.LogError(
                "Could not deposit the wave completion reward.", this);
        }
        else if (currentWaveIndex + 1 < allWaves.Length)
        {
            SoundManager.Instance?.PlaySFX(waveCompleteSFX);
        }

        currentWaveIndex++;
        _isFastForward = false;
        Time.timeScale = 1f;

        if (currentWaveIndex >= allWaves.Length)
        {
            _state = WaveState.Finished;
            RefreshControlButton();

            Debug.Log("All waves completed!", this);
            AllWavesCompleted?.Invoke();
            yield break;
        }

        _state = WaveState.Cooldown;
        ShowGameplayHint();
        RefreshControlButton();

        if (timeBetweenWaves > 0f)
            yield return new WaitForSeconds(timeBetweenWaves);

        _state = WaveState.Waiting;
        RefreshControlButton();
    }

    private IEnumerator WaitForEnemiesToClear()
    {
        while (Time.timeScale == 0f || HasActiveEnemies())
            yield return null;
    }

    private bool HasActiveEnemies()
    {
        foreach (EnemyTarget enemy in EnemyTarget.ActiveEnemies)
        {
            if (enemy != null && enemy.isActiveAndEnabled)
                return true;
        }

        return false;
    }

    private void StopWaves()
    {
        StopAllCoroutines();
        _waveAnimation = null;

        _state = WaveState.Stopped;
        _isFastForward = false;

        if (enemySpawner != null)
            enemySpawner.StopSpawning();

        if (Time.timeScale > 0f)
            Time.timeScale = 1f;

        RefreshControlButton();

        if (_visualReady && waveNumberTMP != null)
        {
            waveNumberTMP.rectTransform.anchoredPosition = _wavePosition;
            waveNumberTMP.rectTransform.localScale = _waveScale;
            waveNumberTMP.alpha = _waveAlpha;
        }
    }
#endregion

#region Gameplay Hints
    private void ShowGameplayHint()
    {
        if (gameplayHint == null)
            return;

        if (gameplayHints == null || gameplayHints.Length == 0)
        {
            gameplayHint.text = string.Empty;
            return;
        }

        int hintIndex = currentWaveIndex == 0
            ? 0
            : UnityEngine.Random.Range(0, gameplayHints.Length);
        gameplayHint.text = gameplayHints[hintIndex];
        gameplayHint.alpha = 0f;
    }
#endregion

#region Wave Number Animation
    private IEnumerator AnimateWaveNumber()
    {
        RectTransform textTransform = waveNumberTMP.rectTransform;

        Vector2 startPosition =
            _wavePosition + Vector2.down * waveRiseDistance;

        float elapsed = 0f;

        textTransform.anchoredPosition = startPosition;
        textTransform.localScale = _waveScale;
        waveNumberTMP.alpha = 0f;

        while (elapsed < waveAnimationDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Interpolate.GetLerpTime(
                elapsed, waveAnimationDuration);

            float eased = Ease.OutCubic(t);
            float pulse = Ease.QuarticPulse(t);

            textTransform.anchoredPosition =
                startPosition + (_wavePosition - startPosition) * eased;

            textTransform.localScale =
                _waveScale * (1f + pulse * wavePulseAmount);

            waveNumberTMP.alpha = _waveAlpha * eased;

            yield return null;
        }

        textTransform.anchoredPosition = _wavePosition;
        textTransform.localScale = _waveScale;
        waveNumberTMP.alpha = _waveAlpha;

        _waveAnimation = null;
    }
#endregion
}
