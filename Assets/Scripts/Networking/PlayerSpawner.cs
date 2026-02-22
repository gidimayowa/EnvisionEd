using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

/*
Setup (Class 1 scene):
1) Create GameObject: SpawnSystem.
2) Add components: NetworkObject + PlayerSpawner (this script).
3) Assign Player Avatar prefab into playerAvatarPrefab (must be a network prefab).
4) Optional: assign spawnPoints (Transform array). If empty, players spawn at Vector3.zero.
5) Add LocalRigReferences to a scene object and assign Meta rig anchors there.
6) Assign PlayerSpawner.localRigReferences to that LocalRigReferences component.
7) Ensure Class 1 is loaded through FusionBootstrap.LoadClassScene(...) so this scene object is networked.
*/
public sealed class PlayerSpawner : NetworkBehaviour, INetworkRunnerCallbacks
{
    private const string LogTag = "[PlayerSpawner]";

    [Header("Spawn Config")]
    [SerializeField] private NetworkPrefabRef playerAvatarPrefab;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Local Rig Binding (local player only)")]
    [SerializeField] private LocalRigReferences localRigReferences;

    private readonly Dictionary<PlayerRef, NetworkObject> spawnedByPlayer = new Dictionary<PlayerRef, NetworkObject>();

    public bool LocalAvatarSpawned { get; private set; }
    public NetworkObject LocalAvatarObject { get; private set; }

    public override void Spawned()
    {
        if (Runner == null)
        {
            Debug.LogError($"{LogTag} Runner is null in Spawned.");
            return;
        }

        Runner.AddCallbacks(this);
        Debug.Log($"{LogTag} Registered callbacks on runner '{Runner.name}'. StateAuthority={Object.HasStateAuthority}");

        if (Object.HasStateAuthority)
        {
            SpawnForPlayerIfNeeded(Runner.LocalPlayer);
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (runner != null)
        {
            runner.RemoveCallbacks(this);
        }

        spawnedByPlayer.Clear();
        LocalAvatarSpawned = false;
        LocalAvatarObject = null;
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (!Object.HasStateAuthority)
        {
            return;
        }

        SpawnForPlayerIfNeeded(player);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (!Object.HasStateAuthority)
        {
            return;
        }

        if (!spawnedByPlayer.TryGetValue(player, out var avatar) || avatar == null)
        {
            return;
        }

        Debug.Log($"{LogTag} Despawning avatar for player {player.PlayerId}. Object={avatar.name}");
        runner.Despawn(avatar);
        spawnedByPlayer.Remove(player);

        if (LocalAvatarObject == avatar)
        {
            LocalAvatarObject = null;
            LocalAvatarSpawned = false;
            Debug.Log($"{LogTag} Local avatar despawned for player {player.PlayerId}.");
        }
    }

    private void SpawnForPlayerIfNeeded(PlayerRef player)
    {
        if (Runner == null)
        {
            Debug.LogError($"{LogTag} Cannot spawn. Runner is null.");
            return;
        }

        if (!playerAvatarPrefab.IsValid)
        {
            Debug.LogError($"{LogTag} Player avatar prefab is not assigned.");
            return;
        }

        if (spawnedByPlayer.ContainsKey(player))
        {
            Debug.Log($"{LogTag} Skipping spawn for player {player.PlayerId}: already spawned.");
            return;
        }

        var spawnPosition = Vector3.zero;
        var spawnRotation = Quaternion.identity;

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            var index = Mathf.Abs(player.PlayerId) % spawnPoints.Length;
            var point = spawnPoints[index];
            if (point != null)
            {
                spawnPosition = point.position;
                spawnRotation = point.rotation;
            }
        }

        Debug.Log($"{LogTag} Spawning avatar for PlayerRef={player.PlayerId} at {spawnPosition}.");

        var avatar = Runner.Spawn(playerAvatarPrefab, spawnPosition, spawnRotation, player);
        if (avatar == null)
        {
            Debug.LogError($"{LogTag} Spawn failed for player {player.PlayerId}.");
            return;
        }

        spawnedByPlayer[player] = avatar;
        Debug.Log($"{LogTag} Spawned avatar for player {player.PlayerId} as '{avatar.name}'.");

        if (player == Runner.LocalPlayer)
        {
            LocalAvatarObject = avatar;
            LocalAvatarSpawned = true;
            Debug.Log($"{LogTag} Local avatar assigned for local player {player.PlayerId}.");
        }

        TryBindLocalRig(player, avatar);
    }

    private void TryBindLocalRig(PlayerRef player, NetworkObject avatar)
    {
        if (Runner == null || avatar == null)
        {
            return;
        }

        if (player != Runner.LocalPlayer)
        {
            return;
        }

        var rigProxy = avatar.GetComponent<NetworkRigProxy>();
        if (rigProxy == null)
        {
            Debug.LogWarning($"{LogTag} Spawned local avatar has no NetworkRigProxy component.");
            return;
        }

        if (localRigReferences == null)
        {
            Debug.LogWarning($"{LogTag} localRigReferences is not assigned. Local rig cannot be bound.");
            return;
        }

        Debug.Log($"{LogTag} Binding local rig refs for local player {player.PlayerId}.");
        rigProxy.BindLocalRig(localRigReferences.Head, localRigReferences.LeftHand, localRigReferences.RightHand);
        Debug.Log($"{LogTag} Local rig bound to local player avatar.");
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }
}
