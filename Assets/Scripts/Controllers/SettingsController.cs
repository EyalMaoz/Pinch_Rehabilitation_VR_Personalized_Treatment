using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static MainController;
using UnityEngine.UI;

public class SettingsController : AbstractController
{
    #region Unity Input
    [SerializeField]
    private GameObject m_colorPickerPopup;
    [SerializeField]
    private GameObject m_colorPickedPrefab;
    private ColorPickerTriangle m_ColorPicker;
    private bool m_isPaint = false;
    private ColorBlock TempColorBlock = new ColorBlock();
    private Color TempButtonColor;
    private Color TempColor;

    private GameObject m_colorPickergameObj;
    #endregion

    public new void Start()
    {
        base.Start();

        if (m_colorPickerPopup == null || m_colorPickedPrefab == null)
        {
            Debug.LogError("All fields must be initilized in SettingsController");
        }
        TempButtonColor.r = 0.7843137f;
        TempButtonColor.g = 0.7843137f;
        TempButtonColor.b = 0.7843137f;
        TempButtonColor.a = 0.5019608f;
        TempColorBlock.disabledColor = TempButtonColor;
    }

    void Update()
    {
        try
        {
            if (m_isPaint)// we need to constatnly update the background color
            {

                m_mainController.SkyBoxMat.SetColor(Shader.PropertyToID("_Tint"), m_ColorPicker.TheColor);
                TempButtonColor = m_mainController.SkyBoxMat.GetColor(Shader.PropertyToID("_Tint"));
                if (TempButtonColor != m_mainController.CurrentSettings.SkyBoxColor) // if the chosen color on the color picker is different from the current skybox color
                {
                    m_mainController.CurrentSettings.SkyBoxColor = m_mainController.SkyBoxMat.GetColor(Shader.PropertyToID("_Tint"));
                    UpdateButtonsColor();
                }
            }
                    }
        catch (Exception e)
        {
            PrintToLog(e.ToString(), MainController.LogType.Error);
        }
    }

    #region On Click Events
    public void OnSendInformationLogClicked()
    {
        MailsSystemManager.SendInformationLogInMail(m_mainController.LoggedInTherapist);
        m_mainController.ShowPopup(MessageController.MessageType.Succsess, "Mail will be sent in few minutes.");
        try
        {
            PrintToLog("Information log was sent to therapist mail: Requsted by the therapist: " + m_mainController.LoggedInTherapist.FirstName + " " + m_mainController.LoggedInTherapist.LastName + ", id: "
                + m_mainController.LoggedInTherapist.Username + ".", MainController.LogType.Information);
        }
        catch (Exception e)
        {
            PrintToLog(e.ToString(), MainController.LogType.Error);
        }
    }

    public void OnSendErrorLogClicked()
    {
        MailsSystemManager.SendErrorLogInMail(m_mainController.LoggedInTherapist);
        m_mainController.ShowPopup(MessageController.MessageType.Succsess, "Mail will be sent in few minutes.");
        try
        {
            PrintToLog("Error log was sent to therapist mail: Requsted by the therapist: " + m_mainController.LoggedInTherapist.FirstName + " " + m_mainController.LoggedInTherapist.LastName + ", id: "
                + m_mainController.LoggedInTherapist.Username + ".", MainController.LogType.Information);
        }
        catch (Exception e)
        {
            PrintToLog(e.ToString(), MainController.LogType.Error);
        }
    }

    public void OnDeleteErrorLogClicked()
    {
        m_mainController.ShowPopup(MessageController.MessageType.Warning,
                 "You pressed delete error log, are you sure?", OnPopupAnswerFromDeleteError);
        try
        {
            PrintToLog("Error log was deleted: Requsted by the therapist: " + m_mainController.LoggedInTherapist.FirstName + " " + m_mainController.LoggedInTherapist.LastName + ", id: "
                + m_mainController.LoggedInTherapist.Username + ".", MainController.LogType.Information);
        }
        catch (Exception e)
        {
            PrintToLog(e.ToString(), MainController.LogType.Error);
        }
    }

    public void OnDeleteInformationLogClicked()
    {
        m_mainController.ShowPopup(MessageController.MessageType.Warning,
                 "You pressed delete information log, are you sure?", OnPopupAnswerFromDeleteInfo);
        try
        {
            PrintToLog("Information log was deleted: Requsted by the therapist: " + m_mainController.LoggedInTherapist.FirstName + " " + m_mainController.LoggedInTherapist.LastName + ", id: "
                + m_mainController.LoggedInTherapist.Username + ".", MainController.LogType.Information);
        }
        catch (Exception e)
        {
            PrintToLog(e.ToString(), MainController.LogType.Error);
        }
    }

    public void OnBackButtonClicked()
    {
        m_mainController.SettingsPanel.SetActive(false);
    }

    public void OnChangeColorsButtonClicked()
    {
        StartPaint();
    }

    public void OnSaveChangesButtonClicked()// in color picker screen
    {
        try
        {
            m_mainController.LoggedInTherapist.Settings.SkyBoxColor = m_mainController.CurrentSettings.SkyBoxColor;
            m_mainController.LoggedInTherapist.Settings.ButtonsColorBlock = m_mainController.CurrentSettings.ButtonsColorBlock;
            m_mainController.LoggedInTherapist.Settings.ButtonsTextColor = m_mainController.CurrentSettings.ButtonsTextColor;
            StopPaint();
            m_mainController.ShowPopup(MessageController.MessageType.Succsess, "The changes were saved!");
        }
        catch (Exception e)
        {
            PrintToLog(e.ToString(), MainController.LogType.Error);
        }
    }

