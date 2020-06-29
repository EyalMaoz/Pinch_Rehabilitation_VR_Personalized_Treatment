using Assets.Game.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// PinchGrabable is a script that attached to any object you want to
// be grabable by a pinch movement.

public class PinchGrabable : OVRGrabbable
{
    [SerializeField]
    //public TherapyData.PinchType PinchType = TherapyData.PinchType.None;
    public TherapyData.PinchAction PinchAction = new TherapyData.PinchAction(TherapyData.PinchType.None,TherapyData._DifficultyMin);
}
