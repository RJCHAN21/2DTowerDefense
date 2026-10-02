using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
#region Unity Inspector Fields
    [Header("Health")]
    [SerializeField] private Image HPBar;
    [SerializeField] private TextMeshProUGUI HPTxt;
    [SerializeField] private float maxHP = 20f;

    [Header("Ghost Bar")]
    [SerializeField] private Image ghostHPBar;

    [Tooltip("Seconds the ghost bar waits after the latest damage.")]
    [SerializeField, Min(0f)] private float ghostDelay = 0.3f;

    [Tooltip("Seconds the ghost bar takes to reach the real HP bar.")]
    [SerializeField, Min(0.01f)] private float ghostDuration = 0.6f;
#endregion

#region Public Properties
    public float PlayerHP => _hp;
#endregion

#region Private Properties
    private float _hp;
    private float _ghostStartFill;
    private float _ghostElapsed;
    private bool _ghostAnimating;
#endregion

#region Unity Life Cycle
    private void Awake()
    {
        if (maxHP <= 0f)
        {
            Debug.LogError("Maximum HP must be greater than zero.", this);
            enabled = false;
            return;
        }

        _hp = maxHP;
        UpdateHUDVisual();

        if (ghostHPBar != null)
            ghostHPBar.fillAmount = 1f;
    }

    private void Update()
    {
        if (!_ghostAnimating || ghostHPBar == null) return;

        _ghostElapsed += Time.deltaTime;

        if (_ghostElapsed < ghostDelay) return;

        float progress = Interpolate.GetLerpTime(
            _ghostElapsed - ghostDelay,
            ghostDuration);
        
        float easedProgress = Ease.OutQuadratic(progress);
        float targetFill = _hp / maxHP;

        ghostHPBar.fillAmount = _ghostStartFill
            + (targetFill - _ghostStartFill) * easedProgress;

        if (progress >= 1f)
            _ghostAnimating = false;
    }
#endregion

#region Health
    public void TakeDamage(float damage)
    {
        if (!isActiveAndEnabled || damage <= 0f || _hp <= 0f)
            return;
        
        _hp = Mathf.Max(0f, _hp - damage);
        UpdateHUDVisual();

        if (_hp <= 0f)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.CallGameEnd();
            else
                Debug.LogError("No active GameManager exists.", this);
        }

        if (ghostHPBar == null) return;

        _ghostStartFill = ghostHPBar.fillAmount;
        _ghostElapsed = 0f;
        _ghostAnimating = true;
    }

    private void UpdateHUDVisual()
    {
        if (HPBar != null)
            HPBar.fillAmount = _hp / maxHP;
        
        if (HPTxt != null)
            HPTxt.text = $"{_hp} / {maxHP} HP";
    }
#endregion
}
