using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
Setup (Class 1 scene):
1) Add this script to your ClassRoom screen controller object.
2) Assign statusText and startTeachingButton.
3) Assign classSessionState (scene object with NetworkObject + ClassSessionState).
4) Button will be shown only for teacher/host.
*/
public sealed class ClassRoomUI : MonoBehaviour
{
    private const string LogTag = "[ClassRoomUI]";

    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button startTeachingButton;
    [SerializeField] private ClassSessionState classSessionState;

    private void Awake()
    {
        if (startTeachingButton != null)
        {
            startTeachingButton.onClick.AddListener(OnStartTeachingClicked);
        }
    }

    private void OnEnable()
    {
        if (classSessionState != null)
        {
            classSessionState.TeachingStartedChanged += OnTeachingStartedChanged;
            OnTeachingStartedChanged(classSessionState.TeachingStarted);
        }
        else
        {
            SetStatus("Waiting for teacher…");
            Debug.LogWarning($"{LogTag} classSessionState not assigned.");
        }

        RefreshTeacherControls();
    }

    private void OnDisable()
    {
        if (classSessionState != null)
        {
            classSessionState.TeachingStartedChanged -= OnTeachingStartedChanged;
        }
    }

    private void OnDestroy()
    {
        if (startTeachingButton != null)
        {
            startTeachingButton.onClick.RemoveListener(OnStartTeachingClicked);
        }
    }

    private void RefreshTeacherControls()
    {
        var appState = AppState.Instance;
        var isTeacherRole = appState != null && appState.Role != null && appState.Role.activeRole == Role.Teacher;
        var isHost = appState != null && appState.Session != null && appState.Session.isHost;
        var canStart = isTeacherRole || isHost;

        if (startTeachingButton != null)
        {
            startTeachingButton.gameObject.SetActive(canStart);
        }
    }

    private void OnStartTeachingClicked()
    {
        if (classSessionState == null)
        {
            SetStatus("Session state unavailable.");
            Debug.LogWarning($"{LogTag} Start clicked but classSessionState is null.");
            return;
        }

        var ok = classSessionState.RequestStartTeaching();
        if (!ok)
        {
            SetStatus("Start denied (teacher/authority required).");
            return;
        }

        SetStatus("Teaching started");
    }

    private void OnTeachingStartedChanged(bool started)
    {
        SetStatus(started ? "Teaching started" : "Waiting for teacher…");
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}
