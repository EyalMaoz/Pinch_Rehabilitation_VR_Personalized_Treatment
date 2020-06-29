using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using TMPro;
using UnityEngine;
using static Assets.Game.Scripts.TherapyData;

namespace Assets.Game.Scripts
{
    // CalibrationManager is in charge of creating the calibration scene. 
    // When loading the calibration scene it takes the data about the patient from the MainController
    // In addition it monitor the progress of the calibration and updates the data directly to the MainController.
    public class CalibrationManager : MonoBehaviour
    {
        private MainController m_mainController;
        public TMP_Text m_feedbackText;

        [SerializeField]
        private PinchManager m_pinchManager;
        [SerializeField]
        private GameObject m_2tipHand;
        [SerializeField]
        private GameObject m_3tipHand;
        [SerializeField]
        private GameObject m_2padHand;
        [SerializeField]
        private GameObject m_3padHand;
        [SerializeField]
        private AudioClip[] m_calibrationSounds;
        private AudioSource m_audioSource;
        Stopwatch stopwatch = new Stopwatch();

        private int timeToWaitInSec = 20;
        private int stepNum = 0;
        private float strength = 0;
        [SerializeField]
        public static bool isReady = false;
        int instructionsSoundsIndex = 0;

        private void Awake()
        {
            m_mainController = new MainController();
            m_mainController = FindObjectOfType<MainController>();
            isReady = false;
            m_audioSource = GetComponent<AudioSource>();
            StartCoroutine(PlayInstructionsSounds(3, 2));
        }

        void Start()
        {
            try
            {
                m_pinchManager.SetHand(m_mainController.CurrentPatient.Hand);
                // Save old values
                if (m_mainController.CurrentPatient.MotionRange.IsCalibrated)
                {
                    m_mainController.CurrentPatient.MotionRangeHistory.Add(m_mainController.CurrentPatient.MotionRange);
                }
                m_mainController.CurrentPatient.MotionRange = new MainController.MotionRange()
                {
                    CreationTime = DateTime.Now.ToShortDateString() + " " +
                DateTime.Now.ToLongTimeString()
                };
            }
            catch (Exception e)
            {
                MainController.PrintToLog(e.ToString(), MainController.LogType.Error);
            }
        }

        private void Update()
        {
            if (!isReady) return;
            UpdateTimeRemain();
            switch (stepNum)
            {
                case 0:
                    StartCoroutine(NextLevel());
                    break;
                case 1:
                    MakeStep(PinchType.Tip2);
                    break;
                case 2:
                    MakeStep(PinchType.Tip3);
                    break;
                case 3:
                    MakeStep(PinchType.Pad2);
                    break;
                case 4:
                    MakeStep(PinchType.Pad3);
                    break;
            }
        }

        private void UpdateTimeRemain()
        {
            int time = (int)(timeToWaitInSec - stopwatch.ElapsedMilliseconds / 1000);
            if (time >= 0)
            { m_feedbackText.text = "Time to next move: " + time; }
            else
                m_feedbackText.text = string.Empty;
        }

        private void MakeStep(PinchType type)
        {
            try
            {
                float currentValue;
                switch (type)
                {// we have this annoying switch cases because the json Converter cannot convert Dictionary, so we just added 4 properties...
                    case PinchType.Pad2:
                        currentValue = m_mainController.CurrentPatient.MotionRange.Pad2;
                        break;
                    case PinchType.Pad3:
                        currentValue = m_mainController.CurrentPatient.MotionRange.Pad3;
                        break;
                    case PinchType.Tip2:
                        currentValue = m_mainController.CurrentPatient.MotionRange.Tip2;
                        break;
                    case PinchType.Tip3:
                        currentValue = m_mainController.CurrentPatient.MotionRange.Tip3;
                        break;
                    default: return;

                }
                strength = m_pinchManager.GetPinchStrength(type);

                if (strength > currentValue)
                {
                    switch (type)
                    {
                        case PinchType.Pad2:
                            m_mainController.CurrentPatient.MotionRange.Pad2 = strength;
                            break;
                        case PinchType.Pad3:
                            m_mainController.CurrentPatient.MotionRange.Pad3 = strength;
                            break;
                        case PinchType.Tip2:
                            m_mainController.CurrentPatient.MotionRange.Tip2 = strength;
                            break;
                        case PinchType.Tip3:
                            m_mainController.CurrentPatient.MotionRange.Tip3 = strength;
                            break;
                        default: return;

                    }
                }
                if (strength >= 1 || stopwatch.ElapsedMilliseconds > timeToWaitInSec * 1000)
                {
                    StartCoroutine(NextLevel());
                }
            }
            catch (Exception e)
            {
                MainController.PrintToLog(e.ToString(), MainController.LogType.Error);
            }
        }

        private IEnumerator NextLevel()
        {
            isReady = false;
            float secondsToWait = 5;
            switch (stepNum)// the level that we just finished
            {
                case 0:
                    StartCoroutine(PlayInstructionsSounds(3, 1));
                    m_2tipHand.SetActive(true);
                    stopwatch.Start();
                    m_feedbackText.text = "Let's Start!";
                    yield return new WaitForSeconds(secondsToWait);
                    break;
                case 1:
                    m_2tipHand.SetActive(false);
                    m_feedbackText.text = "Good job! 3 more";
                    yield return new WaitForSeconds(secondsToWait);
                    m_3tipHand.SetActive(true);
                    break;
                case 2:
                    m_3tipHand.SetActive(false);
                    m_feedbackText.text = "Good job! 2 more";
                    yield return new WaitForSeconds(secondsToWait);
                    m_2padHand.SetActive(true);
                    break;
                case 3:
                    m_2padHand.SetActive(false);
                    m_feedbackText.text = "Good job! 1 more";
                    yield return new WaitForSeconds(secondsToWait);
                    m_3padHand.SetActive(true);
                    break;
                case 4:
                    StartCoroutine(PlayInstructionsSounds(2, 0));
                    m_3padHand.SetActive(false);
                    m_feedbackText.text = "Finished!\nGoing back to main menu...";
                    m_mainController.CurrentPatient.MotionRange.IsCalibrated = true;
                    try
                    {
                        QuestFileManager.UpdatePatientOnFile(m_mainController.CurrentPatient, PinchConstants.PatientsDirectoryPath + m_mainController.CurrentPatient.Id);
                    }
                    catch (Exception e)
                    {
                        MainController.PrintToLog("Can't update patient file in CalibrationManager - " + e.ToString(), MainController.LogType.Error);
                    }
                    yield return new WaitForSeconds(secondsToWait + 2);
                    MainController.LoadMainScene();
                    break;
            }
            stepNum++;
            isReady = true;
            stopwatch.Reset();
            stopwatch.Start();
        }

        private IEnumerator PlayInstructionsSounds(int soundsToPlaycount, float delayTime)
        {
            int i = 0;
            int firstInstructionDelay = 3;
            while(i < soundsToPlaycount)
            {
                m_audioSource.clip = m_calibrationSounds[instructionsSoundsIndex];
                if (instructionsSoundsIndex == 0)
                {
                    m_audioSource.PlayDelayed(firstInstructionDelay);
                    yield return new WaitForSeconds(m_calibrationSounds[instructionsSoundsIndex].length + firstInstructionDelay);
                }
                else
                {
                    m_audioSource.Play();
                    yield return new WaitForSeconds(m_calibrationSounds[instructionsSoundsIndex].length + delayTime);
                }
                instructionsSoundsIndex++;
                i++;
            }   
        }
    }
}

