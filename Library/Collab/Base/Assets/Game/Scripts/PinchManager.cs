using UnityEngine;
using TMPro;
using System;
using Assets.Game.Scripts;
using static Assets.Game.Scripts.TherapyData;

public class PinchManager : MonoBehaviour
{
    public OVRHand m_LeftHand;
    public OVRHand m_RightHand;

    public static int PT = 0;

    private OVRPlugin.Hand whichHand;
    private OVRHand m_Hand = null;

    private PinchGrabber rightGrabber;
    private PinchGrabber leftGrabber;

    private OVRPlugin.HandState state;

    private bool m_is2PadPinch = false;
    private bool m_is3PadPinch = false;
    private bool m_is2TipPinch = false;
    private bool m_is3TipPinch = false;

    private void Start()
    {
        rightGrabber = m_RightHand.GetComponent<PinchGrabber>();
        leftGrabber = m_LeftHand.GetComponent<PinchGrabber>();
    }

    void Update()
    {
        try
        {
            if (m_Hand == null || !m_Hand.IsTracked || !m_Hand.IsDataHighConfidence)
            {
                ChangePinchStateToTrue(TherapyData.PinchType.None);
                return;
            }

            OVRPlugin.GetHandState(OVRPlugin.Step.Physics, whichHand, ref state);
            //LateralTestsAndPinchDebuging();

            if (m_Hand.GetFingerIsPinching(OVRHand.HandFinger.Index))
            {
                OVRPlugin.Quatf index = state.BoneRotations[(int)OVRSkeleton.BoneId.Hand_Index3];
                OVRPlugin.Quatf thumb = state.BoneRotations[(int)OVRSkeleton.BoneId.Hand_Thumb3];
                float angle = Vector3.Angle(new Vector3(index.x, index.y, index.z), new Vector3(thumb.x, thumb.y, thumb.z));

                if (angle < 70)// Tip Pinch
                {
                    if (m_Hand.GetFingerIsPinching(OVRHand.HandFinger.Middle))// 3 finger are pinching
                        ChangePinchStateToTrue(TherapyData.PinchType.Tip3);
                    else
                        ChangePinchStateToTrue(TherapyData.PinchType.Tip2);
                }
                else if (angle > 70)// Pad Pinch
                {
                    if (m_Hand.GetFingerIsPinching(OVRHand.HandFinger.Middle))// 3 finger are pinching
                        ChangePinchStateToTrue(TherapyData.PinchType.Pad3);
                    else
                        ChangePinchStateToTrue(TherapyData.PinchType.Pad2);
                }
            }
            else// Not pinching at all
            {
                ChangePinchStateToTrue(TherapyData.PinchType.None);
            }

        }
        catch (Exception e)
        {
            MainController.PrintToLog(e.ToString(), MainController.LogType.Error);
            //m_debugText.text = e.ToString();
        }
    }

    internal void SetHand(string hand)
    {
        switch (hand.ToLower())
        {
            case "left":
                if (rightGrabber != null && leftGrabber != null)
                {
                    rightGrabber.enabled = false;
                    leftGrabber.enabled = true;
                }
                whichHand = OVRPlugin.Hand.HandLeft;
                m_Hand = m_LeftHand;
                break;
            case "right":
                if (rightGrabber != null && leftGrabber != null)
                {
                    leftGrabber.enabled = false;
                    rightGrabber.enabled = true;
                }
                whichHand = OVRPlugin.Hand.HandRight;
                m_Hand = m_RightHand;
                break;
            default:
                MainController.PrintToLog("Hand wasn't set correctly.", MainController.LogType.Error);
                SetHand("Right");
                break;
        }
    }

    private void ChangePinchStateToTrue(TherapyData.PinchType type)
    {
        switch (type)
        {
            case PinchType.Pad2:
                m_is2PadPinch = true;
                m_is3PadPinch = false;
                m_is2TipPinch = false;
                m_is3TipPinch = false;
                break;
            case PinchType.Pad3:
                m_is2PadPinch = false;
                m_is3PadPinch = true;
                m_is2TipPinch = false;
                m_is3TipPinch = false;
                break;
            case PinchType.Tip2:
                m_is2PadPinch = false;
                m_is3PadPinch = false;
                m_is2TipPinch = true;
                m_is3TipPinch = false;
                break;
            case PinchType.Tip3:
                m_is2PadPinch = false;
                m_is3PadPinch = false;
                m_is2TipPinch = false;
                m_is3TipPinch = true;
                break;
            default:
                m_is2PadPinch = false;
                m_is3PadPinch = false;
                m_is2TipPinch = false;
                m_is3TipPinch = false;
                break;
        }
    }

    public bool IsPinchingWithRange(TherapyData.PinchType type, float threshold)
    {
        switch (type)
        {
            case TherapyData.PinchType.Pad2:
                return (m_is2PadPinch && m_Hand.GetFingerPinchStrength(OVRHand.HandFinger.Index) >= threshold);
            case TherapyData.PinchType.Pad3:
                return (m_is3PadPinch && m_Hand.GetFingerPinchStrength(OVRHand.HandFinger.Index) >= threshold);
            case TherapyData.PinchType.Tip2:
                return (m_is2TipPinch && m_Hand.GetFingerPinchStrength(OVRHand.HandFinger.Index) >= threshold);
            case TherapyData.PinchType.Tip3:
                return (m_is3TipPinch && m_Hand.GetFingerPinchStrength(OVRHand.HandFinger.Index) >= threshold);
        }
        return false;
    }

