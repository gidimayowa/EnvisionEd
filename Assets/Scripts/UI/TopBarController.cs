using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class TopBarController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BootRouter bootRouter;
    [SerializeField] private TMP_Text emailText;
    [SerializeField] private TMP_Text roleText;
    [SerializeField] private TMP_Text sessionText;
    [SerializeField] private Button switchRoleButton;
    [SerializeField] private Button signOutButton;
    [SerializeField] private Button leaveClassButton;

    private void Awake()
    {
        if (switchRoleButton != null)
        {
            switchRoleButton.onClick.AddListener(OnSwitchRoleClicked);
        }

        if (signOutButton != null)
        {
            signOutButton.onClick.AddListener(OnSignOutClicked);
        }

        if (leaveClassButton != null)
        {
            leaveClassButton.onClick.AddListener(OnLeaveClassClicked);
        }
    }

    private void OnDestroy()
    {
        if (switchRoleButton != null)
        {
            switchRoleButton.onClick.RemoveListener(OnSwitchRoleClicked);
        }

        if (signOutButton != null)
        {
            signOutButton.onClick.RemoveListener(OnSignOutClicked);
        }

        if (leaveClassButton != null)
        {
            leaveClassButton.onClick.RemoveListener(OnLeaveClassClicked);
        }
    }

    private void Update()
    {
        RefreshLabels();
    }

    private void RefreshLabels()
    {
        var appState = AppState.Instance;

        if (emailText != null)
        {
            var email = appState != null && appState.User != null && !string.IsNullOrWhiteSpace(appState.User.email)
                ? appState.User.email
                : "No user";
            emailText.text = email;
        }

        if (roleText != null)
        {
            var currentRole = appState != null && appState.Role != null ? appState.Role.activeRole : Role.None;
            roleText.text = currentRole.ToString();
        }

        if (sessionText != null)
        {
            if (appState == null || appState.Session == null || string.IsNullOrWhiteSpace(appState.Session.classCodeOrName))
            {
                sessionText.text = "No session";
            }
            else
            {
                sessionText.text = $"Host: {appState.Session.isHost} | {appState.Session.classCodeOrName}";
            }
        }
    }

    private void OnSwitchRoleClicked()
    {
        var appState = AppState.Instance;
        if (appState == null)
        {
            return;
        }

        appState.SetRole(Role.None);
        appState.ResetSession();

        if (bootRouter != null)
        {
            bootRouter.GoToRoleSelect();
        }
    }

    private void OnSignOutClicked()
    {
        var appState = AppState.Instance;
        if (appState == null)
        {
            return;
        }

        appState.SignOut();
        appState.ClearSavedData();

        if (bootRouter != null)
        {
            bootRouter.GoToSignIn();
        }
    }

    private void OnLeaveClassClicked()
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
    }
}
