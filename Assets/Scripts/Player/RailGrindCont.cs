using UnityEngine;
using System.Collections.Generic;

public class RailGrindCont : MonoBehaviour
{
    // rail grab variables
    private float maxGrabAngle = 90.0f;
    private float maxGrabDist = 3.0f;
    private float restrictedGrabAngle = 20.0f;

    // private variables
    private ShipMasterComp masterComp;
    private GravityTrack track;
    private GravityRail.RailInfo currentRailInfo = null;
    private PID distanceController;
    public bool noRailsInRange { get; private set; }
    private bool grindReleased = true;
    private uint framesToGrab = 3;
    private uint grabFramesLeft = 0;
    private float maxGrabDot;
    private float restrictedGrabDot;
    private Timer grabCoolDown = new Timer(0.25f, true);
    private float sharpSwapAngle = 60.0f;
    private float acceptableSwitchVal;
    public LogicLocker grindLockers { get; private set; }

    // rail jump variables
    public float railJumpPercent { get; private set; }
    private float railJumpBonus = 0.0f;
    private float railJumpSpeed = 45.0f;
    private Timer railJumpTimer = new Timer(0.15f);
    private float defaultRailJumpTime;
    private float railJumpTimeToCharge = 1.5f;
    private bool canGetGrabBonus = false;
    private string JUMP_SPEED_ADJUSTER_NAME = "rail jump";

    // rail grab movement bonuses
    private string ATTACHED_ADJUSTER_NAME = "attached to rail";
    //[SerializeField]
    private float ON_RAIL_BONUS_SPEED = 10.0f;
    //[SerializeField]
    private float ON_RAIL_BONUS_ACC = 25.0f;
    private float MAX_ALLIGNMENT_BONUS_SPEED = 35.0f;
    private float ALIGNMENT_SPEED_CHANGE_RATE = 10.0f;
    private float currAlignmentSpeedBonus = 0.0f;

    void Awake()
    {
        // get component references
        masterComp = GetComponent<ShipMasterComp>();
        track = GameObject.FindGameObjectWithTag("Track").GetComponent<GravityTrack>();

        // init distance PID controller
        distanceController = new PID("distance controller");
        distanceController.Kp = 300.0f;
        distanceController.Ki = 0.0f;
        distanceController.Kd = 50.0f;

        // init other data
        maxGrabDot = Mathf.Cos(maxGrabAngle * Mathf.Deg2Rad);
        restrictedGrabDot = Mathf.Cos(restrictedGrabAngle * Mathf.Deg2Rad);
        acceptableSwitchVal = Mathf.Cos(sharpSwapAngle * Mathf.Deg2Rad);
        defaultRailJumpTime = railJumpTimer.timerStart;

        railJumpPercent = 0.0f;
        noRailsInRange = false;

        // tag adjustor names
        grindLockers = new LogicLocker();
        JUMP_SPEED_ADJUSTER_NAME += gameObject.GetInstanceID();
        ATTACHED_ADJUSTER_NAME += gameObject.GetInstanceID();
    }

    // Update is called once per frame
    void Update()
    {
        if (grindLockers.IsLocked()) return;

        // get all of the gravity rails in range
        float grabDist = maxGrabDist;//!masterComp.steeringCont.IsStunned() ? maxGrabDist : maxGrabDist * 0.75f;
        List<GravityRail.RailInfo> railsInRange = track.GetRailInfoWithinDist(transform.position, grabDist);

        // see if grind is released
        if (masterComp.inputCont.shipInputs.grind < 0.25f)
        {
            grindReleased = true;
            grabFramesLeft = 0;

            if (currentRailInfo != null)
            {
                // launch the player off the rail and detach
                LaunchOffRail();
                // start rail jump slow timer
                railJumpTimer.Activate();
            }
        }
        else
        {
            // set grab frames left to 0 if stunned to make it harder to grab when stunned
            //if (masterComp.steeringCont.IsStunned()) { grabFramesLeft = 0; }

            // grab is pressed if block is entered
            /*
            Attempt to grab a rail if:
            1. No currently attached to a rail
            2.  - Grab is not on cooldown and grind was released last frame
                - There are grab frames left from attempting to grab in prev frame
            */
            if (currentRailInfo == null
               && ((!grabCoolDown.isActive && grindReleased) || grabFramesLeft > 0))
            {
                AttemptToGrabRail(railsInRange);
            }

            // set grab on cooldown if just pressed
            if (grindReleased && !grabCoolDown.isActive) { grabCoolDown.Activate(); }

            // save that grind has been pressed
            grindReleased = false;
        }

        // save whether not in range of any tracks
        noRailsInRange = railsInRange.Count == 0;
        if (noRailsInRange) canGetGrabBonus = true;

        // tick grab cooldown
        grabCoolDown.Update(Time.deltaTime);

        // update jump cooldown
        if (railJumpTimer.isActive)
        {
            if (railJumpTimer.Update(Time.deltaTime))
            {
                masterComp.speedCont.maxSpeedAdjustors.RemoveAdjustor(JUMP_SPEED_ADJUSTER_NAME);
            }
        }
    }

