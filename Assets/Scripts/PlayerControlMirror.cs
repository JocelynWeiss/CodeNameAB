using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class PlayerControlMirror : NetworkBehaviour
{
    public float m_jumpForce = 300.0f;

    [HideInInspector] public Tools2Mirror m_tool = null; // A tool that is spawned by the client

    [SyncVar(hook = nameof(SetAvatarColour))]
    public Color m_syncColor = Color.grey;

    Renderer m_renderer;
    [HideInInspector] public Rigidbody m_rb;

    public int m_ammoCount = 100;
    [SyncVar(hook = nameof(SetAvatarLife))] public float m_curLife = 1000.0f;
    public float m_curHitAmount = 0.0f; // cumulated hits taken in current frame

    public PillarMirror m_myPillar;
    public List<ElementsNet> m_myElems = new List<ElementsNet>();


    public override void OnStartClient()
    {
        m_renderer = GetComponent<Renderer>();
        if (m_renderer == null)
        {
            Debug.LogError($"OnStartClient @ {Time.fixedTime}s cannot initialize renderer.");
        }

        m_rb = GetComponent<Rigidbody>();
        if (m_rb == null)
        {
            //Debug.LogError($"OnStartClient @ {Time.fixedTime}s cannot initialize RigidBody.");
            // Jow: Might happens depending on the prefab...
        }
        /*
        else if (isClient)
        {
            m_rb.isKinematic = true;
            m_rb.useGravity = false;
        }
        */

        int n = NetworkManager.singleton.numPlayers;
        //transform.SetPositionAndRotation(new Vector3(0.0f + n, 2.0f, 1.0f), Quaternion.identity);
        uint netid = GetComponent<NetworkIdentity>().netId;
        //transform.SetPositionAndRotation(new Vector3(0.0f + n, 2.0f, 1.0f), Quaternion.identity);
        Quaternion q = Quaternion.identity;
        transform.SetPositionAndRotation(new Vector3(1.0f + GameMan.s_instance.m_allPlayers.Count, 2.0f, 1.0f), q);

        JowLogger.Log($"Creating client {n} --->netId {netid}: {this} OnStartClient @ {Time.fixedTime}s hasAuthority {hasAuthority}");
        JowLogger.Log($"{transform.position}");
        /*
        if (hasAuthority)
        {
            Vector3 newCol = Random.insideUnitSphere;
            m_syncColor = new Color(newCol.x, newCol.y, newCol.z);
            c = m_syncColor;
            JowLogger.Log($"\t {m_syncColor}");
        }
        else
        {
            c = m_syncColor;
        }
        */
        ChangeAvatarColour();

        m_renderer.material.color = m_syncColor;

        GameMan.s_instance.RegisterNewPlayer(this.gameObject, hasAuthority);
    }


    // Start is called before the first frame update (Don't use this, use OnStartClient instead)
    void Start()
    {
        if (isServer)
        {
            JowLogger.Log($"{NetworkManager.singleton.numPlayers} +++ {this} NetId= {netId} Start @ {Time.fixedTime}s");
        }
        else
        {
            JowLogger.Log($"+++ {this} NetId= {netId} Start @ {Time.fixedTime}s numPlayers {NetworkManager.singleton.numPlayers} over gameMan playerNb {GameMan.s_instance.m_allPlayers.Count}");
        }

        //transform.SetPositionAndRotation(new Vector3(1.0f, 1.0f, 1.0f), Quaternion.identity);
    }


    public override void OnStopClient()
    {
        //JowLogger.Log($"OnStopClient <-- netId {netId} @ {System.DateTime.Now} hasAuthority {hasAuthority}");
        base.OnStopClient();

        GameMan.s_instance.DisconnectPlayer(this);
    }


    // private function that is called when server is synching m_syncColor
    void SetAvatarColour(Color oldColor, Color newColor)
    {
        if (m_renderer == null)
        {
            m_renderer = GetComponent<Renderer>();
            JowLogger.Log($"========================= Setting color from {oldColor} to {newColor} @ {Time.fixedTime}s m_syncColor {m_syncColor}"); // JowTodo: Why is this called every frame ?
        }

        //m_syncColor = newColor;
        m_renderer.material.color = m_syncColor;
        //m_renderer.material.color = newColor;
        //JowLogger.Log($"MMMMMMMMMMMMMMM Setting color from {oldColor} to {newColor} @ {Time.fixedTime}s m_syncColor {m_syncColor}");
    }


    // private function that is called when server is synching life
    void SetAvatarLife(float oldLife, float newLife)
    {
        if (hasAuthority) // if we are the local player only
        {
            GameMan.s_instance.m_playerLifeBar.m_cur = m_curLife;
        }
    }


    public void ChangeAvatarColour()
    {
        if (isServer)
        {
            Vector3 newCol = Random.insideUnitSphere;
            m_syncColor = new Color(newCol.x, newCol.y, newCol.z);
        }
        else
        {
            CmdChangeAvatarColour();
        }
    }


    // Executed by host from client
    [Command] void CmdChangeAvatarColour()
    {
        Vector3 newCol = Random.insideUnitSphere;
        m_syncColor = new Color(newCol.x, newCol.y, newCol.z);
    }


    // Apply a jump force on rigidbody
    public void Jump()
    {
        if (m_rb == null)
            return;

        if (isServer)
        {
            m_rb.AddForce(Vector3.up * m_jumpForce);
        }
        else
        {
            CmdJump();
        }
    }


    // Executed by host from client
    [Command] void CmdJump()
    {
        if (m_rb == null)
            return;

        m_rb.AddForce(Vector3.up * m_jumpForce);
    }


    public void PositionPlayer(Vector3 pos)
    {
        CmdPositionPlayer(pos);
    }


    [Command] void CmdPositionPlayer(Vector3 pos)
    {
        transform.SetPositionAndRotation(pos, transform.rotation);
    }


    // Spawn an object which is then client controled (by physics)
    public void SpawnMyTool(Vector3 pos, Quaternion rot)
    {
        m_ammoCount--;
        CmdSpawnTool(m_syncColor, pos, rot);
    }


    [Command] void CmdSpawnTool(Color myCol, Vector3 pos, Quaternion rot)
    {
        GameObject toolPrefab = NetworkManager.singleton.spawnPrefabs[0];
        if (toolPrefab)
        {
            /*
            GameObject tool = Instantiate(toolPrefab, transform.position, transform.rotation);
            tool.transform.position = transform.position + new Vector3(0.0f, -0.2f, 0.0f);
            */
            GameObject tool = Instantiate(toolPrefab, pos, rot);
            //tool.transform.position = trs.position + new Vector3(0.0f, -0.2f, 0.0f);
            tool.transform.position = pos + new Vector3(0.0f, 0.0f, 0.0f);
            ToolsMirror compTool = tool.GetComponent<ToolsMirror>();
            compTool.m_Owner = this.gameObject;
            compTool.SetToolColour(myCol); // Set it before the spawn so as a sync var it's properly set
            NetworkServer.Spawn(tool);
        }
    }


    // Spawn an object which has its transform synchronized
    public void SpawnMyTool2()
    {
        CmdSpawnTool2();
    }


    [Command]
    void CmdSpawnTool2()
    {
        GameObject toolPrefab = NetworkManager.singleton.spawnPrefabs[1];
        if (toolPrefab)
        {
            GameObject tool = Instantiate(toolPrefab, transform.position, transform.rotation);
            Vector3 pos = transform.position + new Vector3(0.0f, +0.2f, 0.0f);
            Quaternion rot = transform.rotation;
            tool.transform.SetPositionAndRotation(pos, rot);
            tool.transform.Rotate(new Vector3(0.0f, 0.0f, 90.0f));
            //tool.transform.SetParent(transform); // make it a child of its owner
            Tools2Mirror compTool = tool.GetComponent<Tools2Mirror>();
            compTool.name = "AvatarTool2";
            compTool.m_Owner = this.gameObject;
            compTool.SetToolColour(m_syncColor);
            NetworkServer.Spawn(tool, this.gameObject); // Second param here set the authority
        }
    }


    private void FixedUpdate()
    {
        /*
        if (m_renderer == null)
        {
            m_renderer = GetComponent<Renderer>();
        }

        Color curCol = m_renderer.material.color;
        if (m_syncColor != curCol)
        {
            m_renderer.material.color = m_syncColor;
            //JowLogger.Log($"========================= Seting color to {m_syncColor} @ {Time.fixedTime}s"); // JowTodo: Why is this called every frame ?
        }
        */
    }


    // Update is called once per frame
    void Update()
    {
        /*
        if (GetComponent<NetworkIdentity>().hasAuthority)//make sure this is an object that we are controlling
        {
            GetComponent<Renderer>().material.color = c;//change color
        }
        else
        {
            Color curCol = GetComponent<Renderer>().material.color;
            if (m_syncColor != curCol)
            {
                curCol = m_syncColor;
            }
        }
        */

        /*
        Color curCol = m_renderer.material.color;
        if (m_syncColor != curCol)
        {
            //curCol = m_syncColor;
            m_renderer.material.color = m_syncColor;
            JowLogger.Log($"========================= Seting color to {m_syncColor} @ {Time.fixedTime}s");
        }
        */
    }


    [Command] public void CmdEndOfWave(int newWaveNb)
    {
        JowLogger.Log($"CmdEndOfWave from netId {netId}, newWaveNb = {newWaveNb}");
        RpcWaveNb(newWaveNb);
    }


    // Update wave nb on the client (execute on the host as well of course)
    [ClientRpc]
    public void RpcWaveNb(int newWaveNb)
    {
        JowLogger.Log($"netId {netId}, newWaveNb = {newWaveNb}");
        GameMan.s_instance.m_waveNb = newWaveNb;
        m_ammoCount = 100;
        m_curLife = 1000.0f;
        GameMan.s_instance.m_playerLifeBar.m_maximum = (int)m_curLife;
        GameMan.s_instance.m_playerLifeBar.m_cur = m_curLife;
        GameMan.s_instance.SetPlayerInfoText();
        GameMan.s_instance.m_logTitle.text = "";
        LoadElements(netId);
        RepositionElements();
    }


    // Client side to update next wave time
    [ClientRpc]
    public void RpcNextWaveTime(double _AODate)
    {
        JowLogger.Log($"Player {netId} Next wave date {_AODate} = {System.DateTime.FromOADate(_AODate)}");
        GameMan.s_instance.m_nextWaveDate = _AODate;

        if (_AODate > 0.0)
        {
            HideElements();
            AudioSource.PlayClipAtPoint(GameMan.s_instance.m_audioSounds[4], transform.position);
        }
    }


    [Command] public void LoadElements(uint _netId)
    {
        GameObject elemPrefab = NetworkManager.singleton.spawnPrefabs[5];
        if (elemPrefab)
        {
            int count = m_myElems.Count + 1;
            //Vector3 pos = m_myPillar.transform.position + new Vector3(-1.2f, 2.0f + ((float)count * 0.2f), 0.0f);
            //pos += m_myPillar.transform.forward * 0.8f;
            Vector3 pos = m_myPillar.transform.position + new Vector3(-1.0f, 2.25f + ((float)count * 0.2f), 0.0f);
            pos += m_myPillar.transform.forward * 0.5f;
            GameObject newCube = Instantiate(elemPrefab, pos, Quaternion.identity);
            newCube.transform.position = pos + new Vector3(0.0f, 0.0f, 0.0f);
            ElementsNet elem = newCube.GetComponent<ElementsNet>();
            int matId = Random.Range(0, 4);
            elem.ChangeType((Elements)matId, GameMan.s_instance.m_CubesElemMats[matId]);
            elem.m_ownerId = _netId;
            NetworkServer.Spawn(newCube);
        }
    }


    public void HideElements()
    {
        foreach (ElementsNet elem in m_myElems)
        {
            elem.gameObject.SetActive(false);
        }
    }


    public void RepositionElements()
    {
        int count = 1;
        foreach (ElementsNet elem in m_myElems)
        {
            if (elem.m_used == true)
                continue;

            //Vector3 pos = m_myPillar.transform.position + new Vector3(-1.2f, 2.0f + ((float)count * 0.2f), 0.0f);
            //pos += m_myPillar.transform.forward * 0.8f;
            Vector3 pos = m_myPillar.transform.position + new Vector3(-1.0f, 2.25f + ((float)count * 0.2f), 0.0f);
            pos += m_myPillar.transform.forward * 0.5f;
            elem.transform.position = pos;
            elem.gameObject.SetActive(true);
            count++;
        }
    }
}
