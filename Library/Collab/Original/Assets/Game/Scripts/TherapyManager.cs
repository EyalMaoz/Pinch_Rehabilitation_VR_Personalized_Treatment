using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Game.Scripts;
using UnityEngine;
using static Assets.Game.Scripts.TherapyData;
using static FloatTextController;
using Random = UnityEngine.Random;

public class TherapyManager : MonoBehaviour
{
    private MainController m_mainController;
    private FloatTextController m_floatTextController;

    Dictionary<PinchType, UnityEngine.Color> pinchToColorDict = new Dictionary<PinchType, UnityEngine.Color>()
    {
        {PinchType.Pad2,UnityEngine.Color.green },
        {PinchType.Tip2,UnityEngine.Color.red },
        {PinchType.Tip3,UnityEngine.Color.yellow},
        {PinchType.Pad3,UnityEngine.Color.blue}
    };


    private bool m_notFinished = true;
    [SerializeField]
    private TMPro.TMP_Text m_debugText;
    [SerializeField]
    private GameObject m_PinchableParent;

    private List<GameObject> m_PinchableObjList = new List<GameObject>();

    //public float minX;
    //public float maxX;
    //public float minY;
    //public float maxY;
    //public float minZ;
    //public float maxZ;
    /// <summary>
    /// The width of the pinchable area to deploy the objects
    /// </summary>
    public float m_areaWidth = 1.4f;
    public float m_areaHeight = 1.5f;
    public float m_pad2_XAnchor = -0.237f;
    public float m_pad3_XAnchor = 0.539f;
    public float m_tip2_XAnchor = -0.647f;
    public float m_tip3_XAnchor = 0.169f;
    private float maxDistance = 0;

    private void Awake()
    {
        m_mainController = FindObjectOfType<MainController>();
        m_floatTextController = FindObjectOfType<FloatTextController>();
    }

    void Start()
    {
        try
        {
            if (m_mainController == null || m_mainController.CurrentPatient == null)
            {
                //Debug.LogError("Program didnt Initialize.");
            }
            //if (minX == maxX && minY == maxY && minZ == maxZ)
            //{
            //    Debug.LogError("Initialize boundry in TherapyInitialization.");
            //}
            // Init algorithm parametes according to patient abillity:
            dpP2 = (int)m_mainController.CurrentPatient.MotionRange.Pad2 * _DifficultyRange;
            dpT2 = (int)m_mainController.CurrentPatient.MotionRange.Tip2 * _DifficultyRange;
            dpP3 = (int)m_mainController.CurrentPatient.MotionRange.Pad3 * _DifficultyRange;
            dpT3 = (int)m_mainController.CurrentPatient.MotionRange.Tip3 * _DifficultyRange;
            maxDistance = (float)Math.Sqrt(Math.Pow(m_areaWidth, 2) + Math.Pow(m_areaHeight, 2));
            PinchManager.PT = 0;
            FindObjectOfType<PinchManager>().SetHand(m_mainController.CurrentPatient.Hand);
            InitilizeSceneAccordingToPlan();
        }
        catch (Exception e)
        {
            MainController.PrintToLog(e.ToString(), MainController.LogType.Error);
        }
    }

    private void InitilizeSceneAccordingToPlan()
    {
        try
        {
            m_notFinished = true;

            foreach (Challenge challenge in m_mainController.CurrentPatient.CurrentTreatment.Plan)
            {
                foreach (PinchAction action in challenge.ActionsList)
                {
                    PrimitiveType shape = PrimitiveType.Cube;
                    switch (action.Type)
                    {
                        case PinchType.Pad2:
                            shape = PrimitiveType.Cube;
                            break;
                        case PinchType.Pad3:
                            shape = PrimitiveType.Capsule;
                            break;
                        case PinchType.Tip2:
                            shape = PrimitiveType.Cylinder;
                            break;
                        case PinchType.Tip3:
                            shape = PrimitiveType.Sphere;
                            break;
                    }
                    CreatePinchableObject(shape, action);
                }
            }
        }
        catch (Exception e)
        {
            MainController.PrintToLog(e.ToString(), MainController.LogType.Error);
        }
    }

