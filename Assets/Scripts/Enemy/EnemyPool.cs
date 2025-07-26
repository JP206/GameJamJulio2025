using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    [SerializeField] GameObject enemyPrefab, bossPrefab, galloPrefab;

    int poolSize = 10;
    List<GameObject> enemyPool = new();
    List<GameObject> bossPool = new();
    List<GameObject> galloPool = new();

    void Start()
    {
        InitializePool();
    }

    void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject enemyInstance = Instantiate(enemyPrefab);
            enemyInstance.SetActive(false);
            enemyPool.Add(enemyInstance);

            GameObject bossInstance = Instantiate(bossPrefab);
            bossInstance.SetActive(false);
            bossPool.Add(bossInstance);

            GameObject galloInstance = Instantiate(galloPrefab);
            galloInstance.SetActive(false);
            galloPool.Add(galloInstance);
        }
    }

    public GameObject GetEnemy()
    {
        foreach (GameObject enemy in enemyPool)
        {
            if (!enemy.activeSelf)
            {
                enemy.SetActive(true);
                return enemy;
            }
        }

        // Si sale del for y no encuentra enemigo, se agrega uno a la pool
        GameObject enemyInstance = Instantiate(enemyPrefab);
        enemyPool.Add(enemyInstance);
        return enemyInstance;
    }

    public GameObject GetBoss()
    {
        foreach (GameObject boss in bossPool)
        {
            if (!boss.activeSelf)
            {
                boss.SetActive(true);
                return boss;
            }
        }

        // Si sale del for y no encuentra enemigo, se agrega uno a la pool
        GameObject bossInstance = Instantiate(bossPrefab);
        bossPool.Add(bossInstance);
        return bossInstance;
    }

    public GameObject GetGallo()
    {
        foreach (GameObject gallo in galloPool)
        {
            if (!gallo.activeSelf)
            {
                gallo.SetActive(true);
                return gallo;
            }
        }

        // Si sale del for y no encuentra enemigo, se agrega uno a la pool
        GameObject galloInstance = Instantiate(galloPrefab);
        bossPool.Add(galloInstance);
        return galloInstance;
    }
}
