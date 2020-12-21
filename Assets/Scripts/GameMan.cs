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
    private GameObject m_avatar; // Deprecated, Todo: remove me
    private PlayerControlMirror m_myAvatar;
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
        m_netMan.networkAddress = "localhost"; // overwrite public field when not at Henigma...
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

        PlayerControlMirror player = newPlayer.GetComponent<PlayerControlMirror>();
        if (hasAuthority)
        {
            newPlayer.name = "MyAvatar"; // There is only one client which has the authority
            m_avatar = newPlayer;
            m_myAvatar = player;
        }

        Debug.Log($"---+++ Registering new player @ {Time.fixedTime}s, {newPlayer}, netId {player.netId}");
    }


    public void RegisterNewTool2(Tools2Mirror tool, bool hasAuthority)
    {
        if (hasAuthority)
        {
            m_myAvatar.m_tool = tool;
        }

        //Debug.Log($"+++ Registering new tool2 @ {Time.fixedTime}s, {tool}, netId {tool.netId}, authority= {hasAuthority}, isLocalPlayer {player.isLocalPlayer}");

        Debug.Log($"+++ Registering new tool2 @ {Time.fixedTime}s, {tool}, netId {tool.netId}, authority= {hasAuthority}");
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

        if (m_avatar)
        {
            m_logTitle.text = $"{m_avatar.transform.position}";
        }

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
            else
            {
                Application.Quit();
            }
        }

        // Spawn unsync entity
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

        // Spawn sync entity
        if (Input.GetKeyUp(KeyCode.T))
        {
            //*
            if (m_avatar != null)
            {
                PlayerControlMirror player = m_avatar.GetComponent<PlayerControlMirror>();

                // Check if already there
                if (player.m_tool == null)
                {
                    GameObject go = GameObject.Find("AvatarTool2");
                    Tools2Mirror tool = null;
                    if (go != null)
                    {
                        tool = go.GetComponent<Tools2Mirror>();
                    }
                    if (tool != null)
                    {
                        player.m_tool = tool;
                    }
                    else
                    {
                        player.SpawnMyTool2();

                        // Assign player tool2 by name
                        /*
                        if (player.m_tool == null)
                        {
                            player.m_tool = GameObject.Find("AvatarTool2").GetComponent<Tools2Mirror>();
                        }
                        */
                    }
                }
            }
            //*/
        }

        if (Input.GetKey(KeyCode.Keypad6))
        {
            if (m_avatar != null)
            {
                PlayerControlMirror player = m_avatar.GetComponent<PlayerControlMirror>();
                player.m_tool.transform.RotateAround(player.m_tool.transform.position, Vector3.up, 100.0f * Time.deltaTime);
            }
        }
        if (Input.GetKey(KeyCode.Keypad4))
        {
            if (m_avatar != null)
            {
                PlayerControlMirror player = m_avatar.GetComponent<PlayerControlMirror>();
                player.m_tool.transform.RotateAround(player.m_tool.transform.position, Vector3.up, -100.0f * Time.deltaTime);
            }
        }
        if (Input.GetKey(KeyCode.Keypad9))
        {
            if (m_avatar != null)
            {
                PlayerControlMirror player = m_avatar.GetComponent<PlayerControlMirror>();
                Vector3 pos = player.m_tool.transform.position;
                pos += Vector3.up * 1.0f * Time.deltaTime;
                Quaternion q = player.m_tool.transform.rotation;
                player.m_tool.transform.SetPositionAndRotation(pos, q);
            }
        }
        if (Input.GetKey(KeyCode.Keypad7))
        {
            if (m_avatar != null)
            {
                PlayerControlMirror player = m_avatar.GetComponent<PlayerControlMirror>();
                Vector3 pos = player.m_tool.transform.position;
                pos -= Vector3.up * 1.0f * Time.deltaTime;
                Quaternion q = player.m_tool.transform.rotation;
                player.m_tool.transform.SetPositionAndRotation(pos, q);
            }
        }
        if (Input.GetKey(KeyCode.Keypad8))
        {
            if (m_myAvatar != null)
            { 
                if (m_myAvatar.m_tool != null)
                {
                    Transform t = m_myAvatar.m_tool.transform;
                    Vector3 pos = t.position;
                    pos += t.up * 1.0f * Time.deltaTime;
                    Quaternion q = t.rotation;
                    t.SetPositionAndRotation(pos, q);
                }
            }
        }
        if (Input.GetKey(KeyCode.Keypad5))
        {
            if (m_avatar != null)
            {
                PlayerControlMirror player = m_avatar.GetComponent<PlayerControlMirror>();
                Vector3 pos = player.m_tool.transform.position;
                pos += player.m_tool.transform.up * -0.8f * Time.deltaTime;
                Quaternion q = player.m_tool.transform.rotation;
                player.m_tool.transform.SetPositionAndRotation(pos, q);
            }
        }
        if (Input.GetKeyUp(KeyCode.Keypad3))
        {
            if (m_myAvatar != null)
            {
                if (m_myAvatar.m_tool != null)
                {
                    // Set right spinning mode
                    m_myAvatar.m_tool.m_rightSpinning = !m_myAvatar.m_tool.m_rightSpinning;
                    m_myAvatar.m_tool.m_leftSpinning = false;
                }
            }
        }
        if (Input.GetKeyUp(KeyCode.Keypad1))
        {
            if (m_myAvatar != null)
            {
                if (m_myAvatar.m_tool != null)
                {
                    // Set right spinning mode
                    m_myAvatar.m_tool.m_leftSpinning = !m_myAvatar.m_tool.m_leftSpinning;
                    m_myAvatar.m_tool.m_rightSpinning = false;
                }
            }
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
        //---
        /*
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
