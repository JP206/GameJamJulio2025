using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void Jugar()
    {
        // Cargar la escena de carga
        SceneManager.LoadScene("Loading");
    }
}
