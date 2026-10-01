using UnityEngine;

public class CubicPathSolver : MonoBehaviour
{
#region Unity Inspector Fields
    [SerializeField] private Transform objectToMove;
    [SerializeField] private Transform startingPoint;
    [SerializeField] private Transform targetPoint;
    [SerializeField] private Transform controlPointA;
    [SerializeField] private Transform controlPointB;
    [SerializeField] private float timeToReachTarget = 3f;
    [SerializeField] private float resolution = 50f;
#endregion

#region Private Properties
    private float _totalTime;
    private Vector3 _startPos;
#endregion

#region Unity Life Cycle
    private void Update()
    {
        if (objectToMove == null || 
            startingPoint == null || 
            targetPoint == null || 
            controlPointA == null || 
            controlPointB == null) 
            return;

        _startPos = startingPoint.position;
        _totalTime += Time.deltaTime;
        var lerpTime = Interpolate.GetLerpTime(_totalTime, timeToReachTarget);
        objectToMove.transform.position = Bezier.Cubic(_startPos, controlPointA.position, controlPointB.position, targetPoint.position, lerpTime);
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
