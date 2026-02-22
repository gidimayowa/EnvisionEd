using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("Connection")]
    [SerializeField, Min(3f)] private float connectTimeoutSeconds = 15f;

    private bool isEnteringClass;
    private string lastSessionName = string.Empty;

    private void Awake()
    {
        if (enterClassButton != null)
        {
            enterClassButton.onClick.AddListener(OnEnterClassClicked);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
        }
    }

    private void OnDestroy()
    {
        if (enterClassButton != null)
        {
            enterClassButton.onClick.RemoveListener(OnEnterClassClicked);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(OnBackClicked);
        }
    }

    private void OnEnable()
    {
        Refresh();

        var bootstrap = FusionBootstrap.GetOrCreate();
        if (bootstrap != null)
        {
            bootstrap.SetStatusText(statusText);
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

        if (emailText != null)
        {
            emailText.text = email;
        }

        if (roleText != null)
        {
            roleText.text = role;
        }

        if (modeText != null)
        {
            modeText.text = isHost ? "Host" : "Client";
        }

        if (classText != null)
        {
            classText.text = classCodeOrName;
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

        bootstrap.SetStatusText(statusText);

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

        bootstrap.SetStatusText(statusText);
        return true;
    }

    private void SetEnterButtonInteractable(bool interactable)
    {
        if (enterClassButton != null)
        {
            enterClassButton.interactable = interactable;
        }
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}
