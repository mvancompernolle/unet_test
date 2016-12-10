using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public struct ShipInputs
{
    public struct Button
    {
        public bool isPressed;
        public bool wasPressed;
        public bool wasReleased;
    }

    // I seperated the button names and action names
    // for easier use. Using just one or the other made it
    // "harder" to use, or "harder" to rebind.

    // Esme Allignment Syndrome (EAS), lol.
    public float grind;
    public Button warp;
    public Button join;
    public Button boost;
    public Button pause;
    public Button start;
    public Button special;
    public Button flipShot;
    public Vector2 direction;
};

public abstract class BaseShipInputCont : NetworkBehaviour
{

    public ShipInputs shipInputs;

    // Update is called once per frame
    public abstract void Update();
}
