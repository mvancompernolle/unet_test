using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

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

    public override void OnStartLocalPlayer()
    {
        GetComponent<MeshRenderer>().material.color = Color.red;
    }

    void Awake()
    {
        rbody = GetComponent<Rigidbody2D>();
        speedCont = GetComponent<ShipSpeedCont>();
        steeringCont = GetComponent<ShipSteeringCont>();
        inputCont = GetComponent<ShipControllerInput>();
    }

    public static ShipMasterComp GetShipMasterComp(GameObject obj)
    {
        return obj.transform.root.GetComponent<ShipMasterComp>();
    }

}
