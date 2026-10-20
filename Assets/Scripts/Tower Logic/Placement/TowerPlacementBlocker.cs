using UnityEngine;

public class TowerPlacementBlocker : MonoBehaviour
{
#region Unity Inspector Fields
    [SerializeField, Min(0)] private Vector2 size = Vector2.one;
    [SerializeField] private BlockerShape shape = BlockerShape.Rectangle;
#if UNITY_EDITOR
    [Tooltip("Scene-view key that appends and selects a polygon point.")]
    [SerializeField] private KeyCode addPointKey = KeyCode.N;
#endif
    [SerializeField] private Transform[] points;
#endregion

#region Runtime Properties
    private bool HasValidPolygonPoints
    {
        get
        {
            if (points == null || points.Length < 3)
                return false;

            foreach (Transform point in points)
            {
                if (point == null)
                    return false;
            }

            return true;
        }
    }
#endregion

#region Overlap Blocking
    public bool OverlapsCircle(Vector2 circleCenter, float radius)
    {
        bool result = false;

        if (shape == BlockerShape.Rectangle)
        {
            result = RectangleOverlapsCircle(circleCenter, radius);
        }

        if (shape == BlockerShape.Polygon)
        {
            if (!HasValidPolygonPoints)
                return false;

            result = CircleTouchesPolygonEdge(circleCenter, radius) || ContainsPoint(circleCenter);
        }

        return result;
    }

    public bool RectangleOverlapsCircle (Vector2 circleCenter, float radius)
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

    private bool CircleTouchesPolygonEdge(Vector2 circleCenter, float radius)
    {
        for (int i = 0; i < points.Length; i++)
        {
            int j = (i + 1) % points.Length;
            if (SquaredDistanceToSegment(circleCenter, points[i].position, points[j].position) <= radius * radius)
                return true;
        }
        return false;
    }

    private bool ContainsPoint(Vector2 circleCenter)
    {
        bool inside = false;

        for (int i = 0; i < points.Length; i++)
        {
            int j = (i + 1) % points.Length;
            if ((points[i].position.y > circleCenter.y) != (points[j].position.y > circleCenter.y))
            {
                float edgeYFrac = (circleCenter.y - points[i].position.y) / (points[j].position.y - points[i].position.y);
                float xCross = points[i].position.x + (edgeYFrac * (points[j].position.x - points[i].position.x));
                if (circleCenter.x < xCross)
                    inside = !inside;
            }   
        }
        return inside;
    }

    private float SquaredDistanceToSegment(Vector2 towerCenter, Vector2 edgeStartPt, Vector2 edgeEndPt)
    {
        Vector2 segment = new Vector2(edgeEndPt.x - edgeStartPt.x, edgeEndPt.y - edgeStartPt.y);
        Vector2 ptToTower = new Vector2(towerCenter.x - edgeStartPt.x, towerCenter.y - edgeStartPt.y);
        
        // Keeping this as future reference to understand what dot product actually means:
        // This is basically Vector2.Dot(segment, segment).
        float lengthSq = (segment.x * segment.x) + (segment.y * segment.y);
        
        if (lengthSq == 0) 
            return (towerCenter.x - edgeStartPt.x) * (towerCenter.x - edgeStartPt.x)
                + (towerCenter.y - edgeStartPt.y) * (towerCenter.y - edgeStartPt.y);
        
        // Clamped to avoid infinite line; we're measuring a finite line, not infinite.
        // The numerator is another dot product which can be simplified to Vector2.Dot(ptTower, segment).
        float segLengthFrac = Mathf.Clamp01(((ptToTower.x * segment.x) + (ptToTower.y * segment.y)) / lengthSq);
        float closestXPt = edgeStartPt.x + (segLengthFrac * segment.x);
        float closestYPt = edgeStartPt.y + (segLengthFrac * segment.y);
        float distSq = (towerCenter.x - closestXPt) * (towerCenter.x - closestXPt)
                    + (towerCenter.y - closestYPt) * (towerCenter.y - closestYPt);

        return distSq;
    }
#endregion

#region Editor Visualization
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        if (shape == BlockerShape.Rectangle)
        {
            Gizmos.DrawWireCube(
                transform.position,
                new Vector3(size.x, size.y, 0f));
        }
        else if (shape == BlockerShape.Polygon)
        {
            if (!HasValidPolygonPoints)
                return;

            for (int i = 0; i < points.Length; i++)
            {
                int j = (i + 1) % points.Length;

                Gizmos.DrawLine(points[i].transform.position, points[j].transform.position);
            }
        }
    }
#endregion
}

enum BlockerShape { Rectangle, Polygon};
