using UnityEngine;

public class AppStateSmokeTest : MonoBehaviour
{
    private void Start()
    {
        var app = AppState.Instance;

        if (app == null)
        {
            Debug.LogError("[AppStateSmokeTest] AppState.Instance is null. Add AppState component to a GameObject in the first loaded scene.");
            return;
        }

        Debug.Log($"[AppStateSmokeTest] Initial -> User null: {app.User == null}, Role: {app.Role.activeRole}, Session code: '{app.Session.classCodeOrName}'");

        app.SetEmail("test@school.com");
        Debug.Log($"[AppStateSmokeTest] After SetEmail -> userId: {app.User.userId}, email: {app.User.email}");

        app.SetRole(Role.Teacher);
        Debug.Log($"[AppStateSmokeTest] After SetRole -> Role: {app.Role.activeRole}");

        app.SetSessionAsHost("BIO101");
        Debug.Log($"[AppStateSmokeTest] Host Session -> isHost: {app.Session.isHost}, class: {app.Session.classCodeOrName}");

        app.SetSessionAsClient("CLASS-CODE-7");
        Debug.Log($"[AppStateSmokeTest] Client Session -> isHost: {app.Session.isHost}, class: {app.Session.classCodeOrName}");

        app.ResetSession();
        Debug.Log($"[AppStateSmokeTest] After ResetSession -> isHost: {app.Session.isHost}, class: '{app.Session.classCodeOrName}'");

        app.SignOut();
        Debug.Log($"[AppStateSmokeTest] After SignOut -> User null: {app.User == null}, Role: {app.Role.activeRole}, class: '{app.Session.classCodeOrName}'");
    }

    [ContextMenu("AppState Smoke Test Instructions")]
    private void PrintInstructions()
    {
        Debug.Log("[AppStateSmokeTest] Set values, load another scene, and verify AppState values persist. Ensure only one AppState exists across scenes.");
    }
}
