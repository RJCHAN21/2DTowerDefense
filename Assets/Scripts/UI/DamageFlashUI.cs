using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DamageFlashUI : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private Image flashImage;

    [Header("Player Health Source")]
    [SerializeField] private Health playerHealth;

    [Header("Flash Configuration")]
    [SerializeField] private Color flashColor = new Color(1f, 0f, 0f, 0.45f); 
    [SerializeField] private float fadeDuration = 0.35f;

    private Coroutine _flashRoutine;

    private void Awake()
    {
        if (flashImage == null)
            flashImage = GetComponent<Image>();

        if (flashImage != null)
        {
            Color c = flashColor;
            c.a = 0f;
            flashImage.color = c;
            flashImage.raycastTarget = false;
        }
    }

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.Damaged += TriggerFlash;
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.Damaged -= TriggerFlash;
        }
    }

    public void TriggerFlash(int damageAmount, Vector3 position)
    {
        if (flashImage == null) return;

        flashImage.gameObject.SetActive(true);
        flashImage.enabled = true;

        if (_flashRoutine != null)
        {
            StopCoroutine(_flashRoutine);
        }

        _flashRoutine = StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        flashImage.color = flashColor;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(flashColor.a, 0f, elapsed / fadeDuration);

            Color current = flashColor;
            current.a = alpha;
            flashImage.color = current;

            yield return null;
        }

        Color finalColor = flashColor;
        finalColor.a = 0f;
        flashImage.color = finalColor;
        _flashRoutine = null;
    }
}