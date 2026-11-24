using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Escena CreacionKahoot: número variable de preguntas, cada una con enunciado, 4 respuestas,
// respuesta correcta y duración. Al guardar se escribe el .json en la carpeta de lectura dinámica.
public class crearKahoot : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputNombre;
    [SerializeField] private Transform contenido;            // Content del ScrollView
    [SerializeField] private ItemPreguntaCreador prefabPregunta;
    [SerializeField] private ScrollRect scroll;
    [SerializeField] private TMP_Text textoFeedback;
    [SerializeField] private TMP_Text textoContador;
    [SerializeField] private Button botonGuardar;

    private readonly List<ItemPreguntaCreador> preguntas = new List<ItemPreguntaCreador>();

    private static readonly Color ColorError = new Color(1f, 0.45f, 0.5f);
    private static readonly Color ColorOk = new Color(0.45f, 1f, 0.5f);

    void Start()
    {
        foreach (Transform hijo in contenido)
            { hijo.gameObject.SetActive(false); Destroy(hijo.gameObject); }

        inputNombre.characterLimit = 40;
        inputNombre.onValueChanged.AddListener(_ => Feedback("", Color.white));
        AnadirPregunta();
        Feedback("", Color.white);
    }

    public void AnadirPregunta()
    {
        ItemPreguntaCreador item = Instantiate(prefabPregunta, contenido);
        item.Inicializar(BorrarPregunta);
        preguntas.Add(item);
        Renumerar();
        StartCoroutine(ScrollAl(0f)); // baja hasta la nueva pregunta
    }

    private void BorrarPregunta(ItemPreguntaCreador item)
    {
        if (preguntas.Count <= 1) return;
        preguntas.Remove(item);
        item.gameObject.SetActive(false);
        Destroy(item.gameObject);
        Renumerar();
    }

    private void Renumerar()
    {
        for (int i = 0; i < preguntas.Count; i++)
        {
            preguntas[i].SetNumero(i + 1);
            preguntas[i].SetBorrable(preguntas.Count > 1);
        }
        textoContador.text = preguntas.Count + (preguntas.Count == 1 ? " question" : " questions");
    }

    public void Guardar()
    {
        var kahoot = new Kahoot { nombre = inputNombre.text.Trim() };
        var duracionesValidas = new bool[preguntas.Count];
        for (int i = 0; i < preguntas.Count; i++)
        {
            preguntas[i].MarcarError(false);
            kahoot.questions.Add(preguntas[i].Leer(out duracionesValidas[i]));
        }

        string error = GuardarKahoot.Validar(kahoot, duracionesValidas, out int indiceError);
        if (error != null)
        {
            Feedback(error, ColorError);
            if (indiceError >= 0)
            {
                preguntas[indiceError].MarcarError(true);
                StartCoroutine(ScrollA(indiceError));
            }
            else if (string.IsNullOrWhiteSpace(kahoot.nombre))
            {
                inputNombre.Select();
            }
            return;
        }

        try
        {
            string ruta = KahootRepository.Guardar(kahoot);
            Feedback("Saved: " + System.IO.Path.GetFileName(ruta), ColorOk);
            botonGuardar.interactable = false;
            StartCoroutine(IrAlSelector());
        }
        catch (System.Exception e)
        {
            Feedback("Could not save the kahoot: " + e.Message, ColorError);
        }
    }

    private IEnumerator IrAlSelector()
    {
        yield return new WaitForSeconds(1.2f);
        SceneManager.LoadScene(Escenas.Selector); // el kahoot nuevo ya aparece en la lista
    }

    private IEnumerator ScrollAl(float posicion)
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        scroll.verticalNormalizedPosition = posicion;
    }

    private IEnumerator ScrollA(int indice)
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        float t = preguntas.Count <= 1 ? 1f : 1f - (float)indice / (preguntas.Count - 1);
        scroll.verticalNormalizedPosition = t;
    }

    private void Feedback(string texto, Color color)
    {
        textoFeedback.text = texto;
        textoFeedback.color = color;
    }
}