    public void FixedUpdate()
    {
        if (grindLockers.IsLocked()) return;

        // apply forces to keep ship attached to rail
        if (IsGrinding())
        {
            // update rail info
            // get all of the gravity rails in range
            List<GravityRail.RailInfo> railsInRange = track.GetRailInfoWithinDist(transform.position, maxGrabDist);
            UpdateRailGrinding(railsInRange);

            if (IsGrinding())
            {
                // get direction to the rail
                Vector2 dirToRail = currentRailInfo.closestPoint - (Vector2)transform.position;
                float distanceCorrectionForce = distanceController.GetOutput(dirToRail.magnitude, Time.fixedDeltaTime);
                masterComp.rbody.AddForce(dirToRail.normalized * distanceCorrectionForce);

                // when grinding, increase decrease top speed based on alignment
                float dotVal = Mathf.Abs(Vector2.Dot(masterComp.steeringCont.GetCurrentDirection(), currentRailInfo.tangent));
                bool movingForward = Vector2.Dot(masterComp.speedCont.GetVelocity().normalized, masterComp.steeringCont.GetCurrentDirection()) > 0.0f;
                float threshold = Mathf.Cos(25.0f * Mathf.Deg2Rad);
                if (movingForward && dotVal >= threshold)
                {
                    // increase ships max speed
                    float percent = (dotVal - threshold) / (1.0f - threshold);
                    currAlignmentSpeedBonus += percent * ALIGNMENT_SPEED_CHANGE_RATE * Time.fixedDeltaTime;

                }
                else if (movingForward)
                {
                    // decrease ships max speed
                    float percent = (threshold - dotVal) / threshold;
                    currAlignmentSpeedBonus -= percent * ALIGNMENT_SPEED_CHANGE_RATE * Time.fixedDeltaTime;
                }
                else
                {
                    currAlignmentSpeedBonus -= ALIGNMENT_SPEED_CHANGE_RATE * Time.fixedDeltaTime;
                }
                currAlignmentSpeedBonus = Mathf.Clamp(currAlignmentSpeedBonus, 0.0f, MAX_ALLIGNMENT_BONUS_SPEED);
                masterComp.speedCont.maxSpeedAdjustors.SetAdjustor(ATTACHED_ADJUSTER_NAME, ON_RAIL_BONUS_SPEED + currAlignmentSpeedBonus);
            }
        }
        else
        {
            currAlignmentSpeedBonus = Mathf.Max(currAlignmentSpeedBonus - ALIGNMENT_SPEED_CHANGE_RATE * Time.fixedDeltaTime, 0.0f);
        }
    }

    private void AttemptToGrabRail(List<GravityRail.RailInfo> railsInRange)
    {
        // iterate over rails in range and find the one most aligned with
        float bestDot = 0.0f;
        float grabDot = maxGrabDot; //(masterComp.steeringCont.IsStunned() || masterComp.warpCont.IsWarping()) ? restrictedGrabDot : maxGrabDot;
        Vector2 forward = masterComp.steeringCont.GetCurrentDirection();
        foreach (GravityRail.RailInfo railInfo in railsInRange)
        {
            float dotVal = Mathf.Abs(Vector2.Dot(forward, railInfo.tangent));
            if (dotVal >= grabDot && dotVal > bestDot)
            {
                // set as current rail if better aligned than current best
                bestDot = dotVal;
                currentRailInfo = railInfo;
            }
        }

        // if we found a rail to grab
        if (bestDot > 0.0f)
        {
            distanceController.Reset();

            // keep speed based on how well timed the grab was
            Vector2 direction = Vector2.Dot(forward, currentRailInfo.tangent) > 0.0f ? currentRailInfo.tangent : -currentRailInfo.tangent;
            long numerator = (grabFramesLeft + 1) - (grabFramesLeft == 0 ? 1 : 0);
            float bonusFromSkill = (0.4f * (1.0f - numerator / ((float)framesToGrab + 1)));
            masterComp.speedCont.SetVelocity(direction * masterComp.speedCont.GetSpeed() * (0.5f + bonusFromSkill));

            // charge up rail jump based on how skilled the rail grab was
            if (canGetGrabBonus)
            {
                canGetGrabBonus = false;
                railJumpPercent = ((currentRailInfo.distance) / maxGrabDist) <= 0.5f ? 1.0f : 0.0f;
            }

            grabFramesLeft = 0;

            // increase max speed and acceleration of the ship
            masterComp.speedCont.maxSpeedAdjustors.SetAdjustor(ATTACHED_ADJUSTER_NAME, ON_RAIL_BONUS_SPEED);
            masterComp.speedCont.accAdjustors.SetAdjustor(ATTACHED_ADJUSTER_NAME, ON_RAIL_BONUS_ACC);

            // remove stun if grabbing while stunned
            //masterComp.steeringCont.RemoveStun();
        }
        else
        {
            Detach();
            // track grab frames left if failed to grab rail
            if (grabFramesLeft == 0) { grabFramesLeft = framesToGrab; }
            else { --grabFramesLeft; }
        }
    }

