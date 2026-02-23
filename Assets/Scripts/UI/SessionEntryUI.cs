using TMPro;
using UnityEngine;
using System.Collections.Generic;

public sealed class SessionEntryUI : MonoBehaviour
{
    private const string LogTag = "[SessionEntryUI]";

    [Header("References")]
    [SerializeField] private BootRouter bootRouter;
    [SerializeField] private VirtualKeyboardHandler keyboardHandler;
    [SerializeField] private TMP_InputField classInputField;
    [SerializeField] private TMP_InputField xrClassInputField;
    [SerializeField] private TMP_InputField mobileClassInputField;
    [SerializeField] private TMP_Text errorText;
    [SerializeField] private string emptyClassMessage = "Please enter a class code or name.";

    private readonly List<TMP_InputField> inputFieldBuffer = new List<TMP_InputField>();

    private void Awake()
    {
        // Backward compatibility with existing scenes that only assigned classInputField.
        if (xrClassInputField == null && classInputField != null)
        {
            xrClassInputField = classInputField;
        }
    }

    private void OnEnable()
    {
        SubscribeDeviceMode();
        var activeInput = ResolveActiveInputField();

        if (keyboardHandler != null && activeInput != null)
        {
            keyboardHandler.SetTargetInputField(activeInput);
        }
    }

    private void OnDisable()
    {
        UnsubscribeDeviceMode();
    }

    private void OnDeviceModeChanged(DeviceMode mode)
    {
        var activeInput = ResolveActiveInputField();

        if (keyboardHandler != null && activeInput != null)
        {
            keyboardHandler.SetTargetInputField(activeInput);
        }
    }

    public void OnJoinClassClicked()
    {
        Debug.Log($"{LogTag} OnJoinClassClicked triggered");
        var classValue = GetClassValue();
        Debug.Log($"{LogTag} Raw input: '{classValue}'");
        if (string.IsNullOrWhiteSpace(classValue))
        {
            ShowError(emptyClassMessage);
            return;
        }

        if (AppState.Instance == null)
        {
            Debug.LogError($"{LogTag} AppState.Instance is null.");
            return;
        }

        AppState.Instance.SetSessionAsClient(classValue);
        Debug.Log($"{LogTag} AppState session now: isHost=" + AppState.Instance.Session.isHost +
                  ", class='" + AppState.Instance.Session.classCodeOrName + "'");
        Debug.Log($"{LogTag} Student session set: " + classValue);
        NavigateToLobby();
    }

    public void OnCreateClassClicked()
    {
        Debug.Log($"{LogTag} OnCreateClassClicked triggered");
        var classValue = GetClassValue();
        Debug.Log($"{LogTag} Raw input: '{classValue}'");
        if (string.IsNullOrWhiteSpace(classValue))
        {
            ShowError(emptyClassMessage);
            return;
        }

        if (AppState.Instance == null)
        {
            Debug.LogError($"{LogTag} AppState.Instance is null.");
            return;
        }

        AppState.Instance.SetSessionAsHost(classValue);
        Debug.Log($"{LogTag} AppState session now: isHost=" + AppState.Instance.Session.isHost +
                  ", class='" + AppState.Instance.Session.classCodeOrName + "'");
        Debug.Log($"{LogTag} Teacher session set: " + classValue);
        NavigateToLobby();
    }

    private string GetClassValue()
    {
        HideError();

        var activeInput = ResolveActiveInputField();
        if (activeInput == null)
        {
            Debug.LogError($"{LogTag} No active class input field resolved.");
            return string.Empty;
        }

        return activeInput.text != null ? activeInput.text.Trim() : string.Empty;
    }

    private void NavigateToLobby()
    {
        if (bootRouter == null)
        {
            Debug.LogError($"{LogTag} BootRouter is not assigned.");
            return;
        }

        Debug.Log($"{LogTag} Routing now... bootRouter null? " + (bootRouter == null));
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

    private TMP_InputField ResolveActiveInputField()
    {
        var mode = AppState.Instance != null ? AppState.Instance.CurrentDeviceMode : DeviceMode.Mobile_AR;
        TMP_InputField preferred = mode == DeviceMode.XR_HMD ? xrClassInputField : mobileClassInputField;
        TMP_InputField fallback = mode == DeviceMode.XR_HMD ? mobileClassInputField : xrClassInputField;

        if (preferred != null && preferred.gameObject.activeInHierarchy)
        {
            return preferred;
        }

        if (fallback != null && fallback.gameObject.activeInHierarchy)
        {
            return fallback;
        }

        if (classInputField != null && classInputField.gameObject.activeInHierarchy)
        {
            return classInputField;
        }

        inputFieldBuffer.Clear();
        GetComponentsInChildren(true, inputFieldBuffer);
        for (var i = 0; i < inputFieldBuffer.Count; i++)
        {
            var candidate = inputFieldBuffer[i];
            if (candidate != null && candidate.gameObject.activeInHierarchy)
            {
                return candidate;
            }
        }

        return preferred ?? fallback ?? classInputField;
    }

    private void SubscribeDeviceMode()
    {
        if (AppState.Instance != null)
        {
            AppState.Instance.DeviceModeChanged -= OnDeviceModeChanged;
            AppState.Instance.DeviceModeChanged += OnDeviceModeChanged;
        }
    }

    private void UnsubscribeDeviceMode()
    {
        if (AppState.Instance != null)
        {
            AppState.Instance.DeviceModeChanged -= OnDeviceModeChanged;
        }
    }
}
