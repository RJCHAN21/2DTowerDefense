using UnityEngine;

public class QuadraticPathSolver : MonoBehaviour
{
#region Unity Inspector Fields
    [SerializeField] private Transform startingPoint;
    [SerializeField] private Transform targetPoint;
    [SerializeField] private Transform controlPoint;
    [SerializeField] private float timeToReachTarget = 3f;
    [SerializeField] private float resolution = 50f;
#endregion

#region Path Calculation

    public float Duration => timeToReachTarget;

    public bool IsValid =>
        startingPoint != null &&
        targetPoint != null &&
        controlPoint != null &&
        timeToReachTarget > 0f;

    public Vector3 GetPath(float progress)
    {
        return Bezier.Quadratic(
            startingPoint.position,
            controlPoint.position,
            targetPoint.position,
            progress);
    }

#endregion

#region Gizmos
    private void OnDrawGizmos() 
    {
        if (startingPoint == null || 
            targetPoint == null || 
            controlPoint == null)
            return;
        
        Gizmos.color = Color.cyan;

        var previousLine = startingPoint.position;

        for (int i = 1; i <= resolution; i++)
        {
            var gap = i / resolution;

            var newPos = Bezier.Quadratic(
                startingPoint.position,
                controlPoint.position,
                targetPoint.position,
                gap
            );

            Gizmos.DrawLine(previousLine, newPos);

            previousLine = newPos;
        }
    }
#endregion
}