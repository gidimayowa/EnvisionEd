using System;
using TMPro;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Experimental.UI;

public class VirtualKeyboardHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NonNativeKeyboard keyboard;
    [SerializeField] private TMP_InputField separateInputField;

    private bool isTextSubmitted;

    private void OnEnable()
    {
        if (keyboard == null)
        {
            return;
        }

        keyboard.OnTextSubmitted += HandleTextSubmitted;
        keyboard.OnTextUpdated += HandleTextUpdated;
    }

    private void OnDisable()
    {
        if (keyboard == null)
        {
            return;
        }

        keyboard.OnTextSubmitted -= HandleTextSubmitted;
        keyboard.OnTextUpdated -= HandleTextUpdated;
    }

    private void HandleTextSubmitted(object sender, EventArgs e)
    {
        if (keyboard == null || separateInputField == null)
        {
            return;
        }

        isTextSubmitted = true;
        separateInputField.text = keyboard.InputField != null ? keyboard.InputField.text : string.Empty;
        Debug.Log("Keyboard submitted: " + separateInputField.text);
    }

    private void HandleTextUpdated(string updatedText)
    {
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
}
