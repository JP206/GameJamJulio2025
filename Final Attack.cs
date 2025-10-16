using System;
using UnityEngine;

public Animator animHolyShotExplode;
public Animator animHolyBlast;

void Start()
{
    StartCoroutine(Secuencia());
}

void StartCoroutine(IEnumerator enumerator)
{
    throw new NotImplementedException();
}

IEnumerator Secuencia()
{
    animJugador.Play("HolyShotExplode");
    yield return new WaitForSeconds(0.5f);
    animCubo.Play("HolyBlast");
}