    public void OnCancelButtonClicked()// in color picker screen
    {
        try
        {
            if (m_mainController.LoggedInTherapist.Settings.SkyBoxColor != m_mainController.CurrentSettings.SkyBoxColor) // Changes were not saved and back button was pressed
            {
                m_colorPickergameObj.SetActive(false);
                m_mainController.ShowPopup(MessageController.MessageType.Warning,
                 "You pressed back, the changes will not be saved. are you sure?", OnPopupAnswerFromPaint);
            }
            else
            {
                StopPaint();
            }
        }
        catch (Exception e)
        {
            PrintToLog(e.ToString(), MainController.LogType.Error);
        }
    }
    #endregion

    private void OnPopupAnswerFromPaint(MessageController.MessageAnswer answer)
    {
        if (answer == MessageController.MessageAnswer.Yes)
        {
            if (m_isPaint)
            {
                StopPaint();
                m_mainController.CurrentSettings.SkyBoxColor = m_mainController.LoggedInTherapist.Settings.SkyBoxColor;
                m_mainController.CurrentSettings.ButtonsColorBlock = m_mainController.LoggedInTherapist.Settings.ButtonsColorBlock;
                m_mainController.CurrentSettings.ButtonsTextColor = m_mainController.LoggedInTherapist.Settings.ButtonsTextColor;
                m_mainController.UpdateColors();
            }
        }
        else if (answer == MessageController.MessageAnswer.Cancel)
        {
            if (m_isPaint)
            {
                m_colorPickergameObj.SetActive(true);
            }
        }
    }

    private void OnPopupAnswerFromDeleteError(MessageController.MessageAnswer answer)
    {
        if (answer == MessageController.MessageAnswer.Yes)
        {
            QuestFileManager.DeleteErrorLogFile();
            m_mainController.ShowPopup(MessageController.MessageType.Succsess, "Error Log was deleted!");
        }
    }

    private void OnPopupAnswerFromDeleteInfo(MessageController.MessageAnswer answer)
    {
        if (answer == MessageController.MessageAnswer.Yes)
        {
            QuestFileManager.DeleteInformationLogFile();
            m_mainController.ShowPopup(MessageController.MessageType.Succsess, "Information Log was deleted!");
        }
    }

    #region Color Picker Methods
    private void StartPaint()
    {
        Color SkyBoxColor;
        m_colorPickerPopup.SetActive(true);
        SkyBoxColor = m_mainController.SkyBoxMat.GetColor(Shader.PropertyToID("_Tint"));
        m_colorPickergameObj = (GameObject)Instantiate(m_colorPickedPrefab, transform.position + Vector3.forward * (-150f) + Vector3.down * 10f, Quaternion.identity, gameObject.transform);
        m_colorPickergameObj.transform.localScale = Vector3.one * 100f;
        m_colorPickergameObj.transform.LookAt(Camera.main.transform);
        m_ColorPicker = m_colorPickergameObj.GetComponent<ColorPickerTriangle>();
        m_ColorPicker.SetNewColor(SkyBoxColor);
        m_isPaint = true;
    }

    private void StopPaint()
    {
        m_colorPickerPopup.SetActive(false);
        Destroy(m_colorPickergameObj);
        m_isPaint = false;
    }

    private Color ChangeColorByDelta(float colorDeltaChange, Color color)
    {
        color.r = color.r + colorDeltaChange;
        color.g = color.g + colorDeltaChange;
        color.b = color.b + colorDeltaChange;
        color.a = 1;
        return color;
    }

    private void UpdateButtonsColor()
    {
        TempButtonColor = ChangeColorByDelta((float)-0.15, TempButtonColor);
        TempColorBlock.highlightedColor = TempButtonColor;
        TempButtonColor = ChangeColorByDelta((float)-0.15, TempButtonColor);
        TempColorBlock.pressedColor = TempButtonColor;
        TempButtonColor = ChangeColorByDelta((float)-0.15, TempButtonColor);
        TempColorBlock.normalColor = TempButtonColor;

        TempColorBlock.colorMultiplier = 1;

        foreach (Button button in m_mainController.NonImageButtons)
        {
            button.colors = TempColorBlock;
            m_mainController.CurrentSettings.ButtonsColorBlock = TempColorBlock;
            if (TempButtonColor.r < (float)0.15 && TempButtonColor.g < (float)0.15 ||
                TempButtonColor.r < (float)0.15 && TempButtonColor.b < (float)0.15 ||
                TempButtonColor.g < (float)0.15 && TempButtonColor.b < (float)0.15)
            {
                button.GetComponentInChildren<TMP_Text>().color = Color.white;
                m_mainController.CurrentSettings.ButtonsTextColor = Color.white;
            }
            else
            {
                button.GetComponentInChildren<TMP_Text>().color = Color.black;
                m_mainController.CurrentSettings.ButtonsTextColor = Color.black;
            }
        }
    }
    #endregion
}
