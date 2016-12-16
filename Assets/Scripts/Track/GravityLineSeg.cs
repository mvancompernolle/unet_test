using UnityEngine;
using System.Collections;

public class GravityLineSeg : GravityRail
{
    // public variables
    public float toleranceDistance;
    public Vector2 point1;
    public Vector2 point2;

    // private varialbes
    private Vector2 lineDirection;
    private float lengthSquared;
    private float toleranceT;

    // Use this for initialization
    public override void Start()
    {
        base.Start();

        UpdateLines();

        lineRenderer.SetVertexCount(2);
        lineRenderer.SetPosition(0, point1);
        lineRenderer.SetPosition(1, point2);
    }

    public override void Reset()
    {
        point1 = new Vector2(-1.0f, 0.0f);
        point2 = new Vector2(1.0f, 0.0f);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public override bool isPointInRange(Vector2 point, float dist)
    {
        if (lengthSquared == 0.0f) return false;

        // project the point onto the line
        Vector2 toPlayer = point - point1;
        // lineDir and toPlayer not normalized, so divide by length Squared to get t between 0 and 1
        float t = Vector2.Dot(lineDirection, toPlayer) / lengthSquared;
        if (t + toleranceT < 0.0f || t - toleranceT > 1.0f) return false;
        Vector2 projection = point1 + t * lineDirection;
        Vector2 offset = point - projection;
        return offset.magnitude <= dist;
    }

    public override RailInfo GetRailInfoAtPoint(Vector2 point)
    {
        Vector2 toPlayer = point - point1;
        float t = Vector2.Dot(lineDirection, toPlayer) / lengthSquared;
        if (t + toleranceT < 0.0f || t - toleranceT > 1.0f)
        {
            return new RailInfo(this);
        }
        Vector2 projection = point1 + t * lineDirection;
        return new RailInfo(this, projection, lineDirection.normalized, (point - projection).magnitude);
    }

    protected override void UpdateLines()
    {
        // update length squared and direction
        lineDirection = point2 - point1;
        lengthSquared = lineDirection.x * lineDirection.x + lineDirection.y * lineDirection.y;

        // update tolerance
        toleranceT = toleranceDistance / Mathf.Sqrt(lengthSquared);
    }

    public override float GetRailLength()
    {
        return Mathf.Sqrt(lengthSquared);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawLine(point1, point2);
    }
}
