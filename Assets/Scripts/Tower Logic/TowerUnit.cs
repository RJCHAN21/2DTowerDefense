using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class TowerUnit : TowerTarget, IHittable
{
#region Inspector Fields
    [Header("Health Setup")]
    [SerializeField] private Health health;

    [Header("Tower Config")]
    
    [Tooltip("How far the tower can see. Note that this is the base range for towers without a specified range.")]
    [SerializeField] private float baseRange = 3f;
    
    [Tooltip("How much space the tower occupies on the map.")]
    [SerializeField] private float baseRadius = 0.35f;

    [Tooltip("How fast the tower's gun can turn and aim at the target.")]
    [SerializeField] private float gunTurningSpeed = 90f;

    [Tooltip("Defines how the tower will behave.")]
    [SerializeField] private ScriptableObject towerTypeBehavior;

    // No longer needed: but if in case I ever make a new tower type with a cone range in the future, you do need this.
    // [Tooltip("The angle (in degrees) that this turret can see in a field-of-view arc.")]
    // [SerializeField] private float detectionAngle = 60f;

    [Header("Health Regeneration")]
    [Tooltip("Seconds without taking damage before the first health recovery.")]
    [SerializeField, Min(0f)] private float regenerationDelay = 5f;

    [Tooltip("Seconds between each subsequent recovery of 1 HP.")]
    [SerializeField, Min(0.01f)] private float regenerationInterval = 2f;

    [Header("Pooling")]
    [SerializeField] private PooledGun pooledGun;

    [Header("Line of Sight Visual")]
    [SerializeField] private LineRenderer lineRenderer;
#endregion

#region Public Properties
    public LineRenderer LineRenderer => lineRenderer;
    public bool IsInvincible { get; set; }
    public Health Health => health;
    public void SetPreview(bool enabled)
    {
        _isPreview = enabled;

        if (enabled)
            towers.Remove(this);
        else if (Application.IsPlaying(gameObject) &&
                isActiveAndEnabled &&
                !towers.Contains(this))
            towers.Add(this);

        SetTargetable(!enabled);
    }
    public float BaseRadius => baseRadius;
    public static IReadOnlyList<TowerUnit> Towers => towers;
    public Vector2 HitPosition => transform.position;
    public float HitRadius => 0.6f;
    public TowerDefinition Definition => _definition;
    public int CurrentStageIndex => _currentStageIndex;
    public int TotalInvestment => _totalInvestment;
    public IReadOnlyList<TowerUpgradeOption> AvailableUpgrades
    {
        get
        {
            if (_definition == null)
                return Array.Empty<TowerUpgradeOption>();

            TowerUpgradeDefinition upgrades = _definition.UpgradeDefinition;

            if (upgrades == null)
                return Array.Empty<TowerUpgradeOption>();

            TowerUpgradeStage[] stages = upgrades.Stages;

            if (stages == null ||
                _currentStageIndex < 0 ||
                _currentStageIndex >= stages.Length)
                return Array.Empty<TowerUpgradeOption>();
            
            TowerUpgradeOption[] options = stages[_currentStageIndex].Options;

            if (options == null)
                return Array.Empty<TowerUpgradeOption>();
            
            return options;
        }   
    }
    public string DisplayName => _displayName;
    public Sprite DisplayIcon => _displayIcon;
    public TowerFocus Focus => _focus;
    public int SellValue =>
        (int)((long)_totalInvestment * 75 / 100);
#endregion

#region Private Properties
    private Transform _target;
    private ScriptableObject _runtimeTowerType;
    private ITowerType AssignedTowerType =>
        (Application.IsPlaying(gameObject)
            ? _runtimeTowerType
            : towerTypeBehavior) as ITowerType;
    private bool _canFire = true;
    private static TowerUnit selectedTower = null;
    private bool _isInvincible = false;
    private bool _isHit = false;
    private float _nextRegenerationTime;
    private bool _isPreview = false;
    private static List<TowerUnit> towers = new List<TowerUnit>();
    private TowerDefinition _definition;
    private int _currentStageIndex;
    private int _totalInvestment;
    private string _displayName;
    private Sprite _displayIcon;
    private TowerFocus _focus = TowerFocus.First;
#endregion

#region Unity Life Cycle
    protected override void OnEnable()
    {
        SetTargetable(!_isPreview);

        if (health == null)
        {
            health = GetComponent<Health>();
        }

        if (health != null)
        {
            health.Died += OnDied;
            health.Damaged += OnDamaged;
        }
        
        if (!Application.IsPlaying(gameObject)) return;

        if (lineRenderer != null)
            lineRenderer.enabled = false;

        if (_runtimeTowerType == null && towerTypeBehavior is ITowerType)
            _runtimeTowerType = Instantiate(towerTypeBehavior);

        if (AssignedTowerType is Flamethrower flamethrower)
            flamethrower.ResetFiring();
        
        if (!_isPreview)
            towers.Add(this);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (health != null)
        {
            health.Died -= OnDied;
            health.Damaged -= OnDamaged;
        }

        if (AssignedTowerType is Flamethrower flamethrower)
            flamethrower.ResetFiring();

        towers.Remove(this);
    }

    private void OnDamaged(int damage, Vector3 position)
    {
        _nextRegenerationTime = Time.time + regenerationDelay;

        if (DamageNumberManager.Instance != null)
        {
            DamageNumberManager.Instance.SpawnDamageNumber(damage, position, isTower: true);
        }
    }

    private void Start()
    {
        if (!Application.IsPlaying(gameObject)) return;

        if (_target != null)
            pooledGun.SetHitTarget(_target.GetComponent<IHittable>());
    }

    private void Update()
    {
        if (!Application.IsPlaying(gameObject))
        {
            if (lineRenderer != null &&
                baseRange >= 0f &&
                !Mathf.Approximately(
                    lineRenderer.transform.lossyScale.x, 0f))
            {
                MapVisualToRange();
            }

            return;
        }

        if (_isPreview)
        {
            MapVisualToRange();
            return;
        }

        RegenerateHealth();

        if (!_canFire) return;

        ITowerType towerTypeBehavior = AssignedTowerType;
        if (towerTypeBehavior == null) return;

        pooledGun.SetTowerType(towerTypeBehavior);

        MapVisualToRange();

        Transform detectedEnemy = FindVisibleEnemy();

        if (_target != detectedEnemy)
        {
            _target = detectedEnemy;

            pooledGun.SetHitTarget(
                _target != null ? _target.GetComponent<IHittable>() : null);
        }

        bool shouldFire = false;

        if (_target != null)
        {
            LookAtTarget(_target);
            shouldFire = IsFacingTarget(_target);
        }

        if (towerTypeBehavior is Flamethrower flamethrower)
        {
            flamethrower.UpdateFiring(pooledGun, shouldFire);
        }
        else if (shouldFire)
        {
            pooledGun.Shoot();
        }
    }

    private void OnDestroy()
    {
        if (_runtimeTowerType != null)
            Destroy(_runtimeTowerType);
    }
#endregion

#region Public Methods
    public void StunTower()
    {
        _canFire = false;

        if (AssignedTowerType is Flamethrower flamethrower)
            flamethrower.ResetFiring();
    }

    public void CycleFocus(int direction)
    {
        int nextFocus = ((int)_focus + direction) % 3;

        if (nextFocus < 0)
            nextFocus += 3;

        _focus = (TowerFocus)nextFocus;
    }

    public bool TrySell(Currency currency)
    {
        if (!Application.IsPlaying(gameObject) ||
            !isActiveAndEnabled ||
            _isPreview ||
            _isHit ||
            health == null ||
            health.HP <= 0)
            return false;

        if (currency == null || !currency.TryDeposit(SellValue))
            return false;

        _isHit = true;
        gameObject.SetActive(false);
        Destroy(gameObject);

        return true;
    }
#endregion

#region Enemy Detection
    private Transform FindVisibleEnemy()
    {
        Enemy bestEnemy = null;
        float bestScore = float.NegativeInfinity;

        foreach (EnemyTarget target in EnemyTarget.ActiveEnemies)
        {
            if (target == null || !target.isActiveAndEnabled)
                continue;

            if (!(target is Enemy enemy))
                continue;

            if (enemy.CurrentHealth <= 0 || !IsInSight(enemy.transform))
                continue;

            float score;

            switch (_focus)
            {
                case TowerFocus.Last:
                    score = -enemy.PathProgress;
                    break;

                case TowerFocus.Strongest:
                    score = enemy.CurrentHealth;
                    break;

                default:
                    score = enemy.PathProgress;
                    break;
            }

            if (bestEnemy == null || score > bestScore)
            {
                bestScore = score;
                bestEnemy = enemy;
            }
        }

        return bestEnemy != null ? bestEnemy.transform : null;
    }

    private bool IsInSight(Transform target)
    {
        return AssignedTowerType != null &&
            AssignedTowerType.IsInSight(transform, target);
    }
#endregion

#region Attack Behavior
    private void LookAtTarget(Transform target)
    {
        Vector2 dir = target.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        Quaternion desiredRotation = Quaternion.Euler(0f, 0f, angle);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            desiredRotation,
            gunTurningSpeed * Time.deltaTime
        );
    }

    private bool IsFacingTarget(Transform target)
    {
        Vector2 forward = transform.up;
        Vector2 toTarget = ((Vector2)target.position
            - (Vector2)transform.position).normalized;

        float dot = Vector2.Dot(forward, toTarget);
        bool facingTarget = dot >= Mathf.Cos(2f * Mathf.Deg2Rad);

        return facingTarget;
    }
