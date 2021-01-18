using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
using OculusSampleFramework;


// A game manager dedicated to test mirror networking services...
//JowNext: recentrer le cylindre, tester le grab des éléments, tirer de la main


// Our 4 Elements in the game
public enum Elements
{
    Earth,
    Water,
    Air,
    Fire
}


public class GameMan : MonoBehaviour
{
    public static GameMan s_instance = null;

    TextMeshProUGUI m_logTitle;

    public NetworkManager m_netMan;

    public int m_playerNb = 0;
    private float m_lastConnected = 0.0f;
    private GameObject m_avatar; // Deprecated, Todo: remove me
    private PlayerControlMirror m_myAvatar;
    private GameObject m_myPillar; // The pillar where this avatar is sitting on
    public float m_upForce = 300.0f;
    public float m_rotForce = 10.0f;
    public float m_YForce = 5.0f;
    private float m_curRotForce = 0.0f;
    private float m_curYForce = 0.0f;
    private bool m_tryingConnectAsClient = false;
    public int m_waveNb = 0;
    public float m_nextWaveTime = 0.0f;
    public WaveClass m_wave; // The current mobs wave
    public JowProgressBar m_playerLifeBar;

    GameObject m_MobA;

    private bool m_isUsingHands = false;
    private GameObject m_localPlayerHead;
    OVRHand m_leftHand;
    OVRHand m_rightHand;
    //GameObject m_leftAnchor;
    //GameObject m_rightAnchor;
    private float m_rightLastBulletTime = 0.0f;
    private float m_leftLastBulletTime = 0.0f;
    private float m_rightRPS = 1.0f; // Round per seconds (seconds between 2 shots)
    private float m_leftRPS = 1.0f; // Round per seconds (seconds between 2 shots)
    public OVRCameraRig m_cameraRig;

    Material[] m_CubesElemMats = new Material[4];
    public int m_startElementCount = 4;
    public GameObject m_elementCubePrefab;
    List<ElementsScript> m_elemCubes = new List<ElementsScript>();

    // Audio...
    //[InspectorNote("Sound Setup", "Press '1' to play testSound1 and '2' to play testSound2")]
    //public OVR.SoundFXRef testSound1;
    //public OVR.SoundFXRef testSound2;
    [InspectorNote("Audio Sounds Setup")]
    public List<AudioClip> m_audioSounds;


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
        m_cameraRig = GameObject.Find("OVRCameraRig").gameObject.GetComponent<OVRCameraRig>();
        //m_localTrackingSpace = rig.transform.Find("TrackingSpace").gameObject;
        m_localPlayerHead = m_cameraRig.transform.Find("TrackingSpace/CenterEyeAnchor").gameObject;
        // Grab hands
        m_leftHand = GameObject.Find("OVRCameraRig/TrackingSpace/LeftHandAnchor/LeftControllerAnchor/OVRHandPrefab").GetComponent<OVRHand>();
        m_rightHand = GameObject.Find("OVRCameraRig/TrackingSpace/RightHandAnchor/RightControllerAnchor/OVRHandPrefab").GetComponent<OVRHand>();
        //m_leftAnchor = GameObject.Find("OVRCameraRig/TrackingSpace/LeftHandAnchor");
        //m_rightAnchor = GameObject.Find("OVRCameraRig/TrackingSpace/RightHandAnchor");

        if ((m_leftHand == null) || (m_rightHand == null))
        {
            Debug.LogError($"GameMan Init Cannot find hands !");
        }

        LoadElementsMats();
        LoadElementsCubes();

        m_wave = new WaveClass();
        m_playerNb = 0;

