using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MainController;
using TMPro;
using System;

public class SearchPatientController : AbstractController
{
    #region Unity Input
    [SerializeField]
    private TMP_Text m_idField;
    [SerializeField]
    private TMP_Text m_statusText;
    #endregion

    private string FilePath
    {
        get { return PinchConstants.PatientsDirectoryPath + m_idField.text; }
    }

    public new void Start()
    {
        base.Start();

        if (m_idField == null)
        {
            Debug.LogError("Id field must be initilized in SearchPatientController");
        }
    }

    #region On Click Events
    public void OnSearchClicked()
    {
        int id;
        try
        {
            m_statusText.text = "";
            if (m_idField.text == string.Empty)
            {
                m_statusText.text = "Please enter patient ID";
                return;
            }
            if (!int.TryParse(m_idField.text, out id))
            {
                m_statusText.text = "Please enter only numbers on ID field";
                return;
            }
            Patient patient = QuestFileManager.GetPatientFromFile(FilePath);
            //Patient patient = new Patient() { Id = m_idField.text, FullName = "Bar Attaly", Gender = "Female", Height = "172", Weight = "70" };
            if (patient == null)
            {
                m_statusText.text = "Patient doesn't exists.";
                return;
            }
            m_mainController.SetCurrentPatient(patient);
            m_mainController.ShowScreen(ScreensIndex.PatientScreen);
            ClearScreen();
        }
        catch (Exception e)
        {
            PrintToLog(e.ToString(), MainController.LogType.Error);
        }
    }

    public void OnHomeButtonClicked()
    {
        m_mainController.ShowScreen(ScreensIndex.TherapistScreen);
        ClearScreen();
    }

    public void OnSettingsClicked()
    {
        m_mainController.SettingsPanel.SetActive(true);
    }
    #endregion

    public void ClearScreen()
    {
        m_idField.text = string.Empty;
        m_statusText.text = string.Empty;
    }
}
