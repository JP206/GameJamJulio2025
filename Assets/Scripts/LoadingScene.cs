using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LoadingScene : MonoBehaviour
{
    public string nombreEscenaJuego = "Nivel1"; // La escena a la que irá después
    public Slider barraProgreso; // Opcional, si quieres una barra

    void Start()
    {
        StartCoroutine(CargarJuego());
    }

    IEnumerator CargarJuego()
    {
        AsyncOperation operacion = SceneManager.LoadSceneAsync(nombreEscenaJuego);
        operacion.allowSceneActivation = false;

        // Mientras carga, actualiza la barra (si existe)
        while (!operacion.isDone)
        {
            float progreso = Mathf.Clamp01(operacion.progress / 0.9f);

            if (barraProgreso != null)
                barraProgreso.value = progreso;

            // Cuando termine de cargar (progress >= 0.9)
            if (operacion.progress >= 0.9f)
            {
                yield return new WaitForSeconds(8f); // pequeño delay opcional
                operacion.allowSceneActivation = true; // activa la escena del juego
            }

            yield return null;
        }
    }
}

