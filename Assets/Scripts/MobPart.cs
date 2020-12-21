using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobPart : MonoBehaviour
{
    public float m_lifeAddon; // Amount of life this module brings in. Don't change over time.
    public float m_armorAddon;// Amount of armor this module brings in. Don't change over time.
    public float m_hitDelayLength = 1.0f; // in seconds
    public float m_damageAmount = 5.0f; // JowNext: Put it in the bullets
    public float m_pushbackForce = 0.01f; // JowNext: Put it in the bullets
    [Range(0.0f, 1.0f)] public float m_curLifeP; // current life pourcentage 
    [Range(0.0f, 1.0f)] public float m_curArmorP; // current armor pourcentage
    Renderer m_renderer;
    public float m_lastHit;



    private void Awake()
    {
        Initialize();
    }


    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }


    public void Initialize()
    {
        m_curLifeP = 0.0f;
        m_curArmorP = 0.0f;
        m_lastHit = 0.0f;

        if (m_lifeAddon > 0.0f)
        {
            m_curLifeP = 1.0f;
        }

        if (m_armorAddon > 0.0f)
        {
            m_curArmorP = 1.0f;
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"{Time.fixedTime}s collision {collision.gameObject.name}");

        m_renderer = GetComponent<Renderer>();
        if (m_renderer)
        {
            m_renderer.material.color = Color.red;
            m_lastHit = Time.fixedTime;

            // Set damages
            float life = m_lifeAddon * m_curLifeP;
            life -= m_damageAmount;
            if (life < 0.0f)
            {
                life = 0.0f;
            }
            float p = life / m_lifeAddon;
            m_curLifeP = p;

            /*
            ToolsMirror bullet = collision.gameObject.GetComponent<ToolsMirror>();
            if (bullet)
            {

            }
            */

            Mobs mob = transform.parent.gameObject.GetComponent<Mobs>();
            if (mob)
            {
                Vector3 pos = mob.transform.position;
                pos.z += m_pushbackForce;
                mob.transform.SetPositionAndRotation(pos, mob.transform.rotation);

                mob.CalcLifeAndArmor();
            }
        }
    }


    private void FixedUpdate()
    {
        float hitDelay = m_hitDelayLength;
        if ((m_lastHit > 0.0f) && (Time.fixedTime - hitDelay > m_lastHit))
        {
            m_lastHit = 0.0f;
            m_renderer.material.color = Color.grey;
        }
    }
}
