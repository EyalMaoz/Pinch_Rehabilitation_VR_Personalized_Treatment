using System;
using TMPro;
using UnityEngine;
using static MainController;

public class MessageController : AbstractController
{
    #region Unity Input
    [SerializeField]
    private TMP_Text m_successStatusText;
    [SerializeField]
    private TMP_Text m_warningStatusText;
    [SerializeField]
    private GameObject m_messagePanel;
    [SerializeField]
    private GameObject m_successPanel;
    [SerializeField]
    private GameObject m_warningPanel;
    #endregion

    public new void Start()
    {
        base.Start();
        if (m_successStatusText == null || m_warningStatusText == null ||
           m_messagePanel == null || m_successPanel == null || m_warningPanel == null)
        {
            Debug.LogError("All fields must be initilized in MessageController");
        }
    }


    private event GotAnswerDelegate gotAnswer;

    public void ShowMessage(MessageType msgType, string msg, GotAnswerDelegate function)
    {
        gotAnswer -= gotAnswer;
        if (function != null)
        {
            gotAnswer += function;
        }

        SetAndViewPanelByType(msgType, msg);
    }

    private void SetAndViewPanelByType(MessageType msgType, string msg)
    {
        switch (msgType)
        {
            case MessageType.Succsess:
                m_successStatusText.text = msg;
                m_successPanel.SetActive(true);
                m_warningPanel.SetActive(false);
                m_messagePanel.SetActive(true);
                break;
            case MessageType.Warning:
                m_warningStatusText.text = msg;
                m_warningPanel.SetActive(true);
                m_successPanel.SetActive(false);
                m_messagePanel.SetActive(true);
                break;
        }
    }

    public void OnOkClicked()
    {
        OnAnswerReceived(MessageAnswer.Ok);
    }

    public void OnYesClicked()
    {
        OnAnswerReceived(MessageAnswer.Yes);
    }

    public void OnCancelClicked()
    {
        OnAnswerReceived(MessageAnswer.Cancel);
    }

    private void OnAnswerReceived(MessageAnswer ans)
    {
        gotAnswer?.Invoke(ans);
        m_successPanel.SetActive(false);
        m_warningPanel.SetActive(false);
        m_messagePanel.SetActive(false);
    }

    #region Data Types
    public enum MessageType
    {
        Succsess, Warning
    }

    public enum MessageAnswer
    {
        Ok, Yes, Cancel
    }
    #endregion
}
