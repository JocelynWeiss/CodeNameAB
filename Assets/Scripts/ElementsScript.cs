using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OculusSampleFramework;

public class ElementsScript : MonoBehaviour
{
    public Elements m_elemType = Elements.Air;

    ColorGrabbable m_grabbable;


    public ColorGrabbable GetColorGrabbable()
    {
        return m_grabbable;
    }


    private void Awake()
    {
        m_grabbable = GetComponent<ColorGrabbable>();
        /*
        if (m_grabbable == null)
        {
            Debug.Log($"Getting grabbable from parent...");
            m_grabbable = transform.parent.GetComponent<ColorGrabbable>();
            if (m_grabbable == null)
            {
                Debug.LogError($"Cannot get the grabbable");
            }
        }
        */
    }


    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"ElementsScript {name} {m_elemType} Start @ {Time.fixedTime}s");
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
        if (m_grabbable.m_lastGrabbed != 0.0f)
        {
            TriggerElement();
            m_grabbable.m_lastGrabbed = 0.0f;
        }
    }


    // Trigger this element
    public void TriggerElement()
    {
        Vector3 eyePos = GameMan.s_instance.m_cameraRig.transform.position;
        //float dist = Vector3.Distance(eyePos, transform.position);
        float dist = transform.position.y - eyePos.y;

        if (dist > 1.0f)
        {
            GameMan.s_instance.TriggerElement(this);
            //AudioSource.PlayClipAtPoint(GameMan.s_instance.m_audioSounds[1], transform.position);

            StartCoroutine(DelayedFall(3.0f));
        }
    }


    // Remove this object from the scene
    private void OnDestroy()
    {
        Destroy(gameObject);
    }


    // Make it fall after waitSec
    public IEnumerator DelayedFall(float waitSec)
    {
        yield return new WaitForSeconds(waitSec);

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;
    }
}
