using UnityEngine;
using Fusion;
using System.Collections.Generic;
using Fusion.Sockets;
using System;

namespace EasyPeasyFirstPersonController
{
    [RequireComponent(typeof(FirstPersonController_Motor))]
    [RequireComponent(typeof(CharacterController))]
    public class PlayerNetworkController : NetworkBehaviour, INetworkRunnerCallbacks
    {
        private FirstPersonController_Motor _motor;

        private void Awake()
        {
            _motor = GetComponent<FirstPersonController_Motor>();
        }

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                // This is our player. Tell the motor to run visual effects.
                _motor.IsLocalPlayer = true;
                if (Camera.main != null) Camera.main.gameObject.SetActive(false);
                _motor.SetCursorVisibility(false); // Use the motor's own method
            }
            else
            {
                // This is a remote player. Disable their camera and tell motor not to run visuals.
                _motor.playerCamera.gameObject.SetActive(false);
                _motor.IsLocalPlayer = false;
            }
            if (HasStateAuthority)
             {
                _motor.IsLocalPlayer = true;
                if (Camera.main != null) Camera.main.gameObject.SetActive(false);

                 // This line tells our manager to show the in-game UI.
                GameUIManager.Instance.ShowInGameUI();
             }
            else
             {
                 _motor.playerCamera.gameObject.SetActive(false);
                _motor.IsLocalPlayer = false;
             }   
        }
        

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            var data = new NetworkInputData();
            data.moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            data.lookDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            // For buttons, only send the event on the frame it happens
            data.buttons.Set(MyButtons.Jump, Input.GetKeyDown(KeyCode.Space));
            data.buttons.Set(MyButtons.Crouch, Input.GetKey(KeyCode.LeftControl));
            // We don't need to send Sprint as the motor calculates it from MoveInput and Crouch state
            input.Set(data);
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData data))
            {
                // Feed the networked data into the motor's public properties
                _motor.MoveInput = data.moveInput;
                _motor.MouseXInput = data.lookDelta.x * _motor.mouseSensitivity * 10f * Runner.DeltaTime;
                _motor.MouseYInput = data.lookDelta.y * _motor.mouseSensitivity * 10f * Runner.DeltaTime;
                _motor.JumpInputDown = data.buttons.IsSet(MyButtons.Jump);
                _motor.CrouchInputHeld = data.buttons.IsSet(MyButtons.Crouch);

                // Now, command the motor to run its update logic with this new data
                _motor.MotorUpdate(Runner.DeltaTime);
            }
        }
        
        // Orb collection logic remains here as it's a network task
        private void OnTriggerEnter(Collider other)
        {
            if (!HasStateAuthority) return;
            if (other.TryGetComponent<GhostOrb_Networked>(out var orb))
            {
                var manager = Runner.GetSingleton<GhostOrbManager_Networked>();
                if (manager != null)
                {
                    manager.RPC_PlayerCollectedOrb(orb.GetComponent<NetworkObject>());
                }
            }
        }

        #region Full INetworkRunnerCallbacks Implementation
        // All required methods from the interface are here
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
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
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        #endregion
    }
}