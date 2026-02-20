using UnityEngine;

public class StudentTeacherOptionsNavigator : MonoBehaviour
{
    [Header("UI Containers")]
    [SerializeField] private GameObject signInScreen;
    [SerializeField] private GameObject studentTeacherOptions;
    [SerializeField] private GameObject studentHomeScreen;
    [SerializeField] private GameObject teacherHomeScreen;

    public bool GoToStudentTeacherOptions()
    {
        if (studentTeacherOptions == null)
        {
            return false;
        }

        if (studentHomeScreen != null)
        {
            studentHomeScreen.SetActive(false);
        }

        if (teacherHomeScreen != null)
        {
            teacherHomeScreen.SetActive(false);
        }

        if (signInScreen != null)
        {
            bool optionsInsideSignInRoot = studentTeacherOptions.transform.IsChildOf(signInScreen.transform);
            if (!optionsInsideSignInRoot)
            {
                signInScreen.SetActive(false);
            }
            else
            {
                for (int i = 0; i < signInScreen.transform.childCount; i++)
                {
                    Transform child = signInScreen.transform.GetChild(i);
                    bool childContainsOptions = studentTeacherOptions.transform.IsChildOf(child);
                    if (childContainsOptions)
                    {
                        continue;
                    }

                    child.gameObject.SetActive(false);
                }
            }
        }

        studentTeacherOptions.SetActive(true);
        studentTeacherOptions.transform.SetAsLastSibling();
        Debug.Log("[StudentTeacherOptionsNavigator] Transitioned to Student Teacher Options. " +
                  "signInScreenActive=" + (signInScreen != null && signInScreen.activeSelf) +
                  ", studentTeacherOptionsActive=" + studentTeacherOptions.activeSelf);
        return true;
    }

    public void GoToStudentTeacherOptionsFromHome()
    {
        if (studentTeacherOptions == null)
        {
            return;
        }

        if (studentHomeScreen != null)
        {
            studentHomeScreen.SetActive(false);
        }

        if (teacherHomeScreen != null)
        {
            teacherHomeScreen.SetActive(false);
        }

        studentTeacherOptions.SetActive(true);
        studentTeacherOptions.transform.SetAsLastSibling();
        Debug.Log("[StudentTeacherOptionsNavigator] Returned from home screen to Student Teacher Options.");
    }
}
