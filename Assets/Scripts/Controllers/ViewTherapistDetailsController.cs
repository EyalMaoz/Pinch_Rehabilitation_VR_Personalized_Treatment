using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MainController;
using TMPro;
using System;

public class ViewTherapistDetailsController : AbstractController
{
    #region Unity Ouput
    [SerializeField]
    private TMP_Text m_usernameField;
    [SerializeField]
    private TMP_Text m_passwordField;
    [SerializeField]
    private TMP_Text m_firstNameField;
    [SerializeField]
    private TMP_Text m_lastNameField;
    [SerializeField]
    private TMP_Text m_emailField;
    #endregion

    private string FilePath
    {
        get { return PinchConstants.TherapistsDirectoryPath + m_mainController.CurrentPatient.Id; }
    }

    public override void InitializeController()
    {
        if (m_mainController == null) return;// The start routine did not called yet. We will call InitializeController from start().
        m_usernameField.text = m_mainController.LoggedInTherapist?.Username;
        m_passwordField.text = m_mainController.LoggedInTherapist?.Password;
        m_firstNameField.text = m_mainController.LoggedInTherapist?.FirstName;
        m_lastNameField.text = m_mainController.LoggedInTherapist?.LastName;
        m_emailField.text = m_mainController.LoggedInTherapist?.Email;
    }

    public new void Start()
    {
        base.Start();

        if (m_usernameField == null || m_passwordField == null || m_firstNameField == null ||
            m_lastNameField == null || m_emailField == null)
        {
            Debug.LogError("All fields must be initilized in ViewTherapistDetailsController");
        }
    }

    #region On Click Events
    public void OnEditDetailsClicked()
    {
        m_mainController.ShowScreen(ScreensIndex.EditTherapistDetails);
    }

    public void OnHomeButtonClicked()
    {
        m_mainController.ShowScreen(ScreensIndex.TherapistScreen);
    }

    public void OnSettingsClicked()
    {
        m_mainController.SettingsPanel.SetActive(true);
    }

    #endregion

    public void ClearScreen()
    {
        m_usernameField.text = string.Empty;
        m_passwordField.text = string.Empty;
        m_firstNameField.text = string.Empty;
        m_lastNameField.text = string.Empty;
        m_emailField.text = string.Empty;
    }
}
