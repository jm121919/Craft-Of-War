using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawnManager : SingleTon<MonsterSpawnManager> 
{
    /*[SerializeField] Transform[] spawnPoints;
    [SerializeField] GameObject[] monsterPrefab;
    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            for(int i = 0; i< spawnPoints.Length; i++)
            {
                GameObject monster = GameManager.Instance.monsterObjectPool.Pop(0);
                monster.transform.position = spawnPoints[i].position;
            }
            
        }
    }*/
            
    /*public void SpawnMonster()
    {   

        GameObject monster;
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (i >= 5)
            {
                monster = GameManager.Instance.monsterObjectPool.Pop(1);
            }
            else
            {
                monster = GameManager.Instance.monsterObjectPool.Pop(0);
            }
       
            monster.transform.position = spawnPoints[i].transform.position;
            monster.GetComponent<Monster>().OriginPos = monster.transform.position;
        }
    }*/
}
