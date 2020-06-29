using System;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using TMPro;
using static MainController;

public class AddNewTherapistController : AbstractController
{
    #region Unity Input
    [SerializeField]
    private TMP_Text m_userNameField;
    [SerializeField]
    private TMP_Text m_passwordField;
    [SerializeField]
    private TMP_Text m_firstNameField;
    [SerializeField]
    private TMP_Text m_lastNameField;
    [SerializeField]
    private TMP_Text m_emailField;
    [SerializeField]
    private TMP_Text m_statusText;
    #endregion

    private string FilePath
    {
        get { return PinchConstants.TherapistsDirectoryPath + m_userNameField.text; }
    }

    private Therapist GetUserFromGui()
    {
        Therapist user = null;
        if (user == null)
        {
            user = new Therapist();
        }
        user.UserName = m_userNameField.text;
        user.Password = m_passwordField.text;
        user.FirstName = m_firstNameField.text;
        user.LastName = m_lastNameField.text;
        user.Email = m_emailField.text;
        return user;
    }

    public new void Start()
    {
        base.Start();

        if (m_userNameField == null || m_passwordField == null || m_emailField == null ||
            m_firstNameField == null || m_lastNameField == null || m_statusText == null)
        {
            Debug.LogError("All fields must be initilized in AddNewTherapistController");
        }
    }
    #region On Click Events

    public void OnConfirmClicked()
    {
        int UserName;
        try
        {
            m_statusText.text = "";
            if (m_userNameField.text == string.Empty || m_passwordField.text == string.Empty ||
                m_firstNameField.text == string.Empty || m_lastNameField.text == string.Empty)
            {
                m_statusText.text = "Please fill all data";
                return;
            }
            if (!int.TryParse(m_userNameField.text, out UserName))
            {
                m_statusText.text = "Please enter only numbers on ID field";
                return;
            }
            /*if (File.Exists(FilePath))
            {
                m_statusText.text = "Therapist is already exists";
                return;
            }*/
            if (!m_firstNameField.text.All(char.IsLetter) || !m_lastNameField.text.All(char.IsLetter))
            {
                m_statusText.text = "Please enter only letters on Name fields";
                return;
            }
            if (!IsValidEmail(m_emailField.text))
            {
                m_statusText.text = "Please enter a valid email address";
                return;
            }

            if (!QuestFileManager.SaveTherapistToFile(GetUserFromGui(), FilePath))
            {
                m_statusText.text = "Therapist already exists.";
                return;
            }

            m_mainController.ShowPopup(MessageController.MessageType.Succsess, "New therapist account has created!", OnPopupAnswer);
        }
        catch (Exception e)
        {
            PrintToLog(e.ToString(), MainController.LogType.Error);
        }
    }

    private void OnPopupAnswer(MessageController.MessageAnswer answer)
    {
        if (answer == MessageController.MessageAnswer.Ok) // When new therapist user was created
        {
            m_mainController.ShowScreen(ScreensIndex.Login);
            ClearScreen();
        }
        else if (answer == MessageController.MessageAnswer.Yes) // When some of the input fields where not empty and back button was pressed
        {
            m_mainController.ShowScreen(ScreensIndex.Login);
            ClearScreen();
        }
        else if (answer == MessageController.MessageAnswer.Cancel) // When some of the input fields where not empty and back button was pressed
        {
            m_mainController.ShowScreen(ScreensIndex.AddNewTherapist);
        }
    }

    public void OnBackButtonClicked()
    {
        try
        {
            if (!(m_userNameField.text == string.Empty) || !(m_passwordField.text == string.Empty) ||
                !(m_firstNameField.text == string.Empty) || !(m_lastNameField.text == string.Empty))
                // When some of the input fields where not empty
                m_mainController.ShowPopup(MessageController.MessageType.Warning,
                "You pressed back, the details will delete. are you sure?", OnPopupAnswer);
            else
                m_mainController.ShowScreen(ScreensIndex.Login);
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
        m_firstNameField.text = string.Empty;
        m_lastNameField.text = string.Empty;
        m_emailField.text = string.Empty;
        m_statusText.text = string.Empty;
    }

    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;
        try
        {
            // Normalize the domain
            email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                  RegexOptions.None, TimeSpan.FromMilliseconds(200));

            // Examines the domain part of the email and normalizes it.
            string DomainMapper(Match match)
            {
                // Use IdnMapping class to convert Unicode domain names.
                var idn = new IdnMapping();

                // Pull out and process domain name (throws ArgumentException on invalid)
                var domainName = idn.GetAscii(match.Groups[2].Value);

                return match.Groups[1].Value + domainName;
            }
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }

        try
        {
            return Regex.IsMatch(email,
                @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }
}




