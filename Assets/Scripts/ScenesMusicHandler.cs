using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenesMusicHandler : MonoBehaviour
{
    private static bool isFirstTime = true;

    void Start()
    {
        if (!isFirstTime)
        {
            Destroy(gameObject);
            return;
        }
        isFirstTime = false;
        DontDestroyOnLoad(gameObject); // dont destroy ScenesMusicHandler
    }
}
