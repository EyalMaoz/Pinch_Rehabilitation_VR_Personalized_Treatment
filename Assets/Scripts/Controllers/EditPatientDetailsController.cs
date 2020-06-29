using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static MainController;
using TMPro;
using System.Linq;
using System;

public class EditPatientDetailsController : AbstractController
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
        get { return PinchConstants.PatientsDirectoryPath + m_mainController.CurrentPatient.Id; }
    }

    private Patient GetUserFromGui()
    {
        Patient patient = new Patient();
        patient.FullName = m_fullNameField.text;
        if (m_genderSlider.GetComponent<Slider>().value == 0)
            patient.Gender = "Male";
        else
            patient.Gender = "Female";
        patient.Height = m_heightField.text;
        patient.Weight = m_weightField.text;
        if (m_targetHandSlider.GetComponent<Slider>().value == 0)
            patient.Hand = "Left"; 
        else
            patient.Hand = "Right";
        return patient;
    }

    public override void InitializeController()
    {
        if (m_mainController == null) return;// The start routine did not called yet. We will call InitializeController from start().

        m_idField.text = m_mainController.CurrentPatient?.Id;
        m_fullNameField.text = m_mainController.CurrentPatient?.FullName;
        if (m_mainController.CurrentPatient.Gender == "Female")
            m_genderSlider.GetComponent<Slider>().value = 1;
        else if (m_mainController.CurrentPatient.Gender == "Male")
            m_genderSlider.GetComponent<Slider>().value = 0;
        m_heightField.text = m_mainController.CurrentPatient?.Height;
        m_weightField.text = m_mainController.CurrentPatient?.Weight;
        if (m_mainController.CurrentPatient.Hand == "Right")
            m_genderSlider.GetComponent<Slider>().value = 1;
        else if (m_mainController.CurrentPatient.Hand == "Left")
            m_genderSlider.GetComponent<Slider>().value = 0;

    }

    public new void Start()
    {
        base.Start();

        if (m_idField == null || m_fullNameField == null || m_genderSlider == null ||
            m_heightField == null || m_weightField == null || m_statusText == null || m_targetHandSlider == null)
        {
            Debug.LogError("All fields must be initilized in EditPatientDetailsController");
        }
    }

    #region On Click Events

    public void OnConfirmClicked()
    {
        int id;
        try
        {
            m_statusText.text = string.Empty;
            if (m_fullNameField.text == string.Empty || m_heightField.text == string.Empty || m_weightField.text == string.Empty)
            {
                m_statusText.text = "Please fill all data";
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
            if (QuestFileManager.GetPatientFromFile(FilePath) == null)
            {
                m_statusText.text = "Patient doesn't exists";
                return;
            }
            Patient patient = GetUserFromGui();
            m_mainController.CurrentPatient.FullName = patient.FullName;
            m_mainController.CurrentPatient.Gender = patient.Gender;
            m_mainController.CurrentPatient.Height = patient.Height;
            m_mainController.CurrentPatient.Weight = patient.Weight;
            m_mainController.CurrentPatient.Hand = patient.Hand;
            QuestFileManager.UpdatePatientOnFile(m_mainController.CurrentPatient, FilePath);
            m_mainController.ShowPopup(MessageController.MessageType.Succsess, "Patient details has been updated!", OnPopupAnswerFromConfirm);
            m_statusText.text = string.Empty;
            PrintToLog("Patient's details update: " + m_mainController.CurrentPatient.FullName + " " + ", id: " + m_mainController.CurrentPatient.Id
                + ". Updated by the therapist: " + m_mainController.LoggedInTherapist.FirstName + " " + m_mainController.LoggedInTherapist.LastName + ", id: "
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
            Patient patient = GetUserFromGui();
            if (m_mainController.CurrentPatient.FullName != patient.FullName || m_mainController.CurrentPatient.Gender != patient.Gender
                || m_mainController.CurrentPatient.Height != patient.Height || m_mainController.CurrentPatient.Weight != patient.Weight
                || m_mainController.CurrentPatient.Hand != patient.Hand)
            { // When some of the details of the patient were changed
                m_mainController.ShowPopup(MessageController.MessageType.Warning,
                "You pressed home, the changes will not be saved and you will logout patient. are you sure?", OnPopupAnswerFromHome);
            }
            else
            {
                m_mainController.ShowPopup(MessageController.MessageType.Warning,
                "You pressed home, you will logout patient. are you sure?", OnPopupAnswerFromHome);
            }

        }
        catch (Exception e)
        {
            PrintToLog(e.ToString(), MainController.LogType.Error);
        }
    }

    public void OnBackButtonClicked()
    {
        try
        {
            Patient patient = GetUserFromGui();
            if (m_mainController.CurrentPatient.FullName != patient.FullName || m_mainController.CurrentPatient.Gender != patient.Gender
                || m_mainController.CurrentPatient.Height != patient.Height || m_mainController.CurrentPatient.Weight != patient.Weight
                || m_mainController.CurrentPatient.Hand != patient.Hand)
            { // When some of the details of the patient were changed
                m_mainController.ShowPopup(MessageController.MessageType.Warning,
                "You pressed back, the changes will not save. are you sure?", OnPopupAnswerFromBack);
            }
            else
            {
                m_mainController.ShowScreen(ScreensIndex.ViewPatientDetails);
                m_statusText.text = string.Empty;
            }
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

    private void OnPopupAnswerFromBack(MessageController.MessageAnswer answer)
    {
        if (answer == MessageController.MessageAnswer.Yes)
        {
            m_mainController.ShowScreen(ScreensIndex.ViewPatientDetails);
            m_statusText.text = string.Empty;
        }

    }

    private void OnPopupAnswerFromHome(MessageController.MessageAnswer answer)
    {
        if (answer == MessageController.MessageAnswer.Yes)
        {
            m_mainController.ShowScreen(ScreensIndex.TherapistScreen);
            m_statusText.text = string.Empty;
        }

    }

    private void OnPopupAnswerFromConfirm(MessageController.MessageAnswer answer)
    {
        m_mainController.ShowScreen(ScreensIndex.ViewPatientDetails);
    }
}
