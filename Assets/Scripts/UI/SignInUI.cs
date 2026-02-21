using TMPro;
using UnityEngine;

public class SignInUI : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private TMP_InputField emailInputField;
    [SerializeField] private VirtualKeyboardHandler keyboardHandler;

    [Header("Screens")]
    [SerializeField] private GameObject signInScreen;
    [SerializeField] private GameObject studentTeacherOptions;

    [Header("Error UI")]
    [SerializeField] private TMP_Text errorText;
    [SerializeField] private string emptyEmailMessage = "user hasn't signed in";

    private void OnEnable()
    {
        if (keyboardHandler != null && emailInputField != null)
        {
            keyboardHandler.SetTargetInputField(emailInputField);
        }
    }

    public void OnSignInClicked()
    {
        HideError();

        if (emailInputField == null)
        {
            Debug.LogError("[SignInUI] Email Input Field is not assigned.");
            return;
        }

        string email = emailInputField.text != null ? emailInputField.text.Trim() : string.Empty;
        if (string.IsNullOrWhiteSpace(email))
        {
            ShowError(emptyEmailMessage);
            return;
        }

        AppState.Instance.SetEmail(email);
        Debug.Log("[SignInUI] AppState email now: " + AppState.Instance.User.email);

        if (signInScreen != null)
        {
            signInScreen.SetActive(false);
        }

        if (studentTeacherOptions != null)
        {
            studentTeacherOptions.SetActive(true);
            studentTeacherOptions.transform.SetAsLastSibling();
        }

        Debug.Log("[SignInUI] Switched to Student Teacher Options for email: " + email);
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
