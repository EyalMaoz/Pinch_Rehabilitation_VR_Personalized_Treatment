using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandInteraction : MonoBehaviour
{   // Eyal Maoz
    // This is the first class and interaction we did with the Hand Tracking beta feature.
    // With the right hand we changed the text in a scene and with left we changed the brightness of a Panel in a scene.
    public OVRHand LeftHand;
    public OVRHand RightHand;
    public Text text;

    // Update is called once per frame
    void Update()
    {
        text.text = RightHand.GetFingerPinchStrength(OVRHand.HandFinger.Index).ToString();
        if (RightHand.GetFingerConfidence(OVRHand.HandFinger.Index) == OVRHand.TrackingConfidence.Low)
            text.text = "0";
        if (LeftHand.GetFingerConfidence(OVRHand.HandFinger.Index) == OVRHand.TrackingConfidence.High)
        {
            Image img = GameObject.Find("Panel").GetComponent<Image>();
            img.color = new Color(img.color.r, img.color.g, img.color.b,
            LeftHand.GetFingerPinchStrength(OVRHand.HandFinger.Index));
        }
    }
}
