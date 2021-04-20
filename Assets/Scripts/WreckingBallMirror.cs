using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


// Jow: Transform is synchronized, serverside
// It needs the components NetworkIdentity + NetworkTransform


public class WreckingBallMirror : NetworkBehaviour
{
    [SyncVar(hook = nameof(SetBallColour))]
    public Color m_syncColor = Color.black;
    public float m_speedFactor = 0.8f;
    public float m_damages = 300.0f; // damage per fixedDeltaTime
    Renderer m_renderer;
    float m_enterTime = 0.0f;
    List<PlayerControlMirror> m_plrColliding = new List<PlayerControlMirror>(); // Server only
    public AudioSource m_audio;


    public override void OnStartClient()
    {
        m_renderer = GetComponent<Renderer>();
        if (m_renderer == null)
        {
            Debug.LogError($"{gameObject} OnStartClient @ {Time.fixedTime}s cannot initialize renderer.");
        }

        m_audio.spatialize = true;
        m_audio.transform.SetParent(gameObject.transform);

        m_audio.clip = GameMan.s_instance.m_audioSounds[3];
        m_audio.Play();
    }


    public override void OnStopClient()
    {
        JowLogger.Log($"xxx XXX xxx OnStopClient --- {gameObject} --- netId {netId}");
        base.OnStopClient();
    }


    public override void OnStartServer()
    {
        m_plrColliding.Clear();
        base.OnStartServer();
    }


    public override void OnStopServer()
    {
        m_plrColliding.Clear();
        base.OnStopServer();
    }


    // private function that is called when server is synching m_syncColor
    void SetBallColour(Color oldColor, Color newColor)
    {
        if (m_renderer == null)
        {
            m_renderer = GetComponent<Renderer>();
        }

        m_renderer.material.color = m_syncColor;
        //JowLogger.Log($"========================= Setting {netId} color from {oldColor} to {newColor} @ {Time.fixedTime}s m_syncColor {m_syncColor}");
    }


    private void FixedUpdate()
    {
        if (NetworkManager.singleton.mode != NetworkManagerMode.Host)
            return;

        transform.position = transform.position + transform.forward * m_speedFactor * Time.fixedDeltaTime;

        if (m_enterTime <= 0.0f)
        {
            float c = Mathf.Sin(Time.time * 20.0f);
            m_syncColor = Color.Lerp(Color.black, Color.white, c);
        }
        else
        {
            m_syncColor = Color.red;
        }

        foreach (PlayerControlMirror plr in m_plrColliding)
        {
            plr.m_curLife -= Time.fixedDeltaTime * m_damages;
        }
    }


    /*
    private void OnCollisionEnter(Collision collision)
    {
        JowLogger.Log($"========================= COLLISION {netId} with {collision}, {collision.gameObject}");

        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        rb.isKinematic = true; // Cancel physics collision and carry on along its path
    }


    private void OnCollisionExit(Collision collision)
    {
        JowLogger.Log($"----------------------- EXIT COLLISION {netId} with {collision.gameObject.name}");
    }
    */


    private void OnTriggerEnter(Collider other)
    {
        JowLogger.Log($"++++++++++++++++++++++ OnTriggerEnter {netId} with {other.gameObject.name}");

        PlayerControlMirror player = other.gameObject.GetComponent<PlayerControlMirror>();
        if (player != null)
        {
            m_enterTime = Time.time;
            m_audio.clip = GameMan.s_instance.m_audioSounds[6];
            m_audio.Play();

            GameMan.s_instance.m_shaker.Shake2(GameMan.s_instance.m_cameraRig.transform, 8.0f);

            if (NetworkManager.singleton.mode != NetworkManagerMode.Host)
                return;

            if (m_plrColliding.Contains(player) == false)
            {
                m_plrColliding.Add(player);
            }
        }
    }


    private void OnTriggerExit(Collider other)
    {
        float last = Time.time - m_enterTime;
        JowLogger.Log($"---------------------- OnTriggerExit {netId} with {other.gameObject.name}, last {last}s");

        PlayerControlMirror player = other.gameObject.GetComponent<PlayerControlMirror>();
        if (player != null)
        {
            m_enterTime = 0.0f;
            if (m_audio.isPlaying == true)
            {
                m_audio.Stop();
            }
            GameMan.s_instance.m_shaker.StopShaking(0.5f);

            if (NetworkManager.singleton.mode != NetworkManagerMode.Host)
                return;

            if (m_plrColliding.Contains(player))
            {
                m_plrColliding.Remove(player);
            }

            Invoke(nameof(DestroySelf), 0.5f);
        }
    }


    // destroy for everyone on the server
    [Server] void DestroySelf()
    {
        JowLogger.Log($"xxxxxxxxxxxxxxxxxxxxxxxxxxxxx DestroySelf {netId}");
        GameMan.s_instance.RemoveWrecking(this);
        NetworkServer.Destroy(gameObject);
    }
}
