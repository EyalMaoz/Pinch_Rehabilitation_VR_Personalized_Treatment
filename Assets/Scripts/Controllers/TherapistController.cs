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

    public override void InitializeController()
    {
        if (m_mainController == null) return;// The start routine did not called yet. We will call InitializeController from start().
        m_welcomeUserText.text = "Welcome " + m_mainController.LoggedInTherapist?.FirstName + "!";
    }

    #region On Click Events

    public void OnLogoutClicked()
    {
        m_mainController.ShowPopup(MessageController.MessageType.Warning, "Are you sure you want to logout?", OnPopupAnswer);
    }

    public void OnSettingsClicked()
    {
        m_mainController.SettingsPanel.SetActive(true);
    }

    public void OnMyDetailsClicked()
    {
        m_mainController.ShowScreen(ScreensIndex.ViewTherapistDetails);
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
        PrintToLog("All patient's reports sent to therapist mail: " + m_mainController.LoggedInTherapist.FirstName + " " + m_mainController.LoggedInTherapist.LastName + ", id: "
                + m_mainController.LoggedInTherapist.Username + ".", MainController.LogType.Information);
    }

    private void OnPopupAnswer(MessageController.MessageAnswer answer)
    {
        if (answer == MessageController.MessageAnswer.Yes)
        {
            m_mainController.LogoutTherapist();
        }
    }
    #endregion

}
