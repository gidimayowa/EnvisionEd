using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
Setup (Menu scene):
1) Create GameObject: NetworkBootstrap.
2) Add components: NetworkRunner, NetworkSceneManagerDefault, FusionBootstrap (this script).
3) Ensure this object exists in your startup Menu scene only once.
4) Optional: assign a TMP_Text from Lobby via SetStatusText(...) at runtime.
*/
[DisallowMultipleComponent]
public sealed class FusionBootstrap : MonoBehaviour, INetworkRunnerCallbacks
{
    private const string LogTag = "[FusionBootstrap]";

    public static FusionBootstrap Instance { get; private set; }

    public static FusionBootstrap GetOrCreate()
    {
        if (Instance != null)
        {
            return Instance;
        }

        var existing = FindFirstObjectByType<FusionBootstrap>();
        if (existing != null)
        {
            return existing;
        }

        var bootstrapObject = new GameObject("NetworkBootstrap (Auto)");
        return bootstrapObject.AddComponent<FusionBootstrap>();
    }

    [Header("Optional UI")]
    [SerializeField] private TMP_Text statusText;

    private NetworkRunner runner;
    private NetworkSceneManagerDefault sceneManager;
    private string activeSessionName = string.Empty;
    private bool isStarting;

    public NetworkRunner Runner => runner;
    public bool IsStarting => isStarting;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"{LogTag} Duplicate bootstrap detected. Destroying duplicate on {gameObject.name}.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        runner = GetComponent<NetworkRunner>();
        if (runner == null)
        {
            runner = gameObject.AddComponent<NetworkRunner>();
            Debug.Log($"{LogTag} Added missing NetworkRunner component.");
        }

        sceneManager = GetComponent<NetworkSceneManagerDefault>();
        if (sceneManager == null)
        {
            sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();
            Debug.Log($"{LogTag} Added missing NetworkSceneManagerDefault component.");
        }

