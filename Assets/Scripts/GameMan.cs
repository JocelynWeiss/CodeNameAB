using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
using OculusSampleFramework;


// The game manager using mirror networking services...
//JowNext: Add a game over if no more life
// Hold a list of players affected by walls in wreckingball class to compute damages on the server.
// Trigger Bonuses



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
    public float m_endOfWaveDelay = 8.0f; // seconds the game is still running after all mobs death.

    public List<PlayerControlMirror> m_allPlayers = new List<PlayerControlMirror>();
    private float m_lastConnected = 0.0f;
    private PlayerControlMirror m_myAvatar;
    public float m_avatarHeight = 1.0f; // height above the pillar
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
    private HandsMirror m_rightHandMirror;
    private HandsMirror m_leftHandMirror;
    private float m_rightLastBulletTime = 0.0f;
    private float m_leftLastBulletTime = 0.0f;
    private float m_rightRPS = 1.0f; // Round per seconds (seconds between 2 shots)
    private float m_leftRPS = 1.0f; // Round per seconds (seconds between 2 shots)
    private bool m_doubleShot = false;
    public OVRCameraRig m_cameraRig;
    [HideInInspector] public OVRScreenFade m_fader;
    [HideInInspector] public CamShakeScript m_shaker;

    public Material[] m_CubesElemMats = new Material[4];
    public int m_maxElementCount = 4; // Max element per player
    public GameObject m_elementCubePrefab;
    //List<ElementsScript> m_elemCubes = new List<ElementsScript>(); // Deprecated

    List<BonusNet> m_allBonuses = new List<BonusNet>();

    public Material m_MedicMat;

    List<WreckingBallMirror> m_wreckings = new List<WreckingBallMirror>(); // Server Side only

    // Audio...
    //[InspectorNote("Sound Setup", "Press '1' to play testSound1 and '2' to play testSound2")]
    //public OVR.SoundFXRef testSound1;
    //public OVR.SoundFXRef testSound2;
    [InspectorNote("Audio Sounds Setup")]
    public List<AudioClip> m_audioSounds;
    bool[] m_countDownSound = new bool[5];


    public List<GameObject> GetPillarPool()
    {
        return m_pillarsPool;
    }


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
            //*
            if (UnityEngine.Application.platform != RuntimePlatform.Android)
                JowLogger.m_addTimeToName = true; // only to avoid collision with multiple instances
            //*/
        }

        JowLogger.Log($"GameMan Awake @ {Time.fixedTime}s Version {Application.version}");

        Canvas myCanvas = GameObject.Find("Canvas").gameObject.GetComponent<Canvas>();
        if (myCanvas != null)
        {
            GameObject intro = myCanvas.transform.GetChild(0).gameObject;
            m_logTitle = intro.GetComponent<TextMeshProUGUI>();
            m_logTitle.text = "entry_" + Application.version;
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

        m_fader = m_cameraRig.GetComponentInChildren<OVRScreenFade>();
        if (m_fader == null)
        {
            Debug.LogError($"Make sure OVRScreenFade is attached to cameraRig->TrackingSpace->CenterEyeAnchor");
        }

        m_shaker = GetComponent<CamShakeScript>();

        LoadElementsMats();
        LoadMedicMat();

        m_wave = new WaveClass();

        ResetCountDownSounds();
    }


    void ResetCountDownSounds()
    {
        for (int i = 0; i < m_countDownSound.Length; i++)
        {
            m_countDownSound[i] = false;
        }
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


    public PlayerControlMirror GetPlayerPerId(uint _netId)
    {
        PlayerControlMirror ret = null;
        foreach (PlayerControlMirror plr in m_allPlayers)
        {
            if (plr.netId == _netId)
            {
                ret = plr;
                break;
            }
        }

        return ret;
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


    public PlayerControlMirror GetClosestPlayer(Vector3 _pos)
    {
        PlayerControlMirror ret = null;
        float dist = float.MaxValue;

        foreach (PlayerControlMirror plr in m_allPlayers)
        {
            float d = (_pos - plr.transform.position).magnitude;
            if (d < dist)
            {
                dist = d;
                ret = plr;
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


    // Load Medic mob mat
    void LoadMedicMat()
    {
        m_MedicMat = Resources.Load("MedicMat", typeof(Material)) as Material;
        if (m_MedicMat == null)
        {
            Debug.LogError("Could not load Medic material, place it in Resources Folder!");
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


    // Initialise the next wave to come (Server side only)
    public void InitNewWave()
    {
        m_waveNb++;
        //m_waveNb = 5; // Force to test
        foreach (PlayerControlMirror plr in m_allPlayers)
        {
            plr.RpcWaveNb(m_waveNb);
            plr.RpcNextWaveTime(0.0);
        }
        m_wave.InitWave(m_waveNb);
        m_nextWaveDate = 0.0;
        //m_logTitle.text = $"Mobs {m_wave.m_mobs.Count}";

        // Test WreckingBall
        if (m_waveNb >= 5)
        {
            //*
            foreach (PlayerControlMirror plr in m_allPlayers)
            {
                StartCoroutine(SpawnWreckingBallDelayed(3.0f, plr));
            }
            //*/
        }

        ResetCountDownSounds();

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
            Vector3 pos = pil.transform.position + new Vector3(0.0f, m_avatarHeight, 0.0f);
            Quaternion rot = pil.transform.rotation;
            m_myAvatar.transform.SetPositionAndRotation(pos, rot);
            m_cameraRig.transform.SetPositionAndRotation(pos, rot);

            //LoadElementsCubes(m_myAvatar.m_myPillar); // Deprecated
        }

        m_allPlayers.Add(player);
        JowLogger.Log($"---+++ Registering new player @ {Time.fixedTime}s, {newPlayer}, netId {player.netId}, playerCount {m_allPlayers.Count}");
        JowLogger.Log($"on pillar {player.m_myPillar.name}, isServer {player.isServer}");

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


    public void RegisterHand(HandsMirror hand, bool hasAuthority)
    {
        if (hasAuthority)
        {
            if (hand.m_handType == OVRHand.Hand.HandRight)
            {
                m_rightHandMirror = hand;
            }
            else if (hand.m_handType == OVRHand.Hand.HandLeft)
            {
                m_leftHandMirror = hand;
            }
            else
            {
                Debug.LogError($"{gameObject} HandMirror: {hand} has incorrect settings: {hand.m_handType}.");
            }
        }

        JowLogger.Log($"+++ Registering new hand {hand}, {hand.netId}, authority= {hasAuthority}, type {hand.m_handType}");
    }


    public void RegisterBonus(BonusNet bonus)
    {
        m_allBonuses.Add(bonus);
    }


    public void UnregisterBonus(BonusNet bonus)
    {
        if (m_allBonuses.Contains(bonus))
        {
            m_allBonuses.Remove(bonus);
        }
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

        // No more patient for medics
        ReleasePatients(deadMob);

        // Spawn a bonus
        /*
        if (deadMob.m_mobType != MobsType.MobMedic)
        {
            Vector3 f = GetClosestPlayer(deadMob.transform.position).transform.position;
            f = (f - deadMob.transform.position).normalized;
            SpawnBonus(m_myAvatar, deadMob.transform.position, f);
        }
        */

        // Check for the end of the wave
        if (m_wave.m_deathNb == m_wave.m_mobs.Count)
        {
            StartCoroutine(WaveEndDelayed(m_endOfWaveDelay));
        }
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


    // Remove a wrecking from the list
    public void RemoveWrecking(WreckingBallMirror wrk)
    {
        if (m_wreckings.Contains(wrk))
        {
            m_wreckings.Remove(wrk);
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

        // Destroy wreckings
        foreach (WreckingBallMirror wrk in m_wreckings)
        {
            NetworkServer.Destroy(wrk.gameObject);
        }
        m_wreckings.Clear();

        // Destroy remaining bonuses
        foreach (BonusNet bonus in m_allBonuses)
        {
            NetworkServer.Destroy(bonus.gameObject);
        }
        m_allBonuses.Clear();
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

            if ((d <= 5.0) && (d > 4.0) && (m_countDownSound[4] == false))
            {
                m_countDownSound[4] = true;
                AudioSource.PlayClipAtPoint(m_audioSounds[7], m_myAvatar.transform.position);
            }
            else if ((d <= 4.0) && (d > 3.0) && (m_countDownSound[3] == false))
            {
                m_countDownSound[3] = true;
                AudioSource.PlayClipAtPoint(m_audioSounds[7], m_myAvatar.transform.position);
            }
            else if ((d <= 3.0) && (d > 2.0) && (m_countDownSound[2] == false))
            {
                m_countDownSound[2] = true;
                AudioSource.PlayClipAtPoint(m_audioSounds[7], m_myAvatar.transform.position);
            }
            else if ((d <= 2.0) && (d > 1.0) && (m_countDownSound[1] == false))
            {
                m_countDownSound[1] = true;
                AudioSource.PlayClipAtPoint(m_audioSounds[7], m_myAvatar.transform.position);
            }
            else if ((d <= 1.0) && (d > 0.0) && (m_countDownSound[0] == false))
            {
                m_countDownSound[0] = true;
                AudioSource.PlayClipAtPoint(m_audioSounds[7], m_myAvatar.transform.position);
            }
        }

        //---Stop bonuses when close to player---
        foreach (BonusNet b in m_allBonuses)
        {
            if (b.m_used == false)
            {
                float dist = (m_myAvatar.transform.position - b.transform.position).magnitude;
                if (dist < 0.5f)
                {
                    Rigidbody rb = b.GetComponent<Rigidbody>();
                    //rb.velocity = Vector3.zero;
                    //rb.angularVelocity = Vector3.zero;
                    rb.drag = 1.0f;
                    rb.angularDrag = 1.0f;
                }
            }
        }
        //---

#if UNITY_ANDROID
        // Update local player head position from headset ... Setup Henigma
        //if ((m_isUsingHands) && (m_myAvatar.isLocalPlayer)) // To be able to control head from keyboard
        if (m_myAvatar.isLocalPlayer)
        {
            m_myAvatar.transform.SetPositionAndRotation(m_localPlayerHead.transform.position, m_localPlayerHead.transform.rotation);
        }
#endif
        if (m_myAvatar.isLocalPlayer)
        {
            if (m_isUsingHands)
            {
                m_rightHandMirror.transform.SetPositionAndRotation(m_rightHand.transform.position, m_rightHand.transform.rotation);
                m_leftHandMirror.transform.SetPositionAndRotation(m_leftHand.transform.position, m_leftHand.transform.rotation);
            }
            else
            {
                Vector3 pos = m_myAvatar.transform.position;
                Quaternion q = m_myAvatar.transform.rotation;
                Vector3 r = m_myAvatar.transform.right * 0.2f;
                Vector3 l = m_myAvatar.transform.right * -0.2f;
                m_rightHandMirror.transform.SetPositionAndRotation(pos + r, q);
                m_leftHandMirror.transform.SetPositionAndRotation(pos + l, q);

                //m_rightHandMirror.transform.localScale *= 0.1f; // It seems the whole transform is sync anyway
            }
        }


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
        /*
        foreach (PlayerControlMirror plr in m_allPlayers)
        {
            plr.m_curHitAmount = 0.0f;
        }
        */
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
                plr.m_curHitAmount = 0.0f;
            }
        }

        //TestRotatingElem();

        // Draw a forward line from pillars
        /*
        foreach (GameObject pil in m_pillarsPool)
        {
            Vector3 start = pil.transform.position;
            Vector3 end = start + pil.transform.forward * 5.0f;
            Debug.DrawLine(start, end, Color.red);
            float angle = 45.0f;
            Quaternion rot = Quaternion.AngleAxis(angle, pil.transform.up);
            end = (rot * pil.transform.forward).normalized;
            end = start + end * 5.0f;
            Debug.DrawLine(start, end, Color.yellow);
        }
        */
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


    // Deprecated
    void TestRotatingElem()
    {
        /*
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
        */
    }


    // Update is called once per frame
    void Update()
    {
        //m_logTitle.text = $"{Time.fixedTime}s\n{m_netMan.networkAddress}\n{m_lastConnected}s"; // Connexion timer

        /*
        if (m_myAvatar)
        {
            string mobLife = "-";
            float lifeSum = 0.0f;
            foreach (Mobs mob in m_wave.m_mobs)
            {
                if (mob.m_mobType != MobsType.MobMedic)
                {
                    lifeSum += mob.m_life;
                }
            }
            mobLife = lifeSum.ToString("f2");
            m_logTitle.text = $"{m_myAvatar.transform.position}\n{mobLife}";
        }
        */

        if (m_myAvatar)
        {
            if (m_fader)
            {
                float f = 0.0f;
                if (m_nextWaveDate == 0.0f) // Only during a wave
                {
                    f = Mathf.Lerp(0.0f, 0.75f, 0.5f - m_playerLifeBar.m_fill.fillAmount);

                    // Death if any player is dead
                    foreach (PlayerControlMirror plr in m_allPlayers)
                    {
                        float lifeP = plr.m_curLife / m_playerLifeBar.m_maximum;
                        if (lifeP < 0.08f)
                        {
                            f = 1.0f - lifeP;
                        }
                    }
                }
                m_fader.SetFadeLevel(f);
            }
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
                    m_cameraRig.transform.position = m_myAvatar.m_myPillar.transform.position + new Vector3(0.0f, m_avatarHeight, 0.0f);
                }
            }
        }

        // Display bonus rot
        //CheckGrabbedBonus();

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

        // Activate first unused bonus
        if (Input.GetKeyUp(KeyCode.B))
        {
            foreach (BonusNet b in m_allBonuses)
            {
                if (b.m_used == false)
                {
                    b.GetColorGrabbable().m_lastGrabbed = Time.time;
                    //CheckGrabbedBonus();
                    b.AddAngularVelocity(new Vector3(0.0f, 5.0f, 0.0f));
                    b.GetColorGrabbable().Highlight = true;
                    b.GetColorGrabbable().UpdateColor();
                    break;
                }
            }
        }
        /*
        if (Input.GetKey(KeyCode.B))
        {
            foreach (BonusNet b in m_allBonuses)
            {
                if (b.m_used == false)
                {
                    b.GetColorGrabbable().m_lastGrabbed = Time.time;
                    CheckGrabbedBonus();
                    break;
                }
            }
        }
        */

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

        // Kill first alive mob in the list
        if (Input.GetKeyUp(KeyCode.K))
        {
            foreach (Mobs mob in m_wave.m_mobs)
            {
                if (mob.KillMob() == false)
                    continue;
                else
                    return;
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

        if (Input.GetKeyUp(KeyCode.M))
        {
            if (m_allPlayers.Count > 1)
            {
                PlayerControlMirror plr = m_allPlayers[0];
                if (plr == m_myAvatar)
                    plr = m_allPlayers[1];

                JowLogger.Log($"================== elem count {plr.m_myElems.Count}");
                if (plr.m_myElems.Count > 0)
                {
                    ElementsNet elem = plr.m_myElems[0];
                    if (elem.m_used == false)
                    {
                        elem.GetColorGrabbable().m_lastGrabbed = Time.time;
                        elem.transform.position = m_cameraRig.transform.position + Vector3.up * 2.0f; // Put it in the activation range
                    }
                }
            }
        }

        if (Input.GetKeyUp(KeyCode.N))
        {
            /*
            foreach (PlayerControlMirror plr in m_allPlayers)
            {
                JowLogger.Log($"Calling cmd from {plr.netId}... plr.hasAuthority {plr.hasAuthority}");
                plr.CmdTest();
            }
            */

            // Test cam shaking
            /*
            if (m_shaker)
            {
                //m_shaker.Shake1(m_cameraRig.transform, 3.0f); // wobble

                if (m_shaker.m_shaking)
                {
                    m_shaker.StopShaking(0.5f);
                }
                else
                {
                    m_shaker.Shake2(m_cameraRig.transform, 30.0f);
                }
            }
            */

            if (NetworkManager.singleton.mode == NetworkManagerMode.Host)
            {
                Vector3 pos = Vector3.zero;
                Mobs mob = m_wave.m_mobs[0];
                Vector3 f = (m_myAvatar.transform.position - mob.transform.position).normalized;
                SpawnBonus(m_myAvatar, mob.transform.position, f);
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

        if (Input.GetKeyUp(KeyCode.End))
        {
            StartCoroutine(WaveEndDelayed(m_endOfWaveDelay));
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

            if (m_isUsingHands == false)
            {
                m_isUsingHands = true;
            }

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
        JowLogger.Log($"Triggering element {elem.m_elemType}, {elem.netId}");
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
    }


    public void TriggerBonus()
    {
        if (m_myAvatar.hasAuthority)
        {
            JowLogger.Log($"netId {m_myAvatar.netId}, asking to spawn a new element...");
            m_myAvatar.LoadElements(m_myAvatar.netId);
            m_myAvatar.RepositionElements(); // JowTodo: Clean this! Shouldn't need it!
        }
    }


    // Spawn a big wrecking ball to aim to one player. (ServerSide only)
    public void SpawnWreckingBall(PlayerControlMirror plr)
    {
        int prefabIdx = 8;
        if (NetworkManager.singleton.spawnPrefabs.Count <= prefabIdx)
        {
            Debug.LogWarning($"No prefab for wrekcingBall, did you drag&drop them in the inspector?");
            return;
        }

        // Pick a player to aim to
        //PlayerControlMirror plr = m_allPlayers[0];
        Vector3 plrPos = plr.transform.position;
        Vector3 spawnPoint = plrPos + plr.m_myPillar.transform.forward * 5.0f;
        spawnPoint.y += 2.0f;
        Quaternion facing = Quaternion.LookRotation(plrPos - spawnPoint);
        GameObject prefab = NetworkManager.singleton.spawnPrefabs[prefabIdx];
        GameObject go = GameObject.Instantiate(prefab, spawnPoint, facing);
        //go.transform.localScale *= 2.0f;
        WreckingBallMirror ball = go.GetComponent<WreckingBallMirror>();
        //ball.m_speedFactor = 0.01f;
        m_wreckings.Add(ball);
        NetworkServer.Spawn(go);
    }


    // Check grabbed bonus
    void CheckGrabbedBonus()
    {
        foreach (BonusNet b in m_allBonuses)
        {
            if (b.m_used == false)
            {
                float lastGrabbed = b.GetColorGrabbable().m_lastGrabbed;
                if (lastGrabbed > 0.0f)
                {
                    Rigidbody rb = b.GetComponent<Rigidbody>();
                    m_logTitle.text = $"{b.netId} r: {rb.angularVelocity.magnitude}";
                    //JowLogger.Log($"Grabbed {b.netId} rot: {rb.angularVelocity.magnitude}");
                    break;
                }
            }
        }
    }


    public void SpawnBonus(PlayerControlMirror plr, Vector3 pos, Vector3 forward)
    {
        //m_myAvatar.SpawnBonus(m_myAvatar.netId, pos + forward, forward * 25.0f);
        //m_myAvatar.SpawnBonus(m_myAvatar.netId, pos + forward, forward * 50.0f);
        //m_myAvatar.SpawnBonus(m_myAvatar.netId, pos + forward, forward * 100.0f);
        m_myAvatar.SpawnBonus(m_myAvatar.netId, pos + forward, forward * 20.0f * pos.magnitude);
    }


    // Spawn a wreking ball delayed by x seconds for a specific player
    public IEnumerator SpawnWreckingBallDelayed(float waitSec, PlayerControlMirror plr)
    {
        yield return new WaitForSeconds(waitSec);

        SpawnWreckingBall(plr);
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


    public IEnumerator DestroyElementDelayed(float waitSec, ElementsNet elem)
    {
        yield return new WaitForSeconds(waitSec);

        JowLogger.Log($"aAa --- Trying to destroy {elem.netId} elem.hasAuthority {elem.hasAuthority} mode {NetworkManager.singleton.mode}, isServer {m_myAvatar.isServer}");
        m_myAvatar.DestroyElem(elem.netId, elem.m_ownerId);
    }


    IEnumerator WaveEndDelayed(float waitSec)
    {
        yield return new WaitForSeconds(waitSec);
        WaveEnded();
    }
}
