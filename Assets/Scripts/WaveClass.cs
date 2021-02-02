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
        Debug.Log($"{Time.fixedTime}s InitWave with hint {hint}...");
        m_mobs.Clear();
        m_deathNb = 0;

        if (NetworkManager.singleton.spawnPrefabs.Count < 3)
        {
            Debug.LogWarning($"No prefab for mobs, did you drag&drop them in the inspector?");
            return;
        }

        Vector3 pillarPos = GameMan.s_instance.GetPillar().transform.position;

        GameObject mobAPrefab = NetworkManager.singleton.spawnPrefabs[2];
        if ((hint == 1) || (hint > 6))
        {
            float y = 0.75f + hint * 0.8f;
            y = pillarPos.y + 3.0f;
            //Vector3 spawnPoint = new Vector3(1.0f, y, 1.7f);
            Vector3 spawnPoint = new Vector3(1.0f, y, 5.0f);
            GameObject mobGo = GameObject.Instantiate(mobAPrefab, spawnPoint, Quaternion.identity);
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
            GameObject mobGo = GameObject.Instantiate(mobAPrefab, spawnPoint, Quaternion.identity);
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
            GameObject mobBPrefab = NetworkManager.singleton.spawnPrefabs[3];
            if (mobBPrefab)
            {
                float y = pillarPos.y + 3.0f;
                //Vector3 spawnPoint = new Vector3(2.2f, y, 1.7f);
                Vector3 spawnPoint = new Vector3(2.2f, y, 5.0f);
                GameObject mobGo = GameObject.Instantiate(mobBPrefab, spawnPoint, Quaternion.identity);
                mobGo.name = "NewMobB";
                Mobs mob = mobGo.GetComponent<Mobs>();
                m_mobs.Add(mob);

                mob.CalcLifeAndArmor();

                NetworkServer.Spawn(mobGo);
            }
        }

        if (hint == 4)
        {
            GameObject mobBPrefab = NetworkManager.singleton.spawnPrefabs[4];
            if (mobBPrefab)
            {
                float y = pillarPos.y + 3.0f;
                Vector3 spawnPoint = new Vector3(2.2f, y, 5.0f);
                GameObject mobGo = GameObject.Instantiate(mobBPrefab, spawnPoint, Quaternion.identity);
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
            GameObject mobGo = GameObject.Instantiate(mobAPrefab, spawnPoint, Quaternion.identity);
            Mobs mob = mobGo.GetComponent<Mobs>();
            m_mobs.Add(mob);
            mob.CalcLifeAndArmor();
            NetworkServer.Spawn(mobGo);

            spawnPoint = new Vector3(0.0f, y, 5.0f);
            mobGo = GameObject.Instantiate(mobAPrefab, spawnPoint, Quaternion.identity);
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
                GameObject mobGo = GameObject.Instantiate(mobBPrefab, spawnPoint, Quaternion.identity);
                Mobs mob = mobGo.GetComponent<Mobs>();
                m_mobs.Add(mob);
                mob.CalcLifeAndArmor();
                NetworkServer.Spawn(mobGo);

                spawnPoint = new Vector3(3.0f, y + 1.0f, 5.0f);
                mobGo = GameObject.Instantiate(mobAPrefab, spawnPoint, Quaternion.identity);
                mob = mobGo.GetComponent<Mobs>();
                m_mobs.Add(mob);
                mob.CalcLifeAndArmor();
                NetworkServer.Spawn(mobGo);
            }
        }
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
}
