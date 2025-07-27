using System.Collections;
using TMPro;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] TextMeshProUGUI roundText, killCountText;
    [SerializeField] AudioSource audioSource, musicSource;
    [SerializeField] AudioClip waveMusic, bossMusic;

    EnemyPool enemyPool;
    int round = 1, enemiesToSpawn = 5, bossesToSpawn = 1, deadEnemies = 0, bossWaves = 3, killCount = 0;

    void Start()
    {
        enemyPool = GetComponent<EnemyPool>();

        StartWave();
    }

    public void NotifyDeath()
    {
        deadEnemies++;
        killCount++;
        killCountText.text = "Kill count: " + killCount.ToString();

        if (deadEnemies == enemiesToSpawn)
        {
            audioSource.Stop();
            musicSource.Stop();
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
        roundText.text = "Round: " + round.ToString();

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
        
            for (int i = 0; i < (int) (bossesToSpawn / 3); i++)
            {
                GameObject galloInstance = enemyPool.GetGallo();
                galloInstance.GetComponent<EnemyController>().SetWaveManager(this);
                galloInstance.transform.position = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
                RandomizePosition(galloInstance.transform);
            }

            musicSource.clip = bossMusic;
        }
        else
        {
            musicSource.clip = waveMusic;
        }

        musicSource.Play();
        audioSource.Play();
    }

    void RandomizePosition(Transform transform)
    {
        float randomX = Random.Range(0, 3f);
        float randomY = Random.Range(0, 3f);

        transform.position = new Vector2(transform.position.x + randomX, transform.position.y + randomY);
    }
}
