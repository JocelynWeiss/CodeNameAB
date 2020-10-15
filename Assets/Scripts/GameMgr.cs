using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Platform;
using Oculus.Platform.Models;
using System.Diagnostics.Eventing.Reader;
using TMPro;

public class GameMgr : MonoBehaviour
{
    protected static GameMgr s_instance = null;
    public bool m_coreInitialized = false;
    public Room m_clientRoom;

    protected RoomManager m_roomManager;
    protected P2PManager m_p2pManager;

    protected GameObject m_localTrackingSpace;
    protected GameObject m_localPlayerHead;


    public void Awake()
    {
        Debug.Log($"GameMgr Awake @ {Time.fixedTime}s");

        // make sure only one instance of this manager ever exists
        if (s_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        s_instance = this;
        DontDestroyOnLoad(gameObject);

        Canvas myCanvas = GameObject.Find("Canvas").gameObject.GetComponent<Canvas>();
        if (myCanvas != null)
        {
            GameObject intro = myCanvas.transform.GetChild(0).gameObject;
            intro.GetComponent<TextMeshProUGUI>().text = "entry_room_1";
        }

        // Set up the local player
        OVRCameraRig rig = GameObject.Find("OVRCameraRig").gameObject.GetComponent<OVRCameraRig>();
        m_localTrackingSpace = rig.transform.Find("TrackingSpace").gameObject;
        m_localPlayerHead = rig.transform.Find("TrackingSpace/CenterEyeAnchor").gameObject;

        Core.AsyncInitialize().OnComplete(InitCallback);

        m_roomManager = new RoomManager();
        m_p2pManager = new P2PManager();
    }


    void InitCallback(Message<PlatformInitialize> msg)
    {
        if (msg.IsError)
        {
            //TerminateWithError(msg);
            Debug.LogError($"Cannot init platform @ {Time.fixedTime}, error: {msg.GetError().Message}");
            return;
        }

        LaunchDetails launchDetails = ApplicationLifecycle.GetLaunchDetails();
        Debug.Log($"App launched @ {Time.fixedTime} with LaunchType " + launchDetails.LaunchType);

        // First thing we should do is perform an entitlement check to make sure
        // we successfully connected to the Oculus Platform Service.
        Entitlements.IsUserEntitledToApplication().OnComplete(IsEntitledCallback);

        // Next get the identity of the user that launched the Application.
        Users.GetLoggedInUser().OnComplete(GetLoggedInUserCallback);
    }


    // Start is called before the first frame update
    void Start()
    {
        //OVRP_PUBLIC_FUNCTION(ovrRequest) ovr_User_GetLoggedInUser();

        //Core.Initialize();
        //ovr_PopMessage();
        //Platform.Net.Connect
        //Core.Initialize("JowTestApp"); // sort of ok

        //Platform
        Debug.Log($"GameMgr Init @ {Time.fixedTime}");
    }


    // Update is called once per frame
    void Update()
    {
        if ((Core.IsInitialized()) && (!m_coreInitialized))
        {
            m_coreInitialized = true;
            Debug.Log($"Core is initialized !");
        }

        if (Input.GetKeyUp(KeyCode.C))
        {
            if (m_clientRoom != null)
            {
                Debug.Log($"Room is created !");
            }
            else
            {
                //m_clientRoom = new Room(666);
            }
        }
    }


    void IsEntitledCallback(Message msg)
    {
        if (msg.IsError)
        {
            Debug.LogError($"Cannot IsEntitledCallback @ {Time.fixedTime}, error: {msg.GetError().Message}");
            return;
        }

        // Next get the identity of the user that launched the Application.
        Users.GetLoggedInUser().OnComplete(GetLoggedInUserCallback);
    }


    void GetLoggedInUserCallback(Message<User> msg)
    {
        if (msg.IsError)
        {
            Debug.LogError($"Cannot GetLoggedInUserCallback @ {Time.fixedTime}, error: {msg.GetError().Message}");
            return;
        }
    }
}
