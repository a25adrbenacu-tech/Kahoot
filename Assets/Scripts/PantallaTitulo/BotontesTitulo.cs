using UnityEngine;
using UnityEngine.SceneManagement;

// Menú principal: navegación a Quizzes, Leaderboards, Creador y About.
public class BotontesTitulo : MonoBehaviour
{
    public void Jugar() => SceneManager.LoadScene(Escenas.Selector);
    public void VerLeaderboards() => SceneManager.LoadScene(Escenas.Leaderboards);
    public void cambiarEscenaCreacionKahoot() => SceneManager.LoadScene(Escenas.Creador);
    public void VerAbout() => SceneManager.LoadScene(Escenas.About);

    public void Salir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
