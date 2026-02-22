using Fusion;
using TMPro;
using UnityEngine;

/*
Setup:
1) Add this script to PlayerAvatar_Net prefab root.
2) It creates a world-space TextMeshPro label at runtime (no inspector refs required).
3) Label shows ME/REMOTE, and local role suffix for ME if available.
*/
public sealed class PlayerNameTag : NetworkBehaviour
{
    private const string LogTag = "[PlayerNameTag]";

    [SerializeField] private Vector3 localOffset = new Vector3(0f, 1.9f, 0f);

    private TextMeshPro label;
    private Transform labelTransform;

    public override void Spawned()
    {
        CreateLabelIfNeeded();
        UpdateLabelText();
        Debug.Log($"{LogTag} Label created for '{name}'.");
    }

    private void LateUpdate()
    {
        if (label == null)
        {
            return;
        }

        UpdateLabelText();

        if (Camera.main != null)
        {
            labelTransform.forward = Camera.main.transform.forward;
        }
    }

    private void CreateLabelIfNeeded()
    {
        if (label != null)
        {
            return;
        }

        var go = new GameObject("PlayerNameLabel");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = localOffset;
        go.transform.localRotation = Quaternion.identity;

        label = go.AddComponent<TextMeshPro>();
        label.fontSize = 2.5f;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.outlineWidth = 0.2f;
        label.outlineColor = Color.black;
        label.enableWordWrapping = false;

        labelTransform = go.transform;
    }

    private void UpdateLabelText()
    {
        if (label == null)
        {
            return;
        }

        var isLocal = Object != null && Object.HasInputAuthority;
        if (!isLocal)
        {
            label.text = "REMOTE";
            return;
        }

        var suffix = string.Empty;
        var appState = AppState.Instance;
        if (appState != null && appState.Role != null)
        {
            if (appState.Role.activeRole == Role.Teacher)
            {
                suffix = " (Teacher)";
            }
            else if (appState.Role.activeRole == Role.Student)
            {
                suffix = " (Student)";
            }
        }

        label.text = "ME" + suffix;
    }
}
