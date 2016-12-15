using UnityEngine;
using System.Collections.Generic;
using InControl;
using UnityEngine.Networking;

public class ShipActions : PlayerActionSet
{
    // public variables
    public PlayerAction warp;
    public PlayerAction special;
    public PlayerAction boost;
    public PlayerAction flipShot;
    public PlayerAction up;
    public PlayerAction down;
    public PlayerAction left;
    public PlayerAction right;
    public PlayerAction grind;
    public PlayerAction join;
    public PlayerAction start;
    public PlayerTwoAxisAction movement;

    // private variables

    public ShipActions()
    {
        flipShot = CreatePlayerAction("FlipShot");
        special = CreatePlayerAction("Special");
        boost = CreatePlayerAction("Boost");
        warp = CreatePlayerAction("Warp");
        grind = CreatePlayerAction("Grind");
        join = CreatePlayerAction("Join");
        start = CreatePlayerAction("Start");

        up = CreatePlayerAction("Up");
        down = CreatePlayerAction("Down");
        left = CreatePlayerAction("Left");
        right = CreatePlayerAction("Right");
        movement = CreateTwoAxisPlayerAction(left, right, down, up);
    }

    public static ShipActions CreateWithControllerBindings()
    {
        var actions = new ShipActions();

        actions.flipShot.AddDefaultBinding(InputControlType.Action1);
        actions.special.AddDefaultBinding(InputControlType.Action2);
        actions.boost.AddDefaultBinding(InputControlType.Action3);
        actions.warp.AddDefaultBinding(InputControlType.Action4);
        actions.grind.AddDefaultBinding(InputControlType.RightTrigger);
        actions.join.AddDefaultBinding(InputControlType.Action1);
        actions.start.AddDefaultBinding(InputControlType.Start);

        actions.up.AddDefaultBinding(InputControlType.LeftStickUp);
        actions.down.AddDefaultBinding(InputControlType.LeftStickDown);
        actions.left.AddDefaultBinding(InputControlType.LeftStickLeft);
        actions.right.AddDefaultBinding(InputControlType.LeftStickRight);

        return actions;
    }
}

[NetworkSettings(sendInterval = 0.3f)]
public class ShipControllerInput : BaseShipInputCont
{

    public bool inputIsAttached { get; private set; }
    public ShipActions actions { get; private set; }
    public static List<InputDevice> devicesInUse = new List<InputDevice>();
    Timer cmdTimer = new Timer(0.33f, true);

    void Start()
    {
        actions = ShipActions.CreateWithControllerBindings();
    }

    public void SetInput(InputDevice device)
    {
        if (device != null)
        {
            actions.Device = device;
            inputIsAttached = true;
        }
        else
        {
            inputIsAttached = false;
        }
    }

    // Update is called once per frame
    public override void Update()
    {
        // if not local player, ignore an controller inputs!!!!
        if (!isLocalPlayer) return;

        // check for controller assignment to the player
        if (!inputIsAttached)
        {
            // check for an available device
            foreach (InputDevice device in InputManager.Devices)
            {
                if (device.Action1.WasPressed && !devicesInUse.Contains(device))
                {
                    SetInput(device);
                    break;
                }
            }
        }

        // if you have an attached controller
        if (inputIsAttached)
        {
            // get inputs from the controller and save them
            shipInputs.warp.isPressed = actions.warp.IsPressed;
            shipInputs.boost.isPressed = actions.boost.IsPressed;
            shipInputs.special.isPressed = actions.special.IsPressed;
            shipInputs.flipShot.isPressed = actions.flipShot.IsPressed;
            shipInputs.start.isPressed = actions.start.IsPressed;

            shipInputs.warp.wasPressed = actions.warp.WasPressed;
            shipInputs.boost.wasPressed = actions.boost.WasPressed;
            shipInputs.special.wasPressed = actions.special.WasPressed;
            shipInputs.flipShot.wasPressed = actions.flipShot.WasPressed;
            shipInputs.start.wasPressed = actions.start.WasPressed;

            shipInputs.warp.wasReleased = actions.warp.WasReleased;
            shipInputs.boost.wasReleased = actions.boost.WasReleased;
            shipInputs.special.wasReleased = actions.special.WasReleased;
            shipInputs.flipShot.wasReleased = actions.flipShot.WasReleased;
            shipInputs.start.wasReleased = actions.start.WasReleased;

            shipInputs.grind = actions.grind.Value;
            shipInputs.direction.x = actions.movement.X;
            shipInputs.direction.y = actions.movement.Y;

            // send inputs from controller to the server
            if (cmdTimer.Update(Time.deltaTime))
            {
                CmdUploadInput(shipInputs);
                cmdTimer.Activate();
            }
            //CmdUploadInput(shipInputs);
        }
    }

    [Command(channel = 1)]
    private void CmdUploadInput(ShipInputs inputs)
    {
        // server recieves inputs from client
        shipInputs = inputs;
        if (!isLocalPlayer) Debug.Log("cmd recieved");
    }
}
