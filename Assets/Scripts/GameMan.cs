using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;


// A game manager dedicated to test mirror networking services...


public class GameMan : MonoBehaviour
{
    public static GameMan s_instance = null;

    TextMeshProUGUI m_logTitle;

    public NetworkManager m_netMan;

    private float m_lastConnected = 0.0f;
    private GameObject m_avatar;
    public float m_upForce = 300.0f;
    public float m_rotForce = 10.0f;
    public float m_YForce = 5.0f;
    private float m_curRotForce = 0.0f;
    private float m_curYForce = 0.0f;
    private bool m_tryingConnectAsClient = false;

    private GameObject m_localPlayerHead;
    OVRHand m_leftHand;
    OVRHand m_rightHand;


    public void Awake()
    {
        Debug.Log($"GameMan Awake @ {Time.fixedTime}s");

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
            m_logTitle = intro.GetComponent<TextMeshProUGUI>();
            m_logTitle.text = "entry_room_1";
        }

        // Set up the local player
        OVRCameraRig rig = GameObject.Find("OVRCameraRig").gameObject.GetComponent<OVRCameraRig>();
        //m_localTrackingSpace = rig.transform.Find("TrackingSpace").gameObject;
        m_localPlayerHead = rig.transform.Find("TrackingSpace/CenterEyeAnchor").gameObject;
        // Grab hands
        m_leftHand = GameObject.Find("OVRCameraRig/TrackingSpace/LeftHandAnchor/LeftControllerAnchor/OVRHandPrefab").GetComponent<OVRHand>();
        m_rightHand = GameObject.Find("OVRCameraRig/TrackingSpace/RightHandAnchor/RightControllerAnchor/OVRHandPrefab").GetComponent<OVRHand>();

        if ((m_leftHand == null) || (m_rightHand == null))
        {
            Debug.LogError($"GameMan Init Cannot find hands !");
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"GameMan Init @ {Time.fixedTime}s");
    }


    public void RegisterNewPlayer(GameObject newPlayer, bool hasAuthority)
    {
        /*
        PlayerControlMirror player = newPlayer.GetComponent<PlayerControlMirror>();
        if (player.hasAuthority)
        {
            Vector3 newCol = Random.insideUnitSphere;
            player.m_syncColor = new Color(newCol.x, newCol.y, newCol.z);
            player.c = player.m_syncColor;
        }
        */
        if (hasAuthority)
        {
            m_avatar = newPlayer;
        }

        Debug.Log($"---+++ Registering new player @ {Time.fixedTime}s, {newPlayer}");
    }


    private void FixedUpdate()
    {
        if (m_curRotForce != 0.0f)
        {
            PlayerControlMirror player = m_avatar.GetComponent<PlayerControlMirror>();
            player.transform.RotateAround(player.transform.position, Vector3.up, m_curRotForce * Time.fixedDeltaTime);
        }

        if (m_curYForce != 0.0f)
        {
            PlayerControlMirror player = m_avatar.GetComponent<PlayerControlMirror>();
            Vector3 pos = player.transform.position;
            pos += Vector3.up * m_curYForce * Time.fixedDeltaTime;
            player.transform.SetPositionAndRotation(pos, player.transform.rotation);
        }
    }


    // Update is called once per frame
    void Update()
    {
        m_logTitle.text = $"{Time.fixedTime}s\n{m_netMan.networkAddress}\n{m_lastConnected}s";

        if (!m_netMan.isNetworkActive)
        {
            // Force start as a client/host
            if (Time.fixedTime > 8.0f) // First client connect failed, force host
            {
                m_lastConnected = Time.fixedTime;
                m_netMan.StartHost();
            }
            // Try connecting to host as normal client
            else if ((Time.fixedTime > m_lastConnected + 5.0f) && (m_tryingConnectAsClient == false))
            {
                m_tryingConnectAsClient = true;
                m_netMan.StartClient();
            }
        }
        else
        {
            m_lastConnected = Time.fixedTime;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            // ...
            /*
            m_avatar = GameObject.Find("AvatarPrefab_A(Clone)").gameObject;
            m_avatar.name = $"Avatar_{m_netMan.numPlayers}";
            m_avatar.transform.SetPositionAndRotation(new Vector3(1.0f, 1.0f, 1.0f), Quaternion.identity);
            */

            /*
            PlayerControlMirror player = m_avatar.GetComponent<PlayerControlMirror>();
            if (player.hasAuthority)
            {
                player.m_rb.AddForce(Vector3.up * m_upForce);
            }
            */

            if (m_avatar != null)
            {
                PlayerControlMirror player = m_avatar.GetComponent<PlayerControlMirror>();
                player.Jump();
            }
        }

        if (Input.GetKeyUp(KeyCode.C))
        {
            if (m_avatar != null)
            {
                PlayerControlMirror player = m_avatar.GetComponent<PlayerControlMirror>();
                player.ChangeAvatarColour();
            }
        }

        // Place player at pos
        if (Input.GetKeyUp(KeyCode.P))
        {
            if (m_avatar != null)
            {
                PlayerControlMirror player = m_avatar.GetComponent<PlayerControlMirror>();
                //player.PositionPlayer(new Vector3(0.1f, 1.0f, 1.8f));
                player.transform.SetPositionAndRotation(new Vector3(0.1f, 1.0f, 1.8f), Quaternion.identity);
            }
        }

        // Spawn entity
        if (Input.GetKeyUp(KeyCode.R))
        {
            //*
            if (m_avatar != null)
            {
                PlayerControlMirror player = m_avatar.GetComponent<PlayerControlMirror>();
                player.SpawnMyTool();
            }
            //*/
        }

        m_curRotForce = 0.0f;
        m_curYForce = 0.0f;
        /*
        if (Input.GetKey(KeyCode.D))
        {
            if (m_avatar != null)
            {
                m_curRotForce = m_rotForce;
            }
        }
        else if (Input.GetKey(KeyCode.A))
        {
            if (m_avatar != null)
            {
                m_curRotForce = -m_rotForce;
            }
        }
        */

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        m_curRotForce = horizontal * m_rotForce;
        m_curYForce = vertical * m_YForce;

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Application.Quit();
        }

        // Check hands
        if (m_leftHand.GetFingerIsPinching(OVRHand.HandFinger.Middle))
        {
            PlayerControlMirror player = m_avatar.GetComponent<PlayerControlMirror>();
            player.SpawnMyTool();
        }
        else if (m_rightHand.GetFingerIsPinching(OVRHand.HandFinger.Middle))
        {
            PlayerControlMirror player = m_avatar.GetComponent<PlayerControlMirror>();
            player.SpawnMyTool();
        }

        // Update head
#if UNITY_EDITOR
#else
        //*
        if (NetworkManager.singleton.mode == NetworkManagerMode.Host)
        {
            if (m_avatar != null)
            {
                PlayerControlMirror player = m_avatar.GetComponent<PlayerControlMirror>();
                if (player.isServer)
                {
                    player.transform.SetPositionAndRotation(m_localPlayerHead.transform.position, m_localPlayerHead.transform.rotation);
                }
            }
        }
        //*/
#endif
    }
}
