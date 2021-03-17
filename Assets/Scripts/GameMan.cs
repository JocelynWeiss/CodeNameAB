using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
using OculusSampleFramework;


// A game manager dedicated to test mirror networking services...
//JowNext: Add a game over if no more life
// Sync end of wave to elevate client pillars, use RPC in PlayerControlMirror



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
    public static JowLogger s_log;

    public TextMeshProUGUI m_logTitle;

    public NetworkManager m_netMan;
    public int m_minPlayerNb = 2; // Minimum player number required to launch first wave
    public float m_firstWaveDelay = 10.0f; // seconds before launching first wave

    public List<PlayerControlMirror> m_allPlayers = new List<PlayerControlMirror>();
    private float m_lastConnected = 0.0f;
    private PlayerControlMirror m_myAvatar;
    List<GameObject> m_pillarsPool = new List<GameObject>();
    public float m_upForce = 300.0f;
    public float m_rotForce = 10.0f;
    public float m_YForce = 5.0f;
    private float m_curRotForce = 0.0f;
    private float m_curYForce = 0.0f;
    private bool m_tryingConnectAsClient = false;
    public int m_waveNb = 0;
    public double m_nextWaveDate = 0.0; // Date as an AODate
    public WaveClass m_wave; // The current mobs wave
    public JowProgressBar m_playerLifeBar;
    public TextMeshProUGUI m_playerInfoText;

    private bool m_isUsingHands = false;
    private GameObject m_localPlayerHead;
    OVRHand m_leftHand;
    OVRHand m_rightHand;
    private float m_rightLastBulletTime = 0.0f;
    private float m_leftLastBulletTime = 0.0f;
    private float m_rightRPS = 1.0f; // Round per seconds (seconds between 2 shots)
    private float m_leftRPS = 1.0f; // Round per seconds (seconds between 2 shots)
    private bool m_doubleShot = false;
    public OVRCameraRig m_cameraRig;

    public Material[] m_CubesElemMats = new Material[4];
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
        // make sure only one instance of this manager ever exists
        if (s_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        s_instance = this;
        DontDestroyOnLoad(gameObject);
        Random.InitState(1966);

        if (s_log == null)
        {
            s_log = new JowLogger();
            if (UnityEngine.Application.platform != RuntimePlatform.Android)
                JowLogger.m_addTimeToName = true; // only to avoid collision with multiple instances
        }

        JowLogger.Log($"GameMan Awake @ {Time.fixedTime}s Version {Application.version}");

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

        m_wave = new WaveClass();
    }


    // Start is called before the first frame update
    void Start()
    {
        m_netMan.networkAddress = "localhost"; // overwrite public field when not at Henigma...
        JowLogger.Log($"GameMan Init @ {Time.fixedTime}");
        JowLogger.m_logTime = true;
    }


    // Terminate the game
    private void OnDestroy()
    {
        s_log.Close();
    }


    public PlayerControlMirror GetLocalPlayer()
    {
        return m_myAvatar;
    }


    public GameObject GetClosestPillar(Vector3 _pos)
    {
        GameObject ret = null;
        float dist = float.MaxValue;

        foreach (GameObject go in m_pillarsPool)
        {
            float d = (_pos - go.transform.position).magnitude;
            if (d < dist)
            {
                dist = d;
                ret = go;
            }
        }

        return ret;
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


    // Instantiate a new element (DEPRECATED)
    void AddNewElement(PillarMirror _pillar)
    {
        /*
        int count = m_elemCubes.Count;
        GameObject obj = GameObject.Instantiate(m_elementCubePrefab);
        obj.name = $"Elem_{count}";
        Vector3 pos = _pillar.transform.position + new Vector3(-0.5f, 1.8f + ((float)count * 0.2f), 0.2f);
        obj.transform.SetPositionAndRotation(pos, Quaternion.identity);

        ElementsScript elem = obj.GetComponent<ElementsScript>();
        if (elem)
        {
            //int matId = count % 4;
            int matId = Random.Range(0, 4);
            elem.ChangeType((Elements)matId, m_CubesElemMats[matId]);
        }

        m_elemCubes.Add(elem);
        */

        /*
        ColorGrabbable cg = obj.GetComponent<ColorGrabbable>();
        if (cg)
        {
            //cg.Highlight = true;
            cg.UpdateColor();
        }
        //*/
    }


    void LoadElementsCubes(PillarMirror pillar)
    {
        for (int i = 0; i < m_startElementCount; ++i)
        {
            AddNewElement(pillar);
        }
    }


    // Initialise the next wave to come (Server side only)
    public void InitNewWave()
    {
        m_waveNb++;
        foreach (PlayerControlMirror plr in m_allPlayers)
        {
            plr.RpcWaveNb(m_waveNb);
            plr.RpcNextWaveTime(0.0);
        }
        //m_myAvatar.RpcWaveNb(m_waveNb);
        //m_myAvatar.CmdEndOfWave(m_waveNb);
        //m_myAvatar.RpcNextWaveTime(0.0f);
        m_wave.InitWave(m_waveNb);
        //m_logTitle.text = $"Mobs {m_wave.m_mobs.Count}";

        // Add a new element for each wave
        if ((m_waveNb > 1) && (m_elemCubes.Count < 4))
        {
            AddNewElement(m_myAvatar.m_myPillar);
        }

        // Show and reposition elements
        int idx = 0;
        foreach (ElementsScript elem in m_elemCubes)
        {
            Vector3 pos = m_myAvatar.m_myPillar.transform.position + new Vector3(-0.5f, 2.0f + ((float)idx * 0.2f), 0.2f);
            elem.transform.SetPositionAndRotation(pos, Quaternion.identity);

            elem.gameObject.SetActive(true);
            idx++;
        }

        SetPlayerInfoText();
    }


    public void SetPlayerInfoText()
    {
        m_playerInfoText.text = $"Wave {m_waveNb}\nAmmo {m_myAvatar.m_ammoCount} MobNb {m_wave.m_mobs.Count}";
    }


    // Executed on any client even the host
    public void RegisterNewPlayer(GameObject newPlayer, bool hasAuthority)
    {
        // Process some init here as we are sure the server is runing then
        if (m_allPlayers.Count == 0)
        {
            // Add few pillars for players seats (4?)
            GameObject pillarGo = GameObject.Find("PillarA");
            m_pillarsPool.Add(pillarGo);
            pillarGo = GameObject.Find("PillarB");
            m_pillarsPool.Add(pillarGo);
        }

        PlayerControlMirror player = newPlayer.GetComponent<PlayerControlMirror>();
        // Assign Pillars
        PillarMirror pil = m_pillarsPool[m_allPlayers.Count].GetComponent<PillarMirror>();
        player.m_myPillar = pil;

        if (hasAuthority)
        {
            newPlayer.name = "MyAvatar" + player.netId; // There is only one client which has the authority (localClient)
            m_myAvatar = player;

            // Set player pos and rot from pillars
            Vector3 pos = pil.transform.position + new Vector3(0.0f, 1.8f, 0.0f);
            Quaternion rot = pil.transform.rotation;
            m_myAvatar.transform.SetPositionAndRotation(pos, rot);
            m_cameraRig.transform.SetPositionAndRotation(pos, rot);

            LoadElementsCubes(m_myAvatar.m_myPillar);
        }

        m_allPlayers.Add(player);
        JowLogger.Log($"---+++ Registering new player @ {Time.fixedTime}s, {newPlayer}, netId {player.netId}, playerCount {m_allPlayers.Count}");
        JowLogger.Log($"on pillar {player.m_myPillar.name}");

        // JowNext: Add elements for each player


        if (m_netMan.mode == NetworkManagerMode.Host)
        {
            player.RpcWaveNb(m_waveNb); // Send current wave nb
        }
    }


    public void DisconnectPlayer(PlayerControlMirror player)
    {
        m_allPlayers.Remove(player);
        JowLogger.Log($"DisconnectPlayer <-- netId {player.netId} @ {System.DateTime.Now} hasAuthority {player.hasAuthority} playerCount {m_allPlayers.Count}");
    }


    public void RegisterNewTool2(Tools2Mirror tool, bool hasAuthority)
    {
        if (hasAuthority)
        {
            m_myAvatar.m_tool = tool;
        }

        //JowLogger.Log($"+++ Registering new tool2 @ {Time.fixedTime}s, {tool}, netId {tool.netId}, authority= {hasAuthority}, isLocalPlayer {player.isLocalPlayer}");

        JowLogger.Log($"+++ Registering new tool2 @ {Time.fixedTime}s, {tool}, netId {tool.netId}, authority= {hasAuthority}");
    }


    // Return this player pillar
    public GameObject GetPillar()
    {
        return m_myAvatar.m_myPillar.gameObject;
    }


    // Server only
    public void MobIsDead(Mobs deadMob)
    {
        // A new mob is dead
        m_wave.MobIsDead(deadMob);

        if (m_wave.m_deathNb == m_wave.m_mobs.Count)
        {
            WaveEnded();
        }

        // No more patient for medics
        ReleasePatients(deadMob);
    }


    // Set patient pointer to null
    public void ReleasePatients(Mobs deadMob)
    {
        foreach (Mobs mob in m_wave.m_mobs)
        {
            if (mob.GetCurPatient() == deadMob)
            {
                mob.SetPatient(null);
            }
        }
    }


    // End of a wave (no mobs left)
    void WaveEnded()
    {
        System.DateTime toto = System.DateTime.Now.AddSeconds(5.0);
        double date = toto.ToOADate();
        JowLogger.Log($"{Time.fixedTime}s, All mobs of the wave {m_waveNb} are dead. GG ! ({m_wave.m_deathNb}) ------------------ {toto} = {date}");
        m_wave.FinishWave();
        m_myAvatar.RpcNextWaveTime(date);

        // Hide remaining elements
        foreach(ElementsScript elem in m_elemCubes)
        {
            elem.gameObject.SetActive(false);
        }
    }


    private void FixedUpdate()
    {
        if (m_curRotForce != 0.0f)
        {
            m_myAvatar.transform.RotateAround(m_myAvatar.transform.position, Vector3.up, m_curRotForce * Time.fixedDeltaTime);
        }

        if (m_curYForce != 0.0f)
        {
            Vector3 pos = m_myAvatar.transform.position;
            pos += Vector3.up * m_curYForce * Time.fixedDeltaTime;
            m_myAvatar.transform.SetPositionAndRotation(pos, m_myAvatar.transform.rotation);
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

        if (m_nextWaveDate > System.DateTime.Now.ToOADate())
        {
            double d = m_nextWaveDate - System.DateTime.Now.ToOADate();
            d *= 100000.0;
            m_playerInfoText.text = $"Next Wave in {d.ToString("f0")}s";
        }

#if UNITY_ANDROID
        // Update local player head position from headset
        if (m_myAvatar.isLocalPlayer)
        {
            m_myAvatar.transform.SetPositionAndRotation(m_localPlayerHead.transform.position, m_localPlayerHead.transform.rotation);
        }
#endif

        if (m_myAvatar.isServer == false)
            return;

        // Launch Next Phase
        if (m_nextWaveDate > 0.0)
        {
            //string t = $"Next Wave in {(m_nextWaveTime - Time.fixedTime).ToString("f1")}s";
            //m_playerInfoText.text = t;

            if (System.DateTime.Now.ToOADate() >= m_nextWaveDate)
            {
                InitNewWave();
            }
        }
        else
        {
            // Launch Phase 1 if enough player
            if ((m_waveNb == 0) && (m_allPlayers.Count >= m_minPlayerNb))
            {
                m_nextWaveDate = System.DateTime.Now.AddSeconds(m_firstWaveDelay).ToOADate();
            }
        }

        // Reset all damage taken last frame
        foreach (PlayerControlMirror plr in m_allPlayers)
        {
            plr.m_curHitAmount = 0.0f;
        }
        // Handle mobs mouvements
        if (m_wave.m_mobs.Count > 0)
        {
            foreach (Mobs mob in m_wave.m_mobs)
            {
                if (mob.isActiveAndEnabled == false)
                    continue;

                if (mob.m_mobType == MobsType.MobMedic) // Self handed
                    continue;

                Vector3 forward = mob.transform.forward * Time.fixedDeltaTime;
                float dist = (mob.transform.position - m_myAvatar.m_myPillar.transform.position).magnitude;

                if (dist > 4.0f)
                {
                    mob.transform.position += forward * mob.m_curSpeedFactor;
                }

                //float t = 1.0f - Mathf.InverseLerp(0.0f, 5.0f, mob.transform.position.z);
                //hitAmount += t * 100.0f;

                // Compute hit amount for each player
                foreach (PlayerControlMirror plr in m_allPlayers)
                {
                    float d = (mob.transform.position - plr.m_myPillar.transform.position).magnitude;
                    float h = 1.0f - Mathf.InverseLerp(0.0f, 5.0f, d);
                    plr.m_curHitAmount += h * 100.0f;
                }
            }
        }

        // Apply current frame cumulated damages
        foreach (PlayerControlMirror plr in m_allPlayers)
        {
            if (plr.m_curHitAmount > 0.0f)
            {
                plr.m_curLife = plr.m_curLife - plr.m_curHitAmount * Time.fixedDeltaTime;
            }
        }

        //TestRotatingElem();
    }


    // Check if we can fire
    public bool CanFire()
    {
        if (m_waveNb == 0) // Cannot fire before 1st wave
            return false;
        if (m_nextWaveDate > System.DateTime.Now.ToOADate()) // Cannot fire between waves
            return false;
        if (m_myAvatar.m_ammoCount <= 0)
            return false;
        return true;
    }


    // Try to fire from right or left hand
    public bool TryFire(bool isRight, Vector3 pos, Quaternion ori, Vector3 right)
    {
        if (isRight)
        {
            if (Time.time > m_rightLastBulletTime + m_rightRPS)
            {
                m_rightLastBulletTime = Time.time;

                if (m_doubleShot)
                {
                    Vector3 p = pos + right * 0.1f;
                    m_myAvatar.SpawnMyTool(p, ori);
                    p = pos - right * 0.1f;
                    m_myAvatar.SpawnMyTool(p, ori);
                }
                else
                {
                    m_myAvatar.SpawnMyTool(pos, ori);
                }

                SetPlayerInfoText();
                return true;
            }
        }
        else
        {
            if (Time.time > m_leftLastBulletTime + m_leftRPS)
            {
                m_leftLastBulletTime = Time.time;

                if (m_doubleShot)
                {
                    Vector3 p = pos + right * 0.1f;
                    m_myAvatar.SpawnMyTool(p, ori);
                    p = pos - right * 0.1f;
                    m_myAvatar.SpawnMyTool(p, ori);
                }
                else
                {
                    m_myAvatar.SpawnMyTool(pos, ori);
                }

                SetPlayerInfoText();
                return true;
            }
        }

        return false;
    }


    void TestRotatingElem()
    {
        if (m_elemCubes.Count == 0)
            return;

        if (m_wave.m_mobs.Count == 0)
            return;

        ElementsScript elem = m_elemCubes[0];
        Mobs mob = m_wave.m_mobs[0];
        Vector3 mobPos = mob.transform.position;
        Vector3 dec = new Vector3(0.0f, 1.0f, 0.0f);
        Vector3 pos = mobPos + dec;
        //float angle = Mathf.Sin(Time.fixedTime) * Mathf.PI;
        float angle = Time.fixedTime;
        //pos += new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * 1.0f;
        pos += new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), Mathf.Sin(angle)) * 1.0f;
        elem.transform.position = pos;

        // Second one
        float phase = 100.0f;
        angle += phase;
        pos = mobPos + new Vector3(Mathf.Cos(angle), Mathf.Cos(angle) * Mathf.Sin(angle), Mathf.Sin(angle)) * 1.0f;
        m_elemCubes[1].transform.position = pos;
    }


    // Update is called once per frame
    void Update()
    {
        //m_logTitle.text = $"{Time.fixedTime}s\n{m_netMan.networkAddress}\n{m_lastConnected}s"; // Put back

        if (m_myAvatar)
        {
            string mobLife = "-";
            if (m_wave.m_mobs.Count > 0)
                mobLife = m_wave.m_mobs[0].m_life.ToString("f2");

            //m_logTitle.text = $"{m_avatar.transform.position}\n{mobLife}"; // Put back
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
            if (m_nextWaveDate > 0.0)
            {
                if (System.DateTime.Now.ToOADate() < m_nextWaveDate)
                {
                    if (m_netMan.mode == NetworkManagerMode.Host)
                    {
                        foreach (GameObject go in m_pillarsPool)
                        {
                            Vector3 pos = go.transform.position;
                            pos.y += 0.1f * Time.deltaTime;
                            go.transform.position = pos;
                        }

                        foreach (PlayerControlMirror plr in m_allPlayers)
                        {
                            foreach (ElementsNet elem in plr.m_myElems)
                            {
                                Vector3 pos = elem.transform.position;
                                pos.y += 0.1f * Time.deltaTime;
                                elem.transform.position = pos;
                            }
                        }
                    }

                    /*
                    pos = m_cameraRig.transform.position;
                    pos.y += 0.1f * Time.deltaTime;
                    m_cameraRig.transform.position = pos;
                    */
                    m_cameraRig.transform.position = m_myAvatar.m_myPillar.transform.position + new Vector3(0.0f, 1.8f, 0.0f);
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

            if (m_myAvatar != null)
            {
                m_myAvatar.Jump();
            }
        }

        if (Input.GetKeyUp(KeyCode.C))
        {
            if (m_myAvatar != null)
            {
                m_myAvatar.ChangeAvatarColour();
            }
        }

        // Place player at pos
        if (Input.GetKeyUp(KeyCode.P))
        {
            if (m_myAvatar != null)
            {
                //player.PositionPlayer(new Vector3(0.1f, 1.0f, 1.8f));
                m_myAvatar.transform.SetPositionAndRotation(new Vector3(0.1f, 1.0f, 1.8f), Quaternion.identity);
            }
            else
            {
                Application.Quit();
            }
        }

        // Spawn unsync entity
        if (Input.GetKeyUp(KeyCode.R))
        {
            if (m_myAvatar != null)
            {
                if (CanFire() == true)
                {
                    Vector3 p = m_myAvatar.transform.position + m_myAvatar.transform.forward * 0.2f; // Spawn in front to avoid collisions
                    Quaternion q = m_myAvatar.transform.rotation;
                    TryFire(true, p, q, m_myAvatar.transform.right);
                }
            }
        }

        // Activate first unused element
        if (Input.GetKeyUp(KeyCode.L))
        {
            foreach (ElementsNet elem in m_myAvatar.m_myElems)
            {
                if (elem.m_used == false)
                {
                    elem.GetColorGrabbable().m_lastGrabbed = Time.time;
                    elem.transform.position = m_cameraRig.transform.position + Vector3.up * 2.0f; // Put it in the activation range
                    break;
                }
            }
        }

        // Inverse view for testing
        if (Input.GetKeyUp(KeyCode.V))
        {
            Quaternion q = Quaternion.AngleAxis(180.0f, Vector3.up);
            m_cameraRig.transform.rotation = m_cameraRig.transform.rotation * q;
        }

        // Spawn sync entity
        if (Input.GetKeyUp(KeyCode.T))
        {
            //*
            if (m_myAvatar != null)
            {
                // Check if already there
                if (m_myAvatar.m_tool == null)
                {
                    GameObject go = GameObject.Find("AvatarTool2");
                    Tools2Mirror tool = null;
                    if (go != null)
                    {
                        tool = go.GetComponent<Tools2Mirror>();
                    }
                    if (tool != null)
                    {
                        m_myAvatar.m_tool = tool;
                    }
                    else
                    {
                        m_myAvatar.SpawnMyTool2();

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
            if (m_myAvatar != null)
            {
                m_myAvatar.m_tool.transform.RotateAround(m_myAvatar.m_tool.transform.position, Vector3.up, 100.0f * Time.deltaTime);
            }
        }
        if (Input.GetKey(KeyCode.Keypad4))
        {
            if (m_myAvatar != null)
            {
                m_myAvatar.m_tool.transform.RotateAround(m_myAvatar.m_tool.transform.position, Vector3.up, -100.0f * Time.deltaTime);
            }
        }
        if (Input.GetKey(KeyCode.Keypad9))
        {
            if (m_myAvatar != null)
            {
                Vector3 pos = m_myAvatar.m_tool.transform.position;
                pos += Vector3.up * 1.0f * Time.deltaTime;
                Quaternion q = m_myAvatar.m_tool.transform.rotation;
                m_myAvatar.m_tool.transform.SetPositionAndRotation(pos, q);
            }
        }
        if (Input.GetKey(KeyCode.Keypad7))
        {
            if (m_myAvatar != null)
            {
                Vector3 pos = m_myAvatar.m_tool.transform.position;
                pos -= Vector3.up * 1.0f * Time.deltaTime;
                Quaternion q = m_myAvatar.m_tool.transform.rotation;
                m_myAvatar.m_tool.transform.SetPositionAndRotation(pos, q);
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
            if (m_myAvatar != null)
            {
                Vector3 pos = m_myAvatar.m_tool.transform.position;
                pos += m_myAvatar.m_tool.transform.up * -0.8f * Time.deltaTime;
                Quaternion q = m_myAvatar.m_tool.transform.rotation;
                m_myAvatar.m_tool.transform.SetPositionAndRotation(pos, q);
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
            Transform trs = m_myAvatar.transform;
            if (m_isUsingHands)
            {
                trs = m_leftHand.transform;
            }

            if (CanFire() == true)
            {
                Quaternion q = Quaternion.LookRotation(m_leftHand.transform.right);
                TryFire(false, trs.position, q, m_leftHand.transform.forward);
            }
        }
        if (m_rightHand.GetFingerIsPinching(OVRHand.HandFinger.Middle))
        {
            //float pinchStrength = m_rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Middle);
            //m_logTitle.text = $"pinchStrength = {pinchStrength}";
            if (m_isUsingHands == false)
            {
                m_isUsingHands = true;
            }
            Transform trs = m_rightHand.transform;

            if (CanFire() == true)
            {
                Quaternion q = Quaternion.LookRotation(-m_rightHand.transform.right);
                TryFire(true, trs.position, q, m_rightHand.transform.forward);
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
    public void TriggerElement(ElementsNet elem)
    {
        JowLogger.Log($"Triggering element {elem.m_elemType}");
        AudioSource.PlayClipAtPoint(m_audioSounds[1], elem.transform.position);

        switch (elem.m_elemType)
        {
            case Elements.Earth:
                {
                    m_rightRPS = 0.2f;
                    m_leftRPS = 0.2f;
                    StartCoroutine(RestoreRPS(8.0f, true, true, 1.0f));
                    break;
                }
            case Elements.Water:
                {
                    m_doubleShot = true;
                    StartCoroutine(SetDoubleShotD(6.0f, false));
                    break;
                }
            case Elements.Fire:
                {
                    m_rightRPS = 0.2f;
                    m_leftRPS = 0.2f;
                    StartCoroutine(RestoreRPS(8.0f, true, true, 1.0f));
                    break;
                }
            case Elements.Air:
                {
                    m_rightRPS = 0.2f;
                    m_leftRPS = 0.2f;
                    StartCoroutine(RestoreRPS(8.0f, true, true, 1.0f));
                    break;
                }
        }

        StartCoroutine(DestroyElementDelayed(5.0f, elem));

        foreach (PlayerControlMirror plr in m_allPlayers)
        {
            if (plr.m_myElems.Contains(elem))
            {
                plr.m_myElems.Remove(elem);
            }
        }
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


    // Set double shot state after waitSec seconds
    public IEnumerator SetDoubleShotD(float waitSec, bool state)
    {
        yield return new WaitForSeconds(waitSec);

        m_doubleShot = state;
    }


    // Set right or left or both RPS to newVal in waitSec
    public IEnumerator DestroyElementDelayed(float waitSec, ElementsNet elem)
    {
        yield return new WaitForSeconds(waitSec);

        //m_elemCubes.Remove(elem); // JowNext: make sure it's removed from players list...
        GameObject.Destroy(elem);
    }
}
