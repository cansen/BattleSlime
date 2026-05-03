using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public struct NetworkInputData : INetworkInput
    {
        public Vector2 Move;
        public bool Jump;
        public bool Dash;
    }

    [SerializeField] private NetworkObject playerPrefab;
    [SerializeField] private string sessionName = "BattleSlimeRoom";
    [SerializeField] private GameMode gameMode = GameMode.AutoHostOrClient;
    [SerializeField] private float spawnRadius = 20f;
    [SerializeField] private float spawnHeight = 3f;

    private NetworkRunner runner;
    private Dictionary<PlayerRef, NetworkObject> playerObjects = new Dictionary<PlayerRef, NetworkObject>();
    private InputSystem_Actions inputActions;
    private bool jumpPressedBuffered;
    private bool dashPressedBuffered;

    private void Update()
    {
        if (inputActions == null) return;
        if (inputActions.Player.Jump.WasPressedThisFrame()) jumpPressedBuffered = true;
        if (inputActions.Player.Sprint.WasPressedThisFrame()) dashPressedBuffered = true;
    }

    private async void Start()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Player.Enable();
        runner = gameObject.AddComponent<NetworkRunner>();
        runner.ProvideInput = true;

        await runner.StartGame(new StartGameArgs
        {
            GameMode = gameMode,
            SessionName = sessionName,
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (player != runner.LocalPlayer)
        {
            return;
        }

        Vector3 spawnPosition = new Vector3(
            UnityEngine.Random.Range(-spawnRadius, spawnRadius),
            spawnHeight,
            UnityEngine.Random.Range(-spawnRadius, spawnRadius)
        );

        NetworkObject spawned = runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);
        playerObjects[player] = spawned;
        Debug.Log($"[Network] Player {player} joined and spawned.");
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (playerObjects.TryGetValue(player, out NetworkObject obj))
        {
            runner.Despawn(obj);
            playerObjects.Remove(player);
        }
        Debug.Log($"[Network] Player {player} left and despawned.");
    }
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        NetworkInputData data = new NetworkInputData
        {
            Move = inputActions.Player.Move.ReadValue<Vector2>(),
            Jump = jumpPressedBuffered,
            Dash = dashPressedBuffered
        };
        jumpPressedBuffered = false;
        dashPressedBuffered = false;
        input.Set(data);
    }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        playerObjects.Clear();
        inputActions?.Player.Disable();
    }
    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("[Network] Connected to server.");
    }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
}
