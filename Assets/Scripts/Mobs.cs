using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public enum MobsType { MobA, MobB, MobC, MobMedic}


public class Mobs : NetworkBehaviour
{
    public MobsType m_mobType = MobsType.MobA;
    public string m_name;
    [SyncVar(hook = nameof(SetMobLife))] public float m_life;
    [SyncVar] public float m_armor;
    public float m_speedFactor = 0.5f;
    [ViewOnly] public float m_curSpeedFactor = 0.0f; // computed from m_speedFactor
    public List<MobPart> m_parts = new List<MobPart>();

    Mobs m_patient = null; // For medic, their actual patient
    float m_animPhase = 0.0f; // So they are not all on the same path


    
    public override void OnStartServer()
    {
        // Fill a list of mob parts that define this mob
        switch (m_mobType)
        {
            case MobsType.MobA:
                m_name = "MobA" + netId;
                // Most simple mob with only one life part
                //m_speedFactor = 0.15f;
                break;
            case MobsType.MobB:
                m_name = "MobB" + netId;
                //m_life = 100.0f;
                //m_armor = 20.0f;
                //m_speedFactor = 0.1f;
                break;
            case MobsType.MobC:
                m_name = "MobC" + netId;
                //m_speedFactor = 0.25f;
                break;
            case MobsType.MobMedic:
                m_name = "MobMedic" + netId;
                //m_life = 8.0f;

                if (m_parts.Count > 0)
                {
                    m_parts[0].m_lifeAddon = 8.0f;
                }

                m_speedFactor = 0.75f;
                break;
            default:
                m_name = "WrongMob";
                m_life = 0.0f;
                m_armor = 0.0f;
                Debug.LogError($"{Time.fixedTime}s Wrong mob type !");
                break;
        }

        CalcLifeAndArmor();
        JowLogger.Log($"{Time.fixedTime}s OnStartServer---");
    }


    public override void OnStartClient()
    {
        JowLogger.Log($"{Time.fixedTime}s OnStartClient---");
        if (m_mobType != MobsType.MobMedic)
        {
            AudioSource.PlayClipAtPoint(GameMan.s_instance.m_audioSounds[3], transform.position);
        }
    }


    public override void OnStopClient()
    {
        JowLogger.Log($"OnStopClient --- {gameObject} --- netId {netId}");
        AudioSource.PlayClipAtPoint(GameMan.s_instance.m_audioSounds[2], transform.position);
        base.OnStopClient();
    }


    // Start is called before the first frame update
    void Start()
    {
        JowLogger.Log($"{Time.fixedTime}s Start--- {name}");
    }


    // Compute current life and armor based on addons
    public void CalcLifeAndArmor()
    {
        if (NetworkManager.singleton.mode != NetworkManagerMode.Host)
            return;

        float life = 0.0f;
        float armor = 0.0f;
        float speed = 0.0f;
        foreach (MobPart part in m_parts)
        {
            life += part.m_curLifeP * part.m_lifeAddon;
            armor += part.m_curArmorP * part.m_armorAddon;
            speed += part.m_curLifeP * part.m_speedAddon;
        }

        m_life = life;
        m_armor = armor;
        m_curSpeedFactor = m_speedFactor + speed;

        JowLogger.Log($"{Time.fixedTime}s {name} {m_life}pv");

        if (gameObject.activeSelf == true)
        {
            if (m_life < 1.0f) // A mob need at least 1pv
            {
                JowLogger.Log($"{Time.fixedTime}s Mob is DEAD !");
                gameObject.SetActive(false);
                GameMan.s_instance.MobIsDead(this);
                UnspawnSelf();
            }
        }

        // A mob with its first (main) part at 0 life needs to be destroyed
        if (gameObject.activeSelf == true)
        {
            float mainLife = m_parts[0].m_curLifeP;
            if (mainLife <= 0.0f)
            {
                JowLogger.Log($"{Time.fixedTime}s Mob is DEAD ! main part destroyed");
                gameObject.SetActive(false);
                GameMan.s_instance.MobIsDead(this);
                UnspawnSelf();
            }
        }
    }


    // Gameplay update
    private void FixedUpdate()
    {
        if (NetworkManager.singleton.mode != NetworkManagerMode.Host)
            return;

        if (m_mobType == MobsType.MobMedic)
        {
            if (m_patient == null)
            {
                FindPatient();
            }
            if (m_patient == null)
            {
                // Find closest pillar, advance on it
                GameObject pillar = GameMan.s_instance.GetClosestPillar(transform.position);
                Vector3 forward = pillar.transform.position - transform.position;

                if (forward.magnitude > 3.0f)
                {
                    forward.Normalize();
                    transform.position += forward * m_curSpeedFactor * Time.fixedDeltaTime;
                }
                else
                {
                    // Damage to death
                    m_parts[0].TakeDamage(5.0f * Time.fixedDeltaTime, 5.0f * Time.fixedDeltaTime);
                    CalcLifeAndArmor();
                }

                return;
            }

            float angle = Time.fixedTime * m_curSpeedFactor + m_animPhase;
            float heightAngle = angle * m_animPhase * 0.2f;
            float radius = 1.0f;
            radius = 2.0f + Mathf.Sin(m_animPhase + Time.fixedTime * m_curSpeedFactor * 0.1f);
            transform.position = m_patient.transform.position + new Vector3(Mathf.Cos(angle), Mathf.Cos(heightAngle) * Mathf.Sin(heightAngle), Mathf.Sin(angle)) * radius;
        }
    }


    // Called on clients whenever the server is updating
    private void SetMobLife(float oldVal, float newVal)
    {
        //JowLogger.Log($"Updating {name}.{netId} life from {oldVal} to {newVal} now {m_life}");
        if (m_mobType != MobsType.MobMedic)
        {
            AudioSource.PlayClipAtPoint(GameMan.s_instance.m_audioSounds[5], transform.position);
        }
    }


    // Change type for this mob (should be done before network spawning)
    public void ChangeType(MobsType _Type, float _Phase = 0.0f)
    {
        //JowLogger.Log($"{Time.fixedTime}s Change type mob {name} from {m_mobType} to {_Type} with phase {_Phase}");
        m_mobType = _Type;
        m_animPhase = _Phase;
    }


    // Return current patient if any or null
    public Mobs GetCurPatient()
    {
        return m_patient;
    }


    // Set current patient to _patient (may be null)
    public void SetPatient(Mobs _patient)
    {
        m_patient = _patient;
    }


    // Find a patient for a medic
    public Mobs FindPatient()
    {
        foreach (Mobs target in GameMan.s_instance.m_wave.m_mobs)
        {
            if ((target.m_mobType != MobsType.MobMedic) && (target.m_life < 500.0f) && (target.m_life > 1.0f))
            {
                m_patient = target;
                return m_patient;
            }
        }
        return null;
    }


    // Called on Client when a part is detroyed on server
    [ClientRpc] public void RpcOnPartDestroyed(string partName)
    {
        GameObject go = transform.Find(partName).gameObject;
        if (go)
        {
            JowLogger.Log($"{Time.fixedTime}s detroying mob part {partName}");
            go.SetActive(false);
        }
        else
        {
            JowLogger.Log($"{Time.fixedTime}s CAN'T detroy mob part {partName} from {name}");
        }
    }


    // destroy for everyone on the server
    [Server] void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }


    // unspawn for everyone on the server
    [Server] void UnspawnSelf()
    {
        NetworkServer.UnSpawn(gameObject);
    }
}
