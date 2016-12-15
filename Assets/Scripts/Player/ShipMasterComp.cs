using UnityEngine;
using System.Collections;
using UnityEngine.Networking;


[System.Serializable]
public struct PlayerPhysicsState
{
    // data pertaining to to location and movement of the player
    public Vector2 pos;
    public Vector2 vel;
    public Quaternion rotation;
    public float angularVel;

    public PlayerPhysicsState(Vector2 pos, Vector2 vel, Quaternion rotation, float angularVel)
    {
        this.pos = pos;
        this.vel = vel;
        this.rotation = rotation;
        this.angularVel = angularVel;
    }
}

[RequireComponent (typeof(Rigidbody2D))]
[RequireComponent(typeof(ShipSpeedCont))]
[RequireComponent(typeof(BaseShipInputCont))]
[RequireComponent(typeof(ShipSteeringCont))]
public class ShipMasterComp : NetworkBehaviour
{

    // components
    public Rigidbody2D rbody { get; private set; }
    public ShipSpeedCont speedCont { get; private set; }
    public BaseShipInputCont inputCont { get; private set; }
    public ShipSteeringCont steeringCont { get; private set; }
    // player info
    public int playerNum { get; private set; }

    // networking

    // the function prototype for the event callback that send the last location and movement of a player
    private delegate void ServerPhysicsStateDelegate(PlayerPhysicsState lastState);

    // the event object to register player state updates to
    // on channel 0 which is designated to state updates
    [SyncEvent(channel = 0)]
    private event ServerPhysicsStateDelegate EventPlayerPhysicsState;

    // client only: stores the last known state of the player on the server
    private PlayerPhysicsState lastPhysicsState;
    private bool receivedPhysicsState = false;

    // maximum offsets to snap
    [Header("SERVER SNAP VARIABLES")]
    public float SPEED_SNAP_PERCENT = 0.5f;
    public float SPEED_MIN_SNAP_DIFF = 5.0f;
    public float VEL_DIR_SNAP_DEG = 20.0f;
    public float ANGLE_SNAP_DEG = 90.0f;
    public float ANGULAR_VEL_SNAP_PERCENT = 0.5f;
    public float ANGULAR_VEL_SNAP_MIN_DIFF = 5.0f;
    public float POS_OFFSET_SNAP_DISTANCE = 2.0f;

    // have your local ship turn red so you know who you are
    public override void OnStartLocalPlayer()
    {
        GetComponent<MeshRenderer>().material.color = Color.red;
    }

    private void OnServerPlayerPhysicsState(PlayerPhysicsState lastState)
    {
        //Debug.Log("got physics state message");
        receivedPhysicsState = true;
        lastPhysicsState = lastState;
    }

    void Awake()
    {
        // components
        rbody = GetComponent<Rigidbody2D>();
        speedCont = GetComponent<ShipSpeedCont>();
        steeringCont = GetComponent<ShipSteeringCont>();
        inputCont = GetComponent<ShipControllerInput>();
    }

    void Start()
    {
        if (isClient)
        {
            // clients only
            // register to recieve state updates from the server
            EventPlayerPhysicsState += OnServerPlayerPhysicsState;
        }
    }

    void FixedUpdate()
    {
        // send the current physics state to all clients
        if (isServer)
        {

            // get the current phsyics state
            //Debug.Log("sending state");
            PlayerPhysicsState state = new PlayerPhysicsState(transform.position, rbody.velocity, transform.rotation, rbody.angularVelocity);
            EventPlayerPhysicsState(state);
        }
        if (isClient)
        {
            // attempt to sync the local physics state with the last state recieved by the server
            SyncStateWithServer();
        }
    }

    [Client]
    private void SyncStateWithServer()
    {
        //Debug.Log("vel before sync: " + rbody.velocity);
        if (!receivedPhysicsState) return;

        // lerp all physics aspest
        rbody.velocity = Vector2.Lerp(rbody.velocity, lastPhysicsState.vel, 0.1f);
        rbody.angularVelocity = Mathf.Lerp(rbody.angularVelocity, lastPhysicsState.angularVel, 0.1f);
        transform.position = Vector2.Lerp(transform.position, lastPhysicsState.pos, 0.1f);
        transform.rotation = Quaternion.Lerp(transform.rotation, lastPhysicsState.rotation, 0.1f);

        // if still extremely different than server state, snap to the server state
        
        /*
        // distance snap
        if(((Vector2)transform.position - lastPhysicsState.pos).magnitude >= POS_OFFSET_SNAP_DISTANCE)
        {
            transform.position = lastPhysicsState.pos;
        }
        // rotation snap
        if(Mathf.Abs(Quaternion.Angle(transform.rotation, lastPhysicsState.rotation)) >= ANGLE_SNAP_DEG)
        {
            transform.rotation = lastPhysicsState.rotation;
        }
        // angular velocity snap
        if (Mathf.Abs(rbody.angularVelocity - lastPhysicsState.angularVel) >= ANGULAR_VEL_SNAP_MIN_DIFF &&  
            (rbody.angularVelocity < lastPhysicsState.angularVel && ((lastPhysicsState.angularVel - rbody.angularVelocity) / lastPhysicsState.angularVel) >= ANGULAR_VEL_SNAP_PERCENT) ||
            (rbody.angularVelocity > lastPhysicsState.angularVel && ((rbody.angularVelocity - lastPhysicsState.angularVel) / lastPhysicsState.angularVel) >= ANGULAR_VEL_SNAP_PERCENT) )
        {
            rbody.angularVelocity = lastPhysicsState.angularVel;
        }
        // speed snap
        float currSpeed = rbody.velocity.magnitude;
        float lastSpeed = lastPhysicsState.vel.magnitude;
        if(Mathf.Abs(currSpeed - lastSpeed) >= SPEED_MIN_SNAP_DIFF &&
            (currSpeed < lastSpeed && ((lastSpeed - currSpeed)/lastSpeed >= SPEED_SNAP_PERCENT)) ||
            (lastSpeed < currSpeed && ((currSpeed - lastSpeed) / lastSpeed >= SPEED_SNAP_PERCENT)) ) {
            rbody.velocity = rbody.velocity.normalized * lastSpeed;
        }
        // snap velocity direction
        float snapDotVal = Mathf.Cos(Mathf.Deg2Rad * VEL_DIR_SNAP_DEG);
        if(Vector2.Dot(rbody.velocity.normalized, lastPhysicsState.vel.normalized) <= snapDotVal)
        {
            rbody.velocity = lastPhysicsState.vel.normalized * rbody.velocity.magnitude;
        }
        */
        //Debug.Log("vel after sync: " + rbody.velocity);
    }

    public static ShipMasterComp GetShipMasterComp(GameObject obj)
    {
        return obj.transform.root.GetComponent<ShipMasterComp>();
    }

}
