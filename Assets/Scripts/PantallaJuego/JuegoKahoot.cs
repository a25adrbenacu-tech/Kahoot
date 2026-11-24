using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class JuegoKahoot : MonoBehaviour
{
    [SerializeField] private TMP_Text textoPregunta;
    [SerializeField] private TMP_Text textoProgreso;
    [SerializeField] private TMP_Text textoTiempo;
    [SerializeField] private Image anilloTiempo;
    [SerializeField] private TMP_Text textoPuntuacion;
    [SerializeField] private GameObject panelFeedback;
    [SerializeField] private Image fondoFeedback;
    [SerializeField] private TMP_Text textoFeedback;
    [SerializeField] private BotonRespuesta[] botones;
    [SerializeField] private float segundosRevelando = 2.5f;

    private crearKahoot.preguntasKahoot kahoot;
    private int respuestaElegida = -1;
    private bool esperandoRespuesta = false;
    private int puntuacion = 0;
    private int aciertos = 0;
    private float tiempoTotal = 0;

    void Start()
    {
        if (KahootSeleccionado.kahoot == null)
        {
            SceneManager.LoadScene("SelectorKahoot");
            return;
        }

        kahoot = KahootSeleccionado.kahoot.datos;

        for (int i = 0; i < botones.Length; i++)
            botones[i].Inicializar(i, responder);

        StartCoroutine(partida());
    }

    private IEnumerator partida()
    {
        for (int i = 0; i < kahoot.questions.Count; i++)
        {
            yield return StartCoroutine(jugarPregunta(i));
        }

        terminarPartida();
    }

    private IEnumerator jugarPregunta(int numPregunta)
    {
        crearKahoot.Questions pregunta = kahoot.questions[numPregunta];

        panelFeedback.SetActive(false);
        textoPregunta.text = pregunta.question;
        textoProgreso.text = "Question " + (numPregunta + 1) + " / " + kahoot.questions.Count;

        for (int i = 0; i < botones.Length; i++)
            botones[i].Configurar(pregunta.answers[i]);

        respuestaElegida = -1;
        esperandoRespuesta = true;
        float tiempoRestante = pregunta.duration;

        while (esperandoRespuesta && tiempoRestante > 0)
        {
            tiempoRestante -= Time.deltaTime;
            if (tiempoRestante < 0)
                tiempoRestante = 0;

            textoTiempo.text = Mathf.CeilToInt(tiempoRestante).ToString();
            anilloTiempo.fillAmount = tiempoRestante / pregunta.duration;

            if (tiempoRestante <= 5)
                anilloTiempo.color = new Color(0.89f, 0.11f, 0.24f);
            else
                anilloTiempo.color = Color.white;

            yield return null;
        }
        esperandoRespuesta = false;

        float tiempoUsado = pregunta.duration - tiempoRestante;
        tiempoTotal += tiempoUsado;

        int puntos = 0;
        if (respuestaElegida == pregunta.correctAnswer)
        {
            puntos = Mathf.RoundToInt(1000 * (1 - (tiempoUsado / pregunta.duration) / 2));
            aciertos++;
        }
        puntuacion += puntos;
        textoPuntuacion.text = puntuacion.ToString();

        for (int i = 0; i < botones.Length; i++)
            botones[i].MostrarResultado(i == pregunta.correctAnswer, i == respuestaElegida);

        if (respuestaElegida == -1)
            mostrarFeedback("Time's up!", new Color(0.25f, 0.25f, 0.3f));
        else if (respuestaElegida == pregunta.correctAnswer)
            mostrarFeedback("Correct!  +" + puntos, new Color(0.15f, 0.54f, 0.05f));
        else
            mostrarFeedback("Incorrect", new Color(0.89f, 0.11f, 0.24f));

        yield return new WaitForSeconds(segundosRevelando);
    }

    private void responder(int respuesta)
    {
        if (!esperandoRespuesta)
            return;

        respuestaElegida = respuesta;
        esperandoRespuesta = false;

        for (int i = 0; i < botones.Length; i++)
            botones[i].Bloquear();
    }

    private void mostrarFeedback(string texto, Color color)
    {
        textoFeedback.text = texto;
        fondoFeedback.color = color;
        panelFeedback.SetActive(true);
    }

    private void terminarPartida()
    {
        EntradaLeaderboard partida = new EntradaLeaderboard();
        partida.id = System.Guid.NewGuid().ToString();
        partida.jugador = PlayerPrefs.GetString("usuario", "Player");
        partida.puntuacion = puntuacion;
        partida.aciertos = aciertos;
        partida.totalPreguntas = kahoot.questions.Count;
        partida.tiempo = Mathf.Round(tiempoTotal * 10) / 10;
        partida.fecha = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");

        KahootSeleccionado.ultimaPartida = partida;
        KahootSeleccionado.posicion = Leaderboard.guardarPartida(KahootSeleccionado.kahoot.id, partida);

        SceneManager.LoadScene("Resultados");
    }
}
