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


    public void InitWave(int hint)
    {
        Debug.Log($"{Time.fixedTime}s InitWave with hint {hint}...");
        m_mobs.Clear();
        m_deathNb = 0;

        GameObject mobAPrefab = NetworkManager.singleton.spawnPrefabs[2];
        if (mobAPrefab)
        {
            float y = 0.75f + hint * 0.8f;
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
            float y = 0.75f + hint * 0.8f;
            //Vector3 spawnPoint = new Vector3(2.2f, y, 1.7f);
            Vector3 spawnPoint = new Vector3(2.2f, y, 5.0f);
            GameObject mobGo = GameObject.Instantiate(mobAPrefab, spawnPoint, Quaternion.identity);
            mobGo.SetActive(true);
            Mobs mob = mobGo.GetComponent<Mobs>();
            m_mobs.Add(mob);

            // Give hime some extra armour
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
                float y = 0.75f + hint * 0.8f;
                //Vector3 spawnPoint = new Vector3(2.2f, y, 1.7f);
                Vector3 spawnPoint = new Vector3(2.2f, y, 5.0f);
                GameObject mobGo = GameObject.Instantiate(mobBPrefab, spawnPoint, Quaternion.identity);
                mobGo.name = "NewMobB";
                //mobGo.SetActive(true);
                Mobs mob = mobGo.GetComponent<Mobs>();
                m_mobs.Add(mob);

                mob.CalcLifeAndArmor();

                NetworkServer.Spawn(mobGo);
            }
        }

        if (hint == 1)
        {
            GameObject mobBPrefab = NetworkManager.singleton.spawnPrefabs[4];
            if (mobBPrefab)
            {
                float y = 0.75f + hint * 0.8f;
                Vector3 spawnPoint = new Vector3(2.2f, y, 5.0f);
                GameObject mobGo = GameObject.Instantiate(mobBPrefab, spawnPoint, Quaternion.identity);
                mobGo.name = "NewMobC";
                Mobs mob = mobGo.GetComponent<Mobs>();
                m_mobs.Add(mob);

                mob.CalcLifeAndArmor();

                NetworkServer.Spawn(mobGo);
            }
        }
    }


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
