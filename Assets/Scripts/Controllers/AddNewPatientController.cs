using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static MainController;
using TMPro;
using System.IO;
using System.Linq;

public class AddNewPatientController : AbstractController
{
    #region Unity Input
    [SerializeField]
    private TMP_Text m_idField;
    [SerializeField]
    private TMP_Text m_fullNameField;
    [SerializeField]
    private Slider m_genderSlider; // 0 = Male, 1 = Female
    [SerializeField]
    private TMP_Text m_heightField;
    [SerializeField]
    private TMP_Text m_weightField;
    [SerializeField]
    private Slider m_targetHandSlider; // 0 = Left, 1 = Right
    [SerializeField]
    private TMP_Text m_statusText;

    #endregion

    private string FilePath
    {
        get { return PinchConstants.PatientsDirectoryPath + m_idField.text; }
    }

    private Patient GetUserFromGui()
    {
        Patient patient = new Patient();
        patient.Id = m_idField.text;
        patient.FullName = m_fullNameField.text;
        if (m_genderSlider.GetComponent<Slider>().value == 0)
            patient.Gender = "Male";
        else patient.Gender = "Female";
        patient.Height = m_heightField.text;
        patient.Weight = m_weightField.text;
        if (m_targetHandSlider.GetComponent<Slider>().value == 0)
            patient.Hand = "Left";
        else
            patient.Hand = "Right";
        return patient;
    }

    public new void Start()
    {
        base.Start();

        if (m_idField == null || m_fullNameField == null || m_genderSlider == null ||
            m_heightField == null || m_weightField == null || m_statusText == null || m_targetHandSlider == null)
        {
            Debug.LogError("All fields must be initilized in AddNewPatientController");
        }
    }

    #region On Click Events
    public void OnConfirmClicked()
    {
        int id;
        try
        {
            m_statusText.text = string.Empty;
            if (m_idField.text == string.Empty || m_fullNameField.text == string.Empty ||
                m_heightField.text == string.Empty || m_weightField.text == string.Empty)
            {
                m_statusText.text = "Please fill all data";
                return;
            }
            if (!int.TryParse(m_idField.text, out id))
            {
                m_statusText.text = "Please enter only numbers on ID field";
                return;
            }
            if (!m_fullNameField.text.All(char.IsLetter) && !m_fullNameField.text.Contains(" "))
            {
                m_statusText.text = "Please enter only letters on full name field";
                return;
            }
            if (!int.TryParse(m_heightField.text, out id))
            {
                m_statusText.text = "Please enter only numbers on height field";
                return;
            }
            if (!int.TryParse(m_weightField.text, out id))
            {
                m_statusText.text = "Please enter only numbers on weight field";
                return;
            }
            if (!QuestFileManager.SaveNewPatientToFile(GetUserFromGui(), FilePath))
            {
                m_statusText.text = "Patient already exists";
                return;
            }
            m_mainController.SetCurrentPatient(GetUserFromGui());
            m_mainController.ShowPopup(MessageController.MessageType.Succsess, "New patient account has been created!", OnPopupAnswer);
            PrintToLog("New patient account was created: " + m_mainController.CurrentPatient.FullName + " " + ", id: " + m_mainController.CurrentPatient.Id 
                + ". Created by the therapist: " + m_mainController.LoggedInTherapist.FirstName + " " + m_mainController.LoggedInTherapist.LastName + ", id: " 
                + m_mainController.LoggedInTherapist.Username + ".", MainController.LogType.Information);
        }
        catch (Exception e)
        {
            PrintToLog(e.ToString(), MainController.LogType.Error);
        }
    }

    public void OnHomeButtonClicked()
    {
        try
        {
            if (!(m_idField.text == string.Empty) || !(m_fullNameField.text == string.Empty) ||
                !(m_heightField.text == string.Empty) || !(m_weightField.text == string.Empty))
                // When some of the input fields where not empty
                m_mainController.ShowPopup(MessageController.MessageType.Warning,
                "You pressed home, the details will be deleted. are you sure?", OnPopupAnswer);
            else
                m_mainController.ShowScreen(ScreensIndex.TherapistScreen);
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

    private void OnPopupAnswer(MessageController.MessageAnswer answer)
    {
        if (answer == MessageController.MessageAnswer.Ok) // When new patient user was created
        {
            m_mainController.ShowScreen(ScreensIndex.PatientScreen);
            ClearScreen();
        }
        else if (answer == MessageController.MessageAnswer.Yes) // When some of the input fields where not empty and home button was pressed
        {
            m_mainController.ShowScreen(ScreensIndex.TherapistScreen);
            ClearScreen();
        }
    }
    #endregion

    public void ClearScreen()
    {
        m_idField.text = string.Empty;
        m_fullNameField.text = string.Empty;
        m_genderSlider.GetComponent<Slider>().value = 0;
        m_heightField.text = string.Empty;
        m_weightField.text = string.Empty;
        m_targetHandSlider.GetComponent<Slider>().value = 0;
        m_statusText.text = string.Empty;
    }
}
