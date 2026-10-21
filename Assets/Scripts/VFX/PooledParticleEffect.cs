using UnityEngine;
using UnityEngine.Pool;

public class PooledParticleEffect : MonoBehaviour
{
    private IObjectPool<PooledParticleEffect> _poolOwner;
    private ParticleSystem _particleSystem;

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    public void Initialize(IObjectPool<PooledParticleEffect> pool)
    {
        _poolOwner = pool;
    }

    public void PlayAt(Vector3 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);
        gameObject.SetActive(true);

        if (_particleSystem != null)
        {
            _particleSystem.Clear();
            _particleSystem.Play();
        }
    }

    private void Update()
    {
        if (_particleSystem != null && !_particleSystem.IsAlive(true))
        {
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        if (_poolOwner != null)
        {
            _poolOwner.Release(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}