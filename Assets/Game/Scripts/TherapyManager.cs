using System;
using System.Collections.Generic;
using UnityEngine;
using static Assets.Game.Scripts.TherapyData;
using static FloatTextController;
using Random = UnityEngine.Random;

// TherapyManager is in charge of creating the game scene. 
// When loading the game scene it takes the data about the patient from the MainController
// And initialise the scene with the appropriate objects.
// In addition it monitor the progress of the game and the treatment and updates the data directly to the MainController.

public class TherapyManager : MonoBehaviour
{
    private MainController m_mainController;
    private FloatTextController m_floatTextController;
    [SerializeField]
    private List<Material> m_skyboksMaterials;
    [SerializeField]
    private AudioClip m_winningSound;
    private AudioSource m_audioSource;

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

    public float m_areaWidth = 1.4f;
    public float m_areaHeight = 0.39f;
    public float m_pad2_XAnchor = -0.237f;
    public float m_pad3_XAnchor = 0.539f;
    public float m_tip2_XAnchor = -0.647f;
    public float m_tip3_XAnchor = 0.169f;
    private float maxDistance = 0;

    private void Awake()
    {
        m_mainController = FindObjectOfType<MainController>();
        m_floatTextController = FindObjectOfType<FloatTextController>();
        m_audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        try
        {
            if (m_mainController == null || m_mainController.CurrentPatient == null
                || m_floatTextController == null || m_skyboksMaterials == null
                || m_audioSource == null || m_winningSound == null)
            {
                Debug.LogError("Program didnt Initialize.");
            }

            // Init algorithm parametes according to patient abillity:
            dpP2 = (int)m_mainController.CurrentPatient.MotionRange.Pad2 * _DifficultyRange;
            dpT2 = (int)m_mainController.CurrentPatient.MotionRange.Tip2 * _DifficultyRange;
            dpP3 = (int)m_mainController.CurrentPatient.MotionRange.Pad3 * _DifficultyRange;
            dpT3 = (int)m_mainController.CurrentPatient.MotionRange.Tip3 * _DifficultyRange;
            maxDistance = m_areaHeight;
            PinchManager.PT = 0;
            FindObjectOfType<PinchManager>().SetHand(m_mainController.CurrentPatient.Hand);
            InitilizeSceneAccordingToPlan();
            ChangeSkyboxMaterial();
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
            m_notFinished = true;
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
            float distanceInPrecentage = (float)pinchAction.DifficultyLevel / (float)(_DifficultyMax);
            float wantedDistance = distanceInPrecentage * maxDistance;

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


            obj.transform.parent = m_PinchableParent.transform;
            //obj.transform.localPosition = Vector3.zero;
            // obj.transform.localPosition = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), Random.Range(minZ, maxZ));
            obj.transform.localPosition = GetRandomLocation(pinchAction, wantedDistance);
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
        while (!finished && count < 10)
        {
            count++;
            Random.InitState(System.DateTime.Now.Millisecond);
            float angle = Random.Range((float)Math.PI, (float)(2 * Math.PI));
            x = (float)Math.Cos(angle) * radius + xCenter;
            z = Math.Min((float)Math.Sin(angle) * radius, m_PinchableParent.transform.position.z);
            //x = xCenter;
            //z = -maxDistance;
            if (CheckIfPointInArea(x, z))
            {
                finished = true;
            }
        }
        return new Vector3(x, 0, z);
    }

    //private bool CheckIfPointInArea(float x, float z)// m_PinchableParent.transform is in the middle
    //{
    //    if (x < m_PinchableParent.transform.position.x||
    //        x > m_PinchableParent.transform.position.x + m_areaWidth) return false;
    //    if (z < m_PinchableParent.transform.position.z||
    //        z > m_PinchableParent.transform.position.z + m_areaHeight) return false;

    //    return true;
    //}

    private bool CheckIfPointInArea(float x, float z)// m_PinchableParent.transform is in the upper middle part of the area
    {
        if (x < -0.7 ||
            x > 0.7) return false;
        if (z < -m_areaHeight || z > -0.01) return false;

        try
        {
            foreach (GameObject obj in m_PinchableObjList)
            {
                if (obj != null)
                {
                    if (obj.transform.localPosition.x == x && obj.transform.localPosition.z == z)
                        return false;
                }
            }
        }
        catch (Exception e)
        {
            MainController.PrintToLog(e.ToString(), MainController.LogType.Error);
        }

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
        if (!m_audioSource.isPlaying)
            m_mainController.m_audioSource.volume = 0.6f;

    }

    private void LevelEnded()
    {
        m_mainController.m_audioSource.volume = 0.1f;
        try
        {
            m_floatTextController.UpdateFloatTextSettings(AnimationTypeIndex.BottomToTop, "Level up!", Color.green);
        }
        catch (Exception e)
        {
            MainController.PrintToLog("Can't do level up animation - " + e.ToString(), MainController.LogType.Error);
        }
        try
        {
            m_audioSource.clip = m_winningSound;
            m_audioSource.Play();
        }
        catch (Exception e)
        {
            MainController.PrintToLog("Can't play winning sound - " + e.ToString(), MainController.LogType.Error);
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
        try
        {
            QuestFileManager.UpdatePatientOnFile(m_mainController.CurrentPatient, PinchConstants.PatientsDirectoryPath + m_mainController.CurrentPatient.Id);
                }
        catch(Exception e)
        {
            MainController.PrintToLog("Can't update patient file in TherapyManager - " + e.ToString(), MainController.LogType.Error);
        }
        InitilizeSceneAccordingToPlan();
    }


    public void ChangeSkyboxMaterial()
    {
        m_mainController.CurrentPatient.LastSkyBoxIndex = (m_mainController.CurrentPatient.LastSkyBoxIndex + 1) % m_skyboksMaterials.Count;
        RenderSettings.skybox = m_skyboksMaterials[m_mainController.CurrentPatient.LastSkyBoxIndex];
    }

    void OnApplicationQuit()
    {
        RenderSettings.skybox = m_skyboksMaterials[0];
    }
}
