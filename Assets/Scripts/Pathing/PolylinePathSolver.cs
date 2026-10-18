using UnityEngine;

public class PolylinePathSolver : MonoBehaviour
{
#region Unity Inspector Fields
    [SerializeField] private Transform startingPoint;
    [SerializeField] private Transform targetPoint;
    [SerializeField] private Transform[] controlPoints;
    [SerializeField] private float timeToReachTarget = 3f;
#endregion

#region Path Calculation

    public float Duration => timeToReachTarget;

    public bool IsValid
    {
        get
        {
            if (startingPoint == null ||
                targetPoint == null ||
                controlPoints == null ||
                !(timeToReachTarget > 0f))
                return false;

            foreach(Transform controlPt in controlPoints)
            {
                if (controlPt == null)
                    return false;
            }

            return true;
        }
    }

    public Vector3 GetPath(float progress)
    {
        Vector3 previousPt = startingPoint.position;
        float totalLength = 0;
        
        for (int i = 0; i < controlPoints.Length; i++)
        {
            Vector3 currPt = controlPoints[i].position;
            Vector3 dist = currPt - previousPt;
            totalLength += dist.magnitude;
            previousPt = currPt;
        }

        Vector3 finalSegLength = targetPoint.position - previousPt;
        totalLength += finalSegLength.magnitude;
        float totalDistTraveled = totalLength * progress;
        float remainingDist = totalDistTraveled;
        previousPt = startingPoint.position;

        for (int i = 0; i < controlPoints.Length; i++)
        {
            Vector3 currPt = controlPoints[i].position;
            Vector3 dist = currPt - previousPt;
            float segLength = dist.magnitude;

            if (segLength == 0)
            {
                previousPt = currPt;
                continue;
            }

            if (segLength < remainingDist)
            {
                remainingDist -= segLength;
                previousPt = currPt;
            } 
            else
            {
                float t = remainingDist / segLength;
                return Vector3.Lerp(previousPt, currPt, t);
            }
        }

        if (finalSegLength.magnitude == 0) return targetPoint.position;

        float finalSegT = remainingDist / finalSegLength.magnitude;
        return Vector3.Lerp(previousPt, targetPoint.position, finalSegT);
    }

#endregion

#region Gizmos
    private void OnDrawGizmos()
    {
        if (!IsValid) return;

        Gizmos.color = Color.yellow;

        var previousPt = startingPoint.position;

        for (int i = 0; i < controlPoints.Length; i++)
        {
            Vector3 currPt = controlPoints[i].position;
            Gizmos.DrawLine(previousPt, currPt);
            previousPt = currPt;
        }
        Gizmos.DrawLine(previousPt, targetPoint.position);
    }
#endregion
}
