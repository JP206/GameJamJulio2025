using System.Collections;
using TMPro;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] TextMeshProUGUI roundText, roundTextShade, killCountText, killCountTextShade;
    [SerializeField] AudioSource audioSource, musicSource, holyCowSource;
    [SerializeField] AudioClip waveMusic, bossMusic;
    [SerializeField] float waveTimeout = 240f;


    EnemyPool enemyPool;
    int round = 1, enemiesToSpawn = 5, bossesToSpawn = 1, deadEnemies = 0, bossWaves = 3, killCount = 0;
    UIAnimation uiAnimation;
    bool waveEnding = false;

    void Start()
    {
        enemyPool = GetComponent<EnemyPool>();
        uiAnimation = FindAnyObjectByType<UIAnimation>();
        StartWave();
    }

    public void NotifyDeath()
    {
        deadEnemies++;
        killCount++;
        killCountText.text = "Kill count: " + killCount.ToString();
        killCountTextShade.text = killCountText.text;
        uiAnimation.Animation(killCountText, killCountTextShade);
        if (killCount % 50 == 0 && killCount > 0)
        {
            PlayHolyCowSound(holyCowSource);
        }

        if (!waveEnding && deadEnemies == enemiesToSpawn)
        {
            EndWave();
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
        waveEnding = false;

        roundText.text = "Round: " + round.ToString();
        roundTextShade.text = roundText.text;
        uiAnimation.Animation(roundText, roundTextShade);

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

            for (int i = 0; i < (int)(bossesToSpawn / 3); i++)
            {
                GameObject galloInstance = enemyPool.GetGallo();
                galloInstance.GetComponent<EnemyController>().SetWaveManager(this);
                galloInstance.transform.position = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
                RandomizePosition(galloInstance.transform);
            }

            if (!musicSource.isPlaying)
            {
                musicSource.clip = bossMusic;
            }
        }
        else
        {
            if (!musicSource.isPlaying)
            {
                musicSource.clip = waveMusic;
            }
        }

        if (!musicSource.isPlaying)
        {
            musicSource.Play();
        }

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }

        StartCoroutine(WaveTimer());
    }

    IEnumerator WaveTimer()
    {
        yield return new WaitForSeconds(waveTimeout);

        if (!waveEnding)
        {
            EndWave();
        }
    }

    void EndWave()
    {
        waveEnding = true;

        audioSource.Stop();

        if (!BossesAlive())
        {
            musicSource.Stop();
        }

        StartCoroutine(TimeBetweenWaves());
    }

    void RandomizePosition(Transform transform)
    {
        float randomX = Random.Range(0, 3f);
        float randomY = Random.Range(0, 3f);

        transform.position = new Vector2(transform.position.x + randomX, transform.position.y + randomY);
    }

    bool BossesAlive()
    {
        EnemyController[] enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);

        foreach (EnemyController enemy in enemies)
        {
            if (enemy.tag.Equals("Boss"))
            {
                return true;
            }
        }

        return false;
    }

    private void PlayHolyCowSound(AudioSource source)
    {
        if (source != null && source.clip != null)
        {
            source.PlayOneShot(source.clip);
        }
    }
}
