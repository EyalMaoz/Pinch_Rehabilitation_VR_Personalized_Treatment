using Assets.Game.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static MessageController;
using static Assets.Game.Scripts.TherapyData;

public class MainController : MonoBehaviour
{
    public Therapist LoggedInTherapist { get; private set; }
    public Patient CurrentPatient { get; set; }
    public TreatmentPlan DefaultPlan = new TreatmentPlan();

    [SerializeField]
    public Settings CurrentSettings;
    [SerializeField]
    public Settings DefaultSettings;
    [SerializeField]
    public GameObject SettingsPanel;
    [SerializeField]
    public Material SkyBoxMat;
    public List<Button> NonImageButtons;
    private static bool isFirstTime = true;
    #region Unity Input
    [SerializeField]
    private MessageController m_messageController;
    [SerializeField]
    private GameObject[] m_screens;
    [SerializeField]
    private AbstractController[] m_controllers;
    #endregion

    private void Awake()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite) ||
                    !Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
        }
    }

    private void Start()
    {
        Button[] buttons;
        try
        {
            if (!isFirstTime)
            {
                Destroy(gameObject);
                return;
            }
            if (m_messageController == null || m_screens == null || m_controllers == null
                || SettingsPanel == null || SkyBoxMat == null)
            {
                Debug.LogError("All fields must be initilized in MainController");
            }
            QuestFileManager.InitialPaths();
            //ShowScreen(ScreensIndex.Login);

            buttons = Resources.FindObjectsOfTypeAll<Button>();
            NonImageButtons = new List<Button>();

            if (buttons != null)
            {
                foreach (Button button in buttons)
                {
                    if (button.GetComponentInChildren<Image>().sprite != null)
                    {
                        if (button.GetComponentInChildren<Image>().sprite.name == "UISprite")
                        {
                            string test = button.GetComponentInChildren<TMP_Text>().text;
                            if (button.GetComponentInChildren<TMP_Text>().text != string.Empty && button.GetComponentInChildren<TMP_Text>().text != "Ok" &&
                                button.GetComponentInChildren<TMP_Text>().text != "Yes" && button.GetComponentInChildren<TMP_Text>().text != "Cancel" &&
                                button.GetComponentInChildren<TMP_Text>().text != "Save Changes" && button.GetComponentInChildren<TMP_Text>().text != "Button")
                            {
                                NonImageButtons.Add(button);
                            }
                        }
                    }
                }
            }
            InitializeColors();
            isFirstTime = false;
            DontDestroyOnLoad(gameObject);// dont destroy menu
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }
        catch (Exception e)
        {
            PrintToLog(e.ToString(), MainController.LogType.Error);
        }
    }

    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if (SceneManager.GetActiveScene().name == "MainScene")
        {
            gameObject.SetActive(true);
        }
        else
            gameObject.SetActive(false);
    }

    private void OnApplicationQuit()
    {
        if (CurrentPatient != null)
            LogoutPatient(PinchConstants.PatientsDirectoryPath + CurrentPatient.Id);
        if (LoggedInTherapist != null)
            LogoutTherapist();
    }
    #region Methods

    public void StartTherapy()
    {
        INITFORTEST();
        SceneManager.LoadScene("Game");
    }


    public void INITFORTEST()
    {
        if (CurrentPatient == null)
        {
            CurrentPatient = new Patient();
            CurrentPatient.CurrentTreatment = DefaultPlan;
            CurrentPatient.TreatmentsHistory = new List<TreatmentPlan>();
        }
        DefaultPlan.Plan = new List<Challenge>(){
             new Challenge("Challenge1", new List<PinchAction>()
                 {
                     new PinchAction(PinchType.Tip2, (int)(CurrentPatient.MotionRange.Tip2* _DifficultyRange),1),
                     new PinchAction(PinchType.Tip2, (int)(CurrentPatient.MotionRange.Tip2* _DifficultyRange),1),
                 }),
             new Challenge("Challenge2", new List<PinchAction>()
                 {
                     new PinchAction(PinchType.Tip3, (int)(CurrentPatient.MotionRange.Tip3* _DifficultyRange),5),
                     new PinchAction(PinchType.Tip3, (int)(CurrentPatient.MotionRange.Tip3* _DifficultyRange),5),
                 }),
             new Challenge("Challenge3", new List<PinchAction>()
                 {
                     new PinchAction(PinchType.Pad2, (int)(CurrentPatient.MotionRange.Pad2* _DifficultyRange),10),
                     new PinchAction(PinchType.Pad2, (int)(CurrentPatient.MotionRange.Pad2* _DifficultyRange),10),
                 }),
             new Challenge("Challenge4", new List<PinchAction>()
                 {
                     new PinchAction(PinchType.Pad3,(int)(CurrentPatient.MotionRange.Pad3* _DifficultyRange)),
                 })
        };
        DefaultPlan.CreationTime = DateTime.Now.ToShortDateString() + " " +
                DateTime.Now.ToLongTimeString();
        if (CurrentPatient.CurrentTreatment == null || CurrentPatient.CurrentTreatment.Plan == null || CurrentPatient.CurrentTreatment.Plan.Count == 0)
        {
            CurrentPatient.CurrentTreatment = DefaultPlan;
            CurrentPatient.TreatmentsHistory = new List<TreatmentPlan>();
        }
    }

    public static void LoadMainScene()
    {
        SceneManager.LoadScene("MainScene");
    }

    public static void LoadCalibrationScene()
    {
        SceneManager.LoadScene("CalibrationScene");
    }

    #region Login & Logout

    public void SetLoggedInTherapist(Therapist therapist)
    {
        if (LoggedInTherapist != null)// in case there was login while a therapist was already loggeding we want to know.
            PrintToLog("The therapist " + LoggedInTherapist.FirstName + " (" + LoggedInTherapist.Username +
                ")was stil logged in while " + therapist.FirstName + " (" + therapist.Username + ") loggedin.", LogType.Error);
        LoggedInTherapist = therapist;
        PrintToLog("Login: " + LoggedInTherapist.FirstName + " " + LoggedInTherapist.LastName + " id:" + LoggedInTherapist.Username, LogType.Information);
        CurrentSettings.SkyBoxColor = LoggedInTherapist.Settings.SkyBoxColor;
        CurrentSettings.ButtonsColorBlock = LoggedInTherapist.Settings.ButtonsColorBlock;
        CurrentSettings.ButtonsTextColor = LoggedInTherapist.Settings.ButtonsTextColor;
        UpdateColors();
    }

    public void LogoutTherapist()
    {
        PrintToLog("Logout: " + LoggedInTherapist.FirstName + " " + LoggedInTherapist.LastName + " id:" + LoggedInTherapist.Username, LogType.Information);
        CurrentSettings.SkyBoxColor = DefaultSettings.SkyBoxColor;
        CurrentSettings.ButtonsColorBlock = DefaultSettings.ButtonsColorBlock;
        CurrentSettings.ButtonsTextColor = DefaultSettings.ButtonsTextColor;
        UpdateColors();
        QuestFileManager.UpdateTherapistOnFile(LoggedInTherapist, PinchConstants.TherapistsDirectoryPath + LoggedInTherapist.Username);
        LoggedInTherapist = null;
        ShowScreen(ScreensIndex.Login);
    }
    #endregion

    #region Patients
    public void SetCurrentPatient(Patient patient)
    {
        if (CurrentPatient != null) // in case there was login while a patient was already loggeding we want to know.
        {
            PrintToLog("The patient " + CurrentPatient.FullName + " (" + CurrentPatient.Id +
                ") was stil logged in while " + patient.FullName + " (" + patient.Id + ") loggedin.", LogType.Error);
        }
        CurrentPatient = patient;
    }

    public void LogoutPatient(string filePath)
    {
        if (!QuestFileManager.UpdatePatientOnFile(CurrentPatient, filePath))
        {
            PrintToLog("The patient " + CurrentPatient.FullName + " (" + CurrentPatient.Id +
                ") file could not be found in order to update.", LogType.Error);
        }
        CurrentPatient = null;
        ShowScreen(ScreensIndex.TherapistScreen);
    }
    #endregion

    #region Utilities

    /// <summary>
    /// This function display "screen" and hides all the rest
    /// </summary>
    /// <param name="screen"></param>
    public void ShowScreen(ScreensIndex screen)
    {
        for (int i = 0; i < m_screens.Length; i++)
        {
            if (i == (int)screen)
            {
                m_screens[i].SetActive(true);
            }
            else
            {
                m_screens[i].SetActive(false);
            }
        }
        m_controllers[(int)screen]?.InitializeController();
        TalesFromTheRift.CanvasKeyboard.Close();
    }

    public void ShowPopup(MessageType messageType, string message, GotAnswerDelegate function = null)
    {
        m_messageController.ShowMessage(messageType, message, function);
    }

    public static void PrintToLog(string message, LogType type)
    {
        try
        {
            string toLog = DateTime.Now.ToShortDateString() + " " +
                DateTime.Now.ToLongTimeString() + "    " + message;
            StreamWriter sw;
            switch (type)
            {
                case LogType.Information:
                    sw = File.AppendText(PinchConstants.InformationLogFilePath);
                    break;
                default://case LogMessageType.Error:
                    sw = File.AppendText(PinchConstants.ErrorLogFilePath);
                    break;
            }
            sw.WriteLine(toLog);
            sw.Close();
        }
        catch (Exception)
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite) ||
                    !Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageWrite);
                Permission.RequestUserPermission(Permission.ExternalStorageRead);
                PrintToLog(message, type);// if we didnt have the permissions - request and rewrite to log
            }
            else
                Debug.LogError(message);
        }
    }

    public void InitializeColors()
    {
        try
        {
            CurrentSettings.ButtonsColorBlock = DefaultSettings.ButtonsColorBlock;
            CurrentSettings.ButtonsTextColor = DefaultSettings.ButtonsTextColor;
            CurrentSettings.SkyBoxColor = DefaultSettings.SkyBoxColor;
            SkyBoxMat.SetColor(Shader.PropertyToID("_Tint"), DefaultSettings.SkyBoxColor);
        }
        catch (Exception e)
        {
            PrintToLog(e.ToString(), MainController.LogType.Error);
        }

    }

    public void UpdateColors()
    {
        try
        {
            SkyBoxMat.SetColor(Shader.PropertyToID("_Tint"), CurrentSettings.SkyBoxColor);
            foreach (Button button in NonImageButtons)
            {
                button.colors = CurrentSettings.ButtonsColorBlock;
                button.GetComponentInChildren<TMP_Text>().color = CurrentSettings.ButtonsTextColor;
            }
        }
        catch (Exception e)
        {
            PrintToLog(e.ToString(), MainController.LogType.Error);
        }
    }
    #endregion

    #endregion

    #region Data Types
    public delegate void GotAnswerDelegate(MessageAnswer answer);

    [Serializable]
    public class MotionRange
    {
        public float Pad2;
        public float Pad3;
        public float Tip2;
        public float Tip3;

        public MotionRange()
        {
            Pad2 = .1f;
            Pad3 = .1f;
            Tip2 = .1f;
            Tip3 = .1f;
        }

    }

    [Serializable]
    public class Therapist
    {
        public string Username; //therapist id number
        public string Password;
        public string FirstName;
        public string LastName;
        public string Email;
        public Settings Settings;
    }

    [Serializable]
    public class Patient
    {
        public string Id;
        public string FullName;
        public string Gender;
        public string Height;
        public string Weight;
        /// <summary>
        /// "Left" for left, "Right" for right.
        /// </summary>
        public string Hand;
        public MotionRange MotionRange;
        public TreatmentPlan CurrentTreatment;
        public List<TreatmentPlan> TreatmentsHistory;

        public Patient()
        {
            MotionRange = new MotionRange();
            CurrentTreatment = null;
            TreatmentsHistory = new List<TreatmentPlan>();
            Hand = "Right";
        }
    }

    public enum ScreensIndex
    {
        None = -1, Login = 0, AddNewTherapist, TherapistScreen,
        ViewTherapistDetails, EditTherapistDetails, SearchPatient,
        AddNewPatient, PatientScreen, ViewPatientDetails,
        EditPatientDetails, CreateNewTreatment, Message, Settings,
        ColorPicker
    }

    public enum LogType
    {
        Information, Error
    }
    #endregion
}
