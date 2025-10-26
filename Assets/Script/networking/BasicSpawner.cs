using UnityEngine;
using Fusion;
using System.Collections.Generic;
using Fusion.Sockets;
using System;

public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Player Prefab")]
    [SerializeField] private NetworkPrefabRef _playerPrefab;

    // --- NEW: List for Spawn Points ---
    // Drag your empty GameObjects (your spawn points) here in the Inspector.
    [Header("Spawn Points")]
    [SerializeField] private Transform[] _spawnPoints;

    // A variable to keep track of which spawn point to use next.
    private int _nextSpawnPointIndex = 0;

    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            Debug.Log($"OnPlayerJoined: Spawning character for player {player}.");

            // --- NEW: Spawn Point Selection Logic ---
            Vector3 spawnPosition;
            Quaternion spawnRotation;

            // Check if any spawn points have been assigned in the Inspector.
            if (_spawnPoints != null && _spawnPoints.Length > 0)
            {
                // Get the transform of the next spawn point in the list.
                Transform spawnPoint = _spawnPoints[_nextSpawnPointIndex];
                spawnPosition = spawnPoint.position;
                spawnRotation = spawnPoint.rotation;

                // Move to the next spawn point for the next player, wrapping around if we reach the end of the list.
                _nextSpawnPointIndex = (_nextSpawnPointIndex + 1) % _spawnPoints.Length;
            }
            else
            {
                // If no spawn points are set, fall back to a default position and log a warning.
                Debug.LogWarning("No spawn points assigned to the BasicSpawner. Spawning at default position (0,1,0).");
                spawnPosition = new Vector3(0, 1, 0);
                spawnRotation = Quaternion.identity;
            }
            
            // The spawn call now uses the position and rotation we selected.
            NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, spawnRotation, player);

            _spawnedCharacters.Add(player, networkPlayerObject);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
            {
                runner.Despawn(networkObject);
                _spawnedCharacters.Remove(player);
            }
        }
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) 
{
    Debug.Log("Disconnected from server.");
    
    // When we disconnect, tell the UI Manager to hide the in-game UI.
    // This allows the Fusion connection window to reappear cleanly.
    if (GameUIManager.Instance != null)
    {
        GameUIManager.Instance.HideInGameUI();
    }
}

    #region Empty Interface Methods
    // ... (The rest of the script is the same)
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    #endregion
}