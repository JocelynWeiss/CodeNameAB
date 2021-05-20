using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


// Jow: Transform is synchronized, it is controled by the owner only
// It needs the components NetworkIdentity + NetworkTransform


public class HandsMirror : NetworkBehaviour
{
    public PlayerControlMirror m_Owner;
    public OVRHand.Hand m_handType = OVRHand.Hand.None;

    [SyncVar(hook = nameof(SetHandColour))]
    public Color m_syncColor = Color.grey;

    Renderer m_renderer;


    public override void OnStartClient()
    {
        m_renderer = GetComponent<Renderer>();
        if (m_renderer == null)
        {
            Debug.LogError($"{gameObject} OnStartClient @ {Time.fixedTime}s cannot initialize renderer.");
        }

        GameMan.s_instance.RegisterHand(this, hasAuthority);

        // Hide hands for local player as they are rendered from OVRHand
        /*
        if (hasAuthority)
        {
            m_renderer.enabled = false;
        }
        */
    }


    // private function that is called when server is synching m_syncColor
    void SetHandColour(Color oldColor, Color newColor)
    {
        if (m_renderer == null)
        {
            m_renderer = GetComponent<Renderer>();
        }

        //m_syncColor = newColor; // newColor is already set
        m_renderer.material.color = m_syncColor;
        //JowLogger.Log($"========================= Setting {netId} color from {oldColor} to {newColor} @ {Time.fixedTime}s m_syncColor {m_syncColor}");
    }


}
