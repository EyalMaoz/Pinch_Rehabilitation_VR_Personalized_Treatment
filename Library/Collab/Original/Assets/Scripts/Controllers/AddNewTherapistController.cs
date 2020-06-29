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
    public TMP_Text UserNameField;
    public TMP_Text PasswordField;
    public TMP_Text FirstNameField;
    public TMP_Text LastNameField;
    public TMP_Text EmailField;
    public TMP_Text StatusText;
    #endregion

    private string FilePath
    {
        get { return PinchConstants.TherapistsDirectoryPath + UserNameField.text + ".json"; }
    }

    private Therapist GetUserFromGui()
    {
        Therapist user = null;
        if (user == null)
        {
            user = new Therapist();
        }
        user.UserName = UserNameField.text;
        user.Password = PasswordField.text;
        user.FirstName = FirstNameField.text;
        user.LastName = LastNameField.text;
        user.Email = EmailField.text;
        return user;
    }



    public new void Start()
    {
        base.Start();

        if (UserNameField == null || PasswordField == null ||
            FirstNameField == null || LastNameField == null || StatusText == null)
        {
            Debug.LogError("All fields must be initilized in AddNewTherapistController");
        }
    }

    public void OnConfirmClicked()
    {
        int UserName;
        try
        {
            StatusText.text = "";
            if (UserNameField.text == string.Empty || PasswordField.text == string.Empty ||
                FirstNameField.text == string.Empty || LastNameField.text == string.Empty)
            {
                StatusText.text = "Please fill all data";
                return;
            }
            if (!int.TryParse(UserNameField.text, out UserName))
            {
                StatusText.text = "Please enter only numbers on ID field";
                return;
            }
            if (File.Exists(FilePath))
            {
                StatusText.text = "User is already exists";
                return;
            }
            if (!FirstNameField.text.All(char.IsLetter) || !LastNameField.text.All(char.IsLetter))
            {
                StatusText.text = "Please enter only letters on Name fields";
                return;
            }
            if (!IsValidEmail(EmailField.text))
            {
                StatusText.text = "Please enter a valid email address";
                return;
            }

            if(!QuestFileManager.SaveTherapistToFile(GetUserFromGui(), FilePath))
            {
                StatusText.text = "Therapist already exists.";
                return;
            }

            m_mainManager.ShowPopup(MessageController.MessageType.Succsess, "New therapist account has created!", OnPopupAnswer);
        }
        catch (Exception e)
        {
            PrintToLog(e.Message);
            StatusText.text = e.Message;
        }
    }

    private void OnPopupAnswer(MessageController.MessageAnswer answer)
    {
        if (answer == MessageController.MessageAnswer.Ok) // When new therapist user was created
        {
            m_mainManager.ShowScreen(ScreensIndex.Login);
            ClearScreen();
        }
        else if (answer == MessageController.MessageAnswer.Yes) // When some of the input fields where not empty and back button was pressed
        {
            m_mainManager.ShowScreen(ScreensIndex.Login);
            ClearScreen();
        }
        else if (answer == MessageController.MessageAnswer.Cancel) // When some of the input fields where not empty and back button was pressed
        {
            m_mainManager.ShowScreen(ScreensIndex.AddNewTherapist);
        }
    }

    public void OnBackButtonClicked()
    {
        try
        {
            if (!(UserNameField.text == string.Empty) || !(PasswordField.text == string.Empty) ||
                !(FirstNameField.text == string.Empty) || !(LastNameField.text == string.Empty))
                // When some of the input fields where not empty
                m_mainManager.ShowPopup(MessageController.MessageType.Warning,
                "You pressed back, the details will delete. are you sure?", OnPopupAnswer);
            else
                m_mainManager.ShowScreen(ScreensIndex.Login);
        }
        catch (Exception e)
        {
            PrintToLog(e.Message);
            StatusText.text = e.Message;
        }
    }

    public void ClearScreen()
    {
        UserNameField.text = string.Empty;
        PasswordField.text = string.Empty;
        FirstNameField.text = string.Empty;
        LastNameField.text = string.Empty;
        EmailField.text = string.Empty;
        StatusText.text = string.Empty;
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




