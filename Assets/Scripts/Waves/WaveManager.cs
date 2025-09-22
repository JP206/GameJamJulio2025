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
    [SerializeField] private int bossWaveThreshold;

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

        if (round == bossWaveThreshold)
        {
            GameObject bossInstance = enemyPool.GetPolloLoco();
            bossInstance.GetComponent<EnemyController>().SetWaveManager(this);
            bossInstance.transform.position = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
            RandomizePosition(bossInstance.transform);

            if (musicSource.clip != bossMusic)
            {
                musicSource.clip = bossMusic;
                musicSource.Play();
            }

            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }

            return;
        }

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            GameObject enemyInstance = enemyPool.GetEnemy();
            enemyInstance.GetComponent<EnemyController>().SetWaveManager(this);
            enemyInstance.transform.position = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
            RandomizePosition(enemyInstance.transform);
        }

        int gallosToSpawn = 0;

        if (round % bossWaves == 0)
        {
            for (int i = 0; i < bossesToSpawn; i++)
            {
                GameObject bossInstance = enemyPool.GetBoss();
                bossInstance.GetComponent<EnemyController>().SetWaveManager(this);
                bossInstance.transform.position = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
                RandomizePosition(bossInstance.transform);
            }

            gallosToSpawn = (int)(bossesToSpawn / 3);
            for (int i = 0; i < gallosToSpawn; i++)
            {
                GameObject galloInstance = enemyPool.GetGallo();
                galloInstance.GetComponent<EnemyController>().SetWaveManager(this);
                galloInstance.transform.position = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
                RandomizePosition(galloInstance.transform);
            }
        }

        if (gallosToSpawn > 0)
        {
            if (musicSource.clip != bossMusic)
            {
                musicSource.clip = bossMusic;
                musicSource.Play();
            }
        }
        else
        {
            if (musicSource.clip != waveMusic)
            {
                musicSource.clip = waveMusic;
                musicSource.Play();
            }
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

        if (round == bossWaveThreshold)
        {
            return;
        }

        StartCoroutine(TimeBetweenWaves());
    }

    void RandomizePosition(Transform transform)
    {
        float randomX = Random.Range(0, 3f);
        float randomY = Random.Range(0, 3f);

        transform.position = new Vector2(transform.position.x + randomX, transform.position.y + randomY);
    }

    private void PlayHolyCowSound(AudioSource source)
    {
        if (source != null && source.clip != null)
        {
            source.PlayOneShot(source.clip);
        }
    }
}
