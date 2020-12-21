using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


// Jow: Transform isn't synchronized, physics runs both sides independently.
// Self destroy after m_lifeTime seconds


public class ToolsMirror : NetworkBehaviour
{
    public GameObject m_Owner;
    public float m_lifeTime = 5.0f; // life time in seconds

    [SyncVar]
    public Color m_syncColor = Color.grey;

    Renderer m_renderer;
    float m_spawnTime;


    public override void OnStartClient()
    {
        m_renderer = GetComponent<Renderer>();
        if (m_renderer == null)
        {
            Debug.LogError($"{gameObject} OnStartClient @ {Time.fixedTime}s cannot initialize renderer.");
        }
        else
        {
            //Debug.Log($"{gameObject} OnStartClient @ {Time.fixedTime}s m_syncColor {m_syncColor}.");
            m_renderer.material.color = m_syncColor; // To test
        }

        m_spawnTime = Time.fixedTime;

        // This object transform isn't synchronized, add force client side
        Rigidbody rb = GetComponent<Rigidbody>();
        Vector3 force = transform.forward * 100.0f;
        rb.AddForce(force);
    }


    public override void OnStartServer()
    {
        Invoke(nameof(DestroySelf), m_lifeTime);
    }


    public void SetToolColour(Color newCol)
    {
        if (m_renderer == null)
        {
            m_renderer = GetComponent<Renderer>();
        }

        //Debug.Log($"{gameObject} SetToolColour @ {Time.fixedTime}s m_syncColor {m_syncColor} newCol {newCol}");
        m_renderer.material.color = newCol;
        m_syncColor = newCol;
    }


    // Start is called before the first frame update but after OnStartXXX
    void Start()
    {
        m_renderer.material.color = m_syncColor;
    }


    // destroy for everyone on the server
    [Server]
    void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }


    private void FixedUpdate()
    {
    }
}
