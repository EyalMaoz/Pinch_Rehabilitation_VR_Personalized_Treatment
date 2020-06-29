using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MainController;
using TMPro;
using System.Linq;
using System;
using System.Text.RegularExpressions;
using System.Globalization;

public class EditTherapistDetailsController : AbstractController
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

    private bool isBackPressed = false;
    private bool isHomePressed = false;

    private string FilePath
    {
        get { return PinchConstants.TherapistsDirectoryPath + m_userNameField.text; }
    }

    private Therapist GetUserFromGui()
    {
        Therapist user = new Therapist
        {
            Username = m_userNameField.text,
            Password = m_passwordField.text,
            FirstName = m_firstNameField.text,
            LastName = m_lastNameField.text,
            Email = m_emailField.text
        };
        return user;
    }

    public override void InitializeController()
    {
        if (m_mainController == null) return;// The start routine did not called yet. We will call InitializeController from start().
        if (isBackPressed || isHomePressed)
            return;
        else
        {
            m_userNameField.text = m_mainController.LoggedInTherapist?.Username;
            m_passwordField.text = m_mainController.LoggedInTherapist?.Password;
            m_firstNameField.text = m_mainController.LoggedInTherapist?.FirstName;
            m_lastNameField.text = m_mainController.LoggedInTherapist?.LastName;
            m_emailField.text = m_mainController.LoggedInTherapist?.Email;
        }
    }

    public new void Start()
    {
        base.Start();

        if (m_userNameField == null || m_passwordField == null || m_firstNameField == null ||
            m_lastNameField == null || m_emailField == null || m_statusText == null)
        {
            Debug.LogError("All fields must be initilized in EditTherapistDetailsController");
        }
    }

    #region On Click Events
    public void OnConfirmClicked()
    {
        int UserName;
        try
        {
            m_statusText.text = string.Empty;
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
            if (QuestFileManager.GetTherapistFromFile(FilePath) == null)
            {
                m_statusText.text = "Therapist doesn't exists";
                return;
            }
            Therapist therapist = GetUserFromGui();
            m_mainController.LoggedInTherapist.Password = therapist.Password;
            m_mainController.LoggedInTherapist.FirstName = therapist.FirstName;
            m_mainController.LoggedInTherapist.LastName = therapist.LastName;
            m_mainController.LoggedInTherapist.Email = therapist.Email;
            QuestFileManager.UpdateTherapistOnFile(m_mainController.LoggedInTherapist, FilePath);
            m_mainController.ShowPopup(MessageController.MessageType.Succsess, "Therapist details has been updated!", OnPopupAnswer);
            PrintToLog("Therapist's details update: " + m_mainController.LoggedInTherapist.FirstName + " " + m_mainController.LoggedInTherapist.LastName + ", id: "
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
            Therapist therapist = GetUserFromGui();
            isHomePressed = true;
            if (m_mainController.LoggedInTherapist.Password != therapist.Password || m_mainController.LoggedInTherapist.FirstName != therapist.FirstName
                || m_mainController.LoggedInTherapist.LastName != therapist.LastName || m_mainController.LoggedInTherapist.Email != therapist.Email)
            { // When some of the details of the therapist were changed
                m_mainController.ShowPopup(MessageController.MessageType.Warning,
                "You pressed home, the changes will not save. are you sure?", OnPopupAnswer);
            }
            else
            {
                m_mainController.ShowScreen(ScreensIndex.TherapistScreen);
                m_statusText.text = string.Empty;
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
            Therapist therapist = GetUserFromGui();
            isBackPressed = true;
            if (m_mainController.LoggedInTherapist.Password != therapist.Password || m_mainController.LoggedInTherapist.FirstName != therapist.FirstName
                || m_mainController.LoggedInTherapist.LastName != therapist.LastName || m_mainController.LoggedInTherapist.Email != therapist.Email)
            { // When some of the details of the patient were changed
                m_mainController.ShowPopup(MessageController.MessageType.Warning,
                "You pressed back, the changes will not save. are you sure?", OnPopupAnswer);
            }
            else
            {
                m_mainController.ShowScreen(ScreensIndex.ViewTherapistDetails);
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

    private void OnPopupAnswer(MessageController.MessageAnswer answer)
    {
        if (answer == MessageController.MessageAnswer.Ok) // When therapist details were updated
        {
            m_mainController.ShowScreen(ScreensIndex.ViewTherapistDetails);
            m_statusText.text = string.Empty;
        }
        else if (answer == MessageController.MessageAnswer.Yes) // When some of the details of the therapist were changed and home button was pressed
        {
            if (isHomePressed)
            {
                m_mainController.ShowScreen(ScreensIndex.TherapistScreen);
                isHomePressed = false;
            }
            else if (isBackPressed)
            {
                m_mainController.ShowScreen(ScreensIndex.ViewTherapistDetails);
                isBackPressed = false;
            }
            m_statusText.text = string.Empty;
        }
        else if (answer == MessageController.MessageAnswer.Cancel) // When some of the details of the therapist were changed and home button was pressed
        {
            isBackPressed = false;
            isHomePressed = false;
        }
    }
    #endregion

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
