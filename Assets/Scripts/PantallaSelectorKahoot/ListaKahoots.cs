using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// Guarda el kahoot elegido para que lo lea la escena de juego
public static class KahootSeleccionado
{
    public static string ruta;
    public static crearKahoot.preguntasKahoot datos;
}

public class ListaKahoots : MonoBehaviour
{
    [SerializeField] private Transform contenido;     // objeto "Contenido" del ScrollReckt
    [SerializeField] private Button prefabBoton;       // prefab: UI > Button - TextMeshPro
    [SerializeField] private string escenaJuego = "";  // escena a cargar al pulsar (vacío = solo Debug.Log)

    void Start()
    {
        CargarLista();
    }

    public void CargarLista()
    {
        // Borra lo que haya (los textos de prueba o una lista anterior)
        foreach (Transform hijo in contenido)
            Destroy(hijo.gameObject);

        string[] archivos = Directory.GetFiles(Application.persistentDataPath, "*.json");

        foreach (string ruta in archivos)
        {
            crearKahoot.preguntasKahoot kahoot;
            try
            {
                kahoot = JsonUtility.FromJson<crearKahoot.preguntasKahoot>(File.ReadAllText(ruta));
            }
            catch
            {
                Debug.LogWarning("JSON no válido, se ignora: " + ruta);
                continue;
            }
            if (kahoot == null || kahoot.questions == null || kahoot.questions.Count == 0)
                continue; // no es un kahoot (p.ej. KahootPrueba1.json antiguo)

            string nombre = string.IsNullOrEmpty(kahoot.nombre)
                ? Path.GetFileNameWithoutExtension(ruta)
                : kahoot.nombre;

            Button boton = Instantiate(prefabBoton, contenido);
            boton.name = "Boton_" + nombre;
            boton.GetComponentInChildren<TMP_Text>().text = nombre + "  (" + kahoot.questions.Count + " preguntas)";

            string rutaBoton = ruta;
            crearKahoot.preguntasKahoot datosBoton = kahoot;
            boton.onClick.AddListener(() => Seleccionar(rutaBoton, datosBoton));
        }
    }

    private void Seleccionar(string ruta, crearKahoot.preguntasKahoot datos)
    {
        KahootSeleccionado.ruta = ruta;
        KahootSeleccionado.datos = datos;
        Debug.Log("Kahoot elegido: " + ruta);

        if (!string.IsNullOrEmpty(escenaJuego))
            SceneManager.LoadScene(escenaJuego);
    }
}
