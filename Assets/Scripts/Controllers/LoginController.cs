using System;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using static MainController;

public class LoginController : AbstractController
{
    #region Unity Input
    [SerializeField]
    private TMP_Text m_userNameField;
    [SerializeField]
    private TMP_Text m_passwordField;
    [SerializeField]
    private TMP_Text m_statusText;
    #endregion

    private string m_realPassword = string.Empty;

    private Therapist GetUser()
    {
        Therapist user = new Therapist();
        user.Username = m_userNameField.text;
        user.Password = m_realPassword;

        return user;
    }

    private string FilePath
    {
        get { return PinchConstants.TherapistsDirectoryPath + m_userNameField.text; }
    }

    public new void Start()
    {
        base.Start();

        if (m_userNameField == null || m_passwordField == null ||
            m_statusText == null)
        {
            Debug.LogError("All fields must be initilized in LoginManager");
        }
    }

    public void Update()
    {

        if (m_passwordField.text.Length > m_realPassword.Length)
        {
            m_realPassword += m_passwordField.text.Substring(m_passwordField.text.Length - 1);
            m_passwordField.text = m_passwordField.text.Remove(m_passwordField.text.Length - 1) + "*";
        }
        else if (m_passwordField.text.Length < m_realPassword.Length)
        {
            m_realPassword = m_realPassword.Substring(0, m_passwordField.text.Length);
        }

    }

    #region On Click Events

    public void OnLoginClicked()
    {
        int UserName;
        m_statusText.text = "";
        try
        {
            if (m_userNameField.text == string.Empty || m_realPassword == string.Empty)
            {
                m_statusText.text = "Please fill all data";
                return;
            }
            if (!int.TryParse(m_userNameField.text, out UserName))
            {
                m_statusText.text = "Please enter only numbers on username field";
                return;
            }

            Therapist existingUser = QuestFileManager.GetTherapistFromFile(FilePath);

            if (existingUser != null)
            {
                if (existingUser.Password == GetUser().Password)
                {
                    m_mainController.SetLoggedInTherapist(existingUser);
                    //in case this is an old user, and it doesn't have settings attribute
                    if(m_mainController.LoggedInTherapist.Settings == null)
                    {
                        m_mainController.LoggedInTherapist.Settings = new Settings()
                        {
                            SkyBoxColor = m_mainController.DefaultSettings.SkyBoxColor,
                            ButtonsColorBlock = m_mainController.DefaultSettings.ButtonsColorBlock,
                            ButtonsTextColor = m_mainController.DefaultSettings.ButtonsTextColor,
                        };
                    }
                    m_mainController.ShowScreen(ScreensIndex.TherapistScreen);
                    ClearScreen();
                    return;
                }
                m_statusText.text = "Wrong password";
                return;
            }
            m_statusText.text = "User not found";
        }
        catch (Exception e)
        {
            m_statusText.text = "";
            // This is the first place that if we dont have the permission for some reason 
            // an excetion will be thrown so we check that we have the permissions just in case:
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite) ||
                    !Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageWrite);
                Permission.RequestUserPermission(Permission.ExternalStorageRead);
            }
            else
            {
                m_statusText.text = "Something went wrong. Please restart the application.";
            }
            PrintToLog(e.ToString(), MainController.LogType.Error);
        }
    }

    public void OnCreateAccountClicked()
    {
        try
        {
            m_mainController.ShowScreen(ScreensIndex.AddNewTherapist);
        }
        catch (Exception e)
        {
            PrintToLog(e.ToString(), MainController.LogType.Error);
        }
    }
    #endregion

    public void ClearScreen()
    {
        m_userNameField.text = string.Empty;
        m_passwordField.text = string.Empty;
        m_statusText.text = string.Empty;
    }
}
