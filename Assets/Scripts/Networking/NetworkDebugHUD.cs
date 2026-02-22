using System.Text;
using Fusion;
using TMPro;
using UnityEngine;

/*
Setup (Lobby + Class 1):
1) Create a world-space Canvas in each scene.
2) Add a TMP_Text for diagnostics (e.g. NetworkDebugText).
3) Add this script to a scene object and assign outputText.
4) Optional: assign PlayerSpawner via inspector or call SetSpawner(...) at runtime.
*/
public sealed class NetworkDebugHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text outputText;
    [SerializeField] private PlayerSpawner playerSpawner;

    private readonly StringBuilder sb = new StringBuilder(768);

    public void SetText(TMP_Text t) => outputText = t;

    public void SetSpawner(PlayerSpawner s) => playerSpawner = s;

    private void Update()
    {
        if (outputText == null)
        {
            return;
        }

        var appState = AppState.Instance;
        var role = appState != null && appState.Role != null ? appState.Role.activeRole.ToString() : "None";
        var isHost = appState != null && appState.Session != null && appState.Session.isHost;
        var email = appState != null && appState.User != null && !string.IsNullOrWhiteSpace(appState.User.email)
            ? appState.User.email
            : "n/a";
        var classCode = appState != null && appState.Session != null && !string.IsNullOrWhiteSpace(appState.Session.classCodeOrName)
            ? appState.Session.classCodeOrName
            : "n/a";

        var bootstrap = FusionBootstrap.Instance;
        var runner = bootstrap != null ? bootstrap.Runner : null;

        sb.Clear();
        sb.AppendLine("[Net HUD]");
        sb.Append("Role: ").AppendLine(role);
        sb.Append("IsHost: ").AppendLine(isHost ? "True" : "False");
        sb.Append("Email: ").AppendLine(email);
        sb.Append("ClassCode: ").AppendLine(classCode);
        sb.Append("Bootstrap: ").AppendLine(bootstrap != null ? "Yes" : "No");
        sb.Append("Runner: ").AppendLine(runner != null ? "Yes" : "No");

        if (runner != null)
        {
            sb.Append("LocalPlayer: ").AppendLine(runner.LocalPlayer.ToString());
            sb.Append("IsRunning: ").AppendLine(runner.IsRunning ? "True" : "False");
            sb.Append("GameMode: ").AppendLine(runner.GameMode.ToString());
            sb.Append("ActivePlayers: ").AppendLine(GetActivePlayerCount(runner).ToString());

            var sessionName = "n/a";
            if (runner.SessionInfo.IsValid)
            {
                sessionName = string.IsNullOrWhiteSpace(runner.SessionInfo.Name) ? "(empty)" : runner.SessionInfo.Name;
            }

            sb.Append("RunnerSession: ").AppendLine(sessionName);
        }

        if (playerSpawner == null)
        {
            sb.AppendLine("Spawner: not assigned");
        }
        else
        {
            sb.Append("LocalAvatarSpawned: ").AppendLine(playerSpawner.LocalAvatarSpawned ? "True" : "False");

            var localAvatar = playerSpawner.LocalAvatarObject;
            sb.Append("LocalAvatarObject: ").AppendLine(localAvatar != null ? localAvatar.name : "null");

            var proxy = localAvatar != null ? localAvatar.GetComponent<NetworkRigProxy>() : null;
            if (proxy == null)
            {
                sb.AppendLine("RigProxy: null");
            }
            else
            {
                var hasInputAuthority = proxy.Object != null && proxy.Object.HasInputAuthority;
                sb.Append("RigProxy HasInputAuthority: ").AppendLine(hasInputAuthority ? "True" : "False");
                sb.Append("IsPushingLocalPose: ").AppendLine(proxy.IsPushingLocalPose ? "True" : "False");
            }
        }

        outputText.text = sb.ToString();
    }

    private static int GetActivePlayerCount(NetworkRunner runner)
    {
        if (runner == null)
        {
            return 0;
        }

        var count = 0;
        foreach (var _ in runner.ActivePlayers)
        {
            count++;
        }

        return count;
    }
}
