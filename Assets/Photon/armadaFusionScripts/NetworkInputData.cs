using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public Vector2 movementInput;
    public NetworkBool IsRunning;
    public NetworkBool isJumpPressed;
}
