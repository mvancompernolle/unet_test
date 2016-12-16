using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[System.Serializable]
public struct Button
{
    public bool isPressed;
    public bool wasPressed;
    public bool wasReleased;

    /*
    public override bool Equals(object obj)
    {
        if (!(obj is Button)) return false;
        Button button = (Button)obj;
        return isPressed == button.isPressed && wasPressed == button.wasPressed && wasReleased == button.wasReleased;
    }
    */
    public static bool operator ==(Button lhs, Button rhs)
    {
        return lhs.isPressed == rhs.isPressed && lhs.wasReleased == rhs.wasReleased && lhs.wasReleased == rhs.wasReleased;
    }

    public static bool operator !=(Button lhs, Button rhs)
    {
        return lhs.isPressed != rhs.isPressed || lhs.wasReleased != rhs.wasReleased || lhs.wasReleased != rhs.wasReleased;
    }
}

[System.Serializable]
public struct ShipInputs
{

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

    /*
    public override bool Equals(object obj)
    {
        if (!(obj is ShipInputs)) return false;

        ShipInputs inputs = (ShipInputs)obj;
        return boost == inputs.boost;
    }
    */

    public static bool operator ==(ShipInputs lhs, ShipInputs rhs)
    {
        return lhs.boost == rhs.boost;
    }

    public static bool operator !=(ShipInputs lhs, ShipInputs rhs)
    {
        return lhs.boost != rhs.boost;
    }
};

public abstract class BaseShipInputCont : NetworkBehaviour
{

    public ShipInputs shipInputs;

    // Update is called once per frame
    public abstract void Update();
}
