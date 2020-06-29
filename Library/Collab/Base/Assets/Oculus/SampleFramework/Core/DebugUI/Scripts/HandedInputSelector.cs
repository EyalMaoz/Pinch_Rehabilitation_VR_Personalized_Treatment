/************************************************************************************

Copyright (c) Facebook Technologies, LLC and its affiliates. All rights reserved.  

See SampleFramework license.txt for license terms.  Unless required by applicable law 
or agreed to in writing, the sample code is provided “AS IS” WITHOUT WARRANTIES OR 
CONDITIONS OF ANY KIND, either express or implied.  See the license for specific 
language governing permissions and limitations under the license.

************************************************************************************/
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class HandedInputSelector : MonoBehaviour
{
    OVRCameraRig m_CameraRig;
    [SerializeField]
    OVRInputModule m_InputModule;

    void Start()
    {
        m_CameraRig = FindObjectOfType<OVRCameraRig>();
        m_InputModule = FindObjectOfType<OVRInputModule>();
    }

    void Update()
    {
        SetActiveController(OVRInput.GetActiveController());
        if (m_CameraRig == null) 
        {
            m_CameraRig = FindObjectOfType<OVRCameraRig>();
        }
    }

    void SetActiveController(OVRInput.Controller c)
    {
        Transform t = m_CameraRig.rightControllerAnchor;
        switch(c)
        {
            case OVRInput.Controller.LTouch:
                t = m_CameraRig.leftControllerAnchor;
                break;
            case OVRInput.Controller.RTouch:
                t = m_CameraRig.rightControllerAnchor;
                break;
            case OVRInput.Controller.Hands:
            case OVRInput.Controller.RHand:
                t = m_CameraRig.rightHandAnchor;
                break;
            case OVRInput.Controller.LHand:
                t = m_CameraRig.leftHandAnchor;
                break;
         
        }

        m_InputModule.rayTransform = t;
    }
}
