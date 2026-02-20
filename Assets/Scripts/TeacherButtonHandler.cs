using UnityEngine;

public class TeacherButtonHandler : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuRoot;
    [SerializeField] private GameObject teacherHomeRoot;
    [SerializeField] private GameObject studentHomeRoot;

    public void OnTeacherClicked()
    {
        if (mainMenuRoot != null)
        {
            mainMenuRoot.SetActive(false);
        }

        if (studentHomeRoot != null)
        {
            studentHomeRoot.SetActive(false);
        }

        if (teacherHomeRoot != null)
        {
            teacherHomeRoot.SetActive(true);
        }
    }

    public void OnBackToMainMenuClicked()
    {
        if (teacherHomeRoot != null)
        {
            teacherHomeRoot.SetActive(false);
        }

        if (studentHomeRoot != null)
        {
            studentHomeRoot.SetActive(false);
        }

        if (mainMenuRoot != null)
        {
            mainMenuRoot.SetActive(true);
        }
    }
}
