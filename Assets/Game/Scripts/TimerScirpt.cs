using System.Collections;
using System.Collections.Generic;
using System.Timers;
using TMPro;
using UnityEngine;

public class TimerScirpt : MonoBehaviour
{
    [SerializeField]
    private TMP_Text timerText;

    private double timePassed = 0;
    private int seconds = 0;
    private int minutes = 0;

    private void Update()
    {
        timePassed += Time.deltaTime;
        if (timePassed > 1)
        {
            seconds++;
            if (seconds >= 60)
            {
                minutes++;
                seconds = 0;
                if (minutes > 60) MainController.LoadMainScene();// someone forgot the app open...
            }
            timerText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
            timePassed = 0;
        }
    }

}
