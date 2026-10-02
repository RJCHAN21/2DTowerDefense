using System;
using System.Collections;
using UnityEngine;

public static class PathFind
{
    public static IEnumerator FollowPath(
        Transform objectToMove,
        float duration,
        Func<float, Vector3> getPath)
    {
        if (objectToMove == null || getPath == null || duration <= 0f)
        {
            Debug.LogError("Path movement requires an object, a path, and a positive duration.");
            yield break;
        }

        float elapsed = 0f;

        while (objectToMove != null &&
               objectToMove.gameObject.activeInHierarchy &&
               elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress = Interpolate.GetLerpTime(elapsed, duration);
            objectToMove.position = getPath(progress);

            yield return null;
        }
    }
}
