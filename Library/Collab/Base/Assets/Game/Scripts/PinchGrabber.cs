using Assets.Game.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinchGrabber : OVRGrabber
{
    [SerializeField] OVRHand m_Hand = null;
    [SerializeField] TMPro.TMP_Text m_debugText;
    [SerializeField] PinchManager m_pinchManager;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
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
                PinchGrabable closest = (PinchGrabable)FindClosestGrabble();
                //if (m_pinchManager.IsPinching(closest.PinchAction.Type))
                //{
                //    GrabBegin();
                //}
                if (m_pinchManager.IsPinchingWithRange(closest.PinchAction.Type, (closest.PinchAction.DifficultyPerPlayer / TherapyData._DifficultyRange)))
                {
                    GrabBegin();
                }
            }
            else if (m_grabbedObj && !m_pinchManager.IsPinching(((PinchGrabable)m_grabbedObj).PinchAction.Type))
            {
                GrabEnd();
            }
        }
        catch (Exception e)
        {
            m_debugText.text = e.Message;
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
                Vector3 closestPointOnBounds = grabbableCollider.ClosestPointOnBounds(m_gripTransform.position);
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
