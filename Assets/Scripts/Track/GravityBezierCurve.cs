using UnityEngine;
using System.Collections.Generic;

public class GravityBezierCurve : GravityRail
{
    // public variables
    public float toleranceDistance = 5.0f;
    public Vector2[] points = new Vector2[3];
    public int lineResolution = 20;

    // private variables
    private float scale;
    private Vector2 boundsMin;
    private Vector2 boundsMax;
    const float ONE_THIRD = 0.3333333333333333f;
    private float approxLength = 0.0f;

    // Use this for initialization
    public override void Start()
    {
        base.Start();

        UpdateLines();

        CalculateBounds();

        // estimate approximate curve length
        Vector2 nextPos = GetPointOnCurve(0.0f);
        Vector2 currPos = new Vector2();
        approxLength = 0.0f;
        float incrAmount = 0.02f;
        for (float t = 0.0f; t < 1.0f; t += incrAmount)
        {
            currPos = nextPos;
            nextPos = GetPointOnCurve(t + incrAmount);
            approxLength += (nextPos - currPos).magnitude;
        }
    }

    public override void Reset()
    {
        points = new Vector2[]
        {
            new Vector2(1.0f, 0.0f),
            new Vector2(2.0f, 0.0f),
            new Vector2(3.0f, 0.0f),
        };
    }

    // Update is called once per frame
    void Update()
    {
    }

    public override bool isPointInRange(Vector2 point, float dist)
    {
        float bestT = 0.0f;
        Vector2 nearestPoint = GetNearestPoint(point, ref bestT);
        if (bestT < 0.0f || bestT > 1.0f)
        {
            return false;
        }
        return (point - nearestPoint).SqrMagnitude() <= dist;
    }

    public override RailInfo GetRailInfoAtPoint(Vector2 point)
    {
        float bestT = 0.0f;
        Vector2 nearestPoint = GetNearestPoint(point, ref bestT);
        if (bestT < 0.0f || bestT > 1.0f)
        {
            return new RailInfo(this);
        }
        return new RailInfo(this, nearestPoint, GetTangentAtT(bestT), (point - nearestPoint).magnitude);
    }

    protected override void UpdateLines()
    {
        if (linePoints == null)
        {
            linePoints = new List<Vector2>();
        }
        linePoints.Clear();

        // iterate over curve to generate lines
        float stepSize = 1.0f / lineResolution;
        int index = 0;
        for (float i = 0.0f; index <= lineResolution; i += stepSize, ++index)
        {
            Vector2 point = GetPointOnCurve(i);
            linePoints.Add(point);
        }
    }

    public override float GetRailLength()
    {
        return approxLength;
    }

    public float GetScale()
    {
        return scale;
    }

    private Vector2 GetNearestPoint(Vector2 point, ref float bestT)
    {
        float[] sol = new float[3];
        int count = GetClosestT(point, ref sol);

        float distSquared = 0.0f;
        bestT = -1.0f;
        float distMinSquared = float.MaxValue;
        float distZeroSquared = (point - points[0]).SqrMagnitude();
        float distTwoSquared = (point - points[2]).SqrMagnitude();
        Vector2 pos = new Vector2(), minPos = new Vector2();
        for (int i = 0; i < count; ++i)
        {
            if (sol[i] < 0.0f || sol[i] > 1.0f)
            {
                continue;
            }

            // check if it is the new closest point
            pos = GetPointOnCurve(sol[i]);
            distSquared = (point - pos).SqrMagnitude();
            if (distSquared < distMinSquared)
            {
                distMinSquared = distSquared;
                bestT = sol[i];
                minPos = pos;
            }
        }
        return minPos;
    }

    private Vector2 GetPointOnCurve(float t)
    {
        float r = 1.0f - t;
        return new Vector2(r * r * points[0].x + 2.0f * r * t * points[1].x + t * t * points[2].x,
                            r * r * points[0].y + 2.0f * r * t * points[1].y + t * t * points[2].y);
    }

    private Vector2 GetTangentAtT(float t)
    {
        float r = 1.0f - t;
        return new Vector2(2.0f * r * (points[1].x - points[0].x) + 2.0f * t * (points[2].x - points[1].x),
                            2.0f * r * (points[1].y - points[0].y) + 2.0f * t * (points[2].y - points[1].y)).normalized;
    }

