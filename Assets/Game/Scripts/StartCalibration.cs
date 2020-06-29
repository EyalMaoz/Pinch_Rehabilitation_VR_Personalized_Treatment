using Assets.Game.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartCalibration : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        CalibrationManager.isReady = true;
        Destroy(gameObject);
    }
}
