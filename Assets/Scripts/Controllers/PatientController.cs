using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static MainController;

public class PatientController : AbstractController
{
    #region Unity Input
    [SerializeField]
    private TMP_Text m_patientNameText;
    [SerializeField]
    private Button m_continueTreatmentButton;
    #endregion
    private Color disabledColor = new Color(0, 0, 0, 0.5f);
    private Color enabledColor = new Color(0, 0, 0, 1f);

    private string FilePath
    {
        get { return PinchConstants.PatientsDirectoryPath + m_mainController.CurrentPatient.Id; }
    }

    public new void Start()
    {
        base.Start();
        if (m_patientNameText == null || m_continueTreatmentButton == null)
        {
            Debug.LogError("All fields must be initilized in PatientController");
        }
    }

    public override void InitializeController()
    {
       if (m_mainController == null) return;// The start routine did not called yet. We will call InitializeController from start().
        m_patientNameText.text = m_mainController.CurrentPatient?.FullName;

        if (m_mainController.CurrentPatient.MotionRange.IsCalibrated)
        {
            m_continueTreatmentButton.interactable = true;
            m_continueTreatmentButton.GetComponent<Image>().color = enabledColor;
            m_continueTreatmentButton.GetComponentInChildren<TMP_Text>().color = enabledColor;
        }
        else
        {
            m_continueTreatmentButton.GetComponent<Image>().color = disabledColor;
            m_continueTreatmentButton.GetComponentInChildren<TMP_Text>().color = disabledColor;
            m_continueTreatmentButton.interactable = false;
        }

        if (m_mainController.CurrentPatient.CurrentTreatment != null &&
            m_mainController.CurrentPatient.CurrentTreatment.Plan != null &&
            m_mainController.CurrentPatient.CurrentTreatment.Plan.Count > 0)
        {
            m_continueTreatmentButton.GetComponentInChildren<TMP_Text>().text = "Continue treatment plan";
        }
        else
        {
            m_continueTreatmentButton.GetComponentInChildren<TMP_Text>().text = "Start treatment plan";
        }
    }

    #region On Click Events
    public void OnViewPatientDetailsClicked()
    {
        m_mainController.ShowScreen(ScreensIndex.ViewPatientDetails);
    }

    public void OnPinchRangeCalibrationClicked()
    {
        MainController.LoadCalibrationScene();
    }

    public void OnContinueTreatmentPlanClicked()
    {
        m_mainController.StartTherapy();
        if (m_continueTreatmentButton.GetComponentInChildren<TMP_Text>().text == "Start treatment plan")
            m_continueTreatmentButton.GetComponentInChildren<TMP_Text>().text = "Continue treatment plan";
        MainController.PrintToLog("Therapy start: " + m_mainController.CurrentPatient.FullName + " " + ", id: " + m_mainController.CurrentPatient.Id
                + ". Monitored by the therapist: " + m_mainController.LoggedInTherapist.FirstName + " " + m_mainController.LoggedInTherapist.LastName + ", id: "
                + m_mainController.LoggedInTherapist.Username + ".", MainController.LogType.Information);
    }

    public void OnSendPatientReportInMailClicked()
    {
        MailsSystemManager.SendPatientReportInMail(m_mainController.LoggedInTherapist, m_mainController.CurrentPatient);
        m_mainController.ShowPopup(MessageController.MessageType.Succsess, "Mail will be sent in few minutes.");
        PrintToLog("Patient's report sent to therapist mail: Patient " + m_mainController.CurrentPatient.FullName + " " + ", id: " + m_mainController.CurrentPatient.Id
                + ". Requsted by the therapist: " + m_mainController.LoggedInTherapist.FirstName + " " + m_mainController.LoggedInTherapist.LastName + ", id: "
                + m_mainController.LoggedInTherapist.Username + ".", MainController.LogType.Information);
    }

    public void OnHomeButtonClicked()
    {
        try
        {
            m_mainController.ShowPopup(MessageController.MessageType.Warning,
                "You pressed home, you will logout patient. are you sure?", OnPopupAnswer);
        }
        catch (Exception e)
        {
            PrintToLog(e.ToString(), MainController.LogType.Error);
        }
    }

    public void OnSettingsClicked()
    {
        m_mainController.SettingsPanel.SetActive(true);
    }
    #endregion

    private void OnPopupAnswer(MessageController.MessageAnswer answer)
    {
        if (answer == MessageController.MessageAnswer.Yes)
        {
            m_mainController.LogoutPatient(FilePath);
        }
        else if (answer == MessageController.MessageAnswer.Cancel)
        {
            m_mainController.ShowScreen(ScreensIndex.PatientScreen);
        }
    }
}
