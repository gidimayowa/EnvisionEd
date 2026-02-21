using TMPro;
using UnityEngine;

public sealed class SessionEntryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BootRouter bootRouter;
    [SerializeField] private VirtualKeyboardHandler keyboardHandler;
    [SerializeField] private TMP_InputField classInputField;
    [SerializeField] private TMP_Text errorText;
    [SerializeField] private string emptyClassMessage = "Please enter a class code or name.";

    private void OnEnable()
    {
        if (keyboardHandler != null && classInputField != null)
        {
            keyboardHandler.SetTargetInputField(classInputField);
        }
    }

    public void OnJoinClassClicked()
    {
        Debug.Log("[SessionEntryUI] OnJoinClassClicked triggered");
        var classValue = GetClassValue();
        Debug.Log("[SessionEntryUI] Raw input: '" + classValue + "'");
        if (string.IsNullOrWhiteSpace(classValue))
        {
            ShowError(emptyClassMessage);
            return;
        }

        if (AppState.Instance == null)
        {
            Debug.LogError("[SessionEntryUI] AppState.Instance is null.");
            return;
        }

        AppState.Instance.SetSessionAsClient(classValue);
        Debug.Log("[SessionEntryUI] AppState session now: isHost=" + AppState.Instance.Session.isHost +
                  ", class='" + AppState.Instance.Session.classCodeOrName + "'");
        Debug.Log("[SessionEntryUI] Student session set: " + classValue);
        NavigateToLobby();
    }

    public void OnCreateClassClicked()
    {
        Debug.Log("[SessionEntryUI] OnCreateClassClicked triggered");
        var classValue = GetClassValue();
        Debug.Log("[SessionEntryUI] Raw input: '" + classValue + "'");
        if (string.IsNullOrWhiteSpace(classValue))
        {
            ShowError(emptyClassMessage);
            return;
        }

        if (AppState.Instance == null)
        {
            Debug.LogError("[SessionEntryUI] AppState.Instance is null.");
            return;
        }

        AppState.Instance.SetSessionAsHost(classValue);
        Debug.Log("[SessionEntryUI] AppState session now: isHost=" + AppState.Instance.Session.isHost +
                  ", class='" + AppState.Instance.Session.classCodeOrName + "'");
        Debug.Log("[SessionEntryUI] Teacher session set: " + classValue);
        NavigateToLobby();
    }

    private string GetClassValue()
    {
        HideError();

        if (classInputField == null)
        {
            Debug.LogError("[SessionEntryUI] classInputField is not assigned.");
            return string.Empty;
        }

        return classInputField.text != null ? classInputField.text.Trim() : string.Empty;
    }

    private void NavigateToLobby()
    {
        if (bootRouter == null)
        {
            Debug.LogError("[SessionEntryUI] BootRouter is not assigned.");
            return;
        }

        Debug.Log("[SessionEntryUI] Routing now... bootRouter null? " + (bootRouter == null));
        bootRouter.RouteFromState();
    }

    private void ShowError(string message)
    {
        if (errorText == null)
        {
            return;
        }

        errorText.text = message;
        errorText.gameObject.SetActive(true);
    }

    private void HideError()
    {
        if (errorText == null)
        {
            return;
        }

        errorText.text = string.Empty;
        errorText.gameObject.SetActive(false);
    }
}
