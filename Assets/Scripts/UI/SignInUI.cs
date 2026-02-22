using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class SignInUI : MonoBehaviour
{
    private const string LogTag = "[SignInUI]";

    [Header("Input")]
    [SerializeField] private TMP_InputField xrEmailInputField;
    [SerializeField] private TMP_InputField mobileEmailInputField;
    [SerializeField] private VirtualKeyboardHandler keyboardHandler;

    [Header("Screens")]
    [SerializeField] private GameObject signInScreen;
    [SerializeField] private GameObject studentTeacherOptions;

    [Header("Error UI")]
    [SerializeField] private TMP_Text errorText;
    [SerializeField] private string emptyEmailMessage = "user hasn't signed in";

    private TMP_InputField activeEmailInputField;
    private Coroutine bindCoroutine;

    private void OnEnable()
    {
        SubscribeDeviceMode();
        RestartDelayedBind();
    }

    private void OnDisable()
    {
        UnsubscribeDeviceMode();

        if (bindCoroutine != null)
        {
            StopCoroutine(bindCoroutine);
            bindCoroutine = null;
        }
    }

    public void OnSignInClicked()
    {
        HideError();
        ResolveActiveInputField();

        if (activeEmailInputField == null)
        {
            Debug.LogError($"{LogTag} No active email input field assigned.");
            return;
        }

        var email = activeEmailInputField.text != null ? activeEmailInputField.text.Trim() : string.Empty;
        if (string.IsNullOrWhiteSpace(email))
        {
            ShowError(emptyEmailMessage);
            return;
        }

        if (AppState.Instance == null)
        {
            Debug.LogError($"{LogTag} AppState is null. Cannot sign in.");
            return;
        }

        AppState.Instance.SetEmail(email);
        Debug.Log($"{LogTag} AppState email now: {AppState.Instance.User?.email}");

        if (signInScreen != null)
        {
            signInScreen.SetActive(false);
        }

        if (studentTeacherOptions != null)
        {
            studentTeacherOptions.SetActive(true);
            studentTeacherOptions.transform.SetAsLastSibling();
        }

        Debug.Log($"{LogTag} Switched to Student Teacher Options for email: {email}");
    }

    private void OnDeviceModeChanged(DeviceMode mode)
    {
        RestartDelayedBind();
    }

    private void RestartDelayedBind()
    {
        if (bindCoroutine != null)
        {
            StopCoroutine(bindCoroutine);
        }

        bindCoroutine = StartCoroutine(BindKeyboardNextFrame());
    }

    private IEnumerator BindKeyboardNextFrame()
    {
        yield return null;

        ResolveActiveInputField();

        var mode = AppState.Instance != null ? AppState.Instance.CurrentDeviceMode : DeviceMode.Mobile_AR;
        var isActive = activeEmailInputField != null && activeEmailInputField.gameObject.activeInHierarchy;

        if (keyboardHandler != null && isActive)
        {
            keyboardHandler.SetTargetInputField(activeEmailInputField);
        }

        Debug.Log($"{LogTag} Binding keyboard target -> {(activeEmailInputField != null ? activeEmailInputField.name : "null")} mode={mode} active={isActive}");

        if (mode == DeviceMode.Mobile_AR && isActive)
        {
            activeEmailInputField.ActivateInputField();
            activeEmailInputField.Select();

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(activeEmailInputField.gameObject);
            }
        }

        bindCoroutine = null;
    }

    private void ResolveActiveInputField()
    {
        var mode = AppState.Instance != null ? AppState.Instance.CurrentDeviceMode : DeviceMode.Mobile_AR;

        if (mode == DeviceMode.XR_HMD)
        {
            activeEmailInputField = xrEmailInputField != null ? xrEmailInputField : mobileEmailInputField;
        }
        else
        {
            activeEmailInputField = mobileEmailInputField != null ? mobileEmailInputField : xrEmailInputField;
        }

        if (activeEmailInputField == null)
        {
            Debug.LogWarning($"{LogTag} Could not resolve active input field for mode {mode}.");
            return;
        }

        if (!activeEmailInputField.gameObject.activeInHierarchy)
        {
            var fallback = activeEmailInputField == xrEmailInputField ? mobileEmailInputField : xrEmailInputField;
            if (fallback != null && fallback.gameObject.activeInHierarchy)
            {
                activeEmailInputField = fallback;
            }
        }
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
