using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public sealed class LobbyUI : MonoBehaviour
{
    private const string LogTag = "[LobbyUI]";

    [Header("References")]
    [SerializeField] private BootRouter bootRouter;
    [SerializeField] private TMP_Text emailText;
    [SerializeField] private TMP_Text roleText;
    [SerializeField] private TMP_Text modeText;
    [SerializeField] private TMP_Text classText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button enterClassButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Transform xrUiRoot;
    [SerializeField] private Transform mobileUiRoot;

    [Header("Connection")]
    [SerializeField, Min(3f)] private float connectTimeoutSeconds = 15f;

    private bool isEnteringClass;
    private string lastSessionName = string.Empty;
    private TMP_Text activeEmailText;
    private TMP_Text activeRoleText;
    private TMP_Text activeModeText;
    private TMP_Text activeClassText;
    private TMP_Text activeStatusText;
    private Button activeEnterClassButton;
    private Button activeBackButton;
    private bool isSubscribed;
    private readonly List<TMP_Text> textBuffer = new List<TMP_Text>();
    private readonly List<Button> buttonBuffer = new List<Button>();

    private void OnEnable()
    {
        TrySubscribeMode();
        ResolveModeBindings();
        BindButtons();
        Refresh();

        var bootstrap = FusionBootstrap.GetOrCreate();
        if (bootstrap != null)
        {
            bootstrap.SetStatusText(activeStatusText);
        }
    }

    private void OnDisable()
    {
        UnbindButtons();
        UnsubscribeMode();
    }

    private void OnDeviceModeChanged(DeviceMode mode)
    {
        UnbindButtons();
        ResolveModeBindings();
        BindButtons();
        Refresh();

        var bootstrap = FusionBootstrap.GetOrCreate();
        if (bootstrap != null)
        {
            bootstrap.SetStatusText(activeStatusText);
        }
    }

    public void Refresh()
    {
        var appState = AppState.Instance;

        var email = appState != null && appState.User != null && !string.IsNullOrWhiteSpace(appState.User.email)
            ? appState.User.email
            : "No user";
        var role = appState != null && appState.Role != null
            ? appState.Role.activeRole.ToString()
            : Role.None.ToString();
        var isHost = appState != null && appState.Session != null && appState.Session.isHost;
        var classCodeOrName = appState != null && appState.Session != null && !string.IsNullOrWhiteSpace(appState.Session.classCodeOrName)
            ? appState.Session.classCodeOrName
            : "No class";

        if (activeEmailText != null)
        {
            activeEmailText.text = email;
        }

        if (activeRoleText != null)
        {
            activeRoleText.text = role;
        }

        if (activeModeText != null)
        {
            activeModeText.text = isHost ? "Host" : "Client";
        }

        if (activeClassText != null)
        {
            activeClassText.text = classCodeOrName;
        }
    }

    public async void OnEnterClassClicked()
    {
        var appState = AppState.Instance;
        var sessionName = appState != null && appState.Session != null ? appState.Session.classCodeOrName : string.Empty;

        if (string.IsNullOrWhiteSpace(sessionName))
        {
            SetStatus("Enter a class code first.");
            Debug.LogWarning($"{LogTag} Empty class code. Blocking enter class.");
            return;
        }

        await TryConnectAndEnterClass(sessionName);
    }

    public async void OnRetryConnectClicked()
    {
        if (string.IsNullOrWhiteSpace(lastSessionName))
        {
            var appState = AppState.Instance;
            lastSessionName = appState != null && appState.Session != null ? appState.Session.classCodeOrName : string.Empty;
        }

        if (string.IsNullOrWhiteSpace(lastSessionName))
        {
            SetStatus("No class code to retry.");
            Debug.LogWarning($"{LogTag} Retry clicked but no session name available.");
            return;
        }

        await TryConnectAndEnterClass(lastSessionName);
    }

    private async Task TryConnectAndEnterClass(string sessionName)
    {
        if (isEnteringClass)
        {
            SetStatus("Already connecting...");
            return;
        }

        if (!EnsureFusionBootstrap())
        {
            SetStatus("Network bootstrap missing.");
            Debug.LogError($"{LogTag} FusionBootstrap unavailable.");
            return;
        }

        lastSessionName = sessionName.Trim();

        isEnteringClass = true;
        SetEnterButtonInteractable(false);

        SetStatus($"Connecting to {lastSessionName}...");

        var bootstrap = FusionBootstrap.Instance;
        if (bootstrap == null)
        {
            SetStatus("Connection failed. Retry.");
            Debug.LogError($"{LogTag} Bootstrap became null before connection.");
            isEnteringClass = false;
            SetEnterButtonInteractable(true);
            return;
        }

        bootstrap.SetStatusText(activeStatusText);

        var connectTask = bootstrap.StartOrJoinSharedSession(lastSessionName);
        var timeoutTask = Task.Delay((int)(connectTimeoutSeconds * 1000));
        var completed = await Task.WhenAny(connectTask, timeoutTask);

        var success = false;

        if (completed == connectTask)
        {
            success = await connectTask;
        }
        else
        {
            Debug.LogWarning($"{LogTag} Connect timed out after {connectTimeoutSeconds:0}s for '{lastSessionName}'.");
            SetStatus("Connection failed. Retry.");
            await bootstrap.ShutdownRunnerAsync();
        }

        if (success)
        {
            Debug.Log($"{LogTag} Session ready. Loading Class 1 via Fusion.");
            bootstrap.LoadClassScene("Class 1");
        }
        else
        {
            SetStatus("Connection failed. Retry.");
            Debug.LogWarning($"{LogTag} Failed to join session '{lastSessionName}'. User remains in Lobby.");
        }

        isEnteringClass = false;
        SetEnterButtonInteractable(true);
    }

    public async void OnBackClicked()
    {
        var appState = AppState.Instance;
        if (appState == null)
        {
            return;
        }

        var bootstrap = FusionBootstrap.GetOrCreate();
        if (bootstrap != null)
        {
            await bootstrap.ShutdownRunnerAsync();
        }

        appState.ResetSession();

        if (bootRouter != null)
        {
            bootRouter.RouteFromState();
        }

        Debug.Log($"{LogTag} Leaving lobby (session cleared)");
    }

    private bool EnsureFusionBootstrap()
    {
        var bootstrap = FusionBootstrap.GetOrCreate();
        if (bootstrap == null)
        {
            Debug.LogError($"{LogTag} Failed to get/create FusionBootstrap.");
            return false;
        }

        bootstrap.SetStatusText(activeStatusText);
        return true;
    }

    private void SetEnterButtonInteractable(bool interactable)
    {
        if (activeEnterClassButton != null)
        {
            activeEnterClassButton.interactable = interactable;
        }
    }

    private void SetStatus(string message)
    {
        if (activeStatusText != null)
        {
            activeStatusText.text = message;
        }
    }

    private void ResolveModeBindings()
    {
        var mode = AppState.Instance != null ? AppState.Instance.CurrentDeviceMode : DeviceMode.Mobile_AR;
        var preferredRoot = mode == DeviceMode.XR_HMD ? ResolveXrRoot() : ResolveMobileRoot();
        var fallbackRoot = mode == DeviceMode.XR_HMD ? ResolveMobileRoot() : ResolveXrRoot();

        activeEmailText = ResolveText(emailText, preferredRoot, fallbackRoot, "Email Text");
        activeRoleText = ResolveText(roleText, preferredRoot, fallbackRoot, "Role Text");
        activeModeText = ResolveText(modeText, preferredRoot, fallbackRoot, "Mode Text");
        activeClassText = ResolveText(classText, preferredRoot, fallbackRoot, "Class Text");
        activeStatusText = ResolveText(statusText, preferredRoot, fallbackRoot, "Status Text");

        activeEnterClassButton = ResolveButton(enterClassButton, preferredRoot, fallbackRoot, "Create Button", "Enter", "Join");
        activeBackButton = ResolveButton(backButton, preferredRoot, fallbackRoot, "Back");

        var rootName = preferredRoot != null ? preferredRoot.name : "None";
        Debug.Log($"{LogTag} Resolved bindings mode={mode} root={rootName} enterButton={(activeEnterClassButton != null ? activeEnterClassButton.name : "None")}");
    }

    private TMP_Text ResolveText(TMP_Text assigned, Transform preferredRoot, Transform fallbackRoot, string token)
    {
        if (assigned != null && assigned.gameObject.activeInHierarchy)
        {
            return assigned;
        }

        var found = FindText(preferredRoot, token);
        if (found != null)
        {
            return found;
        }

        found = FindText(fallbackRoot, token);
        return found != null ? found : assigned;
    }

    private Button ResolveButton(Button assigned, Transform preferredRoot, Transform fallbackRoot, params string[] tokens)
    {
        if (assigned != null && assigned.gameObject.activeInHierarchy)
        {
            return assigned;
        }

        var found = FindButton(preferredRoot, tokens);
        if (found != null)
        {
            return found;
        }

        found = FindButton(fallbackRoot, tokens);
        return found != null ? found : assigned;
    }

    private void BindButtons()
    {
        if (activeEnterClassButton != null)
        {
            activeEnterClassButton.onClick.RemoveListener(OnEnterClassClicked);
            activeEnterClassButton.onClick.AddListener(OnEnterClassClicked);
        }

        if (activeBackButton != null)
        {
            activeBackButton.onClick.RemoveListener(OnBackClicked);
            activeBackButton.onClick.AddListener(OnBackClicked);
        }
    }

    private void UnbindButtons()
    {
        if (activeEnterClassButton != null)
        {
            activeEnterClassButton.onClick.RemoveListener(OnEnterClassClicked);
        }

        if (activeBackButton != null)
        {
            activeBackButton.onClick.RemoveListener(OnBackClicked);
        }
    }

    private TMP_Text FindText(Transform root, string token)
    {
        if (root == null)
        {
            return null;
        }

        textBuffer.Clear();
        root.GetComponentsInChildren(true, textBuffer);
        for (var i = 0; i < textBuffer.Count; i++)
        {
            var item = textBuffer[i];
            if (item != null && item.gameObject.name.IndexOf(token, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return item;
            }
        }

        return null;
    }

    private Button FindButton(Transform root, params string[] tokens)
    {
        if (root == null)
        {
            return null;
        }

        buttonBuffer.Clear();
        root.GetComponentsInChildren(true, buttonBuffer);
        for (var i = 0; i < buttonBuffer.Count; i++)
        {
            var item = buttonBuffer[i];
            if (item == null)
            {
                continue;
            }

            var name = item.gameObject.name;
            for (var t = 0; t < tokens.Length; t++)
            {
                if (name.IndexOf(tokens[t], System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return item;
                }
            }
        }

        return null;
    }

    private Transform ResolveXrRoot()
    {
        if (xrUiRoot != null)
        {
            return xrUiRoot;
        }

        return FindChildByPrefix("XR_Root");
    }

    private Transform ResolveMobileRoot()
    {
        if (mobileUiRoot != null)
        {
            return mobileUiRoot;
        }

        return FindChildByPrefix("Mobile_Root");
    }

    private Transform FindChildByPrefix(string prefix)
    {
        for (var i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (child != null && child.name.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase))
            {
                return child;
            }
        }

        return null;
    }

    private void TrySubscribeMode()
    {
        if (isSubscribed || AppState.Instance == null)
        {
            return;
        }

        AppState.Instance.DeviceModeChanged -= OnDeviceModeChanged;
        AppState.Instance.DeviceModeChanged += OnDeviceModeChanged;
        isSubscribed = true;
    }

    private void UnsubscribeMode()
    {
        if (!isSubscribed || AppState.Instance == null)
        {
            return;
        }

        AppState.Instance.DeviceModeChanged -= OnDeviceModeChanged;
        isSubscribed = false;
    }
}

