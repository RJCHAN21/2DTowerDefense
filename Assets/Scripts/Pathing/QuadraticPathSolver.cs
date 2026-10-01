using UnityEngine;

public class QuadraticPathSolver : MonoBehaviour
{
#region Unity Inspector Fields
    [SerializeField] private Transform objectToMove;
    [SerializeField] private Transform startingPoint;
    [SerializeField] private Transform targetPoint;
    [SerializeField] private Transform controlPoint;
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
            controlPoint == null)
            return;

        _startPos = startingPoint.position;
        _totalTime += Time.deltaTime;
        var lerpTime = Interpolate.GetLerpTime(_totalTime, timeToReachTarget);
        objectToMove.transform.position = Bezier.Quadratic(_startPos, controlPoint.position, targetPoint.position, lerpTime);
    }
#endregion

#region Gizmos
    private void OnDrawGizmos() 
    {
        if (objectToMove == null || 
            startingPoint == null || 
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