        m_myPillar = GameObject.Find("PillarA");
    }


    // Start is called before the first frame update
    void Start()
    {
        m_netMan.networkAddress = "localhost"; // overwrite public field when not at Henigma...
        Debug.Log($"GameMan Init @ {Time.fixedTime}s");
    }


    // Load Elements materials
    void LoadElementsMats()
    {
        /*
        m_ElementsMats[0] = Resources.Load("Elem_1", typeof(Material)) as Material;
        if (m_ElementsMats[0] == null)
        {
            Debug.LogError("Could not load materials, place it in Resources Folder!");
        }

        m_ElementsMats[1] = Resources.Load("Elem_2", typeof(Material)) as Material;
        if (m_ElementsMats[1] == null)
        {
            Debug.LogError("Could not load materials, place it in Resources Folder!");
        }

        m_ElementsMats[2] = Resources.Load("Elem_3", typeof(Material)) as Material;
        if (m_ElementsMats[2] == null)
        {
            Debug.LogError("Could not load materials, place it in Resources Folder!");
        }

        m_ElementsMats[3] = Resources.Load("Elem_4", typeof(Material)) as Material;
        if (m_ElementsMats[3] == null)
        {
            Debug.LogError("Could not load materials, place it in Resources Folder!");
        }
        */

        m_CubesElemMats[0] = Resources.Load("Elem_exit_1", typeof(Material)) as Material;
        if (m_CubesElemMats[0] == null)
        {
            Debug.LogError("Could not load materials, place it in Resources Folder!");
        }
        m_CubesElemMats[1] = Resources.Load("Elem_exit_2", typeof(Material)) as Material;
        if (m_CubesElemMats[1] == null)
        {
            Debug.LogError("Could not load materials, place it in Resources Folder!");
        }
        m_CubesElemMats[2] = Resources.Load("Elem_exit_3", typeof(Material)) as Material;
        if (m_CubesElemMats[2] == null)
        {
            Debug.LogError("Could not load materials, place it in Resources Folder!");
        }
        m_CubesElemMats[3] = Resources.Load("Elem_exit_4", typeof(Material)) as Material;
        if (m_CubesElemMats[3] == null)
        {
            Debug.LogError("Could not load materials, place it in Resources Folder!");
        }
    }


    void LoadElementsCubes()
    {
        for (int i = 0; i < m_startElementCount; ++i)
        {
            GameObject obj = GameObject.Instantiate(m_elementCubePrefab);
            obj.name = $"Elem_{i}";
            //Vector3 pos = new Vector3(1.5f, 1.0f + ((float)i * 0.2f), 1.0f);
            Vector3 eyePos = m_cameraRig.transform.position;
            //Vector3 eyePos = m_cameraRig.centerEyeAnchor.position;//null
            //Vector3 eyePos = m_cameraRig.trackerAnchor.position;//null
            //Vector3 pos = eyePos + new Vector3(-0.5f, -0.1f + ((float)i * 0.2f), 0.5f);
            Vector3 pos = eyePos + new Vector3(-0.5f, 1.0f + ((float)i * 0.2f), 0.2f);
            obj.transform.SetPositionAndRotation(pos, Quaternion.identity);

            ElementsScript elem = obj.GetComponent<ElementsScript>();
            if (elem)
            {
                int matId = i % 4;
                elem.ChangeType((Elements)matId, m_CubesElemMats[matId]);
            }

            m_elemCubes.Add(elem);

            /*
            ColorGrabbable cg = obj.GetComponent<ColorGrabbable>();
            if (cg)
            {
                //cg.Highlight = true;
                cg.UpdateColor();
            }
            //*/
        }
    }


    public void InitNewWave()
    {
        m_waveNb++;
        m_nextWaveTime = 0.0f;
        m_wave.InitWave(m_waveNb);

        m_playerLifeBar.m_maximum = 1000;
        m_playerLifeBar.m_cur = 1000.0f;
    }


    public void RegisterNewPlayer(GameObject newPlayer, bool hasAuthority)
    {
        PlayerControlMirror player = newPlayer.GetComponent<PlayerControlMirror>();
        if (hasAuthority)
        {
            newPlayer.name = "MyAvatar"; // There is only one client which has the authority
            m_avatar = newPlayer;
            m_myAvatar = player;
        }

        m_playerNb++;
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


    public void MobIsDead(Mobs deadMob)
    {
        // A new mob is dead
        m_wave.MobIsDead(deadMob);

        if (m_wave.m_deathNb == m_wave.m_mobs.Count)
        {
            Debug.Log($"{Time.fixedTime}s, All mobs of the wave are dead. GG ! ({m_wave.m_deathNb})");
            m_wave.FinishWave();
            m_nextWaveTime = Time.fixedTime + 5.0f;
        }
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

        //---
        /*
        if (m_myAvatar != null)
        {
            Debug.DrawRay(m_myAvatar.transform.position, m_myAvatar.transform.forward, Color.red);
        }
        */

        // Early out if no network
        if (!m_netMan.isNetworkActive)
            return;

        // Next is only for the server
        if (m_myAvatar == null)
            return;

        if (m_myAvatar.isServer == false)
            return;

        // Launch Phase 1
        if (m_nextWaveTime > 0.0f)
        {
            if (Time.fixedTime > m_nextWaveTime)
            {
                InitNewWave();
                /*
                if (m_MobA)
                {
                    m_nextWaveTime = 0.0f;
                    Vector3 spawnPoint = new Vector3(1.0f, 1.5f, 1.7f);
                    m_MobA.transform.position = spawnPoint;
                    m_MobA.SetActive(true);
                    m_waveNb = 1;
                }
                else
                {
                    m_MobA = GameObject.Find("MobA");
                }
                */
            }
        }
        else
        {
            if (m_waveNb == 0)
            {
                m_nextWaveTime = Time.fixedTime + 5.0f;
            }
        }

        float hitAmount = 0.0f;
        // Handle mobs mouvements
        if (m_wave.m_mobs.Count > 0)
        {
            Vector3 forward = new Vector3(0.0f, 0.0f, -1.0f) * Time.fixedDeltaTime;
            foreach (Mobs mob in m_wave.m_mobs)
            {
                if (mob.isActiveAndEnabled == false)
                    continue;

                if (mob.transform.position.z > 1.5f)
                {
                    mob.transform.position += forward * mob.m_curSpeedFactor;
                }

                float t = 1.0f - Mathf.InverseLerp(0.0f, 5.0f, mob.transform.position.z);
                hitAmount += t * 100.0f;
            }
        }

        if (hitAmount > 0.0f)
        {
            float life = m_playerLifeBar.m_cur - hitAmount * Time.fixedDeltaTime;
            m_playerLifeBar.m_cur = life;
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

        // Elevate pillar
        if (m_waveNb > 0)
        {
            if (m_nextWaveTime > 0.0f)
            {
                if (Time.time < m_nextWaveTime)
                {
                    Vector3 pos = m_myPillar.transform.position;
                    pos.y += 0.1f * Time.deltaTime;
                    m_myPillar.transform.position = pos;

                    pos = m_cameraRig.transform.position;
                    pos.y += 0.1f * Time.deltaTime;
                    m_cameraRig.transform.position = pos;
                }
            }
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
                //Quaternion q = Quaternion.LookRotation(player.transform.forward);
                Quaternion q = player.transform.rotation;
                Vector3 p = player.transform.position + player.transform.forward * 0.2f; // Spawn in front to avoid collisions
                player.SpawnMyTool(p, q);

                //AudioSource.PlayClipAtPoint(m_audioSounds[0], m_cameraRig.trackingSpace.position);
            }
            //*/
        }

        // Activate an element
        if (Input.GetKeyUp(KeyCode.L))
        {
            if (m_elemCubes.Count > 0)
            {
                m_elemCubes[0].GetColorGrabbable().m_lastGrabbed = Time.time;
            }
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
            //float pinchStrength = m_leftHand.GetFingerPinchStrength(OVRHand.HandFinger.Middle);
            //m_logTitle.text = $"pinchStrength = {pinchStrength}";
            PlayerControlMirror player = m_avatar.GetComponent<PlayerControlMirror>();
            Transform trs = player.transform;
            if (m_isUsingHands)
            {
                trs = m_leftHand.transform;
            }

            if (Time.time > m_leftLastBulletTime + m_leftRPS)
            {
                m_leftLastBulletTime = Time.time;
                Quaternion q = Quaternion.LookRotation(m_leftHand.transform.right);
                player.SpawnMyTool(trs.position, q);
            }
        }
        if (m_rightHand.GetFingerIsPinching(OVRHand.HandFinger.Middle))
        {
            //float pinchStrength = m_rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Middle);
            //m_logTitle.text = $"pinchStrength = {pinchStrength}";
            PlayerControlMirror player = m_avatar.GetComponent<PlayerControlMirror>();
            if (m_isUsingHands == false)
            {
                m_isUsingHands = true;
            }
            Transform trs = m_rightHand.transform;

            if (Time.time > m_rightLastBulletTime + m_rightRPS)
            {
                m_rightLastBulletTime = Time.time;
                Quaternion q = Quaternion.LookRotation(-m_rightHand.transform.right);
                player.SpawnMyTool(trs.position, q);
            }
        }

        // Update head
#if UNITY_EDITOR
#else
        //---
        /*
        if (m_isUsingHands)
        {
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
        }
        */
#endif
    }


    // elem just has been triggered
    public void TriggerElement(ElementsScript elem)
    {
        AudioSource.PlayClipAtPoint(m_audioSounds[1], elem.transform.position);
        m_rightRPS = 0.2f;
        m_leftRPS = 0.2f;

        StartCoroutine(RestoreRPS(8.0f, true, true, 1.0f));
    }


    // Set right or left or both RPS to newVal in waitSec
    public IEnumerator RestoreRPS(float waitSec, bool isRight, bool isLeft, float newVal)
    {
        yield return new WaitForSeconds(waitSec);

        if (isRight)
        {
            m_rightRPS = newVal;
        }
        if (isLeft)
        {
            m_leftRPS = newVal;
        }
    }
}
