//using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class WaveClass
{
    public int m_deathNb = 0;
    public List<Mobs> m_mobs = new List<Mobs>();
    Random.State m_curRndState;


    public WaveClass()
    {
        Random.State rndState = Random.state;
        Random.InitState(666);
        m_curRndState = Random.state;
        Random.state = rndState;
    }


    public void FinishWave()
    {
        foreach( Mobs mob in m_mobs)
        {
            GameObject.Destroy(mob.gameObject);
        }
        m_mobs.Clear();
    }


    // Start a new wave of mobs
    public void InitWave(int hint)
    {
        JowLogger.Log($"{Time.fixedTime}s InitWave with hint {hint}...");
        m_mobs.Clear();
        m_deathNb = 0;

        if (NetworkManager.singleton.spawnPrefabs.Count < 3)
        {
            Debug.LogWarning($"No prefab for mobs, did you drag&drop them in the inspector?");
            return;
        }

        // Save random state
        Random.State rndState = Random.state;
        Random.state = m_curRndState;
        //hint = 9;

        foreach (GameObject pillar in GameMan.s_instance.GetPillarPool())
        {
            Vector3 pillarPos = pillar.transform.position;

            Vector3 spawnPoint = Vector3.zero;
            Quaternion facing = Quaternion.identity;
            GetSpawningPoint(pillar, ref spawnPoint, ref facing);

            GameObject mobAPrefab = NetworkManager.singleton.spawnPrefabs[2];
            if (hint == 1)
            {
                GameObject mobGo = GameObject.Instantiate(mobAPrefab, spawnPoint, facing);
                Mobs mob = mobGo.GetComponent<Mobs>();
                m_mobs.Add(mob);
                NetworkServer.Spawn(mobGo);
            }

            // Same as 1 but with an extra armor on it
            if (hint == 2)
            {
                GameObject mobGo = GameObject.Instantiate(mobAPrefab, spawnPoint, facing);
                Mobs mob = mobGo.GetComponent<Mobs>();
                m_mobs.Add(mob);

                // Give hime some extra armour in code
                mob.m_parts[0].m_armorAddon = 20.0f;
                mob.m_parts[0].m_curArmorP = 1.0f;
                mob.CalcLifeAndArmor();

                NetworkServer.Spawn(mobGo);
            }

            // Same as 1 but with a medic
            if (hint == 3)
            {
                GameObject mobGo = GameObject.Instantiate(mobAPrefab, spawnPoint, facing);
                Mobs mob = mobGo.GetComponent<Mobs>();
                m_mobs.Add(mob);
                NetworkServer.Spawn(mobGo);

                // Spawn a medic mob to help him
                AddMedicMob(spawnPoint);
            }

            if (hint == 4)
            {
                GameObject mobBPrefab = NetworkManager.singleton.spawnPrefabs[3]; // Mob with plate armour
                if (mobBPrefab)
                {
                    GameObject mobGo = GameObject.Instantiate(mobBPrefab, spawnPoint, facing);
                    Mobs mob = mobGo.GetComponent<Mobs>();
                    mobGo.name = "NewMobB" + mob.netId.ToString();
                    m_mobs.Add(mob);

                    mob.CalcLifeAndArmor();

                    NetworkServer.Spawn(mobGo);
                }
            }

            // Same as before but we throw a wrecking ball
            if (hint == 5)
            {
                GameObject mobBPrefab = NetworkManager.singleton.spawnPrefabs[3]; // Mob with plate armour
                if (mobBPrefab)
                {
                    GameObject mobGo = GameObject.Instantiate(mobBPrefab, spawnPoint, facing);
                    Mobs mob = mobGo.GetComponent<Mobs>();
                    mobGo.name = "NewMobB" + mob.netId.ToString();
                    m_mobs.Add(mob);

                    mob.CalcLifeAndArmor();

                    NetworkServer.Spawn(mobGo);
                }
            }

            if (hint == 6)
            {
                GameObject mobBPrefab = NetworkManager.singleton.spawnPrefabs[4]; // Mob with legs
                if (mobBPrefab)
                {
                    GameObject mobGo = GameObject.Instantiate(mobBPrefab, spawnPoint, facing);
                    Mobs mob = mobGo.GetComponent<Mobs>();
                    mobGo.name = "NewMobC" + mob.netId.ToString();
                    m_mobs.Add(mob);

                    mob.CalcLifeAndArmor();

                    NetworkServer.Spawn(mobGo);
                }
            }

            // 2 simple mobs
            if (hint == 7)
            {
                GameObject mobGo = GameObject.Instantiate(mobAPrefab, spawnPoint, facing);
                Mobs mob = mobGo.GetComponent<Mobs>();
                m_mobs.Add(mob);
                mob.CalcLifeAndArmor();
                NetworkServer.Spawn(mobGo);

                GetSpawningPoint(pillar, ref spawnPoint, ref facing);
                mobGo = GameObject.Instantiate(mobAPrefab, spawnPoint, facing);
                mob = mobGo.GetComponent<Mobs>();
                m_mobs.Add(mob);
                mob.CalcLifeAndArmor();
                NetworkServer.Spawn(mobGo);
            }

            if (hint == 8)
            {
                GameObject mobBPrefab = NetworkManager.singleton.spawnPrefabs[3];
                if (mobBPrefab)
                {
                    GameObject mobGo = GameObject.Instantiate(mobBPrefab, spawnPoint, facing);
                    Mobs mob = mobGo.GetComponent<Mobs>();
                    m_mobs.Add(mob);
                    mob.CalcLifeAndArmor();
                    NetworkServer.Spawn(mobGo);

                    GetSpawningPoint(pillar, ref spawnPoint, ref facing);
                    mobGo = GameObject.Instantiate(mobAPrefab, spawnPoint, facing);
                    mob = mobGo.GetComponent<Mobs>();
                    m_mobs.Add(mob);
                    mob.CalcLifeAndArmor();
                    NetworkServer.Spawn(mobGo);
                }
            }

            if (hint == 9)
            {
                GameObject mobBPrefab = NetworkManager.singleton.spawnPrefabs[3]; // Mob with plate armour
                if (mobBPrefab)
                {
                    GameObject mobGo = GameObject.Instantiate(mobBPrefab, spawnPoint, facing);
                    Mobs mob = mobGo.GetComponent<Mobs>();
                    mobGo.name = "NewMobB" + mob.netId.ToString();
                    m_mobs.Add(mob);

                    mob.CalcLifeAndArmor();

                    NetworkServer.Spawn(mobGo);
                }

                GameObject mobCPrefab = NetworkManager.singleton.spawnPrefabs[4]; // Mob with legs
                if (mobCPrefab)
                {
                    GetSpawningPoint(pillar, ref spawnPoint, ref facing);
                    GameObject mobGo = GameObject.Instantiate(mobCPrefab, spawnPoint, facing);
                    Mobs mob = mobGo.GetComponent<Mobs>();
                    mobGo.name = "NewMobC" + mob.netId.ToString();
                    m_mobs.Add(mob);

                    mob.CalcLifeAndArmor();

                    NetworkServer.Spawn(mobGo);
                }
            }

            // A+medic + armour + legs
            if (hint > 9)
            {
                GameObject mobGo = GameObject.Instantiate(mobAPrefab, spawnPoint, facing);
                Mobs mob = mobGo.GetComponent<Mobs>();
                m_mobs.Add(mob);
                mob.CalcLifeAndArmor();
                NetworkServer.Spawn(mobGo);

                AddMedicMob(spawnPoint);

                GameObject mobBPrefab = NetworkManager.singleton.spawnPrefabs[3]; // Mob with plate armour
                if (mobBPrefab)
                {
                    GetSpawningPoint(pillar, ref spawnPoint, ref facing);
                    mobGo = GameObject.Instantiate(mobBPrefab, spawnPoint, facing);
                    mob = mobGo.GetComponent<Mobs>();
                    mobGo.name = "NewMobB" + mob.netId.ToString();
                    m_mobs.Add(mob);

                    mob.CalcLifeAndArmor();

                    NetworkServer.Spawn(mobGo);
                }

                GameObject mobCPrefab = NetworkManager.singleton.spawnPrefabs[4]; // Mob with legs
                if (mobCPrefab)
                {
                    GetSpawningPoint(pillar, ref spawnPoint, ref facing);
                    mobGo = GameObject.Instantiate(mobCPrefab, spawnPoint, facing);
                    mob = mobGo.GetComponent<Mobs>();
                    mobGo.name = "NewMobC" + mob.netId.ToString();
                    m_mobs.Add(mob);

                    mob.CalcLifeAndArmor();

                    NetworkServer.Spawn(mobGo);
                }
            }
        }
		
        m_curRndState = Random.state;
        // restore random state
        Random.state = rndState;
    }


    void GetSpawningPoint(GameObject from, ref Vector3 point, ref Quaternion orientation)
    {
        float angle = Random.Range(0.0f, 90.0f) - 45.0f;
        //JowLogger.Log($"----------------------Spawning at random {angle}...");
        Quaternion rot = Quaternion.AngleAxis(angle, from.transform.up);
        point = from.transform.position + (rot * from.transform.forward).normalized * 5.0f;

        orientation = Quaternion.LookRotation(from.transform.position - point);

        float h = Random.Range(1.0f, 3.0f);
        point.y += h;
    }


    // 
    public void MobIsDead(Mobs deadMob)
    {
        foreach (Mobs mob in m_mobs)
        {
            if (mob == deadMob)
            {
                m_deathNb++;
            }
        }
    }


    // Just add one medic mob
    public void AddMedicMob(Vector3 pos = new Vector3())
    {
        JowLogger.Log($"{Time.fixedTime}s Adding one Medic at {pos}");

        GameObject mobAPrefab = NetworkManager.singleton.spawnPrefabs[2];
        GameObject mobGo = GameObject.Instantiate(mobAPrefab, pos, Quaternion.identity);
        mobGo.transform.localScale *= 0.5f;
        Mobs mob = mobGo.GetComponent<Mobs>();
        mob.ChangeType(MobsType.MobMedic, Random.Range(0.0f, Mathf.PI * 2.0f));
        m_mobs.Add(mob);
        NetworkServer.Spawn(mobGo);
    }


    // Return closest mob from pos
    public Mobs GetClosestMob(Vector3 _pos)
    {
        Mobs ret = null;
        float dist = float.MaxValue;

        foreach (Mobs mob in m_mobs)
        {
            float d = (_pos - mob.transform.position).magnitude;
            if (d < dist)
            {
                dist = d;
                ret = mob;
            }
        }

        return ret;
    }
}
