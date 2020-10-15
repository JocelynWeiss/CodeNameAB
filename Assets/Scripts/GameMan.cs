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
    private float m_curRotForce = 0.0f;

    private GameObject m_localPlayerHead;


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
    }


    // Update is called once per frame
    void Update()
    {
        m_logTitle.text = $"{Time.fixedTime}s\n{m_netMan.networkAddress}";

        if (!m_netMan.isNetworkActive)
        {
            /*
            if (Time.fixedTime > 6.0f)
            {
                m_netMan.StartClient();
                //System.UriBuilder uri = new System.UriBuilder();
                //m_netMan.StartClient(uri.Uri);
            }
            */
            /*
            // Force start as a client
            if (Time.fixedTime > m_lastConnected + 6.0f)
            {
                m_lastConnected = Time.fixedTime;
                m_netMan.StartClient();
            }
            */

            // Force start as a client/host
            if (Time.fixedTime > m_lastConnected + 6.0f)
            {
                m_lastConnected = Time.fixedTime;
                m_netMan.StartHost();
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

        m_curRotForce = 0.0f;
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

        // Update head
        if (m_avatar != null)
        {
            PlayerControlMirror player = m_avatar.GetComponent<PlayerControlMirror>();
            player.transform.SetPositionAndRotation(m_localPlayerHead.transform.position, m_localPlayerHead.transform.rotation);
        }
    }
}
