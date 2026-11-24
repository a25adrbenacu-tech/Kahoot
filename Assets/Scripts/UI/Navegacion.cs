using UnityEngine;
using UnityEngine.SceneManagement;

// Componente genérico para botones de navegación (se enlaza desde el Inspector).
public class Navegacion : MonoBehaviour
{
    public void IrA(string escena)
    {
        SceneManager.LoadScene(escena);
    }

    public void Salir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
