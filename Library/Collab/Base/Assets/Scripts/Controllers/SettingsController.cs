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
    private bool m_isDeleteErrorLogPressed = false;
    private bool m_isDeleteInfoLogPressed = false;
    private bool m_isYesPressed = false;

    private GameObject m_colorPickergameObj;
    #endregion

    public new void Start()
    {
        base.Start();

        if (m_colorPickerPopup == null || m_colorPickedPrefab == null)
        {
            Debug.LogError("All fields must be initilized in SettingsController");
        }
    }

    void Update()
    {
        ColorBlock TempColorBlock = new ColorBlock();
        Color TempButtonColor;
        Color TempColor;
        try
        {
            if (m_isPaint)
            {

                m_mainController.SkyBoxMat.SetColor(Shader.PropertyToID("_Tint"), m_ColorPicker.TheColor);
                TempButtonColor.r = 0.7843137f;
                TempButtonColor.g = 0.7843137f;
                TempButtonColor.b = 0.7843137f;
                TempButtonColor.a = 0.5019608f;
                TempColorBlock.disabledColor = TempButtonColor;
                TempButtonColor = m_mainController.SkyBoxMat.GetColor(Shader.PropertyToID("_Tint"));
                if (TempButtonColor != m_mainController.CurrentSettings.SkyBoxColor) // if the chosen color on the color picker is different from the current skybox color
                {
                    m_mainController.CurrentSettings.SkyBoxColor = m_mainController.SkyBoxMat.GetColor(Shader.PropertyToID("_Tint"));
                    TempButtonColor = ChangeButtonColor((float)-0.15, TempButtonColor);
                    TempColorBlock.highlightedColor = TempButtonColor;
                    TempButtonColor = ChangeButtonColor((float)-0.15, TempButtonColor);
                    TempColorBlock.pressedColor = TempButtonColor;
                    TempButtonColor = ChangeButtonColor((float)-0.15, TempButtonColor);
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
            }
            else if (m_isDeleteErrorLogPressed && m_isYesPressed)
            {
                m_mainController.ShowPopup(MessageController.MessageType.Succsess, "Error Log was deleted!");
                m_isDeleteErrorLogPressed = false;
                m_isYesPressed = false;
            }
            else if (m_isDeleteInfoLogPressed && m_isYesPressed)
            {
                m_mainController.ShowPopup(MessageController.MessageType.Succsess, "Information Log was deleted!");
                m_isDeleteInfoLogPressed = false;
                m_isYesPressed = false;
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
    }

    public void OnSendErrorLogClicked()
    {
        MailsSystemManager.SendErrorLogInMail(m_mainController.LoggedInTherapist);
        m_mainController.ShowPopup(MessageController.MessageType.Succsess, "Mail will be sent in few minutes.");
    }

    public void OnDeleteErrorLogClicked()
    {
        m_mainController.ShowPopup(MessageController.MessageType.Warning,
                 "You pressed delete error log, are you sure?", OnPopupAnswer);
        m_isDeleteErrorLogPressed = true;
    }

    public void OnDeleteInformationLogClicked()
    {
        m_mainController.ShowPopup(MessageController.MessageType.Warning,
                 "You pressed delete error log, are you sure?", OnPopupAnswer);
        m_isDeleteInfoLogPressed = true;
    }

    public void OnBackButtonClicked()
    {
        m_mainController.SettingsPanel.SetActive(false);
    }

    public void OnChangeColorsButtonClicked()
    {
        StartPaint();
    }

    public void OnSaveChangesButtonClicked()
    {
        try
        {
            m_mainController.LoggedInTherapist.Settings.SkyBoxColor = m_mainController.CurrentSettings.SkyBoxColor;
            m_mainController.LoggedInTherapist.Settings.ButtonsColorBlock = m_mainController.CurrentSettings.ButtonsColorBlock;
            m_mainController.LoggedInTherapist.Settings.ButtonsTextColor = m_mainController.CurrentSettings.ButtonsTextColor;
            StopPaint();
            m_mainController.ShowPopup(MessageController.MessageType.Succsess, "The changes were saved!", OnPopupAnswer);
        }
        catch (Exception e)
        {
            PrintToLog(e.ToString(), MainController.LogType.Error);
        }
    }

    public void OnCancelButtonClicked()
    {
        try
        {
            if (m_mainController.LoggedInTherapist.Settings.SkyBoxColor != m_mainController.CurrentSettings.SkyBoxColor) // Changes were not saved and back button was pressed
            {
                m_colorPickergameObj.SetActive(false);
                m_mainController.ShowPopup(MessageController.MessageType.Warning,
                 "You pressed back, the changes will not be saved. are you sure?", OnPopupAnswer);
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



    private void OnPopupAnswerFromPaint(MessageController.MessageAnswer answer)
    {
        if (answer == MessageController.MessageAnswer.Yes)
        {
            // Yes do something with paint
        }
        else
        {
            // Never mind
        }
    }

    private void OnPopupAnswerFromDeleteError(MessageController.MessageAnswer answer)
    {
        if (answer == MessageController.MessageAnswer.Yes)
        {
            // Yes delete Error log!
        }
        else
        {
            // Never mind
        }
    }

    private void OnPopupAnswerFromDeleteInfo(MessageController.MessageAnswer answer)
    {
        if (answer == MessageController.MessageAnswer.Yes)
        {
            // Yes delete info log!
        }
        else
        {
            // Never mind
        }
    }

    private void OnPopupAnswer(MessageController.MessageAnswer answer)
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
            else if (m_isDeleteErrorLogPressed)
            {
                m_isYesPressed = true;
            }
            else if (m_isDeleteInfoLogPressed)
            {
                m_isYesPressed = true;
            }
        }
        else if (answer == MessageController.MessageAnswer.Cancel)
        {
            if (m_isPaint)
            {
                m_colorPickergameObj.SetActive(true);
            }
            else if (m_isDeleteErrorLogPressed)
            {
                m_isDeleteErrorLogPressed = false;
            }
            else if (m_isDeleteInfoLogPressed)
            {
                m_isDeleteInfoLogPressed = false;
            }
        }
    }
    #endregion

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

    private Color ChangeButtonColor(float colorDeltaChange, Color color)
    {
        color.r = color.r + colorDeltaChange;
        color.g = color.g + colorDeltaChange;
        color.b = color.b + colorDeltaChange;
        color.a = 1;
        return color;
    }
    #endregion
}
