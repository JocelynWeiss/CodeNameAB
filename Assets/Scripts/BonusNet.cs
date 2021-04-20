using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OculusSampleFramework;
using Mirror;

public class BonusNet : NetworkBehaviour
{
    [SyncVar] public Elements m_elemType = Elements.Air;
    public bool m_used = false;
    public float m_lifeTime = 15.0f; // life time in seconds

    ColorGrabbable m_grabbable;
    Rigidbody m_rb;


    public ColorGrabbable GetColorGrabbable()
    {
        return m_grabbable;
    }


    public override void OnStartServer()
    {
        //Invoke(nameof(DestroySelf), m_lifeTime);
    }


    // Start is called before the first frame update but after OnStartXXX
    void Start()
    {
        m_grabbable = GetComponent<ColorGrabbable>();
        m_rb = GetComponent<Rigidbody>();
        m_used = false;
        //JowLogger.Log($"ElementsNet Start ++++++++++ {m_elemType}, netId {netId}, hasAuthority {hasAuthority}, avatarAuthority {GameMan.s_instance.GetLocalPlayer().hasAuthority}");
        ChangeType(m_elemType, GameMan.s_instance.m_CubesElemMats[(int)m_elemType]);

        GameMan.s_instance.RegisterBonus(this);
    }


    public void ChangeType(Elements newType, Material newMat)
    {
        m_elemType = newType;
        Renderer rndr = GetComponent<Renderer>();
        if (rndr)
        {
            rndr.material = newMat;
        }

        //m_grabbable.UpdateColor();
    }


    private void FixedUpdate()
    {
        //if (m_grabbable.m_lastGrabbed != 0.0f)
        {
            if (m_used == false)
            {
                //TriggerBonus();
                //m_grabbable.m_lastGrabbed = 0.0f; // Jow: don't force this is still being grabbed
                if (m_rb)
                {
                    if (m_rb.angularVelocity.magnitude > 10.0f)
                    {
                        TriggerBonus2();
                    }
                }
            }
        }
    }


    // Trigger this bonus
    public void TriggerBonus()
    {
        JowLogger.Log($"--- --- --- TriggerBonus {netId}");

        AudioSource.PlayClipAtPoint(GameMan.s_instance.m_audioSounds[7], transform.position);
        m_used = true;
        GameMan.s_instance.TriggerBonus();
        StartCoroutine(DelayedFall(0.0f)); // Put it away from the grab so it will be released
        //DestroySelf();
        //Invoke(nameof(DestroySelf), m_lifeTime);
    }


    // Trigger this bonus
    public void TriggerBonus2()
    {
        JowLogger.Log($"--- --- --- TriggerBonus2 {netId}");

        AudioSource.PlayClipAtPoint(GameMan.s_instance.m_audioSounds[8], transform.position);
        m_used = true;
        m_grabbable.GrabEnd(Vector3.zero, Vector3.zero);
        GameMan.s_instance.TriggerBonus();
        StartCoroutine(DelayedFall(0.0f)); // Put it away from the grab so it will be released

        //m_grabbable.Highlight = false;
        //m_grabbable.UpdateColor();
    }


    public override void OnStopClient()
    {
        base.OnStopClient();
    }


    // Make it fall after waitSec
    public IEnumerator DelayedFall(float waitSec)
    {
        yield return new WaitForSeconds(waitSec);

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;
    }


    public void AddForce(Vector3 force)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.AddForce(force);
        rb.angularVelocity = Random.insideUnitSphere;
    }


    // Add angular velocity
    public void AddAngularVelocity(Vector3 _vel)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.angularVelocity += _vel;
    }


    public void DestroySelf()
    {
        JowLogger.Log($"--- --- --- DestroySelf {netId}");
        NetworkManager.Destroy(this.gameObject);
    }


    public void OnDestroy()
    {
        GameMan.s_instance.UnregisterBonus(this);
    }
}
