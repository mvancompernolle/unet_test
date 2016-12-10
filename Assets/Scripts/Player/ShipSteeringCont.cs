using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class ShipSteeringCont : NetworkBehaviour
{
    // turning variables
    public float maxTorque = 200.0f;
    public PID pidAngle = new PID("Angle Controller");
    public PID pidAngularVelocity = new PID("Angular Velocity Controller");

    // components
    private ShipMasterComp masterComp;
    private Vector2 targetDir;

    void Awake()
    {
        // get component references
        masterComp = GetComponent<ShipMasterComp>();
    }

    // Use this for initialization
    void Start()
    {

    }

    // Input related update
    void Update()
    {
        if (!isLocalPlayer) return;

        // get direction of the left stick
        targetDir.x = masterComp.inputCont.shipInputs.direction.x;
        targetDir.y = masterComp.inputCont.shipInputs.direction.y;
    }

    // Physics related update
    void FixedUpdate()
    {
        if (!isLocalPlayer) return;

        // effect steering if left stick is directed
        if ((Mathf.Abs(targetDir.x) >= 0.2f || Mathf.Abs(targetDir.y) >= 0.2f))
        {
            AimDirection(targetDir, Time.fixedDeltaTime);
        }
    }

    public float GetCurrentAngle(bool returnDegrees = false)
    {
        if (returnDegrees)
            return (transform.eulerAngles.z + 90.0f);
        return (transform.eulerAngles.z + 90.0f) * Mathf.Deg2Rad;
    }

    public Vector2 GetCurrentDirection()
    {
        float currentAngle = GetCurrentAngle();
        return new Vector2(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle));
    }

    public void AimDirection(Vector2 dir, float dt)
    {
        dir = dir.normalized;
        float targetAngle = (Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg) - 90;

        // Drive the ship's angle towards the target angle
        float angleError = Mathf.DeltaAngle(transform.eulerAngles.z, targetAngle);
        float torqueCorrectionForAngle = pidAngle.GetOutput(angleError, dt);

        // Drive the ship's angular velocity to 0
        float angularVelocityError = -masterComp.rbody.angularVelocity;
        float torqueCorrectionForAngularVelocity = pidAngularVelocity.GetOutput(angularVelocityError, dt);

        // Apply both corrections to the ship to force it to the target angle and attempt to hold it there
        float torque = torqueCorrectionForAngle + torqueCorrectionForAngularVelocity;
        torque = Mathf.Clamp(torque, -maxTorque, maxTorque);
        masterComp.rbody.AddTorque(torque, ForceMode2D.Force);
    }
}
