using System.Collections;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField] Transform[] spawnPoints;

    EnemyPool enemyPool;
    int round = 1, enemiesToSpawn = 5, bossesToSpawn = 1, deadEnemies = 0, bossWaves = 3;

    void Start()
    {
        enemyPool = GetComponent<EnemyPool>();

        StartWave();
    }

    public void NotifyDeath()
    {
        deadEnemies++;

        if (deadEnemies == enemiesToSpawn)
        {
            StartCoroutine(TimeBetweenWaves());
        }
    }

    IEnumerator TimeBetweenWaves()
    {
        ScaleDifficulty();

        yield return new WaitForSeconds(3);

        StartWave();
    }

    void ScaleDifficulty()
    {
        enemiesToSpawn += 3;
        round++;

        if (round % bossWaves == 0)
        {
            bossesToSpawn++;
        }
    }

    void StartWave()
    {
        deadEnemies = 0;

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            GameObject enemyInstance = enemyPool.GetEnemy();
            enemyInstance.GetComponent<EnemyController>().SetWaveManager(this);
            enemyInstance.transform.position = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
            RandomizePosition(enemyInstance.transform);
        }

        if (round % bossWaves == 0)
        {
            for (int i = 0; i < bossesToSpawn; i++)
            {
                GameObject bossInstance = enemyPool.GetBoss();
                bossInstance.GetComponent<EnemyController>().SetWaveManager(this);
                bossInstance.transform.position = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
                RandomizePosition(bossInstance.transform);
            }
        }
    }

    void RandomizePosition(Transform transform)
    {
        float randomX = Random.Range(0, 3f);
        float randomY = Random.Range(0, 3f);

        transform.position = new Vector2(transform.position.x + randomX, transform.position.y + randomY);
    }
}
