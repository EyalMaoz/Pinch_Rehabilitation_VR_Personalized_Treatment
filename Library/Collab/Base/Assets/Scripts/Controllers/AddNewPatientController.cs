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
    private TMP_Text m_statusText;
    [SerializeField]
    #endregion

    private Patient GetUserFromGui()
    {
        Patient user = null;
        if (user == null)
        {
            user = new Patient()
            {
                Id = m_idField.text,
                FullName = m_fullNameField.text,
                Gender = m_genderSlider.GetComponent<Slider>().value.ToString(),
                Height = m_heightField.text,
                Weight = m_weightField.text
            };
        }
        user.Id = m_idField.text;
        user.FullName = m_fullNameField.text;
        user.Gender = m_genderSlider.GetComponent<Slider>().value.ToString();
        user.Height = m_heightField.text;
        user.Weight = m_weightField.text;
        return user;
    }

    private string FilePath
    {
        get { return PinchConstants.PatientsDirectoryPath + m_idField.text; }
    }

    public new void Start()
    {
        base.Start();

        if (m_idField == null || m_fullNameField == null || m_genderSlider == null || 
            m_heightField == null || m_weightField == null || m_statusText == null)
        {
            Debug.LogError("All fields must be initilized in AddNewPatientController");
        }
    }

    public void OnConfirmClicked()
    {
        int id;
        try
        {
            m_statusText.text = "";
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
            if (File.Exists(FilePath))
            {
                m_statusText.text = "Patient is already exists";
                return;
            }
            if (!m_fullNameField.text.All(char.IsLetter))
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

            m_mainController.ShowPopup(MessageController.MessageType.Succsess, "New patient account has Created!", OnPopupAnswer);
            m_mainController.SetCurrentPatient(GetUserFromGui());
        }
        catch (Exception e)
        {
            PrintToLog(e.Message, MainController.LogType.Error);
            m_statusText.text = e.Message;
        }
    }

    private void OnPopupAnswer(MessageController.MessageAnswer answer)
    {
        if (answer == MessageController.MessageAnswer.Ok) // When new patient user was created
        {
            m_mainController.ShowScreen(ScreensIndex.PatientScreen);
            ClearScreen();
        }
        else if (answer == MessageController.MessageAnswer.Yes) // When some of the input fields where not empty and back button was pressed
        {
            m_mainController.ShowScreen(ScreensIndex.TherapistScreen);
            ClearScreen();
        }
        else if (answer == MessageController.MessageAnswer.Cancel) // When some of the input fields where not empty and back button was pressed
        {
            m_mainController.ShowScreen(ScreensIndex.AddNewPatient);
        }
    }

    public void OnMainMenuButtonClicked()
    {
        try
        {
            if (!(m_idField.text == string.Empty) || !(m_fullNameField.text == string.Empty) ||
                !(m_heightField.text == string.Empty) || !(m_weightField.text == string.Empty))
                // When some of the input fields where not empty
                m_mainController.ShowPopup(MessageController.MessageType.Warning,
                "You pressed main menu, the details will delete. are you sure?", OnPopupAnswer);
            else
                m_mainController.ShowScreen(ScreensIndex.TherapistScreen);
        }
        catch (Exception e)
        {
            PrintToLog(e.Message, MainController.LogType.Error);
        }
    }

    public void ClearScreen()
    {
        m_idField.text = string.Empty;
        m_fullNameField.text = string.Empty;
        m_genderSlider.GetComponent<Slider>().value = 0;
        m_heightField.text = string.Empty;
        m_weightField.text = string.Empty;
        m_statusText.text = string.Empty;
    }
}
