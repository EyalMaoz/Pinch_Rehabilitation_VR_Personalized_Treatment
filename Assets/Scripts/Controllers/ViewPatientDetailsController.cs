using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MainController;
using TMPro;
using System;

public class ViewPatientDetailsController : AbstractController
{
    #region Unity Ouput
    [SerializeField]
    private TMP_Text m_idField;
    [SerializeField]
    private TMP_Text m_fullNameField;
    [SerializeField]
    private TMP_Text m_genderField;
    [SerializeField]
    private TMP_Text m_heightField;
    [SerializeField]
    private TMP_Text m_weightField;
    [SerializeField]
    private TMP_Text m_targetHandField;
    #endregion

    private string FilePath
    {
        get { return PinchConstants.PatientsDirectoryPath + m_mainController.CurrentPatient.Id; }
    }

    public override void InitializeController()
    {
        if (m_mainController == null) return;// The start routine did not called yet. We will call InitializeController from start().
        m_idField.text = m_mainController.CurrentPatient?.Id;
        m_fullNameField.text = m_mainController.CurrentPatient?.FullName;
        m_genderField.text = m_mainController.CurrentPatient?.Gender;
        m_heightField.text = m_mainController.CurrentPatient?.Height;
        m_weightField.text = m_mainController.CurrentPatient?.Weight;
        m_targetHandField.text = m_mainController.CurrentPatient?.Hand;
    }

    public new void Start()
    {
        base.Start();

        if (m_idField == null || m_fullNameField == null || m_genderField == null ||
            m_heightField == null || m_weightField == null || m_targetHandField == null)
        {
            Debug.LogError("All fields must be initilized in ViewPatientDetailsController");
        }
    }

    #region On Click Events
    public void OnEditDetailsClicked()
    {
        m_mainController.ShowScreen(ScreensIndex.EditPatientDetails);
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

    public void OnBackButtonClicked()
    {
        m_mainController.ShowScreen(ScreensIndex.PatientScreen);
    }

    public void OnSettingsClicked()
    {
        m_mainController.SettingsPanel.SetActive(true);
    }

    private void OnPopupAnswer(MessageController.MessageAnswer answer)
    {
        if (answer == MessageController.MessageAnswer.Yes)
        {
            m_mainController.LogoutPatient(FilePath);
        }
    }
    #endregion

    public void ClearScreen()
    {
        m_idField.text = string.Empty;
        m_fullNameField.text = string.Empty;
        m_genderField.text = string.Empty;
        m_heightField.text = string.Empty;
        m_weightField.text = string.Empty;
        m_targetHandField.text = string.Empty;
    }
}
