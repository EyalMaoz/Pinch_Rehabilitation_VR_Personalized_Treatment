using Assets.Game.Scripts;
using System;
using UnityEngine;


// PinchGrabber is a script that attached to any hand you want
// the ability to pinch grab a PinchGrabable item.

public class PinchGrabber : OVRGrabber
{
    private PinchManager m_pinchManager;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        m_pinchManager = FindObjectOfType<PinchManager>();

    }

    public override void Update()
    {
        base.Update();
        CheckPinch();
    }

    private void CheckPinch()
    {
        try
        {
            if (!m_grabbedObj && m_grabCandidates.Count > 0)
            {
                PinchGrabable closest = FindClosestGrabble() as PinchGrabable;
                if (closest == null) return;
                if (m_pinchManager.IsPinchingWithRange(closest.PinchAction.Type, (closest.PinchAction.DifficultyPerPlayer / TherapyData._DifficultyRange)))
                {
                    GrabBegin();
                    PinchManager.PT++;
                }
            }
            else if (m_grabbedObj && !m_pinchManager.IsPinching(((PinchGrabable)m_grabbedObj).PinchAction.Type))
            {
                GrabEnd();
            }
        }
        catch (Exception e)
        {
            MainController.PrintToLog(e.ToString(), MainController.LogType.Error);
        }
    }

    private OVRGrabbable FindClosestGrabble()// This func from OVRGrabber
    {
        float closestMagSq = float.MaxValue;
        OVRGrabbable closestGrabbable = null;
        Collider closestGrabbableCollider = null;

        // Iterate grab candidates and find the closest grabbable candidate
        foreach (OVRGrabbable grabbable in m_grabCandidates.Keys)
        {
            bool canGrab = !(grabbable.isGrabbed && !grabbable.allowOffhandGrab);
            if (!canGrab)
            {
                continue;
            }

            for (int j = 0; j < grabbable.grabPoints.Length; ++j)
            {
                Collider grabbableCollider = grabbable.grabPoints[j];
                // Store the closest grabbable
                Vector3 closestPointOnBounds = Vector3.zero;
                try
                {
                    closestPointOnBounds = grabbableCollider.ClosestPointOnBounds(m_gripTransform.position);
                }
                catch (NullReferenceException)
                {
                    //MainController.PrintToLog("NullReferenceException - PinchGrabber.FindClosestGrabble()", MainController.LogType.Error);
                    continue;
                }
                float grabbableMagSq = (m_gripTransform.position - closestPointOnBounds).sqrMagnitude;
                if (grabbableMagSq < closestMagSq)
                {
                    closestMagSq = grabbableMagSq;
                    closestGrabbable = grabbable;
                    closestGrabbableCollider = grabbableCollider;
                }
            }
        }
        return closestGrabbable;
    }

    //protected override void GrabEnd()
    //{
    //    if (m_grabbedObj)
    //    {
    //        Vector3 linearVelocity = (transform.position - m_lastPos) / Time.fixedDeltaTime;
    //        Vector3 angularVelocity = (transform.eulerAngles - m_lastRot.eulerAngles) / Time.fixedDeltaTime;

    //        GrabbableRelease(linearVelocity, angularVelocity);
    //    }
    //    GrabVolumeEnable(true);
    //}

}
