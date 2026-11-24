using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class KahootCargado
{
    public string ruta;
    public string id;
    public crearKahoot.preguntasKahoot datos;
}

public static class KahootSeleccionado
{
    public static KahootCargado kahoot;
    public static EntradaLeaderboard ultimaPartida;
    public static int posicion;
}

public class ListaKahoots : MonoBehaviour
{
    [SerializeField] private Transform contenido;
    [SerializeField] private ItemKahoot prefabItem;
    [SerializeField] private TMP_InputField inputUsuario;
    [SerializeField] private TMP_Text textoAviso;
    [SerializeField] private TMP_Text textoVacio;
    [SerializeField] private TMP_Text textoRuta;

    private static bool defaultsCopiados = false;

    void Start()
    {
        inputUsuario.characterLimit = 20;
        inputUsuario.text = PlayerPrefs.GetString("usuario", "");
        inputUsuario.onValueChanged.AddListener(borrarAviso);
        textoRuta.text = "Folder: " + rutaKahoots();
        CargarLista();
    }

    public void CargarLista()
    {
        foreach (Transform hijo in contenido)
        {
            hijo.gameObject.SetActive(false);
            Destroy(hijo.gameObject);
        }

        List<KahootCargado> kahoots = leerKahoots();

        foreach (KahootCargado kahoot in kahoots)
        {
            ItemKahoot item = Instantiate(prefabItem, contenido);
            item.name = "Item_" + kahoot.id;
            item.Configurar(kahoot, "Play", seleccionar);
        }

        textoVacio.gameObject.SetActive(kahoots.Count == 0);
    }

    private void seleccionar(KahootCargado kahoot)
    {
        string usuario = inputUsuario.text.Trim();

        if (usuario == "")
        {
            textoAviso.text = "Write your username before playing";
            textoAviso.color = new Color(1f, 0.45f, 0.5f);
            inputUsuario.Select();
            return;
        }

        PlayerPrefs.SetString("usuario", usuario);
        KahootSeleccionado.kahoot = kahoot;
        SceneManager.LoadScene("Juego");
    }

    private void borrarAviso(string texto)
    {
        textoAviso.text = "";
    }

    public static string rutaKahoots()
    {
        return Path.Combine(Application.persistentDataPath, "Kahoots");
    }

    public static List<KahootCargado> leerKahoots()
    {
        List<KahootCargado> lista = new List<KahootCargado>();
        string[] archivos;

        try
        {
            Directory.CreateDirectory(rutaKahoots());
            copiarKahootsDefault();
            archivos = Directory.GetFiles(rutaKahoots(), "*.json");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("No se puede leer la carpeta de kahoots: " + e.Message);
            return lista;
        }

        System.Array.Sort(archivos);

        foreach (string ruta in archivos)
        {
            crearKahoot.preguntasKahoot kahoot;
            try
            {
                kahoot = JsonUtility.FromJson<crearKahoot.preguntasKahoot>(File.ReadAllText(ruta));
            }
            catch
            {
                Debug.LogWarning("JSON no valido, se ignora: " + ruta);
                continue;
            }

            if (kahoot == null || kahoot.questions == null || kahoot.questions.Count == 0)
                continue;

            arreglarPreguntas(kahoot);

            KahootCargado cargado = new KahootCargado();
            cargado.ruta = ruta;
            cargado.id = Path.GetFileNameWithoutExtension(ruta);
            cargado.datos = kahoot;

            if (string.IsNullOrEmpty(kahoot.nombre))
                kahoot.nombre = cargado.id;

            lista.Add(cargado);
        }

        return lista;
    }

    private static void arreglarPreguntas(crearKahoot.preguntasKahoot kahoot)
    {
        foreach (crearKahoot.Questions pregunta in kahoot.questions)
        {
            if (pregunta.question == null)
                pregunta.question = "";

            string[] respuestas = new string[4];
            for (int i = 0; i < 4; i++)
            {
                if (pregunta.answers != null && i < pregunta.answers.Length && pregunta.answers[i] != null)
                    respuestas[i] = pregunta.answers[i];
                else
                    respuestas[i] = "";
            }
            pregunta.answers = respuestas;

            if (pregunta.duration <= 0)
                pregunta.duration = 20;

            if (pregunta.correctAnswer < 0 || pregunta.correctAnswer > 3)
                pregunta.correctAnswer = 0;
        }
    }

    private static void copiarKahootsDefault()
    {
        if (defaultsCopiados)
            return;
        defaultsCopiados = true;

        string carpetaDefault = Path.Combine(Application.streamingAssetsPath, "Kahoots");
        if (!Directory.Exists(carpetaDefault))
            return;

        foreach (string archivo in Directory.GetFiles(carpetaDefault, "*.json"))
        {
            string destino = Path.Combine(rutaKahoots(), Path.GetFileName(archivo));
            if (!File.Exists(destino))
                File.Copy(archivo, destino);
        }
    }
}
