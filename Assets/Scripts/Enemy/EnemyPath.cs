using UnityEngine;
using System.Collections.Generic;

public enum PathType
{
    Straight,
    Curved
}

public class EnemyPath : MonoBehaviour
{
    [Header("Path Settings")]
    [SerializeField] private List<Vector2> waypoints = new List<Vector2>();
    [SerializeField] private PathType pathType = PathType.Straight;
    [SerializeField] private float curveStrength = 0.5f; // Default curve strength (used if per-segment values not set)
    [SerializeField] private List<float> segmentCurveStrengths = new List<float>(); // Per-segment curve strength (positive = right, negative = left, 0 = straight/auto)
    [SerializeField] private int curveSegments = 20; // Number of segments to draw curves with
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color pathColor = Color.red;

    public List<Vector2> Waypoints => waypoints;
    public PathType PathType => pathType;
    public float CurveStrength => curveStrength;

    private void OnValidate()
    {
        // Sync segmentCurveStrengths list when waypoints change
        SyncCurveStrengths();
    }

    private void SyncCurveStrengths()
    {
        // Ensure segmentCurveStrengths has the right number of elements (one per segment)
        int requiredCount = Mathf.Max(0, waypoints.Count - 1);
        
        // Remove excess elements
        while (segmentCurveStrengths.Count > requiredCount)
        {
            segmentCurveStrengths.RemoveAt(segmentCurveStrengths.Count - 1);
        }
        
        // Add missing elements with default curve strength
        while (segmentCurveStrengths.Count < requiredCount)
        {
            segmentCurveStrengths.Add(curveStrength);
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos || waypoints.Count < 2) return;

        Gizmos.color = pathColor;

        // Draw waypoints
        for (int i = 0; i < waypoints.Count; i++)
        {
            Vector3 worldPos = transform.TransformPoint(waypoints[i]);
            Gizmos.DrawWireSphere(worldPos, 0.2f);

            // Draw path to next waypoint
            if (i < waypoints.Count - 1)
            {
                Vector3 nextWorldPos = transform.TransformPoint(waypoints[i + 1]);
                
                float segmentCurve = GetSegmentCurveStrength(i);
                
                if (pathType == PathType.Curved && Mathf.Abs(segmentCurve) > 0.01f)
                {
                    // Draw curved path
                    Vector3 prevPos = i > 0 ? transform.TransformPoint(waypoints[i - 1]) : worldPos;
                    Vector3 nextNextPos = i < waypoints.Count - 2 ? transform.TransformPoint(waypoints[i + 2]) : nextWorldPos;
                    
                    Vector3 prevPoint = worldPos;
                    for (int j = 1; j <= curveSegments; j++)
                    {
                        float t = j / (float)curveSegments;
                        Vector3 curvedPoint = GetCurvedPoint(worldPos, nextWorldPos, prevPos, nextNextPos, t, segmentCurve, i);
                        Gizmos.DrawLine(prevPoint, curvedPoint);
                        prevPoint = curvedPoint;
                    }
                }
                else
                {
                    // Draw straight line
                    Gizmos.DrawLine(worldPos, nextWorldPos);
                }
            }
        }
    }

    public void AddWaypoint(Vector2 waypoint)
    {
        waypoints.Add(waypoint);
        // Ensure segmentCurveStrengths list matches (one less than waypoints)
        while (segmentCurveStrengths.Count < waypoints.Count - 1)
        {
            segmentCurveStrengths.Add(curveStrength); // Use default curve strength
        }
    }

    public void ClearWaypoints()
    {
        waypoints.Clear();
        segmentCurveStrengths.Clear();
    }

    /// <summary>
    /// Gets the curve strength for a specific segment.
    /// Positive values curve right, negative values curve left.
    /// </summary>
    /// <param name="segmentIndex">Index of the segment (0 = first segment, waypoints.Count - 2 = last segment)</param>
    /// <returns>Curve strength for this segment (positive = right, negative = left, 0 = straight/auto)</returns>
    public float GetSegmentCurveStrength(int segmentIndex)
    {
        if (segmentIndex < 0 || segmentIndex >= waypoints.Count - 1)
        {
            return curveStrength; // Return default if invalid
        }
        
        // If per-segment values exist, use them; otherwise use default
        if (segmentIndex < segmentCurveStrengths.Count)
        {
            return segmentCurveStrengths[segmentIndex];
        }
        
        return curveStrength;
    }

