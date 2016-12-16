using UnityEngine;
using System.Collections.Generic;

public class GravityTrack : MonoBehaviour
{
    // public variables
    public float railSwitchRange = 0.5f;
    [SerializeField]
    private float PARTICLES_PER_UNIT_LENGTH = 5.0f;

    // private variables
    private List<GravityRail> rails = new List<GravityRail>();

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AddTrackSegment(GravityRail rail)
    {
        rails.Add(rail);
    }

    public void RemoveTrackSegment(GravityRail rail)
    {
        rails.Remove(rail);
    }

    public List<GravityRail.RailInfo> GetRailInfoWithinDist(Vector2 pos, float dist)
    {
        List<GravityRail.RailInfo> railsInRange = new List<GravityRail.RailInfo>();
        // loop over each rail in the track and test if it is in range
        foreach (GravityRail rail in rails)
        {
            // get info about rail relative to the passed in position and add it if close enough
            GravityRail.RailInfo info = rail.GetRailInfoAtPoint(pos);
            if (info.alongRail && info.distance <= dist)
            {
                railsInRange.Add(info);
            }
        }
        return railsInRange;
    }

    public float GetRailSwitchRange()
    {
        return railSwitchRange;
    }

    public float GetParticlesPerUnitLength()
    {
        return PARTICLES_PER_UNIT_LENGTH;
    }
}
