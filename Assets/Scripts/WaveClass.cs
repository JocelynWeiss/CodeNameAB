//using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class WaveClass
{
    public int m_deathNb = 0;
    public List<Mobs> m_mobs = new List<Mobs>();


    public WaveClass()
    {
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
        Random.InitState(666);
        //hint = 5;

        int modulo = 0;
        //Vector3 pillarPos = GameMan.s_instance.GetPillar().transform.position;
        foreach (GameObject pillar in GameMan.s_instance.GetPillarPool())
        {
            Vector3 pillarPos = pillar.transform.position;

            Vector2 onCircle = Random.insideUnitCircle; // on the circle around pillar at some distance
            onCircle.Normalize();
            //JowLogger.Log($"--------------------------------onCircle {onCircle}");
            if (modulo % 2 == 0)
            {
                onCircle *= 5.0f;
            }
            else
            {
                onCircle *= 10.0f;
            }
            //modulo++;
            //JowLogger.Log($"onCircle {onCircle} magnitude {onCircle.magnitude}");
            Vector3 spawnPoint2 = new Vector3(onCircle.x, pillarPos.y, onCircle.y);

            Quaternion facing = Quaternion.identity;
            facing = Quaternion.LookRotation(pillarPos - spawnPoint2);

            spawnPoint2.y += 3.0f;

            GameObject mobAPrefab = NetworkManager.singleton.spawnPrefabs[2];
            if ((hint == 1) || (hint > 7))
            {
                float y = 0.75f + hint * 0.8f;
                y = pillarPos.y + 3.0f;
                //Vector3 spawnPoint = new Vector3(1.0f, y, 1.7f);
                Vector3 spawnPoint = new Vector3(1.0f, y, 5.0f);
                spawnPoint = spawnPoint2;
                GameObject mobGo = GameObject.Instantiate(mobAPrefab, spawnPoint, facing);
                //mobGo.transform.localScale *= 0.25f;
                mobGo.SetActive(true);
                Mobs mob = mobGo.GetComponent<Mobs>();
                m_mobs.Add(mob);
                NetworkServer.Spawn(mobGo);
            }

            if (hint == 2)
            {
                float y = pillarPos.y + 3.0f;
                //Vector3 spawnPoint = new Vector3(2.2f, y, 1.7f);
                Vector3 spawnPoint = new Vector3(2.2f, y, 5.0f);
                spawnPoint = spawnPoint2;
                GameObject mobGo = GameObject.Instantiate(mobAPrefab, spawnPoint, facing);
                mobGo.SetActive(true);
                Mobs mob = mobGo.GetComponent<Mobs>();
                m_mobs.Add(mob);

                // Give hime some extra armour in code
                mob.m_parts[0].m_armorAddon = 20.0f;
                mob.m_parts[0].m_curArmorP = 1.0f;
                mob.CalcLifeAndArmor();

                NetworkServer.Spawn(mobGo);
            }

            if (hint == 3)
            {
                GameObject mobBPrefab = NetworkManager.singleton.spawnPrefabs[3]; // Mob with plate armour
                if (mobBPrefab)
                {
                    float y = pillarPos.y + 3.0f;
                    //Vector3 spawnPoint = new Vector3(2.2f, y, 1.7f);
                    Vector3 spawnPoint = new Vector3(2.2f, y, 5.0f);
                    spawnPoint = spawnPoint2;
                    GameObject mobGo = GameObject.Instantiate(mobBPrefab, spawnPoint, facing);
                    mobGo.name = "NewMobB";
                    Mobs mob = mobGo.GetComponent<Mobs>();
                    m_mobs.Add(mob);

                    mob.CalcLifeAndArmor();

                    NetworkServer.Spawn(mobGo);
                }
            }

            if (hint == 4)
            {
                GameObject mobBPrefab = NetworkManager.singleton.spawnPrefabs[4]; // Mob with legs
                if (mobBPrefab)
                {
                    float y = pillarPos.y + 3.0f;
                    Vector3 spawnPoint = new Vector3(2.2f, y, 5.0f);
                    spawnPoint = spawnPoint2;
                    GameObject mobGo = GameObject.Instantiate(mobBPrefab, spawnPoint, facing);
                    mobGo.name = "NewMobC";
                    Mobs mob = mobGo.GetComponent<Mobs>();
                    m_mobs.Add(mob);

                    mob.CalcLifeAndArmor();

                    NetworkServer.Spawn(mobGo);
                }
            }

            if (hint == 5)
            {
                float y = pillarPos.y + 3.0f;
                Vector3 spawnPoint = new Vector3(3.0f, y + 1.0f, 5.0f);
                spawnPoint = spawnPoint2;
                GameObject mobGo = GameObject.Instantiate(mobAPrefab, spawnPoint, facing);
                Mobs mob = mobGo.GetComponent<Mobs>();
                m_mobs.Add(mob);
                mob.CalcLifeAndArmor();
                NetworkServer.Spawn(mobGo);

                spawnPoint = new Vector3(0.0f, y, 6.0f);
                //---
                /*
                onCircle = Random.insideUnitCircle; // on the circle around pillar at some distance
                onCircle.Normalize();
                if (modulo % 2 == 0)
                {
                    onCircle *= 5.0f;
                }
                else
                {
                    onCircle *= 10.0f;
                }
                //spawnPoint2 = new Vector3(onCircle.x + 1.0f, pillarPos.y + 2.0f, onCircle.y);
                spawnPoint2 = new Vector3(onCircle.x, pillarPos.y, onCircle.y);
                facing = Quaternion.LookRotation(pillarPos - spawnPoint2);
                */
                spawnPoint2.y += 2.0f;
                spawnPoint = spawnPoint2;
                //---

                mobGo = GameObject.Instantiate(mobAPrefab, spawnPoint, facing);
                mob = mobGo.GetComponent<Mobs>();
                m_mobs.Add(mob);
                mob.CalcLifeAndArmor();
                NetworkServer.Spawn(mobGo);
            }

            if (hint == 6)
            {
                GameObject mobBPrefab = NetworkManager.singleton.spawnPrefabs[3];
                if (mobBPrefab)
                {
                    float y = pillarPos.y + 3.0f;
                    Vector3 spawnPoint = new Vector3(0.0f, y, 5.0f);
                    spawnPoint = spawnPoint2;
                    GameObject mobGo = GameObject.Instantiate(mobBPrefab, spawnPoint, facing);
                    Mobs mob = mobGo.GetComponent<Mobs>();
                    m_mobs.Add(mob);
                    mob.CalcLifeAndArmor();
                    NetworkServer.Spawn(mobGo);

                    spawnPoint = new Vector3(3.0f, y + 1.0f, 5.0f);
                    //---
                    /*
                    onCircle = Random.insideUnitCircle; // on the circle around pillar at some distance
                    onCircle.Normalize();
                    if (modulo % 2 == 0)
                    {
                        onCircle *= 5.0f;
                    }
                    else
                    {
                        onCircle *= 10.0f;
                    }
                    spawnPoint2 = new Vector3(onCircle.x, pillarPos.y + 2.0f, onCircle.y);
                    facing = Quaternion.LookRotation(pillarPos - spawnPoint2);
                    */
                    spawnPoint2.y += 2.0f;
                    spawnPoint = spawnPoint2;
                    //---
                    mobGo = GameObject.Instantiate(mobAPrefab, spawnPoint, facing);
                    mob = mobGo.GetComponent<Mobs>();
                    m_mobs.Add(mob);
                    mob.CalcLifeAndArmor();
                    NetworkServer.Spawn(mobGo);
                }
            }

            if (hint == 7)
            {
                GameObject mobBPrefab = NetworkManager.singleton.spawnPrefabs[3]; // Mob with plate armour
                if (mobBPrefab)
                {
                    float y = pillarPos.y + 3.0f;
                    Vector3 spawnPoint = new Vector3(0.0f, y, 5.0f);
                    spawnPoint = spawnPoint2;
                    GameObject mobGo = GameObject.Instantiate(mobBPrefab, spawnPoint, facing);
                    mobGo.name = "NewMobB";
                    Mobs mob = mobGo.GetComponent<Mobs>();
                    m_mobs.Add(mob);

                    mob.CalcLifeAndArmor();

                    NetworkServer.Spawn(mobGo);
                }

                GameObject mobCPrefab = NetworkManager.singleton.spawnPrefabs[4]; // Mob with legs
                if (mobCPrefab)
                {
                    float y = pillarPos.y + 3.0f;
                    Vector3 spawnPoint = new Vector3(3.0f, y, 5.0f);
                    //---
                    /*
                    onCircle = Random.insideUnitCircle; // on the circle around pillar at some distance
                    onCircle.Normalize();
                    if (modulo % 2 == 0)
                    {
                        onCircle *= 5.0f;
                    }
                    else
                    {
                        onCircle *= 10.0f;
                    }
                    spawnPoint2 = new Vector3(onCircle.x, pillarPos.y + 2.0f, onCircle.y);
                    facing = Quaternion.LookRotation(pillarPos - spawnPoint2);
                    */
                    spawnPoint2.y += 2.0f;
                    spawnPoint = spawnPoint2;
                    //---
                    GameObject mobGo = GameObject.Instantiate(mobCPrefab, spawnPoint, facing);
                    mobGo.name = "NewMobC";
                    Mobs mob = mobGo.GetComponent<Mobs>();
                    m_mobs.Add(mob);

                    mob.CalcLifeAndArmor();

                    NetworkServer.Spawn(mobGo);
                }
            }

            modulo++;
        }
		
        // Add medics
        if ((hint == 999) || (hint > 7))
		{
			AddMedicMob();
			AddMedicMob();
			AddMedicMob();
		}

        // restore random state
        Random.state = rndState;
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


    // Just add one medic mob JOWIP
    public void AddMedicMob()
    {
        JowLogger.Log($"{Time.fixedTime}s Adding one Medic");

        GameObject mobAPrefab = NetworkManager.singleton.spawnPrefabs[2];
        Vector3 spawnPoint = new Vector3(0.0f, 0.0f, 0.0f);
        GameObject mobGo = GameObject.Instantiate(mobAPrefab, spawnPoint, Quaternion.identity);
        mobGo.transform.localScale *= 0.5f;
        Mobs mob = mobGo.GetComponent<Mobs>();
        mob.ChangeType(MobsType.MobMedic, Random.Range(0.0f, Mathf.PI * 2.0f));
        m_mobs.Add(mob);
        NetworkServer.Spawn(mobGo);
    }
}
