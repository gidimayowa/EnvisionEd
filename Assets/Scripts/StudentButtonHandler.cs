using UnityEngine;

public class StudentButtonHandler : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuRoot;
    [SerializeField] private GameObject studentHomeRoot;
    [SerializeField] private GameObject teacherHomeRoot;

    public void OnStudentClicked()
    {
        if (mainMenuRoot != null)
        {
            mainMenuRoot.SetActive(false);
        }

        if (teacherHomeRoot != null)
        {
            teacherHomeRoot.SetActive(false);
        }

        if (studentHomeRoot != null)
        {
            studentHomeRoot.SetActive(true);
        }
    }

    public void OnBackToMainMenuClicked()
    {
        if (studentHomeRoot != null)
        {
            studentHomeRoot.SetActive(false);
        }

        if (teacherHomeRoot != null)
        {
            teacherHomeRoot.SetActive(false);
        }

        if (mainMenuRoot != null)
        {
            mainMenuRoot.SetActive(true);
        }
    }
}
