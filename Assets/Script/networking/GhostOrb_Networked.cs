using UnityEngine;
using Fusion;

// This script serves as both the "logic" (visual rotation) and the network "tag".
// Because its responsibilities are so simple, we don't need a separate "motor" script.
[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(Collider))] // Ensure it can be triggered
public class GhostOrb_Networked : NetworkBehaviour
{
    [Header("Data")]
    public int orbID; // Kept from your original script in case you need it for puzzle logic.

    [Header("Visuals")]
    public float rotateSpeed = 50f;

    // We use Update() for the rotation because it's a purely visual effect
    // that doesn't need to be perfectly in sync across the network.
    // This code will run on the host and all clients independently.
    private void Update()
    {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime, Space.World);
    }
}