using UnityEngine;
using Fusion;
using System.Collections.Generic;
using Fusion.Sockets;
using System;

// This is a more robust spawner that handles the host spawning correctly in a multi-scene setup.
public class PlayerSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Player Prefab")]
    [SerializeField] private NetworkPrefabRef _playerPrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] _spawnPoints;
    private int _nextSpawnPointIndex = 0;

    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    // This is called by Unity when the object is first created.
    private void Start()
    {
        // Find the active NetworkRunner and register this spawner with it.
        // This is a robust way to connect scene objects to the persistent runner.
        var runner = FindObjectOfType<NetworkRunner>();
        if (runner != null)
        {
            runner.AddCallbacks(this);
            Debug.Log("PlayerSpawner registered with the NetworkRunner.");
        }
        else
        {
            Debug.LogError("No NetworkRunner found in the scene!");
        }
    }

    // --- The key to spawning the host correctly ---
    public void OnSceneLoadDone(NetworkRunner runner)
    {
        // This callback runs after the GameScene has finished loading.
        // We only want the Server/Host to handle spawning.
        if (runner.IsServer)
        {
            Debug.Log("SceneLoadDone: Server is checking for players to spawn.");
            // Iterate through all players currently in the session.
            foreach (var player in runner.ActivePlayers)
            {
                // Check if this player has already been spawned.
                if (!_spawnedCharacters.ContainsKey(player))
                {
                    // If they haven't been spawned, spawn them now.
                    SpawnPlayer(runner, player);
                }
            }
        }
    }

    // This handles players who join AFTER the game scene is already loaded.
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        // The server is the only one who spawns.
        if (runner.IsServer)
        {
            Debug.Log($"Player {player} joined. Spawning now.");
            SpawnPlayer(runner, player);
        }
    }

    // The shared spawning logic.
    private void SpawnPlayer(NetworkRunner runner, PlayerRef player)
    {
        if (_spawnPoints == null || _spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned to the PlayerSpawner.");
            return;
        }

        Transform spawnPoint = _spawnPoints[_nextSpawnPointIndex];
        _nextSpawnPointIndex = (_nextSpawnPointIndex + 1) % _spawnPoints.Length;

        NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPoint.position, spawnPoint.rotation, player);
        _spawnedCharacters.Add(player, networkPlayerObject);
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

    #region Empty Callbacks
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }

    // Implement missing interface method
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }

    // Implement missing interface method
    public void OnSceneLoadStart(NetworkRunner runner) { }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    #endregion
}