using UnityEngine;

public class PlayerHeight : MonoBehaviour
{
    [SerializeField]
    private GameObject ovrCameraRig;

    // Start is called before the first frame update
    void Start()
    {
        if(ovrCameraRig==null)
        {
            Debug.LogError("ovrCameraRig not initialized ");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickUp))
        {
            ovrCameraRig.transform.position = new Vector3(ovrCameraRig.transform.position.x,
                ovrCameraRig.transform.position.y + 0.01f,
                ovrCameraRig.transform.position.z);
        }
        else if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickDown))
        {
            ovrCameraRig.transform.position = new Vector3(ovrCameraRig.transform.position.x,
                ovrCameraRig.transform.position.y - 0.01f,
                ovrCameraRig.transform.position.z);
        }
    }
}
