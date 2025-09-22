using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    [SerializeField] GameObject enemyPrefab, bossPrefab, galloPrefab, polloLocoPrefab;

    int poolSize = 10;
    List<GameObject> enemyPool = new();
    List<GameObject> bossPool = new();
    List<GameObject> galloPool = new();
    List<GameObject> polloLocoPool = new();

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

            GameObject polloLocoInstance = Instantiate(polloLocoPrefab);
            polloLocoInstance.SetActive(false);
            polloLocoPool.Add(polloLocoInstance);
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

        GameObject galloInstance = Instantiate(galloPrefab);
        galloPool.Add(galloInstance);
        return galloInstance;
    }

    public GameObject GetPolloLoco()
    {
        foreach (GameObject pollo in polloLocoPool)
        {
            if (!pollo.activeSelf)
            {
                pollo.SetActive(true);
                return pollo;
            }
        }

        GameObject polloInstance = Instantiate(polloLocoPrefab);
        polloLocoPool.Add(polloInstance);
        return polloInstance;
    }
}
