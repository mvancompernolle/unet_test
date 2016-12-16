using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[NetworkSettings(channel = 2, sendInterval = 0.333f)]
public class GameMode : NetworkBehaviour
{
    // Accessor for the singleton representing the current GameMode.
    public static GameMode singleton { get; private set; }

    // Time since match started given in seconds.
    //  - Synchronized via RPCs from the server.
    public float gameTime;

    [ClientRpc]
    void RpcSyncTime(float time)
    {
        gameTime = time;
    }

    //
    // Unity Events
    //
    void Start()
    {
        if (singleton == null) { singleton = this; }
    }

    void Update()
    {
        gameTime += Time.deltaTime;

        if (isServer) { RpcSyncTime(gameTime); }
    }

    void OnDestroy()
    {
        if (singleton == this) singleton = null;
    }
}