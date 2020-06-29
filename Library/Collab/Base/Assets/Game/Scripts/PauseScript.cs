using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseScript : MonoBehaviour
{
    [SerializeField]
    private Renderer m_buttonRenderer;
    [SerializeField]
    private GameObject m_pauseMenu;
    [SerializeField]
    private LineRenderer m_laserPointerRenderer;

    private bool paused = false;

    private void OnTriggerEnter(Collider other)
    {
        m_buttonRenderer.material.color = Color.black;
        PauseGame();
    }

    private void Update()
    {
        if (paused)
        {
            var activeControl = OVRInput.GetActiveController();
            if (activeControl == OVRInput.Controller.Hands || activeControl == OVRInput.Controller.LHand || activeControl == OVRInput.Controller.RHand)
            {
                m_laserPointerRenderer.enabled = false;
            }
            else if (activeControl == OVRInput.Controller.RTouch || activeControl == OVRInput.Controller.LTouch)
            {
                m_laserPointerRenderer.enabled = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        m_buttonRenderer.material.color = Color.white;
    }

    public void OnReturnClick()
    {
        UnPauseGame();
    }

    public void OnEndTherapyClick()
    {
        //SceneManager.LoadScene(0);
        MainController.EndTherapy();
    }

    private void PauseGame()
    {
        paused = true;
        m_pauseMenu.SetActive(true);
        Time.timeScale = 0;
        m_laserPointerRenderer.enabled = true;
    }

    private void UnPauseGame()
    {
        paused = false;
        m_pauseMenu.SetActive(false);
        m_laserPointerRenderer.enabled = false;
        Time.timeScale = 1;
    }
}