    private int GetClosestT(Vector2 point, ref float[] solutions)
    {
        int count = 0;
        Vector2 dxy = points[0] - point;
        Vector2 exy = points[0] - (2.0f * points[1]) + points[2];
        Vector2 fxy = 2.0f * (points[1] - points[0]);
        float d = 2.0f * exy.SqrMagnitude();
        float a, b, u, v, w, q, p, r, x;

        if (d == 0.0f)
        {
            b = fxy.SqrMagnitude() + 2.0f * Vector2.Dot(dxy, exy);
            if (b < 0.0f && -b < float.Epsilon)
            {
                return count;
            }
            else if (b < float.Epsilon)
            {
                return count;
            }
            solutions[count++] = -Vector2.Dot(fxy, dxy) / b;
        }
        else
        {
            float dInv = 1.0f / d;
            a = 3.0f * Vector2.Dot(fxy, exy) * dInv;
            b = (fxy.SqrMagnitude() + 2.0f * Vector2.Dot(dxy, exy)) * dInv;
            p = -a * a * ONE_THIRD + b;
            q = a * a * a * 0.074074074074074f - a * b * ONE_THIRD + Vector2.Dot(fxy, dxy) * dInv;
            r = q * q * 0.25f + p * p * p * 0.037037037037037f;

            if (r >= 0.0f)
            {
                r = Mathf.Sqrt(r);
                x = Sqrt3(-q * 0.5f + r) + Sqrt3(-q * 0.5f - r) - a * ONE_THIRD;
            }
            else
            {
                x = 2.0f * Mathf.Sqrt(-p * ONE_THIRD) * Mathf.Cos(Mathf.Atan2(Mathf.Sqrt(-r), -q * 0.5f) * ONE_THIRD) - a * ONE_THIRD;
            }

            u = x + a;
            v = x * x + a * x + b;
            w = u * u - 4.0f * v;

            if (w < 0.0f) { solutions[count++] = x; }
            else if (w > 0.0f)
            {
                w = Mathf.Sqrt(w);
                solutions[count++] = x;
                solutions[count++] = (-(u + w) * 0.5f);
                solutions[count++] = ((w - u) * 0.5f);
            }
            else
            {
                solutions[count++] = x;
                solutions[count++] = (-u * 0.5f);
            }
        }
        return count;
    }

    private float Sqrt3(float num)
    {
        return num > 0.0f ? Mathf.Pow(num, ONE_THIRD) : (num < 0.0f ? -Mathf.Pow(-num, ONE_THIRD) : 0.0f);
    }

    private void CalculateBounds()
    {
        // calculate bounds
        boundsMin.x = Mathf.Min(points[0].x, Mathf.Min(points[1].x, points[2].x));
        boundsMin.y = Mathf.Min(points[0].y, Mathf.Min(points[1].y, points[2].y));
        boundsMax.x = Mathf.Max(points[0].x, Mathf.Max(points[1].x, points[2].x));
        boundsMax.y = Mathf.Max(points[0].y, Mathf.Max(points[1].y, points[2].y));

        if (points[1] != (points[0] + points[2]) / 2.0f)
        {
            float u = 0.0f;
            Vector2 A = points[1] - points[0];
            Vector2 B = points[0] - (2.0f * points[1]) + points[2];
            if (boundsMin.x == points[1].x || boundsMax.x == points[1].x)
            {
                u = -A.x / B.x;
                u = GetPointOnCurve(u).x;
                if (boundsMin.x == points[1].x) boundsMin.x = u;
                else boundsMax.x = u;
            }
            if (boundsMin.y == points[1].y || boundsMax.y == points[1].y)
            {
                u = -A.y / B.y;
                u = GetPointOnCurve(u).y;
                if (boundsMin.y == points[1].y) boundsMin.y = u;
                else boundsMax.y = u;
            }
        }
    }

    void OnDrawGizmos()
    {
        UpdateLines();

        // draw line
        Gizmos.color = Color.white;
        for (int i = 0; i < linePoints.Count - 1; ++i)
        {
            Gizmos.DrawLine(linePoints[i], linePoints[(i + 1) % linePoints.Count]);
        }
    }
}
