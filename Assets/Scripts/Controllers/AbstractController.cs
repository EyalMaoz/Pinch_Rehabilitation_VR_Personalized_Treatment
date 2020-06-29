using UnityEngine;

public abstract class AbstractController : MonoBehaviour
{
    // AbstractController is an abstract class for each "screen" controller to inherit from.
    // Each screen should have the one and only MainController instance (similar to singleton).

    protected MainController m_mainController;

    protected void Start()
    {
        m_mainController = FindObjectOfType<MainController>();
        InitializeController();
    }

    public virtual void InitializeController() { }
}
