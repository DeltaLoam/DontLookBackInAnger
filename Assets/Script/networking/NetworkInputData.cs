using Fusion;
using UnityEngine;

// An enum is an efficient way to send button states.
public enum MyButtons
{
    Jump,
    Sprint,
    Crouch
}

// This struct defines the data we send from client to server.
public struct NetworkInputData : INetworkInput
{
    public Vector2 moveInput;
    public Vector2 lookDelta;
    public NetworkButtons buttons; // Fusion's special type for sending button data
}