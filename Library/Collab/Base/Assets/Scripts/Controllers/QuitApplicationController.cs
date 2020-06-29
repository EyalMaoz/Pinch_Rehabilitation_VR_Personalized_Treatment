using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitApplicationController : MonoBehaviour
{
    private MainController m_mainController;

    private void Awake()
    {
        m_mainController = FindObjectOfType<MainController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (m_mainController == null)
        {
            MainController.PrintToLog("Couldnt find MainController in QuitApplicationController.", MainController.LogType.Error);
        }
    }

    public void OnQuitApplicationClicked()
    {
        try
        {
            if (m_mainController.CurrentPatient != null)
                m_mainController.LogoutPatient(PinchConstants.PatientsDirectoryPath + m_mainController.CurrentPatient.Id);
            if (m_mainController.LoggedInTherapist != null)
                m_mainController.LogoutTherapist();
            //If we are running in the editor
#if UNITY_EDITOR
            //Stop playing the scene
            UnityEditor.EditorApplication.isPlaying = false;
            return;
#endif
            Application.Quit();

        }
        catch (Exception e)
        {
            MainController.PrintToLog(e.ToString(), MainController.LogType.Error);
        }

    }
}
