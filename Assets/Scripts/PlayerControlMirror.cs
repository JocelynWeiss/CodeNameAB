using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class PlayerControlMirror : NetworkBehaviour
{
    [SyncVar]
    public Vector3 Control; //This is a sync var, mirror automatically shares and syncs this variable across all of the scripts on objects with the same network identity, but it can only be set by the server.

    public Color c;//color to change to if we are controlling this one
    public float m_jumpForce = 300.0f;

    [SyncVar]
    public Color m_syncColor = Color.grey;

    Renderer m_renderer;
    [HideInInspector] public Rigidbody m_rb;


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
        transform.SetPositionAndRotation(new Vector3(0.0f + netid, 2.0f, 1.0f), Quaternion.identity);

        Debug.Log($"Creating client {n} --->netId {netid}: {this} OnStartClient @ {Time.fixedTime}s hasAuthority {hasAuthority}");
        /*
        if (hasAuthority)
        {
            Vector3 newCol = Random.insideUnitSphere;
            m_syncColor = new Color(newCol.x, newCol.y, newCol.z);
            c = m_syncColor;
            Debug.Log($"\t {m_syncColor}");
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
        Debug.Log($"{NetworkManager.singleton.numPlayers} +++ {this} Start @ {Time.fixedTime}s");

        //transform.SetPositionAndRotation(new Vector3(1.0f, 1.0f, 1.0f), Quaternion.identity);
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
        m_rb.AddForce(Vector3.up * m_jumpForce);
    }


    public void SpawnMyTool()
    {
        CmdSpawnTool(m_syncColor);
    }


    [Command] void CmdSpawnTool(Color myCol)
    {
        GameObject toolPrefab = NetworkManager.singleton.spawnPrefabs[0];
        if (toolPrefab)
        {
            GameObject tool = Instantiate(toolPrefab, transform.position, transform.rotation);
            tool.transform.position = transform.position + new Vector3(0.0f, -0.2f, 0.0f);
            ToolsMirror compTool = tool.GetComponent<ToolsMirror>();
            compTool.m_Owner = this.gameObject;
            //compTool.SetToolColour(myCol);
            NetworkServer.Spawn(tool);
            compTool.SetToolColour(myCol);
        }
    }


    private void FixedUpdate()
    {
        if (m_renderer == null)
        {
            m_renderer = GetComponent<Renderer>();
        }

        Color curCol = m_renderer.material.color;
        if (m_syncColor != curCol)
        {
            m_renderer.material.color = m_syncColor;
            //Debug.Log($"========================= Seting color to {m_syncColor} @ {Time.fixedTime}s"); // JowTodo: Why is this called every frame ?
        }
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
            Debug.Log($"========================= Seting color to {m_syncColor} @ {Time.fixedTime}s");
        }
        */
    }
}
