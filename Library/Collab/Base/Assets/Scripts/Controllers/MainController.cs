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
using UnityEngine.Audio;

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
    private LineRenderer m_laserPointerRenderer;
    [SerializeField]
    private MessageController m_messageController;
    [SerializeField]
    private GameObject[] m_screens;
    [SerializeField]
    private AbstractController[] m_controllers;
    [SerializeField]
    private GameObject m_scenesMusicHandler;
    public AudioClip m_mainSceneSound;
    public AudioClip m_gameSceneSound;
    public AudioSource m_audioSource;
    [SerializeField]
    private AudioMixerSnapshot volumeDown;
    [SerializeField]
    private AudioMixerSnapshot volumeUp;
    #endregion

    private void Awake()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite) ||
                    !Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
        }
        m_audioSource = FindObjectOfType<AudioSource>();
        if(m_audioSource==null)
        {
            PrintToLog("m_audioSource is null", LogType.Error);
        }
    }

    private void Start()
    {
        try
        {
            if (!isFirstTime)
            {
                Destroy(gameObject);
                return;
            }
            if (m_messageController == null || m_screens == null || m_controllers == null
                || SettingsPanel == null || SkyBoxMat == null || m_scenesMusicHandler == null
                || m_mainSceneSound == null || m_gameSceneSound == null || m_audioSource == null
                || volumeDown == null || volumeUp == null)
            {
                Debug.LogError("All fields must be initilized in MainController");
            }
            isFirstTime = false;
            QuestFileManager.InitializePaths();
            InitializeColors();
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
        PlayLevelMusic();
        if (SceneManager.GetActiveScene().name == "MainScene")
        {
            ShowScreen(ScreensIndex.PatientScreen);
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void Update()// maybe add message about the controller
    {
        var activeControl = OVRInput.GetActiveController();
        if (activeControl == OVRInput.Controller.None || activeControl == OVRInput.Controller.Hands || activeControl == OVRInput.Controller.LHand || activeControl == OVRInput.Controller.RHand)
        {
            m_laserPointerRenderer.enabled = false;
        }
        else if (activeControl == OVRInput.Controller.RTouch || activeControl == OVRInput.Controller.LTouch)
        {
            m_laserPointerRenderer.enabled = true;
        }
    }

    //private void OnApplicationQuit()
    //{
    //    if (CurrentPatient != null)
    //        LogoutPatient(PinchConstants.PatientsDirectoryPath + CurrentPatient.Id);
    //    if (LoggedInTherapist != null)
    //        LogoutTherapist();
    //}

    #region Methods

    public void SetDefaultPlanIfNeeded()
    {
        DefaultPlan.Plan = new List<Challenge>()
                {
                new Challenge("Challenge1", new List<PinchAction>()// Red
                 {
                     new PinchAction(PinchType.Tip2,(int)CurrentPatient.MotionRange.Tip2 * _DifficultyRange,1),
                     new PinchAction(PinchType.Tip2,(int)CurrentPatient.MotionRange.Tip2 * _DifficultyRange,1)
                }),
                new Challenge("Challenge2", new List<PinchAction>()// Yellow
                 {
                     new PinchAction(PinchType.Tip3, (int)CurrentPatient.MotionRange.Tip3 * _DifficultyRange,1),
                     new PinchAction(PinchType.Tip3, (int)CurrentPatient.MotionRange.Tip3 * _DifficultyRange,1)
                 }),
                new Challenge("Challenge3", new List<PinchAction>()// Green
                 {
                     new PinchAction(PinchType.Pad2,(int)CurrentPatient.MotionRange.Pad2 * _DifficultyRange,1),
                     new PinchAction(PinchType.Pad2,(int)CurrentPatient.MotionRange.Pad2 * _DifficultyRange,1)
                 }),
                new Challenge("Challenge4", new List<PinchAction>()// Blue
                 {
                     new PinchAction(PinchType.Pad3,(int)CurrentPatient.MotionRange.Pad3 * _DifficultyRange,1),
                     new PinchAction(PinchType.Pad3,(int)CurrentPatient.MotionRange.Pad3 * _DifficultyRange,1)
                 })
        };
        DefaultPlan.TreatmentNumber = 1;
        DefaultPlan.CreationTime = DateTime.Now.ToShortDateString() + " " +
                DateTime.Now.ToLongTimeString();

        //if (CurrentPatient == null)// For tests!!
        //{
        //    CurrentPatient = new Patient();
        //    CurrentPatient.CurrentTreatment = DefaultPlan;
        //}//

        if (CurrentPatient.CurrentTreatment == null || CurrentPatient.CurrentTreatment.Plan == null || CurrentPatient.CurrentTreatment.Plan.Count == 0)
        {
            CurrentPatient.CurrentTreatment = DefaultPlan;
        }
    }

    public static void LoadMainScene()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void StartTherapy()
    {
        SetDefaultPlanIfNeeded();
        SceneManager.LoadScene("Game");
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
        PrintToLog("Therapist login: " + LoggedInTherapist.FirstName + " " + LoggedInTherapist.LastName + ", id:" + LoggedInTherapist.Username + ".", LogType.Information);
        CurrentSettings.SkyBoxColor = LoggedInTherapist.Settings.SkyBoxColor;
        CurrentSettings.ButtonsColorBlock = LoggedInTherapist.Settings.ButtonsColorBlock;
        CurrentSettings.ButtonsTextColor = LoggedInTherapist.Settings.ButtonsTextColor;
        UpdateColors();
    }

    public void LogoutTherapist()
    {
        PrintToLog("Therapist logout: " + LoggedInTherapist.FirstName + " " + LoggedInTherapist.LastName + ", id:" + LoggedInTherapist.Username + ".", LogType.Information);
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
        PrintToLog("Patient login: " + CurrentPatient.FullName + " " + ", id:" + CurrentPatient.Id + ".", LogType.Information);
    }

    public void LogoutPatient(string filePath)
    {
        if (!QuestFileManager.UpdatePatientOnFile(CurrentPatient, filePath))
        {
            PrintToLog("The patient " + CurrentPatient.FullName + " (" + CurrentPatient.Id +
                ") file could not be found in order to update.", LogType.Error);
        }
        CurrentPatient = null;
        PrintToLog("Patient logout: " + CurrentPatient.FullName + " " + ", id:" + CurrentPatient.Id + ".", LogType.Information);
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
            // buttons without image:

            Button[] buttons;
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
            // settings and skybox:
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

    #region MusicMethods

    public void PlayLevelMusic()
    {
        //This switch looks at the last loadedLevel number using the scene index in build settings to decide which music clip to play.
        switch (SceneManager.GetActiveScene().name)
        {
            //If scene index is 0 (main scene) assign the clip m_mainSceneSound to audioSource
            case "MainScene":
                FadeUp(0.01f);   //Fade up the volume very quickly, over resetTime seconds (.01 by default)
                m_audioSource.clip = m_mainSceneSound;
                m_audioSource.Play();
                break;
            //If scene index is 1 (game scene) assign the clip m_gameSceneSound to audioSource
            case "Game":
                FadeUp(0.01f);
                m_audioSource.clip = m_gameSceneSound;
                m_audioSource.Play();
                break;
        }
    }
    public void FadeUp(float fadeTime)
    {
        //call the TransitionTo function of the audioMixerSnapshot volumeUp;
        volumeUp.TransitionTo(fadeTime);
    }

    //Call this function to fade the volume to silence over the length of fadeTime
    public void FadeDown(float fadeTime)
    {
        //call the TransitionTo function of the audioMixerSnapshot volumeDown;
        volumeDown.TransitionTo(fadeTime);
    }
    #endregion

    #endregion

    #region Data Types
    public delegate void GotAnswerDelegate(MessageAnswer answer);

    [Serializable]
    public class MotionRange
    {
        public bool IsCalibrated;
        public float Pad2;
        public float Pad3;
        public float Tip2;
        public float Tip3;
        public string CreationTime;

        public MotionRange()
        {
            IsCalibrated = false;
            Pad2 = .01f;
            Pad3 = .01f;
            Tip2 = .01f;
            Tip3 = .01f;
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
        public int LastSkyBoxIndex;
        public MotionRange MotionRange;
        public List<MotionRange> MotionRangeHistory;
        public TreatmentPlan CurrentTreatment;
        public List<TreatmentPlan> TreatmentsHistory;

        public Patient()
        {
            MotionRange = new MotionRange();
            CurrentTreatment = null;
            TreatmentsHistory = new List<TreatmentPlan>();
            MotionRangeHistory = new List<MotionRange>();
            Hand = "Right";
            LastSkyBoxIndex = 0;
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
