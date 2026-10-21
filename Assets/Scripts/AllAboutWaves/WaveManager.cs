using System.Collections;
using TMPro;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Assign Wave Data files here")]
    [SerializeField] private BaseWave[] allWaves;
    [SerializeField] private TextMeshProUGUI waveNumberTMP;
    
    [SerializeField] private float timeBetweenWaves = 3f; // Time in seconds between waves

    [Header("Wave Number Animation")]
    [SerializeField, Min(0.01f)] private float waveAnimationDuration = 0.5f;
    [SerializeField] private float waveRiseDistance = 20f;
    [SerializeField, Min(0f)] private float wavePulseAmount = 0.15f;

    private Vector2 _wavePosition;
    private Vector3 _waveScale;
    private float _waveAlpha;
    private Coroutine _waveAnimation;
    private int currentWaveIndex = 0; // Index of the current wave

    private BaseWave baseWaveScript;

    void Start()
    {
        // Initialize the active wave scripts array
        BaseWave.ActiveEnemiesInScene = 0;
        _wavePosition = waveNumberTMP.rectTransform.anchoredPosition;
        _waveScale = waveNumberTMP.rectTransform.localScale;
        _waveAlpha = waveNumberTMP.alpha;

        if (allWaves.Length <= 0)
        {
            Debug.LogError("No waves assigned in the WaveManager.");
            return;
        }

        StartNextWave();
    }

    private void StartNextWave()
    {
        if (currentWaveIndex >= allWaves.Length)
        {
            Debug.Log("All waves completed!");
            return;
        }

        waveNumberTMP.text = $"WAVE {currentWaveIndex + 1} / {allWaves.Length}";
        if (_waveAnimation != null)
            StopCoroutine(_waveAnimation);

        _waveAnimation = StartCoroutine(AnimateWaveNumber());
        baseWaveScript = allWaves[currentWaveIndex];
        baseWaveScript.OnWaveComplete += ProgressSequence; // Subscribe to the wave completion event
        baseWaveScript.StartWave();

    }

    private void ProgressSequence()
    {        
        baseWaveScript.OnWaveComplete -= ProgressSequence; // Unsubscribe from the event
        
        currentWaveIndex++;

        StartCoroutine(NextWaveDelay());
    }

    private IEnumerator NextWaveDelay()
    {
        yield return new WaitForSeconds(timeBetweenWaves);
        StartNextWave();
    }

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

    private void OnDisable()
    {
        if (_waveAnimation == null)
            return;

        StopCoroutine(_waveAnimation);
        _waveAnimation = null;

        waveNumberTMP.rectTransform.anchoredPosition = _wavePosition;
        waveNumberTMP.rectTransform.localScale = _waveScale;
        waveNumberTMP.alpha = _waveAlpha;
    }
}
