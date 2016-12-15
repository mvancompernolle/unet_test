﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ShipSpeedCont : NetworkBehaviour
{
    // speed variables
    public float maxSpeed = 40.0f;
    [SerializeField]
    private float acceleration = 12.0f;
    public Vector2 lastVel { get; private set; }
    public float lastAngularVel { get; private set; }

    // components
    private ShipMasterComp masterComp;

    // input
    private Vector2 targetDir;

    void Awake()
    {
        // get component references
        masterComp = GetComponent<ShipMasterComp>();
    }

    // Input related update
    void Update()
    {
        if (!isLocalPlayer) return;

        Debug.Log("speed: " + masterComp.inputCont.shipInputs.direction);

        // get direction of the left stick
        targetDir.x = masterComp.inputCont.shipInputs.direction.x;
        targetDir.y = masterComp.inputCont.shipInputs.direction.y;

        // save last velocity
        CapSpeed();
        lastVel = masterComp.rbody.velocity;
        lastAngularVel = masterComp.rbody.angularVelocity;
    }

    // Physics related update
    void FixedUpdate()
    {
        //if (!isLocalPlayer) return;

        // if left stick is pointing, accelerate
        if ((Mathf.Abs(targetDir.x) >= 0.5f || Mathf.Abs(targetDir.y) >= 0.5f))
        {
            masterComp.rbody.AddForce(masterComp.steeringCont.GetCurrentDirection() * acceleration * ((targetDir.magnitude - 0.5f) * 2.0f));
        }
        lastVel = masterComp.rbody.velocity;
        lastAngularVel = masterComp.rbody.angularVelocity;
    }

    private void CapSpeed()
    {
        // update all speed adjustors and get their current speed left
        float capSpeed = maxSpeed;
        // cap the max speed of the ship
        SetVelocity(masterComp.rbody.velocity.normalized * Mathf.Min(masterComp.rbody.velocity.magnitude, capSpeed));
        //Debug.Log("max speed: " + capSpeed + " curr speed: " + masterComp.rbody.velocity.magnitude);
    }

    public void SetVelocity(Vector2 velocity)
    {
        masterComp.rbody.velocity = velocity;
        lastVel = velocity;
    }

    public Vector2 GetVelocity()
    {
        return masterComp.rbody.velocity;
    }

    public float GetMaxSpeed()
    {
        return maxSpeed;
    }

    public void SetMaxSpeed(float speed)
    {
        maxSpeed = speed;
    }

    public float GetSpeed()
    {
        return masterComp.rbody.velocity.magnitude;
    }
}