    /// <summary>
    /// Sets the curve strength for a specific segment.
    /// Positive values curve right, negative values curve left.
    /// </summary>
    /// <param name="segmentIndex">Index of the segment</param>
    /// <param name="strength">Curve strength (positive = right, negative = left, 0 = straight/auto, range: -1 to 1)</param>
    public void SetSegmentCurveStrength(int segmentIndex, float strength)
    {
        if (segmentIndex < 0 || segmentIndex >= waypoints.Count - 1)
        {
            return; // Invalid segment
        }
        
        // Ensure list is large enough
        while (segmentCurveStrengths.Count <= segmentIndex)
        {
            segmentCurveStrengths.Add(curveStrength);
        }
        
        segmentCurveStrengths[segmentIndex] = Mathf.Clamp(strength, -1f, 1f);
    }

    public Vector2 GetWorldWaypoint(int index)
    {
        if (index < 0 || index >= waypoints.Count)
        {
            return Vector2.zero;
        }
        return transform.TransformPoint(waypoints[index]);
    }

    /// <summary>
    /// Gets a position along the path between two waypoints.
    /// </summary>
    /// <param name="fromIndex">Starting waypoint index</param>
    /// <param name="t">Progress along the segment (0 to 1)</param>
    /// <returns>World position along the path</returns>
    public Vector2 GetPositionAlongPath(int fromIndex, float t)
    {
        if (fromIndex < 0 || fromIndex >= waypoints.Count - 1)
        {
            return GetWorldWaypoint(fromIndex);
        }

        Vector2 from = GetWorldWaypoint(fromIndex);
        Vector2 to = GetWorldWaypoint(fromIndex + 1);

        float segmentCurve = GetSegmentCurveStrength(fromIndex);
        
        if (pathType == PathType.Curved && Mathf.Abs(segmentCurve) > 0.01f)
        {
            // Get previous and next waypoints for curve control
            // For first segment, use next waypoint; for last segment, use previous waypoint
            Vector2 prev;
            Vector2 nextNext;
            
            if (fromIndex == 0)
            {
                // First segment: use next waypoint (index 2) to determine curve direction
                prev = from; // Not used, but needed for function signature
                nextNext = waypoints.Count > 2 ? GetWorldWaypoint(fromIndex + 2) : to;
            }
            else if (fromIndex >= waypoints.Count - 2)
            {
                // Last segment: use previous waypoint to determine curve direction
                prev = GetWorldWaypoint(fromIndex - 1);
                nextNext = to; // Not used, but needed for function signature
            }
            else
            {
                // Middle segments: use both previous and next
                prev = GetWorldWaypoint(fromIndex - 1);
                nextNext = GetWorldWaypoint(fromIndex + 2);
            }
            
            return GetCurvedPoint(from, to, prev, nextNext, t, segmentCurve, fromIndex);
        }
        else
        {
            // Straight line interpolation
            return Vector2.Lerp(from, to, t);
        }
    }

