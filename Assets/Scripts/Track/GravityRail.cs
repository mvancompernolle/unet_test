using UnityEngine;
using System.Collections.Generic;

public abstract class GravityRail : MonoBehaviour
{
    // track variables
    private GravityTrack track;

    // line variables
    protected List<Vector2> linePoints;
    protected LineRenderer lineRenderer;

    public class RailInfo
    {
        public bool alongRail;
        public Vector2 closestPoint;
        public Vector2 tangent;
        public float distance;
        public GravityRail rail;

        public RailInfo(GravityRail myRail)
        {
            alongRail = false;
            rail = myRail;
        }

        public RailInfo(GravityRail myRail, Vector2 point, Vector2 tan, float dist)
        {
            alongRail = true;
            rail = myRail;
            closestPoint = point;
            tangent = tan;
            distance = dist;
        }
    }

    public virtual void Awake()
    {
        // setup line renderer
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.SetWidth(0.5f, 0.5f);
        lineRenderer.enabled = true;
        lineRenderer.SetColors(Color.red, Color.red);
    }

    // Use this for initialization
    public virtual void Start()
    {
        // check to see if part of a gravity track
        track = transform.root.gameObject.GetComponent<GravityTrack>();

        if (track != null)
        {
            // add to the track
            track.AddTrackSegment(this);
        }
    }

    public virtual void Reset()
    {
        transform.position = new Vector3();
        transform.rotation = Quaternion.identity;
        transform.localScale = new Vector3();
    }

    public abstract bool isPointInRange(Vector2 point, float dist);
    public abstract RailInfo GetRailInfoAtPoint(Vector2 point);
    protected abstract void UpdateLines();
    public abstract float GetRailLength();
}