#endregion

#region LineRenderer
    private void MapVisualToRange()
    {
        if (AssignedTowerType != null)
        {
            AssignedTowerType.DrawSightVisual(lineRenderer);
        }
        else
        {
            LineRendShape.DrawCircle(lineRenderer, baseRange);
        }
    }
#endregion

#region Health Regeneration
    private void RegenerateHealth()
    {
        if (_isHit ||
            health == null ||
            !health.isActiveAndEnabled ||
            health.HP <= 0 ||
            health.HP >= health.MaxHP)
            return;

        if (Time.time < _nextRegenerationTime)
            return;

        health.Heal(1);
        _nextRegenerationTime = Time.time + regenerationInterval;
    }
#endregion

#region Hit Logic
    public void Hit(int damage)
    {
        if (_isPreview || _isHit || _isInvincible) return;

        if (health == null)
        {
            health = GetComponent<Health>();
        }

        if(health != null)
        {
            health.TakeDamage(damage);

            if (FXManager.Instance != null)
            {
                FXManager.Instance.PlayTowerHitFeedback(transform.position);
            }
        }

        else
        {
            Debug.LogError("TowerUnit is missing a Health component.", this);
        }
        
    }

    public void Hit(PooledGun sourceGun, int damage)
    {
        Hit(damage);
    }

    private void OnDied()
    {
        _isHit = true;

        if (FXManager.Instance != null)
        {
            FXManager.Instance.PlayExplosion(transform.position);
        }
        
        Destroy(gameObject);
    }
