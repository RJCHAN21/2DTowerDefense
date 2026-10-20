using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class DamageNumberManager : MonoBehaviour
{
    public static DamageNumberManager Instance { get; private set; }

    [SerializeField] private DamageNumber damageNumberPrefab;
    [SerializeField] private Color enemyDamageColor = Color.white;
    [SerializeField] private Color towerDamageColor = new Color(1f, 0.3f, 0.3f); 

    private IObjectPool<DamageNumber> _pool;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _pool = new ObjectPool<DamageNumber>(
            createFunc: () => Instantiate(damageNumberPrefab, transform),
            actionOnGet: null,
            actionOnRelease: num => {if (num != null) num.gameObject.SetActive(false); },
            actionOnDestroy: num => {if (num != null) Destroy(num.gameObject); },
            collectionCheck: false,
            defaultCapacity: 30,
            maxSize: 100
        );
    }

    public void SpawnDamageNumber(int amount, Vector3 position, bool isTower)
    {
        DamageNumber number = _pool.Get();
        Color displayColor = isTower ? towerDamageColor : enemyDamageColor;
        number.Initialize(amount, position, _pool, displayColor);
    }
}