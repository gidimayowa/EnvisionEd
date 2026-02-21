using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class LobbyUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BootRouter bootRouter;
    [SerializeField] private TMP_Text emailText;
    [SerializeField] private TMP_Text roleText;
    [SerializeField] private TMP_Text modeText;
    [SerializeField] private TMP_Text classText;
    [SerializeField] private Button enterClassButton;
    [SerializeField] private Button backButton;

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

    public void OnEnterClassClicked()
    {
        Debug.Log("[LobbyUI] Entering class... loading scene: Class 1");
        SceneManager.LoadScene("Class 1");
    }

    public void OnBackClicked()
    {
        var appState = AppState.Instance;
        if (appState == null)
        {
            return;
        }

        appState.ResetSession();

        if (bootRouter != null)
        {
            bootRouter.RouteFromState();
        }

        Debug.Log("[LobbyUI] Leaving lobby (session cleared)");
    }
}
