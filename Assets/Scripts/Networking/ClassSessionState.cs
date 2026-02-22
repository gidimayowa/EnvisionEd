using System;
using Fusion;
using UnityEngine;

/*
Setup (Class 1 scene):
1) Create a GameObject: ClassSessionState.
2) Add components: NetworkObject + ClassSessionState (this script).
3) Keep this as a scene network object loaded via Fusion.
4) Link ClassRoomUI.classSessionState to this component.
*/
public sealed class ClassSessionState : NetworkBehaviour
{
    private const string LogTag = "[ClassSessionState]";

    public event Action<bool> TeachingStartedChanged;

    [Networked, OnChangedRender(nameof(OnTeachingStartedChangedRender))]
    public NetworkBool TeachingStarted { get; set; }

    public override void Spawned()
    {
        RaiseTeachingStartedChanged();
    }

    public bool RequestStartTeaching()
    {
        var appState = AppState.Instance;
        var isTeacherRole = appState != null && appState.Role != null && appState.Role.activeRole == Role.Teacher;
        var isHost = appState != null && appState.Session != null && appState.Session.isHost;

        if (!isTeacherRole && !isHost)
        {
            Debug.LogWarning($"{LogTag} Request denied. User is not teacher/host.");
            return false;
        }

        if (Object == null || !Object.HasStateAuthority)
        {
            Debug.LogWarning($"{LogTag} Request denied. No state authority in Shared Mode.");
            return false;
        }

        if (TeachingStarted)
        {
            Debug.Log($"{LogTag} Teaching already started.");
            return true;
        }

        TeachingStarted = true;
        Debug.Log($"{LogTag} TeachingStarted set to true.");
        RaiseTeachingStartedChanged();
        return true;
    }

    private void OnTeachingStartedChangedRender()
    {
        RaiseTeachingStartedChanged();
    }

    private void RaiseTeachingStartedChanged()
    {
        TeachingStartedChanged?.Invoke(TeachingStarted);
    }
}
