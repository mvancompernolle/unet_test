using UnityEngine;
using System.Collections;
using UnityEngine.Networking;


[System.Serializable]
public struct PlayerPhysicsState
{
    Vector2 pos;
    Vector2 vel;
    Quaternion rotation;
    float angularVel;

    public PlayerPhysicsState(Vector2 pos, Vector2 vel, Quaternion rotation, float angularVel)
    {
        this.pos = pos;
        this.vel = vel;
        this.rotation = rotation;
        this.angularVel = angularVel;
    }

    public void Apply(Rigidbody2D rbody)
    {
        rbody.transform.position = pos;
        rbody.velocity = vel;
        rbody.transform.rotation = rotation;
        rbody.angularVelocity = angularVel;
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
    private delegate void ServerPhysicsStateDelegate(PlayerPhysicsState lastState);
    [SyncEvent(channel = 0)]
    private event ServerPhysicsStateDelegate EventPlayerPhysicsState;

    public override void OnStartLocalPlayer()
    {
        GetComponent<MeshRenderer>().material.color = Color.red;
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
        // networking
        if (isClient)
        {
            EventPlayerPhysicsState += OnServerPlayerPhysicsState;
        }
    }

    void FixedUpdate()
    {
        // send the current physics state to all clients
        if (isServer)
        {
            // get the current phsyics state
            Debug.Log("sending vel: " + rbody.velocity);
            PlayerPhysicsState state = new PlayerPhysicsState(transform.position, rbody.velocity, transform.rotation, rbody.angularVelocity);
            EventPlayerPhysicsState(state);
        }
    }

    public static ShipMasterComp GetShipMasterComp(GameObject obj)
    {
        return obj.transform.root.GetComponent<ShipMasterComp>();
    }

    private void OnServerPlayerPhysicsState(PlayerPhysicsState lastState)
    {
        //Debug.Log("updated physics state recieved");
        lastState.Apply(rbody);
    }

}
