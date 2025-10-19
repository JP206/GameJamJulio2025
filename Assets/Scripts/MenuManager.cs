using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void Jugar()
    {
        SceneManager.LoadScene("Loading");
    }
}
