using UnityEngine;

public class WaveManager : MonoBehaviour
{
    EnemyPool enemyPool;
    int round = 1, enemiesToSpawn = 5, bossesToSpawn = 1, deadEnemies = 0, bossWaves = 3;

    void Start()
    {
        enemyPool = GetComponent<EnemyPool>();
    }

    public void NotifyDeath()
    {
        deadEnemies++;

        if (deadEnemies == enemiesToSpawn)
        {
            ScaleDifficulty();
            StartWave();
        }
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
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            enemyPool.GetEnemy();
            // cambiar la posicion donde spawnean
        }

        if (round % bossWaves == 0)
        {
            enemyPool.GetBoss();
            //cambiar la posicion donde spawnea
        }
    }
}
