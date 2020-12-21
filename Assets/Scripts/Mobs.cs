using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public enum MobsType { MobA, MobB}


public class Mobs : NetworkBehaviour
{
    public MobsType m_mobType = MobsType.MobA;
    public string m_name;
    [SyncVar] public float m_life;
    [SyncVar] public float m_armor;
    public List<MobPart> m_parts = new List<MobPart>();


    public override void OnStartServer()
    {
        // Fill a list of mob parts that define this mob
        switch (m_mobType)
        {
            case MobsType.MobA:
                m_name = "MobA";
                // Most simple mob with only one life part
                break;
            case MobsType.MobB:
                m_name = "MobB";
                m_life = 100.0f;
                m_armor = 20.0f;
                break;
            default:
                m_name = "WrongMob";
                m_life = 0.0f;
                m_armor = 0.0f;
                Debug.LogError($"{Time.fixedTime}s Wrong mob type !");
                break;
        }

        CalcLifeAndArmor();
        Debug.Log($"{Time.fixedTime}s OnStartServer---");
    }


    public override void OnStartClient()
    {
        Debug.Log($"{Time.fixedTime}s OnStartClient---");
    }


    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"{Time.fixedTime}s Start---");
    }


    // Compute current life and armor based on addons
    public void CalcLifeAndArmor()
    {
        float life = 0.0f;
        float armor = 0.0f;
        foreach (MobPart part in m_parts)
        {
            life += part.m_curLifeP * part.m_lifeAddon;
            armor += part.m_curArmorP * part.m_armorAddon;
        }

        m_life = life;
        m_armor = armor;

        Debug.Log($"{Time.fixedTime}s {m_life}pv");

        if (life <= 0.0f)
        {
            Debug.Log($"{Time.fixedTime}s Mob is DEAD !");
            gameObject.SetActive(false);
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
