using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class crearKahoot : MonoBehaviour
{
    [System.Serializable]
    public class Questions
    {
        public string question;
        public int correctAnswer;
        public string[] answers = new string[4];
        public float duration;
    }

    [System.Serializable]
    public class preguntasKahoot
    {
        public string nombre;
        public List<Questions> questions = new List<Questions>();
    }

    [SerializeField] private TMP_InputField inputNombre;
    [SerializeField] private Transform contenido;
    [SerializeField] private ItemPreguntaCreador prefabPregunta;
    [SerializeField] private ScrollRect scroll;
    [SerializeField] private TMP_Text textoFeedback;
    [SerializeField] private TMP_Text textoContador;
    [SerializeField] private Button botonGuardar;

    private string rutaKahootPrueba1;
    private string rutaimportarkahoots;
    private string nombrearchivo;
    private int contadorKahoots;
    private string nombreFinalArchivo;
    private List<ItemPreguntaCreador> preguntas = new List<ItemPreguntaCreador>();

    void Start()
    {
        rutaimportarkahoots = ListaKahoots.rutaKahoots();

        /**rutaKahootPrueba1 = Application.persistentDataPath + "/KahootPrueba1.json";
        
        Debug.Log(rutaKahootPrueba1);

        Questions firstQuestionKahootPrueba1 = new Questions();
        firstQuestionKahootPrueba1.question = "alo";
        firstQuestionKahootPrueba1.answers = new string[] { "elo", "pelo", "qwefwe", "nose" };
        firstQuestionKahootPrueba1.correctAnswer = 1;

        string Json = JsonUtility.ToJson(firstQuestionKahootPrueba1, true);
        File.WriteAllText(rutaKahootPrueba1, Json);
        Debug.Log(rutaKahootPrueba1);

        string conversion = File.ReadAllText(rutaKahootPrueba1);
        Questions pruebaCnvertirJsonAString = JsonUtility.FromJson<Questions>(conversion);
        Debug.Log(pruebaCnvertirJsonAString.question);
        Debug.Log(pruebaCnvertirJsonAString.correctAnswer);
        Debug.Log(pruebaCnvertirJsonAString.answers);**/

        foreach (Transform hijo in contenido)
        {
            hijo.gameObject.SetActive(false);
            Destroy(hijo.gameObject);
        }

        inputNombre.characterLimit = 40;
        inputNombre.onValueChanged.AddListener(borrarMensaje);
        AnadirPregunta();
        textoFeedback.text = "";
    }

    public void AnadirPregunta()
    {
        ItemPreguntaCreador nueva = Instantiate(prefabPregunta, contenido);
        nueva.Inicializar(borrarPregunta);
        preguntas.Add(nueva);
        actualizarNumeros();
        StartCoroutine(bajarScroll());
    }

    private void borrarPregunta(ItemPreguntaCreador pregunta)
    {
        if (preguntas.Count <= 1)
            return;

        preguntas.Remove(pregunta);
        pregunta.gameObject.SetActive(false);
        Destroy(pregunta.gameObject);
        actualizarNumeros();
    }

    private void actualizarNumeros()
    {
        for (int i = 0; i < preguntas.Count; i++)
        {
            preguntas[i].SetNumero(i + 1);
            preguntas[i].SetBorrable(preguntas.Count > 1);
        }

        if (preguntas.Count == 1)
            textoContador.text = "1 question";
        else
            textoContador.text = preguntas.Count + " questions";
    }

    public void Guardar()
    {
        preguntasKahoot kahootActual = new preguntasKahoot();
        kahootActual.nombre = inputNombre.text.Trim();

        bool[] duracionesBien = new bool[preguntas.Count];
        for (int i = 0; i < preguntas.Count; i++)
        {
            preguntas[i].MarcarError(false);
            kahootActual.questions.Add(preguntas[i].leerPregunta(out duracionesBien[i]));
        }

        int preguntaMal;
        string error = GuardarKahoot.comprobarKahoot(kahootActual, duracionesBien, out preguntaMal);

        if (error != "")
        {
            textoFeedback.text = error;
            textoFeedback.color = new Color(1f, 0.45f, 0.5f);

            if (preguntaMal >= 0)
            {
                preguntas[preguntaMal].MarcarError(true);
                scroll.verticalNormalizedPosition = 1f - (float)preguntaMal / Mathf.Max(1, preguntas.Count - 1);
            }
            else if (kahootActual.nombre == "")
            {
                inputNombre.Select();
            }
            return;
        }

        try
        {
            saveKahoot(kahootActual);
            textoFeedback.text = "Saved: " + nombrearchivo;
            textoFeedback.color = new Color(0.45f, 1f, 0.5f);
            botonGuardar.interactable = false;
            StartCoroutine(irAlSelector());
        }
        catch (System.Exception e)
        {
            textoFeedback.text = "Could not save the kahoot: " + e.Message;
            textoFeedback.color = new Color(1f, 0.45f, 0.5f);
        }
    }

    public void saveKahoot(preguntasKahoot kahootActual)
    {
        Directory.CreateDirectory(rutaimportarkahoots);

        string nombreLimpio = kahootActual.nombre;
        foreach (char c in Path.GetInvalidFileNameChars())
            nombreLimpio = nombreLimpio.Replace(c, '_');
        nombreLimpio = nombreLimpio.Replace(' ', '_');

        contadorKahoots = 0;
        nombrearchivo = nombreLimpio + ".json";
        nombreFinalArchivo = Path.Combine(rutaimportarkahoots, nombrearchivo);

        while (File.Exists(nombreFinalArchivo))
        {
            contadorKahoots++;
            nombrearchivo = nombreLimpio + "_" + contadorKahoots + ".json";
            nombreFinalArchivo = Path.Combine(rutaimportarkahoots, nombrearchivo);
        }

        string json = JsonUtility.ToJson(kahootActual, true);
        File.WriteAllText(nombreFinalArchivo, json);
    }

    private void borrarMensaje(string texto)
    {
        textoFeedback.text = "";
    }

    private IEnumerator bajarScroll()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        scroll.verticalNormalizedPosition = 0f;
    }

    private IEnumerator irAlSelector()
    {
        yield return new WaitForSeconds(1.2f);
        SceneManager.LoadScene("SelectorKahoot");
    }
}
