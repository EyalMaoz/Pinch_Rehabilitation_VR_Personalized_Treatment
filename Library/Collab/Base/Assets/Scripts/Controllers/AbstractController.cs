using UnityEngine;

public abstract class AbstractController : MonoBehaviour
{
    protected MainController m_mainController;

    protected void Start()
    {
        m_mainController = FindObjectOfType<MainController>();
    }
}
