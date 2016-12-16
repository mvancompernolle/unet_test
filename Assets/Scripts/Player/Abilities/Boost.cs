using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Boost : AbilityBase {
    enum BoostState { READY, BOOSTING }

    // boost variables
    [SerializeField]
    private float boostSpeed = 22.5f;
    [SerializeField]
    private float boostDeaccTime = 1.0f;
    private float bonusMass = 1.0f;
    private float speedWhenBoostStarted;
    private bool boostPressed;
    private bool speedAdjustorOn = false;
    private const string BOOST_ADJUSTER_NAME = "boost";
    private Vector2 boostDirection;
    private BoostState state = BoostState.READY;

    /*
    // the function prototype for the event callback that send the last location and movement of a player
    private delegate void ServerBoostUpdate();

    // the event object to register player state updates to
    // on channel 0 which is designated to state updates
    [SyncEvent(channel = 3)]
    private event ServerBoostUpdate EventBoostUpdate;

    private void OnServerBoostUpdate()
    {
        Debug.Log("received server boost update");
    }
    */

    // Use this for initialization
    void Start()
    {
        // get component references
        coolDownTimer.Deactivate();
    }

    // Update is called once per frame (For input)
    public override void Update()
    {
        base.Update();

        switch (state)
        {
            case BoostState.READY:
                if (actions.boost.wasPressed && AttemptToUseAbility())
                {
                    StartBoost();
                }
                break;
            case BoostState.BOOSTING:
                if (speedAdjustorOn && coolDownTimer.timePassed < boostDeaccTime)
                {
                    
                    float velInBoostDir = Vector2.Dot(boostDirection, masterComp.speedCont.GetVelocity());
                    if (velInBoostDir > speedWhenBoostStarted)
                    {
                        // set and velocity
                        float slowAmount = (boostSpeed / boostDeaccTime) * Time.deltaTime;
                        slowAmount = (velInBoostDir - slowAmount < speedWhenBoostStarted ? velInBoostDir - speedWhenBoostStarted : slowAmount);
                        masterComp.speedCont.SetVelocity(masterComp.speedCont.GetVelocity() + (-boostDirection * slowAmount));
                    }

                    // set mass
                    float currMassBonus = bonusMass * (boostDeaccTime - coolDownTimer.timePassed) / boostDeaccTime;
                    masterComp.speedCont.SetMassAdjustor(BOOST_ADJUSTER_NAME, currMassBonus);
                    

                }
                // remove the speed adjustor after slowing down
                else
                {
                    StopBoost();
                }
                break;
        }

        // get boost direction input
        boostPressed = actions.boost.wasPressed;
    }

    private void StopBoost()
    {
        speedAdjustorOn = false;
        //masterComp.speedCont.maxSpeedAdjustors.RemoveAdjustor(BOOST_ADJUSTER_NAME);
        masterComp.speedCont.RemoveMassAdjustor(BOOST_ADJUSTER_NAME);
        state = BoostState.READY;
    }

    public void StartBoost()
    {
        if (!disabled)
        {
            if (state == BoostState.BOOSTING)
                StopBoost();

            state = BoostState.BOOSTING;

            speedAdjustorOn = true;
            
            // determine adjusted direction of the ram
            boostDirection = transform.up;
            coolDownTimer.Activate();

            // apply boost to ship and increase speed cap
            speedWhenBoostStarted = masterComp.speedCont.GetVelocity().magnitude * Vector2.Dot(boostDirection, masterComp.speedCont.GetVelocity().normalized);
            masterComp.speedCont.SetVelocity(masterComp.speedCont.GetVelocity() + boostDirection * boostSpeed);
            masterComp.speedCont.maxSpeedAdjustors.SetAdjustor(BOOST_ADJUSTER_NAME, boostSpeed);
            masterComp.speedCont.SetMassAdjustor(BOOST_ADJUSTER_NAME, bonusMass);
        }
    }

    public override void Cancel()
    {
        // cancel if on
        if (speedAdjustorOn)
        {
            StopBoost();
        }
    }
}
