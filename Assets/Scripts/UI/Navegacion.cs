using UnityEngine;
using UnityEngine.SceneManagement;

public class Navegacion : MonoBehaviour
{
    public void IrA(string escena)
    {
        SceneManager.LoadScene(escena);
    }
}