        runner.AddCallbacks(this);
        UpdateStatus("Ready");
    }

    private void OnDestroy()
    {
        if (runner != null)
        {
            runner.RemoveCallbacks(this);
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void SetStatusText(TMP_Text text)
    {
        statusText = text;
        if (statusText != null)
        {
            statusText.text = $"Network: {(runner != null && runner.IsRunning ? "Connected" : "Idle")}";
        }
    }

    public async Task<bool> StartOrJoinSharedSession(string sessionName)
    {
        if (string.IsNullOrWhiteSpace(sessionName))
        {
            Debug.LogWarning($"{LogTag} StartOrJoinSharedSession called with empty session name.");
            UpdateStatus("Invalid class code.");
            return false;
        }

        if (runner == null)
        {
            Debug.LogError($"{LogTag} NetworkRunner is missing.");
            UpdateStatus("Network init failed.");
            return false;
        }

        if (isStarting)
        {
            Debug.LogWarning($"{LogTag} StartOrJoinSharedSession ignored: already starting.");
            UpdateStatus("Already connecting...");
            return false;
        }

        var cleanSession = sessionName.Trim();

        if (runner.IsRunning)
        {
            if (string.Equals(activeSessionName, cleanSession, StringComparison.Ordinal))
            {
                Debug.Log($"{LogTag} Already connected to session '{cleanSession}'.");
                UpdateStatus($"Connected: {cleanSession}");
                return true;
            }

            Debug.LogWarning($"{LogTag} Runner already active in session '{activeSessionName}'. Requested '{cleanSession}'.");
            UpdateStatus("Already connected to a different class.");
            return false;
        }

        UpdateStatus($"Connecting: {cleanSession}...");
        Debug.Log($"{LogTag} Starting shared session '{cleanSession}'.");

        var args = new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = cleanSession,
            SceneManager = sceneManager
        };

        isStarting = true;

        try
        {
            var result = await runner.StartGame(args);
            if (!result.Ok)
            {
                Debug.LogError($"{LogTag} StartGame failed for session '{cleanSession}'. Reason: {result.ShutdownReason}");
                UpdateStatus($"Connection failed: {result.ShutdownReason}");
                return false;
            }

            activeSessionName = cleanSession;
            Debug.Log($"{LogTag} Shared session ready: '{activeSessionName}'.");
            UpdateStatus($"Connected: {activeSessionName}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"{LogTag} Exception while starting session '{cleanSession}': {ex}");
            UpdateStatus("Connection exception.");
            return false;
        }
        finally
        {
            isStarting = false;
        }
    }

    public async Task ShutdownRunnerAsync()
    {
        if (runner == null)
        {
            Debug.LogWarning($"{LogTag} Shutdown requested but runner is null.");
            activeSessionName = string.Empty;
            isStarting = false;
            UpdateStatus("Network idle.");
            return;
        }

        isStarting = false;

        if (!runner.IsRunning)
        {
            activeSessionName = string.Empty;
            UpdateStatus("Network idle.");
            return;
        }

        Debug.Log($"{LogTag} Shutting down runner...");
        UpdateStatus("Disconnecting...");

        try
        {
            await runner.Shutdown();
        }
        catch (Exception ex)
        {
            Debug.LogError($"{LogTag} Shutdown exception: {ex}");
        }
        finally
        {
            activeSessionName = string.Empty;
            isStarting = false;
            UpdateStatus("Network idle.");
        }
    }

    public void LoadClassScene(string sceneName)
    {
        if (runner == null || !runner.IsRunning)
        {
            Debug.LogWarning($"{LogTag} Refusing scene load. Runner not connected.");
            UpdateStatus("Connect first.");
            return;
        }

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning($"{LogTag} Refusing scene load. Scene name is empty.");
            UpdateStatus("Invalid scene name.");
            return;
        }

        var buildIndex = FindBuildIndexBySceneName(sceneName.Trim());
        if (buildIndex < 0)
        {
            Debug.LogError($"{LogTag} Scene '{sceneName}' not found in Build Settings.");
            UpdateStatus($"Scene missing: {sceneName}");
            return;
        }

        Debug.Log($"{LogTag} Network loading scene '{sceneName}' (build index {buildIndex}).");
        UpdateStatus($"Loading: {sceneName}");
        runner.LoadScene(SceneRef.FromIndex(buildIndex), LoadSceneMode.Single);
    }

    private static int FindBuildIndexBySceneName(string sceneName)
    {
        for (var i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            var path = SceneUtility.GetScenePathByBuildIndex(i);
            var fileName = Path.GetFileNameWithoutExtension(path);
            if (string.Equals(fileName, sceneName, StringComparison.Ordinal))
            {
                return i;
            }
        }

        return -1;
    }

    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }

        Debug.Log($"{LogTag} {message}");
    }

    public void OnConnectedToServer(NetworkRunner networkRunner) => UpdateStatus("Connected to Photon.");

    public void OnShutdown(NetworkRunner networkRunner, ShutdownReason shutdownReason)
    {
        activeSessionName = string.Empty;
        isStarting = false;
        UpdateStatus($"Disconnected: {shutdownReason}");
    }

    public void OnDisconnectedFromServer(NetworkRunner networkRunner, NetDisconnectReason reason)
    {
        activeSessionName = string.Empty;
        isStarting = false;
        UpdateStatus($"Disconnected: {reason}");
    }

    public void OnConnectFailed(NetworkRunner networkRunner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        isStarting = false;
        UpdateStatus($"Connect failed: {reason}");
    }

    public void OnPlayerJoined(NetworkRunner networkRunner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner networkRunner, PlayerRef player) { }
    public void OnInput(NetworkRunner networkRunner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner networkRunner, PlayerRef player, NetworkInput input) { }
    public void OnConnectRequest(NetworkRunner networkRunner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnUserSimulationMessage(NetworkRunner networkRunner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner networkRunner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner networkRunner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner networkRunner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner networkRunner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner networkRunner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner networkRunner) { }
    public void OnSceneLoadStart(NetworkRunner networkRunner) { }
    public void OnObjectEnterAOI(NetworkRunner networkRunner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner networkRunner, NetworkObject obj, PlayerRef player) { }
}
