using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;

// Minimal stub for Fusion.Changed<T> to avoid compile errors when the Fusion package or type
// is not available in the current editor/assembly references; this stub provides only the
// methods and property used by this file so the code can compile — it does not implement
// actual Fusion networking behavior and should be removed once the proper Fusion package
// is referenced.
public struct Changed<T>
{
    // The Behaviour instance (may be default/null if Fusion isn't present).
    public T Behaviour { get; private set; }

    // No-op placeholders to match the expected API surface used in this file.
    public void LoadOld() { }
    public void LoadNew() { }
}

// This is the "Puppeteer". It handles all networking and tells the Logic script what to do.
[RequireComponent(typeof(GhostOrbLogic))] // Ensures the Logic script is always present
public class GhostOrbManager_Networked : NetworkBehaviour
{
    // A private reference to the logic component
    private GhostOrbLogic _logic;

    // The single source of truth for the orb count, synced across the network.
    [Networked]
    public int TotalGhostOrbs { get; set; }

    private void Awake()
    {
        // Get the reference to our "motor"
        _logic = GetComponent<GhostOrbLogic>();
    }

    public override void Spawned()
    {
        // When this object spawns on the network for any client,
        // immediately update the logic and UI with the correct count from the server.
        _logic.SetOrbCount(TotalGhostOrbs);
        _logic.UpdateDisplay();
    }

    // This static callback fires on ALL clients when the 'TotalGhostOrbs' value changes.
    private static void OnOrbCountChanged(Changed<GhostOrbManager_Networked> changed)
    {
        // Get the old value before applying the new one
        changed.LoadOld();
        int oldValue = changed.Behaviour.TotalGhostOrbs;
        
        // Apply the new value
        changed.LoadNew();
        int newValue = changed.Behaviour.TotalGhostOrbs;

        // Get the logic component from the instance of our script
        var logic = changed.Behaviour._logic;

        // Command the logic script to update its state and visuals
        logic.SetOrbCount(newValue);
        logic.UpdateDisplay();

        // Only play the sound if the orb count actually went up
        if (newValue > oldValue)
        {
            logic.PlayCollectSound();
        }
    }

    // This RPC is called by a player, but runs ONLY on the server/host.
    [Rpc(RpcSources.StateAuthority, RpcTargets.StateAuthority)]
    public void RPC_PlayerCollectedOrb(NetworkObject orbToDespawn)
    {
        // The server is the only one who can change the [Networked] property.
        if (orbToDespawn != null && orbToDespawn.HasStateAuthority)
        {
            TotalGhostOrbs++; // This change will automatically trigger OnOrbCountChanged on all clients.
            
            Runner.Despawn(orbToDespawn); // The server despawns the orb for everyone.

            // The server checks the win condition using the data from the logic script.
            if (TotalGhostOrbs >= _logic.requiredOrbsToExit)
            {
                if (!string.IsNullOrEmpty(_logic.nextSceneName))
                {
                    SceneManager.LoadScene(_logic.nextSceneName);
                }
            }
        }
    }
}