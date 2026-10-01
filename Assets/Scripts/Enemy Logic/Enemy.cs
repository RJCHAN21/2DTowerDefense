using UnityEngine;

public class Enemy : EnemyTarget, IHittable
{
#region Unity Inspector Fields
    [Header("Hitbox")]
    [SerializeField] private float hitRadius = 0.5f;
#endregion

#region Private Properties
    private bool isHit;
    private bool isInvincible = false;
#endregion

#region Public Properties
    public Vector2 HitPosition => transform.position;
    public bool IsInvincible { get; set; }   
    public float HitRadius => hitRadius;
#endregion

#region Hit Logic
    public void Hit(PooledGun sourceGun)
    {
        if (isHit || isInvincible) return;
        
        isHit = true;
        gameObject.SetActive(false);
    }
#endregion

#region Gizmos
    private void OnDrawGizmosSelected()
    {
        const int segments = 64;

        Color prevColor = Gizmos.color;
        Gizmos.color = Color.red;

        Vector3 center = new Vector3(
            HitPosition.x, HitPosition.y, transform.position.z);

        Vector3 prevPt = center + Vector3.right * hitRadius;

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;

            Vector3 nextPt = center + new Vector3(
                Mathf.Cos(angle) * hitRadius,
                Mathf.Sin(angle) * hitRadius,
                0f);

            Gizmos.DrawLine(prevPt, nextPt);
            prevPt = nextPt;
        }

        Gizmos.color = prevColor;
    }
#endregion
}
