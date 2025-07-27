﻿using System.Collections;
using UnityEngine;

public class IntroCinematicManager : MonoBehaviour
{
    [Header("References")]
    public GameObject player;
    public GameObject waveManager;
    public GameObject ulCanvas;
    public Transform entryPoint;
    public Transform targetPoint;
    public GameObject cinematicCanvas;

    [Header("Cinematic Settings")]
    public float moveSpeed = 5f;
    public float delayBeforeEnable = 1f;

    private PlayerMovement playerMovement;

    void Start()
    {
        if (cinematicCanvas != null) cinematicCanvas.SetActive(true);

        waveManager.SetActive(false);
        ulCanvas.SetActive(false);

        playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement != null)
            playerMovement.enabled = false;

        player.transform.position = entryPoint.position;

        StartCoroutine(PlayCinematic());
    }

    IEnumerator PlayCinematic()
    {
        Animator animator = player.GetComponent<Animator>();
        if (animator != null)
            animator.SetBool("isRunning", true);

        yield return MoveToPosition(player.transform, targetPoint.position);

        if (animator != null)
            animator.SetBool("isRunning", false);

        yield return new WaitForSeconds(delayBeforeEnable);

        waveManager.SetActive(true);
        ulCanvas.SetActive(true);
        if (playerMovement != null)
            playerMovement.enabled = true;

        if (cinematicCanvas != null) cinematicCanvas.SetActive(false);

        Destroy(gameObject);
    }

    IEnumerator MoveToPosition(Transform obj, Vector3 targetPos)
    {
        while (Vector3.Distance(obj.position, targetPos) > 0.05f)
        {
            obj.position = Vector3.MoveTowards(obj.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }
}