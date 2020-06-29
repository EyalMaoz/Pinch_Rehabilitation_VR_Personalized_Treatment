using TMPro;
using UnityEngine;
using static MainController;

public class MessageController : AbstractController
{
    #region Unity Input
    public TMP_Text successStatusText;
    public TMP_Text warningStatusText;
    public GameObject successPanel;
    public GameObject warningPanel;

    #endregion

    private event GotAnswerDelegate gotAnswer;

    public void ShowMessage(MessageType msgType, string msg, GotAnswerDelegate function)
    {
        if (function != null)
            gotAnswer += function;
        SetAndViewPanelByType(msgType, msg);
    }

    private void SetAndViewPanelByType(MessageType msgType, string msg)
    {
        switch (msgType)
        {
            case MessageType.Succsess:
                successStatusText.text = msg;
                successPanel.SetActive(true);
                break;
            case MessageType.Warning:
                warningStatusText.text = msg;
                warningPanel.SetActive(true);
                break;
        }
    }

    public void OnOkClicked()
    {
        gotAnswer.Invoke(MessageAnswer.Ok);
        successPanel.SetActive(false);
        warningPanel.SetActive(false);
    }

    public void OnYesClicked()
    {
        gotAnswer.Invoke(MessageAnswer.Yes);
        successPanel.SetActive(false);
        warningPanel.SetActive(false);
    }

    public void OnCancelClicked()
    {
        gotAnswer.Invoke(MessageAnswer.Cancel);
        successPanel.SetActive(false);
        warningPanel.SetActive(false);
    }

    public enum MessageType
    {
        Succsess, Warning
    }

    public enum MessageAnswer
    {
        Ok, Yes, Cancel
    }
}
