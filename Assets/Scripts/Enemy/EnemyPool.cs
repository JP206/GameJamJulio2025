using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    [SerializeField] GameObject enemyPrefab;

    int poolSize = 10;
    List<GameObject> enemyPool = new();

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
}