    /// <summary>
    /// Calculates a point along a curved path using quadratic Bezier curve.
    /// </summary>
    private Vector2 GetCurvedPoint(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t, float segmentCurveStrength, int segmentIndex)
    {
        // Calculate direction from p0 to p1
        Vector2 toP1 = (p1 - p0).normalized;
        
        // Determine curve direction based on segment position
        Vector2 curveDirection;
        float angleChange;
        
        if (segmentIndex == 0)
        {
            // First segment: use direction to next waypoint (p3, which is nextNext)
            if (Vector2.Distance(p3, p1) > 0.01f)
            {
                curveDirection = (p3 - p1).normalized;
                angleChange = Vector2.SignedAngle(toP1, curveDirection);
            }
            else
            {
                // No next waypoint - create simple arc
                angleChange = 90f; // Default curve
            }
        }
        else if (segmentIndex >= waypoints.Count - 2)
        {
            // Last segment: use direction from previous waypoint (p2, which is prev)
            if (Vector2.Distance(p2, p0) > 0.01f && Vector2.Distance(p2, p1) > 0.01f)
            {
                Vector2 fromPrev = (p0 - p2).normalized;
                angleChange = Vector2.SignedAngle(fromPrev, toP1);
            }
            else
            {
                // No previous waypoint - create simple arc
                angleChange = 90f; // Default curve
            }
        }
        else
        {
            // Middle segments: use angle between current and next segment
            if (Vector2.Distance(p3, p1) > 0.01f)
            {
                curveDirection = (p3 - p1).normalized;
                angleChange = Vector2.SignedAngle(toP1, curveDirection);
            }
            else
            {
                angleChange = 90f; // Default curve
            }
        }
        
        // Create a control point that creates a smooth curve
        // The control point is offset perpendicular to the line between waypoints
        Vector2 midPoint = (p0 + p1) * 0.5f;
        Vector2 perpendicular = new Vector2(-toP1.y, toP1.x); // Perpendicular vector (right side)
        
        // Calculate how much to curve based on the angle change
        // Use the absolute value of segmentCurveStrength for magnitude
        float curveMagnitude = Mathf.Abs(segmentCurveStrength);
        float curveAmount = Mathf.Sin(angleChange * Mathf.Deg2Rad) * curveMagnitude * Vector2.Distance(p0, p1) * 0.3f;
        
        // If angle is very small or calculation resulted in near-zero, create a simple arc
        if (Mathf.Abs(curveAmount) < 0.01f)
        {
            curveAmount = curveMagnitude * Vector2.Distance(p0, p1) * 0.2f;
        }
        
        // Apply curve direction: use the sign of segmentCurveStrength
        // Positive = curve right, negative = curve left
        // If strength is negative, flip the direction
        if (segmentCurveStrength < 0)
        {
            curveAmount = -Mathf.Abs(curveAmount);
        }
        else
        {
            curveAmount = Mathf.Abs(curveAmount);
        }
        
        Vector2 controlPoint = midPoint + perpendicular * curveAmount;
        
        // Quadratic Bezier curve: (1-t)²P₀ + 2(1-t)tP₁ + t²P₂
        float u = 1f - t;
        return u * u * p0 + 2f * u * t * controlPoint + t * t * p1;
    }

    /// <summary>
    /// Finds the nearest point on the path to a world position (for re-joining path after abilities).
    /// </summary>
    /// <param name="worldPos">Position in world space</param>
    /// <param name="segmentIndex">Output: waypoint index starting the segment (0 to Waypoints.Count - 2)</param>
    /// <param name="segmentT">Output: progress along that segment (0 to 1)</param>
    /// <returns>World position of the nearest point on the path</returns>
    public Vector2 GetNearestPointOnPath(Vector2 worldPos, out int segmentIndex, out float segmentT)
    {
        segmentIndex = 0;
        segmentT = 0f;
        if (waypoints.Count < 2)
        {
            if (waypoints.Count == 1)
                return GetWorldWaypoint(0);
            return worldPos;
        }
        int samplesPerSegment = 20;
        float bestDistSq = float.MaxValue;
        Vector2 bestPoint = GetWorldWaypoint(0);
        for (int seg = 0; seg < waypoints.Count - 1; seg++)
        {
            for (int i = 0; i <= samplesPerSegment; i++)
            {
                float t = i / (float)samplesPerSegment;
                Vector2 p = GetPositionAlongPath(seg, t);
                float distSq = (p - worldPos).sqrMagnitude;
                if (distSq < bestDistSq)
                {
                    bestDistSq = distSq;
                    bestPoint = p;
                    segmentIndex = seg;
                    segmentT = t;
                }
            }
        }
        return bestPoint;
    }
}
