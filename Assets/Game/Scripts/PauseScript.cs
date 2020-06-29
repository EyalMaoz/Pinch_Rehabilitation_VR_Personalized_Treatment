using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseScript : MonoBehaviour
{
    [SerializeField]
    private Renderer m_buttonRenderer;
    [SerializeField]
    private GameObject m_pauseMenu;
    [SerializeField]
    private GameObject m_threeMinMessage;
    [SerializeField]
    private GameObject m_fiveMinMessage;
    [SerializeField]
    private LineRenderer m_laserPointerRenderer;
    private MainController m_mainController;
    private bool paused = false;
    private bool showed3Message = false;
    private bool showed5Message = false;

    private void Awake()
    {
        m_mainController = FindObjectOfType<MainController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        m_buttonRenderer.material.color = Color.black;
        PauseGame();
    }

    void Start()
    {

        if (m_mainController == null)
        {
            Debug.LogError("Main controller didn't initialize.");
        }

    }
    private void Update()
    {
        if (!showed3Message && Time.timeSinceLevelLoad > 180)
        {
            showed3Message = true;
            StartCoroutine(Show3Message());
        }
        if (!showed5Message && Time.timeSinceLevelLoad > 300)
        {
            showed5Message = true;
            StartCoroutine(Show5Message());
        }
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
        UnPauseGame();
        MainController.LoadMainScene();
        if (SceneManager.GetActiveScene().name == "Game")
            MainController.PrintToLog("Therapy end: " + m_mainController.CurrentPatient.FullName + " " + ", id: "
            + m_mainController.CurrentPatient.Id + ".", MainController.LogType.Information);
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
        m_buttonRenderer.material.color = Color.white;
        paused = false;
        m_pauseMenu.SetActive(false);
        m_laserPointerRenderer.enabled = false;
        Time.timeScale = 1;
    }

    private IEnumerator Show3Message()
    {
        m_threeMinMessage.SetActive(true);
        yield return new WaitForSeconds(11);
        m_threeMinMessage.SetActive(false);
        PauseGame();
    }

    private IEnumerator Show5Message()
    {
        m_fiveMinMessage.SetActive(true);
        yield return new WaitForSeconds(11);
        m_fiveMinMessage.SetActive(false);
        PauseGame();
    }
}