    public void CreatePinchableObject(PrimitiveType shapeType, PinchAction pinchAction)
    {
        try
        {
            GameObject obj = GameObject.CreatePrimitive(shapeType);
            obj.transform.parent = m_PinchableParent.transform;
            float distanceInPrecentage = pinchAction.DifficultyLevel / (_DifficultyMax - _DifficultyMin);
            float wantedDistance = distanceInPrecentage * maxDistance;
            // obj.transform.localPosition = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), Random.Range(minZ, maxZ));
            obj.transform.localPosition = GetRandomLocation(pinchAction, wantedDistance);

            switch (shapeType)
            {
                case PrimitiveType.Cylinder:
                    obj.AddComponent<CapsuleCollider>();// for trigger
                    obj.transform.localScale = new Vector3(0.02f, 0.05f, 0.02f);
                    break;
                case PrimitiveType.Cube:
                    obj.AddComponent<BoxCollider>();// for trigger
                    obj.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                    //obj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    break;
                case PrimitiveType.Sphere:
                    obj.AddComponent<SphereCollider>();// for trigger
                    obj.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                    //obj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    break;
                case PrimitiveType.Capsule:
                    obj.AddComponent<CapsuleCollider>();// for trigger
                    obj.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
                    break;
            }

            obj.GetComponent<Collider>().isTrigger = true;


            obj.AddComponent<Rigidbody>();
            obj.GetComponent<Rigidbody>().useGravity = true;

            obj.AddComponent<PinchGrabable>();
            obj.GetComponent<PinchGrabable>().enabled = true;
            obj.GetComponent<PinchGrabable>().PinchAction = pinchAction;

            obj.GetComponent<Renderer>().material.color = pinchToColorDict[pinchAction.Type];

            m_PinchableObjList.Add(obj);
        }
        catch (Exception e)
        {
            MainController.PrintToLog(e.ToString(), MainController.LogType.Error);
        }
    }

    private Vector3 GetRandomLocation(PinchAction pinchAction, float radius)
    {
        float xCenter = 0;
        float x = 0;
        float z = 0;
        int count = 0;
        bool finished = false;
        switch (pinchAction.Type)
        {
            case PinchType.Pad2:
                xCenter = m_pad2_XAnchor;
                break;
            case PinchType.Pad3:
                xCenter = m_pad3_XAnchor;
                break;
            case PinchType.Tip2:
                xCenter = m_tip2_XAnchor;
                break;
            case PinchType.Tip3:
                xCenter = m_tip3_XAnchor;
                break;
        }
        while (!finished && count < 5)
        {
            count++;
            float angle = Random.Range((float)Math.PI, (float)(2 * Math.PI));
            x = (float)Math.Cos(angle) * radius + xCenter;
            z = (float)Math.Sin(angle) * radius;
            if (CheckIfPointInArea(x, z))
            {
                finished = true;
            }
        }
        return new Vector3(x, m_PinchableParent.transform.position.y, z);
    }

    private bool CheckIfPointInArea(float x, float z)
    {
        if (x < m_PinchableParent.transform.position.x ||
            x > m_PinchableParent.transform.position.x + m_areaWidth) return false;
        if (z > m_PinchableParent.transform.position.z ||
            z < m_PinchableParent.transform.position.z - m_areaHeight) return false;

        return true;
    }

    void Update()
    {
        if (m_notFinished)
        {
            if (m_PinchableObjList.Count == 0) return;
            int unactiveCounter = 0;
            foreach (GameObject obj in m_PinchableObjList)
            {
                if (obj != null)
                {
                    if (obj.transform.position.y < 0.1)// if the piece fell on the floor
                    {
                        PinchManager.PT++;
                        Destroy(obj);
                    }
                    continue;
                }
                unactiveCounter++;
            }

            if (unactiveCounter == m_PinchableObjList.Count)// Level over!!
            {
                m_notFinished = false;
                LevelEnded();
            }
        }
    }

    private void LevelEnded()
    {
        try
        {
            m_floatTextController.UpdateFloatTextSettings(AnimationTypeIndex.FarToNear, "Level up!", Color.green);
        }
        catch (Exception e)
        {
            MainController.PrintToLog("cant do level up animation - " + e.ToString(), MainController.LogType.Error);
        }
        m_mainController.CurrentPatient.TreatmentsHistory.Add(m_mainController.CurrentPatient.CurrentTreatment);
        int newTreatmentNumber = m_mainController.CurrentPatient.CurrentTreatment.TreatmentNumber + 1;
        int ST = 0;
        foreach (Challenge c in m_mainController.CurrentPatient.CurrentTreatment.Plan)
        {
            ST += c.ActionsList.Count;
        }

        m_mainController.CurrentPatient.CurrentTreatment = new TreatmentPlan()
        {
            TreatmentNumber = newTreatmentNumber,
            CreationTime = DateTime.Now.ToShortDateString() + " " +
                DateTime.Now.ToLongTimeString(),
            Plan = Algorithm.GenerateNewLevel(m_mainController.CurrentPatient.CurrentTreatment.Plan, PinchManager.PT, ST)
        };
        PinchManager.PT = 0;
        InitilizeSceneAccordingToPlan();
    }
}
