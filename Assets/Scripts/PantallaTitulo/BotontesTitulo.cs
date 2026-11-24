using UnityEngine;
using UnityEngine.SceneManagement;

public class BotontesTitulo : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void Jugar()
    {
        SceneManager.LoadScene("SelectorKahoot");
    }

    public void VerLeaderboards()
    {
        SceneManager.LoadScene("Leaderboards");
    }

    public void cambiarEscenaCreacionKahoot()
    {
        SceneManager.LoadScene("CreacionKahoot");
    }

    public void VerAbout()
    {
        SceneManager.LoadScene("About");
    }

    public void Salir()
    {
        Debug.Log("Salir del juego");
        Application.Quit();
    }
}