    private void UpdateRailGrinding(List<GravityRail.RailInfo> railsInRange)
    {
        // test if still in range of the last rail attached to
        GravityRail.RailInfo updatedRailInfo = currentRailInfo.rail.GetRailInfoAtPoint(transform.position);
        Vector2 forward = masterComp.steeringCont.GetCurrentDirection();
        float bestDot = 0.0f;
        GravityRail.RailInfo bestRailInfo = null;

        // if still on current rail, ignore tracks not in rail switch range
        bool usingRailSwitchRange = updatedRailInfo.alongRail && updatedRailInfo.distance <= maxGrabDist;
        if (usingRailSwitchRange)
        {
            railsInRange = track.GetRailInfoWithinDist(updatedRailInfo.closestPoint, track.GetRailSwitchRange());
        }

        // get the best rail to stay on
        foreach (GravityRail.RailInfo railInfo in railsInRange)
        {
            float dotVal = Mathf.Abs(Vector2.Dot(forward, railInfo.tangent));
            if (dotVal >= bestDot)
            {
                float velDotVal = Mathf.Abs(Vector2.Dot(masterComp.rbody.velocity.normalized, railInfo.tangent));
                // only switch to rail at nearly perpendicular angle to momentum if going slow
                if (velDotVal > acceptableSwitchVal)
                {
                    bestDot = dotVal;
                    bestRailInfo = railInfo;
                }
                else
                {
                    // ensure that player can do sharp rail switch if going slow
                    float shipMomentum = masterComp.rbody.mass * masterComp.speedCont.GetSpeed();
                    // half of the max momentum
                    float maxSwitchMomentum = (masterComp.rbody.mass * masterComp.speedCont.GetMaxSpeed()) * 0.5f;
                    // half of the max switch momentum
                    float minSwitchMomentum = maxSwitchMomentum * 0.5f;
                    float anglePercent = velDotVal / acceptableSwitchVal;
                    // only allow switch if momentum is less than max amount allowed for a rail switch
                    if (shipMomentum < (minSwitchMomentum + (maxSwitchMomentum - minSwitchMomentum) * anglePercent))
                    {
                        bestDot = dotVal;
                        bestRailInfo = railInfo;
                    }
                }
            }
        }

        // either found a rail to switch to or a new rail on the track to grab
        if (bestRailInfo != null)
        {
            //Debug.Log("rail found");
            if (bestRailInfo.rail != currentRailInfo.rail)
            {
                distanceController.Reset();
            }
            currentRailInfo = bestRailInfo;

            // charge up rail jump
            if (railJumpPercent < 1.0f)
            {
                railJumpPercent += Time.fixedDeltaTime / railJumpTimeToCharge;
                railJumpPercent = Mathf.Min(railJumpPercent, 1.0f);
            }
        }
        // else is only reached if there was no rail to switch to and current rail isn't within rail switch range
        else
        {
            if (!usingRailSwitchRange)
            {
                // if no rail in range, detach from rail
                Detach();
            }
            else
            {
                // if in rail switch range and no rail in range found, stay on current rail
                currentRailInfo = updatedRailInfo;
            }
        }

    }

    private void LaunchOffRail()
    {
        // get how much player is facing the direction of their velocity
        Vector2 forward = masterComp.steeringCont.GetCurrentDirection();
        float velDot = Vector2.Dot(masterComp.rbody.velocity.normalized, currentRailInfo.tangent);
        Vector2 velDir = currentRailInfo.tangent * (velDot > 0.0f ? 1 : -1);
        float forwardDot = Vector2.Dot(forward, velDir);

        // keep full speed if leaping forward, keep half speed if leaping backward
        forwardDot = forwardDot >= 0.0f ? 1.0f : 0.5f;
        railJumpBonus = railJumpSpeed + currAlignmentSpeedBonus;
        masterComp.speedCont.SetVelocity(forward * (forwardDot * masterComp.speedCont.GetSpeed() + railJumpBonus));
        masterComp.speedCont.maxSpeedAdjustors.SetAdjustor(JUMP_SPEED_ADJUSTER_NAME, railJumpBonus);
        railJumpTimer.timerStart = defaultRailJumpTime;
        railJumpTimer.Deactivate();

        // detach from current rail
        Detach();
    }

    public void Detach()
    {
        currentRailInfo = null;
        railJumpPercent = 0.0f;

        // remove on rail movement bonuses
        masterComp.speedCont.maxSpeedAdjustors.RemoveAdjustor(ATTACHED_ADJUSTER_NAME);
        masterComp.speedCont.accAdjustors.RemoveAdjustor(ATTACHED_ADJUSTER_NAME);
    }

    public bool IsGrinding()
    {
        return currentRailInfo != null;
    }
}
