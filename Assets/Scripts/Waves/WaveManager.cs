using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WaveManager : MonoBehaviour
{
    [Header("Spawn & References")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private BoxCollider2D topBound;
    [SerializeField] private BoxCollider2D bottomBound;
    [SerializeField] private BoxCollider2D leftBound;
    [SerializeField] private BoxCollider2D rightBound;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private TextMeshProUGUI roundTextShade;
    [SerializeField] private TextMeshProUGUI killCountText;
    [SerializeField] private TextMeshProUGUI killCountTextShade;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource holyCowSource;
    [SerializeField] private AudioClip waveMusic;
    [SerializeField] private AudioClip bossMusic;

    [Header("Wave Settings")]
    [SerializeField] private float waveTimeout = 240f;
    [SerializeField] private int bossWaveThreshold = 5;

    private EnemyPool enemyPool;
    private UIAnimation uiAnimation;

    private int round = 1;
    private int enemiesToSpawn = 5;
    private int bossesToSpawn = 1;
    private int deadEnemies = 0;
    private int bossWaves = 3;
    private int killCount = 0;
    private bool waveEnding = false;

    private void Start()
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

    private IEnumerator TimeBetweenWaves()
    {
        ScaleDifficulty();
        yield return new WaitForSeconds(3);
        StartWave();
    }

    private void ScaleDifficulty()
    {
        enemiesToSpawn += 3;
        round++;

        if (round % bossWaves == 0)
        {
            bossesToSpawn++;
        }
    }

    private void StartWave()
    {
        deadEnemies = 0;
        waveEnding = false;

        roundText.text = "Round: " + round.ToString();
        roundTextShade.text = roundText.text;
        uiAnimation.Animation(roundText, roundTextShade);

        // --- Cinemática del jefe ---
        if (round == bossWaveThreshold)
        {
            var doorCinematic = FindAnyObjectByType<FinalDoorCinematic>();
            if (doorCinematic != null)
            {
                doorCinematic.PlayCinematicToDoor();
            }
            return;
        }

        // --- Spawnear enemigos normales ---
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            GameObject enemyInstance = enemyPool.GetEnemy();
            enemyInstance.GetComponent<EnemyController>().SetWaveManager(this);

            Vector2 spawnPos = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
            spawnPos = RandomizePosition(spawnPos);
            spawnPos = GetValidSpawnPosition(spawnPos);

            enemyInstance.transform.position = spawnPos;
        }

        int gallosToSpawn = 0;

        // --- Spawnear bosses y gallos ---
        if (round % bossWaves == 0)
        {
            for (int i = 0; i < bossesToSpawn; i++)
            {
                GameObject bossInstance = enemyPool.GetBoss();
                bossInstance.GetComponent<EnemyController>().SetWaveManager(this);

                Vector2 spawnPos = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
                spawnPos = RandomizePosition(spawnPos);
                spawnPos = GetValidSpawnPosition(spawnPos);

                bossInstance.transform.position = spawnPos;
            }

            gallosToSpawn = (int)(bossesToSpawn / 3);
            for (int i = 0; i < gallosToSpawn; i++)
            {
                GameObject galloInstance = enemyPool.GetGallo();
                galloInstance.GetComponent<EnemyController>().SetWaveManager(this);

                Vector2 spawnPos = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
                spawnPos = RandomizePosition(spawnPos);
                spawnPos = GetValidSpawnPosition(spawnPos);

                galloInstance.transform.position = spawnPos;
            }
        }

        // --- Música dinámica ---
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

    private IEnumerator WaveTimer()
    {
        yield return new WaitForSeconds(waveTimeout);

        if (!waveEnding)
        {
            EndWave();
        }
    }

    private void EndWave()
    {
        waveEnding = true;
        audioSource.Stop();

        if (round == bossWaveThreshold)
        {
            return;
        }

        StartCoroutine(TimeBetweenWaves());
    }

    // --- Generar pequeña variación de posición ---
    private Vector2 RandomizePosition(Vector2 position)
    {
        float randomX = Random.Range(0f, 3f);
        float randomY = Random.Range(0f, 3f);
        return new Vector2(position.x + randomX, position.y + randomY);
    }

    // --- Corregir posición si está fuera del área de juego ---
    private Vector2 GetValidSpawnPosition(Vector2 proposedPosition)
    {
        Vector2 corrected = proposedPosition;
        float margin = 1.5f; // distancia desde el borde interior

        if (rightBound && proposedPosition.x > rightBound.bounds.min.x)
            corrected.x = rightBound.bounds.min.x - margin;

        if (leftBound && proposedPosition.x < leftBound.bounds.max.x)
            corrected.x = leftBound.bounds.max.x + margin;

        if (topBound && proposedPosition.y > topBound.bounds.min.y)
            corrected.y = topBound.bounds.min.y - margin;

        if (bottomBound && proposedPosition.y < bottomBound.bounds.max.y)
            corrected.y = bottomBound.bounds.max.y + margin;

        return corrected;
    }

    private void PlayHolyCowSound(AudioSource source)
    {
        if (source != null && source.clip != null)
        {
            source.PlayOneShot(source.clip);
        }
    }
}
