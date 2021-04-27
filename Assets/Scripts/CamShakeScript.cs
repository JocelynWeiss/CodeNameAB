using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamShakeScript : MonoBehaviour
{
    public float m_wobblePace = 1.0f;
    public float m_smooth = 10;
    public Vector3 m_amplitudePos;
    public Vector3 m_amplitudeRot;

    public bool m_wobbling = false;
    private float m_wobbleTime = 0.0f;
    private Transform m_transform;


    public bool m_shaking = false;
    float m_duration = 0.0f;
    Vector3 m_originalPos;
    Vector3 m_originalRot;


    // Make the transform wobble on each axis for duration
    public void Shake1(Transform target, float _duration)
    {
        m_transform = target;
        m_originalPos = m_transform.position;
        m_originalRot = m_transform.localEulerAngles;
        m_duration = _duration;
        m_wobbleTime = 0.0f;
        m_wobbling = true;
    }


    // Start is called before the first frame update
    void Start()
    {
        m_duration = 0.0f;
        m_wobbling = false;
        m_shaking = false;
    }


    // Update is called once per frame
    void Update()
    {
        if (m_transform == null)
            return;

        float endTime = Time.time + m_duration;
        if (endTime <= Time.time)
        {
            m_wobbling = false;
            m_shaking = false;
            return;
        }

        if (m_wobbling)
        {
            m_duration -= Time.deltaTime;
            m_wobbleTime += Time.deltaTime * m_wobblePace;

            float pssd = Mathf.Sin(m_wobbleTime);
            Vector3 goalPos = m_originalPos + (pssd * m_amplitudePos);
            m_transform.localPosition = goalPos;

            m_transform.localEulerAngles = m_originalRot + (pssd * m_amplitudeRot);
        }

        // ---Second method using pure random shaking---
        if (m_shaking)
        {
            m_duration -= Time.deltaTime;
            Vector3 shakeA = m_amplitudePos;
            if (m_duration < 1.0f)
                shakeA *= m_duration; // fadeout shaking on the last second

            Vector3 pos = Random.insideUnitSphere;
            pos.Scale(shakeA);
            pos += m_originalPos;
            //m_transform.localPosition = m_originalPos + pos * Time.deltaTime; // no lerp
            m_transform.localPosition = Vector3.Lerp(m_transform.localPosition, pos, Time.deltaTime * m_smooth);
        }
    }


    // Shake the transform with random mouvement using xyz amplitude
    public void Shake2(Transform target, float _duration)
    {
        m_transform = target;
        m_duration = _duration;
        m_originalPos = m_transform.position;
        m_shaking = true;
    }


    // Stop shaking effect in x sec (should be < 1)
    public void StopShaking(float delay)
    {
        m_duration = delay;
    }


    public void IncreaseDuration(float _sec)
    {
        m_duration += _sec;
    }
}
