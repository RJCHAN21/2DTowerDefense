using TMPro;
using UnityEngine;
using UnityEngine.Pool;

public class DamageNumber : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMesh;
    [SerializeField] private float floatSpeed = 1.5f;
    [SerializeField] private float lifetime = 0.75f;

    private IObjectPool<DamageNumber> _pool;
    private float _timer;
    private Color _initialColor;

    private void Awake()
    {
        if (textMesh == null){
            textMesh = GetComponent<TextMeshPro>();
        }
        _initialColor = textMesh != null ? textMesh.color : Color.white;
    }

    public void Initialize(int damage, Vector3 worldPosition, IObjectPool<DamageNumber> pool, Color textColor)
    {
        _pool = pool;
        _timer = 0f;
        
        Vector3 randomOffset = new Vector3(Random.Range(-0.25f, 0.25f), Random.Range(0.1f, 0.3f), 0f);
        transform.position = worldPosition + randomOffset;

        if (textMesh != null)
        {
            textMesh.text = damage.ToString();
            textMesh.color = textColor;
        }

        gameObject.SetActive(true);
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        if (textMesh != null)
        {
            float alpha = Mathf.Clamp01(1f - (_timer / lifetime));
            Color c = textMesh.color;
            c.a = alpha;
            textMesh.color = c;
        }

        if (_timer >= lifetime)
        {
            if (_pool != null)
                _pool.Release(this);
            else
                Destroy(gameObject);
        }
    }
}