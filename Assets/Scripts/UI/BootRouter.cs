using System.Collections.Generic;
using UnityEngine;

public enum ScreenId
{
    SignInScreen,
    StudentTeacherOptions,
    StudentHomeScreen,
    TeacherHomeScreen,
    JoinClassScreen,
    CreateClassScreen,
    LobbyScreen,
    ClassRoomScreen
}

public sealed class BootRouter : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject signInScreen;
    [SerializeField] private GameObject studentTeacherOptions;
    [SerializeField] private GameObject studentHomeScreen;
    [SerializeField] private GameObject teacherHomeScreen;
    [SerializeField] private GameObject joinClassScreen;
    [SerializeField] private GameObject createClassScreen;
    [SerializeField] private GameObject lobbyScreen;
    [SerializeField] private GameObject classRoomScreen;

    private Dictionary<ScreenId, GameObject> screenMap;

    private void Awake()
    {
        screenMap = new Dictionary<ScreenId, GameObject>
        {
            { ScreenId.SignInScreen, signInScreen },
            { ScreenId.StudentTeacherOptions, studentTeacherOptions },
            { ScreenId.StudentHomeScreen, studentHomeScreen },
            { ScreenId.TeacherHomeScreen, teacherHomeScreen },
            { ScreenId.JoinClassScreen, joinClassScreen },
            { ScreenId.CreateClassScreen, createClassScreen },
            { ScreenId.LobbyScreen, lobbyScreen },
            { ScreenId.ClassRoomScreen, classRoomScreen }
        };
    }

    private void Start()
    {
        RouteFromState();
    }

    public void RouteFromState()
    {
        var appState = AppState.Instance;
        if (appState == null || appState.User == null)
        {
            ShowScreen(ScreenId.SignInScreen);
            return;
        }

        var role = appState.Role != null ? appState.Role.activeRole : Role.None;
        if (role == Role.None)
        {
            ShowScreen(ScreenId.StudentTeacherOptions);
            return;
        }

        var classCodeOrName = appState.Session != null ? appState.Session.classCodeOrName : string.Empty;
        if (string.IsNullOrWhiteSpace(classCodeOrName))
        {
            ShowScreen(role == Role.Student ? ScreenId.JoinClassScreen : ScreenId.CreateClassScreen);
            return;
        }

        ShowScreen(ScreenId.LobbyScreen);
    }

    public void ShowScreen(ScreenId id)
    {
        foreach (var panel in screenMap.Values)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        if (screenMap.TryGetValue(id, out var selectedPanel) && selectedPanel != null)
        {
            selectedPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"[BootRouter] Screen reference missing for {id}.");
        }
    }

    public void GoToSignIn()
    {
        ShowScreen(ScreenId.SignInScreen);
    }

    public void GoToRoleSelect()
    {
        ShowScreen(ScreenId.StudentTeacherOptions);
    }

    public void GoToStudentJoin()
    {
        ShowScreen(ScreenId.JoinClassScreen);
    }

    public void GoToTeacherCreate()
    {
        ShowScreen(ScreenId.CreateClassScreen);
    }

    public void GoToLobby()
    {
        ShowScreen(ScreenId.LobbyScreen);
    }
}
