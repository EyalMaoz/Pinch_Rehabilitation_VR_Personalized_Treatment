using TMPro;
using UnityEngine;
using static MainController;

public class TherapistController : AbstractController
{
    #region Unity Input
    [SerializeField]
    private TMP_Text m_welcomeUserText;
    #endregion


    public new void Start()
    {
        base.Start();

        if (m_welcomeUserText == null)
        {
            Debug.LogError("All fields must be initilized in TherapistController");
        }
    }

    public void OnLogoutClicked()
    {
        m_mainController.ShowPopup(MessageController.MessageType.Warning, "Are you sure you want to logout?",OnPopupReturn);
    }

    public void OnSearchPatientClicked()
    {
        m_mainController.ShowScreen(ScreensIndex.SearchPatient);
    }

    public void OnAddNewPatientClicked()
    {
        m_mainController.ShowScreen(ScreensIndex.AddNewPatient);
    }

    public void OnSendAllPatientsReportClicked()
    {
        MailsSystemManager.SendAllReportsInMail(m_mainController.LoggedInTherapist);
        m_mainController.ShowPopup(MessageController.MessageType.Succsess, "Mail will be sent in few minutes.");
    }

    public void OnPopupReturn(MessageController.MessageAnswer ans)
    {
        if(ans == MessageController.MessageAnswer.Yes)
        {
            m_mainController.Logout();
        }
    }

    public void InitializeTherapistName(string therapistFirstName)
    {
        m_welcomeUserText.text = "Welcome " + therapistFirstName + "!";
    }
}
