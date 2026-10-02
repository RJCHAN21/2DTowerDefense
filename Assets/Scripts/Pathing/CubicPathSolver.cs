using UnityEngine;

public class CubicPathSolver : MonoBehaviour
{
#region Unity Inspector Fields
    [SerializeField] private Transform startingPoint;
    [SerializeField] private Transform targetPoint;
    [SerializeField] private Transform controlPointA;
    [SerializeField] private Transform controlPointB;
    [SerializeField] private float timeToReachTarget = 3f;
    [SerializeField] private float resolution = 50f;
#endregion

#region Path Calculation

    public float Duration => timeToReachTarget;

    public bool IsValid =>
        startingPoint != null &&
        targetPoint != null &&
        controlPointA != null &&
        controlPointB != null &&
        timeToReachTarget > 0f;

    public Vector3 GetPath(float progress)
    {
        return Bezier.Cubic(
            startingPoint.position,
            controlPointA.position,
            controlPointB.position,
            targetPoint.position,
            progress);
    }

#endregion

#region Gizmos
    private void OnDrawGizmos()
    {
        if (startingPoint == null ||
            targetPoint == null ||
            controlPointA == null ||
            controlPointB == null)
            return;

        Gizmos.color = Color.red;

        var previousLine = startingPoint.position;

        for (int i = 1; i <= resolution; i++)
        {
            var gap = i / resolution;

            var newPos = Bezier.Cubic(
                startingPoint.position,
                controlPointA.position,
                controlPointB.position,
                targetPoint.position,
                gap
            );

            Gizmos.DrawLine(previousLine, newPos);

            previousLine = newPos;
        }
    }
#endregion
}