#endregion

#region Initialization
    public void Initialize(TowerDefinition definition)
    {
        _definition = definition;
        _currentStageIndex = 0;
        _totalInvestment = definition.Cost;
        _displayName = definition.name;
        _displayIcon = definition.TowerSprite;
    }
#endregion

#region Upgrade Logic
    
    public void ApplyUpgrade(TowerUpgradeEffects effects)
    {
        if (_runtimeTowerType is ITowerType behavior)
        {
            behavior.ApplyUpgrade(effects);
        }
    }

    public bool TryPurchaseUpgrade(
        int optionIndex,
        Currency currency,
        out TowerUnit resultingTower)
    {
        resultingTower = this;

        if (!Application.IsPlaying(gameObject) ||
            !isActiveAndEnabled ||
            _isPreview ||
            _isHit ||
            health == null ||
            health.HP <= 0)
            return false;
        
        if (optionIndex < 0 || optionIndex >= AvailableUpgrades.Count)
            return false;
        
        if (currency == null || !currency.isActiveAndEnabled)
            return false;
        
        if (!(_runtimeTowerType is ITowerType))
            return false;

        TowerUpgradeOption option = AvailableUpgrades[optionIndex];
        int nextStageIndex = option.NextStageIndex;
        TowerUpgradeStage[] stages = _definition.UpgradeDefinition.Stages;

        if (nextStageIndex != -1 &&
            (nextStageIndex <= _currentStageIndex ||
            nextStageIndex >= stages.Length))
            return false;
        
        if (option.Cost < 0 || currency.Balance < option.Cost)
            return false;

        TowerUnit upgradedTower = this;

        if (option.ReplacementPrefab != null)
        {
            upgradedTower = Instantiate(
                option.ReplacementPrefab,
                transform.position,
                transform.rotation,
                transform.parent
            );

            upgradedTower.SetPreview(true);
            upgradedTower.gameObject.SetActive(true);

            if (!(upgradedTower._runtimeTowerType is ITowerType))
            {
                upgradedTower.gameObject.SetActive(false);
                Destroy(upgradedTower.gameObject);
                return false;
            }

            if (upgradedTower.health == null ||
                !upgradedTower.health.isActiveAndEnabled ||
                upgradedTower.health.HP <= 0 ||
                upgradedTower.pooledGun == null ||
                upgradedTower.lineRenderer == null)
            {
                upgradedTower.gameObject.SetActive(false);
                Destroy(upgradedTower.gameObject);
                return false;
            }
        }

        if (!currency.TrySpend(option.Cost))
        {
            if (upgradedTower != this)
            {
                upgradedTower.gameObject.SetActive(false);
                Destroy(upgradedTower.gameObject);
            }

            return false;
        }

        upgradedTower._definition = _definition;
        upgradedTower._currentStageIndex = nextStageIndex;
        upgradedTower._totalInvestment = _totalInvestment + option.Cost;
        upgradedTower._focus = _focus;
        upgradedTower._displayName = _displayName;
        upgradedTower._displayIcon = _displayIcon;

        if (option.ReplacementPrefab != null)
        {
            upgradedTower._displayName = option.ReplacementPrefab.name;

            SpriteRenderer sprite = upgradedTower.GetComponent<SpriteRenderer>();
            upgradedTower._displayIcon = sprite != null ? sprite.sprite : option.Icon;
        }

        upgradedTower.ApplyUpgrade(option.TowerUpgradeEffects);
        upgradedTower.Health.ResetHealth();
        upgradedTower.MapVisualToRange();
     
        if (upgradedTower != this)
        {
            gameObject.SetActive(false);
            upgradedTower.SetPreview(false);
            Destroy(gameObject);
        }

        resultingTower = upgradedTower;
        return true;
    }
#endregion
}

public enum TowerFocus
{
    First,
    Last,
    Strongest
}