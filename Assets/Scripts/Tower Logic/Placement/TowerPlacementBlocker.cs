using UnityEngine;

public class TowerPlacementBlocker : MonoBehaviour
{
#region Unity Inspector Fields
    [SerializeField, Min(0)] private Vector2 size = Vector2.one;
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
        float distSq = (circleCenter.x - closestX) * (circleCenter.x - closestX)
                + (circleCenter.y - closestY) * (circleCenter.y - closestY);
        
        return distSq <= radius * radius;
    }
#endregion

#region Editor Visualization
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            transform.position,
            new Vector3(size.x, size.y, 0f));
    }
#endregion
}

enum BlockerShape { Rectangle, Polygon};
