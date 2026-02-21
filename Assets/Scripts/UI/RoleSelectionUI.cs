using UnityEngine;

public class RoleSelectionUI : MonoBehaviour
{
    [SerializeField] private BootRouter bootRouter;

    private void Awake()
    {
        if (bootRouter == null)
        {
            bootRouter = FindFirstObjectByType<BootRouter>();
        }
    }

    public void OnStudentClicked()
    {
        if (AppState.Instance == null)
        {
            Debug.LogError("[RoleSelectionUI] AppState.Instance is null.");
            return;
        }

        AppState.Instance.SetRole(Role.Student);
        Debug.Log("[RoleSelectionUI] Role set to: " + Role.Student);

        if (bootRouter == null)
        {
            Debug.LogError("[RoleSelectionUI] BootRouter is not assigned.");
            return;
        }

        bootRouter.GoToStudentJoin();
    }

    public void OnTeacherClicked()
    {
        if (AppState.Instance == null)
        {
            Debug.LogError("[RoleSelectionUI] AppState.Instance is null.");
            return;
        }

        AppState.Instance.SetRole(Role.Teacher);
        Debug.Log("[RoleSelectionUI] Role set to: " + Role.Teacher);

        if (bootRouter == null)
        {
            Debug.LogError("[RoleSelectionUI] BootRouter is not assigned.");
            return;
        }

        bootRouter.GoToTeacherCreate();
    }
}
