using UnityEngine;
using System.Collections.Generic;

public class GravityRing : GravityRail
{
    // public variables
    public float curveCircumInDegrees = 360.0f;
    public int circleResolution = 24;
    public float toleranceAngle = 5.0f;
    public Vector2 center;
    public float angle = 90.0f;
    public float radius = 1.0f;

    // private variables
    private float startingRotation = 0.0f;
    private float endRotation = 0.0f;
    private float smallerAngle;
    private float largerAngle;
    private Vector2 forwardDir;
    private float minDotVal;

    // Use this for initialization
    public override void Start()
    {
        base.Start();

        curveCircumInDegrees = Mathf.Clamp(curveCircumInDegrees, 0.0f, 360.0f);
        // limit tolerance to not allow for more than 360 degree range
        if (2.0f * toleranceAngle + curveCircumInDegrees > 360.0f)
        {
            toleranceAngle = (360.0f - curveCircumInDegrees) / 2.0f;
        }
        minDotVal = Mathf.Cos(((curveCircumInDegrees + toleranceAngle) / 2.0f) * Mathf.Deg2Rad);

        calcRangeData();
        UpdateLines();

        lineRenderer.SetVertexCount(linePoints.Count);
        for( int i = 0; i < linePoints.Count; ++i)
        {
            lineRenderer.SetPosition(i, linePoints[i]);
        }
    }

    public override void Reset()
    {
        Debug.Log("rest called");
        center = new Vector2(0.0f, 0.0f);
        curveCircumInDegrees = 360.0f;
        radius = 5.0f;
        angle = 90.0f;
    }

    // Update is called once per frame
    void Update()
    {
    }

    void calcRangeData()
    {
        // calculate circumfrence range
        float halfOfCurve = (curveCircumInDegrees / 2.0f);
        startingRotation = (angle - halfOfCurve) % 360;

        if ((startingRotation + curveCircumInDegrees) % 360 != 360.0f)
            endRotation = startingRotation + curveCircumInDegrees;
        else
            endRotation = 360.0f;

        if (startingRotation <= endRotation)
        {
            smallerAngle = startingRotation;
            largerAngle = endRotation;
        }
        else
        {
            largerAngle = startingRotation;
            smallerAngle = endRotation;
        }

        // get the forward direction;
        forwardDir = new Vector2(Mathf.Cos((angle) * Mathf.Deg2Rad),
                                 Mathf.Sin((angle) * Mathf.Deg2Rad));
    }

    public override bool isPointInRange(Vector2 point, float dist)
    {
        Vector2 offset = point - center;

        // check if in the right direction
        if (Vector2.Dot(offset.normalized, forwardDir) < minDotVal)
        {
            return false;
        }

        // check to see if in grinding distance
        return Mathf.Abs(radius - offset.magnitude) <= dist;
    }

    public override RailInfo GetRailInfoAtPoint(Vector2 point)
    {
        Vector2 offset = point - center;
        // check if in the right direction
        if (Vector2.Dot(offset.normalized, forwardDir) < minDotVal)
        {
            return new RailInfo(this);
        }

        Vector2 pos = center + (offset.normalized * radius);
        return new RailInfo(this, pos, new Vector2(-offset.y, offset.x).normalized, (point - pos).magnitude);
    }

    protected override void UpdateLines()
    {
        // scale and translate points based on transform
        calcRangeData();

        if (linePoints == null)
        {
            linePoints = new List<Vector2>();
        }
        linePoints.Clear();

        float start = smallerAngle * Mathf.Deg2Rad;
        float end = largerAngle * Mathf.Deg2Rad;
        float angleDiff = ((end - start) / circleResolution);

        int index = 0;
        for (float i = start; index <= circleResolution; i += angleDiff)
        {
            Vector3 point = new Vector3(Mathf.Cos(i), Mathf.Sin(i), 0.0f);
            point *= radius;
            point += (Vector3)center;
            linePoints.Add(point);
            index++;
        }
    }

    private float GetControlDistForNSegment(float n)
    {
        return (4.0f / 3.0f) * Mathf.Tan(Mathf.PI / (2.0f * n));
    }

    public override float GetRailLength()
    {
        return (2.0f * Mathf.PI * GetScale()) * (curveCircumInDegrees / 360.0f);
    }

    public float GetScale()
    {
        return radius;
    }

    void OnDrawGizmos()
    {
        UpdateLines();
        calcRangeData();

        Gizmos.color = Color.white;
        for (int i = 0; i < linePoints.Count - 1; ++i)
        {
            Gizmos.DrawLine(linePoints[i], linePoints[(i + 1) % linePoints.Count]);
        }
    }
}
