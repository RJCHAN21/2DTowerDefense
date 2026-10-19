using UnityEngine;

public class TowerPlacementBlocker : MonoBehaviour
{
#region Unity Inspector Fields
    [SerializeField] private Vector2 size = Vector2.one;
#endregion

#region Overlap Blocking
    public bool OverlapsCircle (Vector2 circleCenter, float radius)
    {
        // Locate the Blocker's Rectangular Boundaries
        // NOTE: Circle refers to the tower's base.
        // We're using the SIZE of a RECTANGLE: Our Blocker.
        float l = transform.position.x - size.x / 2;
        float r = transform.position.x + size.x / 2;
        float b = transform.position.y - size.y / 2;
        float t = transform.position.y + size.y / 2;

        float closestX = Mathf.Max(l, Mathf.Min(circleCenter.x, r));
        float closestY = Mathf.Max(b, Mathf.Min(circleCenter.y, t));
        float dist = (circleCenter.x - closestX) * (circleCenter.x - closestX)
                + (circleCenter.y - closestY) * (circleCenter.y - closestY);
        
        return dist <= radius * radius;
    }
#endregion
}
