using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FloatTextController : MonoBehaviour
{
    [SerializeField]
    private Animator m_floatTextAnim;
    [SerializeField]
    private TMP_Text m_floatText;


    private void Start()
    {
        if (m_floatTextAnim == null || m_floatText == null)
        {
            Debug.LogError("All fields must be initilized in FloatTextScript");
        }
    }

    public void UpdateFloatTextSettings(AnimationTypeIndex animType, string text, Color textOutlineColor)
    {
        m_floatText.text = text;
        m_floatText.color = textOutlineColor; //there is a gray mask on the prefab in the inspector - face color, makes the color darker
        m_floatText.outlineColor = textOutlineColor;
        m_floatTextAnim.SetInteger("AnimationType", (int)animType);
    }

    public void ClearAll()
    {
        m_floatTextAnim.SetInteger("AnimationType", -1);
        m_floatText.text = string.Empty;
    }

    public enum AnimationTypeIndex
    {
        None = -1, BottomToTop = 0, FarToNear
    }
}
