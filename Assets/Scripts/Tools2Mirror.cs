using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


// Jow: Transform is synchronized, it is controled by the owner only
// It needs the components NetworkIdentity + NetworkTransform


public class Tools2Mirror : NetworkBehaviour
{
    public GameObject m_Owner;

    [SyncVar] Color m_myColor = Color.grey; // Set at creation
    Renderer m_renderer;
    [SyncVar] float m_spawnTime;
    public bool m_rightSpinning = false;
    public bool m_leftSpinning = false;


    public override void OnStartClient()
    {
        m_renderer = GetComponent<Renderer>();
        if (m_renderer == null)
        {
            Debug.LogError($"{gameObject} OnStartClient @ {Time.fixedTime}s cannot initialize renderer.");
        }
        else
        {
            JowLogger.Log($"{gameObject} OnStartClient @ {Time.fixedTime}s m_syncColor {m_myColor}, authority= {hasAuthority}.");
            m_renderer.material.color = m_myColor;
        }

        GameMan.s_instance.RegisterNewTool2(this, hasAuthority);
    }


    public override void OnStartServer()
    {
        JowLogger.Log($"{gameObject} OnStartServer @ {Time.fixedTime}s.");
        m_spawnTime = Time.fixedTime;
    }


    public void SetToolColour(Color newCol)
    {
        if (m_renderer == null)
        {
            m_renderer = GetComponent<Renderer>();
        }

        if (m_renderer)
        {
            JowLogger.Log($"{gameObject} SetToolColour @ {Time.fixedTime}s m_syncColor {m_myColor} newCol {newCol}");
            m_renderer.material.color = newCol;
            m_myColor = newCol;
        }
    }


    // Start is called before the first frame update but after OnStartXXX
    void Start()
    {
    }


    private void FixedUpdate()
    {
        // Spinning
        if (m_rightSpinning)
        {
            transform.RotateAround(transform.position, Vector3.up, 100.0f * Time.fixedDeltaTime);
        }
        if (m_leftSpinning)
        {
            transform.RotateAround(transform.position, Vector3.up, -100.0f * Time.fixedDeltaTime);
        }
    }
}
