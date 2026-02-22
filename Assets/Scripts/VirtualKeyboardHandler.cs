using System;
using TMPro;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Experimental.UI;

/*
Setup:
1) Keep this on the XR keyboard controller object.
2) Assign NonNativeKeyboard and target TMP_InputField as before.
3) In Mobile_AR mode, this script auto-guards MRTK keyboard behavior and allows native TMP keyboard flow.
4) This script listens for AppState device-mode changes and re-hooks keyboard events when mode flips.
*/
public class VirtualKeyboardHandler : MonoBehaviour
{
    private const string LogTag = "[VirtualKeyboardHandler]";

    [Header("References")]
    [SerializeField] private NonNativeKeyboard keyboard;
    [SerializeField] private TMP_InputField separateInputField;
    public TMP_InputField CurrentTarget => separateInputField;

    private bool isTextSubmitted;
    private bool hooksBound;

    private void OnEnable()
    {
        SubscribeDeviceMode();
        RefreshKeyboardHooks();
    }

    private void OnDisable()
    {
        UnbindHooks();
        UnsubscribeDeviceMode();
    }

    private void OnDeviceModeChanged(DeviceMode mode)
    {
        RefreshKeyboardHooks();
    }

    private void RefreshKeyboardHooks()
    {
        if (!IsXrMode())
        {
            UnbindHooks();
            Debug.Log($"{LogTag} Mobile_AR mode detected. MRTK keyboard hooks disabled.");
            return;
        }

        if (keyboard == null)
        {
            Debug.LogWarning($"{LogTag} keyboard reference is null.");
            return;
        }

        if (hooksBound)
        {
            return;
        }

        keyboard.OnTextSubmitted += HandleTextSubmitted;
        keyboard.OnTextUpdated += HandleTextUpdated;
        hooksBound = true;
        Debug.Log($"{LogTag} XR_HMD mode detected. MRTK keyboard hooks enabled.");
    }

    private void UnbindHooks()
    {
        if (!hooksBound || keyboard == null)
        {
            return;
        }

        keyboard.OnTextSubmitted -= HandleTextSubmitted;
        keyboard.OnTextUpdated -= HandleTextUpdated;
        hooksBound = false;
    }

    private void HandleTextSubmitted(object sender, EventArgs e)
    {
        if (!IsXrMode())
        {
            return;
        }

        if (keyboard == null || separateInputField == null)
        {
            return;
        }

        isTextSubmitted = true;
        separateInputField.text = keyboard.InputField != null ? keyboard.InputField.text : string.Empty;
        Debug.Log($"{LogTag} Keyboard submitted: {separateInputField.text}");
    }

    private void HandleTextUpdated(string updatedText)
    {
        if (!IsXrMode())
        {
            return;
        }

        if (separateInputField == null)
        {
            return;
        }

        if (!isTextSubmitted)
        {
            separateInputField.text = updatedText;
        }
    }

    public void ResetSubmitState()
    {
        isTextSubmitted = false;
    }

    public void SetTargetInputField(TMP_InputField target)
    {
        separateInputField = target;

        // Do not clear text on target swap; this can wipe user input when UI roots toggle.
        ResetSubmitState();

        if (!IsXrMode())
        {
            Debug.Log($"{LogTag} Target set for Mobile_AR: {target?.name}");
            return;
        }

        Debug.Log($"{LogTag} Target set for XR_HMD: {target?.name}");
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

    private static bool IsXrMode()
    {
        return AppState.Instance != null && AppState.Instance.CurrentDeviceMode == DeviceMode.XR_HMD;
    }
}