    public bool IsPinching(TherapyData.PinchType type)
    {
        switch (type)
        {
            case TherapyData.PinchType.Pad2:
                return m_is2PadPinch;
            case TherapyData.PinchType.Pad3:
                return m_is3PadPinch;
            case TherapyData.PinchType.Tip2:
                return m_is2TipPinch;
            case TherapyData.PinchType.Tip3:
                return m_is3TipPinch;
        }
        return false;
    }

    public float GetPinchStrength(TherapyData.PinchType type)
    {
        switch (type)
        {
            case TherapyData.PinchType.Pad2:
            case TherapyData.PinchType.Tip2:
                return m_Hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
            case TherapyData.PinchType.Tip3:
            case TherapyData.PinchType.Pad3:
                return m_Hand.GetFingerPinchStrength(OVRHand.HandFinger.Middle);
        }
        return 0;
    }

    private void LateralTestsAndPinchDebuging()
    {

        // print states:
        //if (rightState.PinchStrength[1] > 0.7f)
        //{
        //    var Index1 = state.BoneRotations[(int)OVRSkeleton.BoneId.Hand_Index1];
        //    var Index2 = state.BoneRotations[(int)OVRSkeleton.BoneId.Hand_Index2];
        //    var Thumb3 = state.BoneRotations[(int)OVRSkeleton.BoneId.Hand_Thumb3];
        //    var Thumb2 = state.BoneRotations[(int)OVRSkeleton.BoneId.Hand_Thumb2];
        //    string str =
        //        "1) Hand_Index1       " + new Vector3(Index1.x, Index1.y, Index1.z).ToString() +
        //        "\n2) Hand_Index2       " + new Vector3(Index2.x, Index2.y, Index2.z).ToString() +
        //        "\n3) Hand_Thumb3       " + new Vector3(Thumb3.x, Thumb3.y, Thumb3.z).ToString() +
        //        "\n4) Hand_Thumb2       " + new Vector3(Thumb2.x, Thumb2.y, Thumb2.z).ToString() +
        //        "\n Angle 1-2       " + Vector3.Angle(new Vector3(Index1.x, Index1.y, Index1.z), new Vector3(Index2.x, Index2.y, Index2.z)) +
        //        "\n Angle 3-4       " + Vector3.Angle(new Vector3(Thumb3.x, Thumb3.y, Thumb3.z), new Vector3(Thumb2.x, Thumb2.y, Thumb2.z));
        //    //"\n Angle 1-2       " + Vector3.Angle(new Vector3(Index1.x, Index1.y, Index1.z), new Vector3(Index2.x, Index2.y, Index2.z));
        //    var sw = File.AppendText(@"/sdcard/PinchProject/System/Lateral_States.txt");
        //    sw.WriteLine(str);
        //    sw.Close();
        //}

        /************************************************************************************************/
        // Leteral pinch test:
        //OVRPlugin.Quatf index1 = state.BoneRotations[(int)OVRSkeleton.BoneId.Hand_Index1];
        //OVRPlugin.Quatf index2 = state.BoneRotations[(int)OVRSkeleton.BoneId.Hand_Index2];
        //OVRPlugin.Quatf thumb3 = state.BoneRotations[(int)OVRSkeleton.BoneId.Hand_Thumb3];
        //OVRPlugin.Quatf thumb2 = state.BoneRotations[(int)OVRSkeleton.BoneId.Hand_Thumb2];
        //bool mycondition = Vector3.Angle(new Vector3(thumb3.x, thumb3.y, thumb3.z), new Vector3(index2.x, index2.y, index2.z)) > 147f &&
        //   Vector3.Angle(new Vector3(index1.x, index1.y, index1.z), new Vector3(index2.x, index2.y, index2.z)) < 90f;

        //m_debugText.text =
        //    "Is Thumb Pinching = " + m_Hand.GetFingerIsPinching(OVRHand.HandFinger.Thumb) +
        //    "\nIs index Pinching = " + m_Hand.GetFingerIsPinching(OVRHand.HandFinger.Index) +
        //    "\nAngle 1-2 = " + Vector3.Angle(new Vector3(index1.x, index1.y, index1.z), new Vector3(index2.x, index2.y, index2.z)) +
        //    "\nAngle TiPad3-2 = " + Vector3.Angle(new Vector3(thumb3.x, thumb3.y, thumb3.z), new Vector3(index2.x, index2.y, index2.z)) +
        //    "\nAngle TiPad3-t2 = " + Vector3.Angle(new Vector3(thumb3.x, thumb3.y, thumb3.z), new Vector3(thumb2.x, thumb2.y, thumb2.z)) +
        //   "\nMy Condition = " + mycondition;


        /*if (Vector3.Angle(new Vector3(index1.x, index1.y, index1.z), new Vector3(index2.x, index2.y, index2.z))<50 &&
            Vector3.Angle(new Vector3(thumb3.x, thumb3.y, thumb3.z), new Vector3(index2.x, index2.y, index2.z))<50 &&
            Vector3.Angle(new Vector3(thumb3.x, thumb3.y, thumb3.z), new Vector3(thumb2.x, thumb2.y, thumb2.z))<45)
        {
            m_debugText.text = "Lateral Pinch!";
        }
        else
            m_debugText.text = "No";*/
    }
}
