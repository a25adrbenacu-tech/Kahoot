using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Escena Juego: muestra las preguntas en secuencia con tiempo límite y puntúa según el tiempo restante.
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

    private static readonly Color Verde = new Color(0.15f, 0.54f, 0.05f);
    private static readonly Color Rojo = new Color(0.89f, 0.11f, 0.24f);
    private static readonly Color Gris = new Color(0.25f, 0.25f, 0.3f);

    private KahootInfo kahoot;
    private int respuestaElegida = -1;
    private bool esperandoRespuesta;
    private int puntuacion;
    private int aciertos;
    private float tiempoTotal;

    void Start()
    {
        kahoot = Sesion.KahootSeleccionado;
        if (kahoot == null || kahoot.NumPreguntas == 0)
        {
            // Se ha abierto la escena sin elegir kahoot: volvemos al selector en vez de romper.
            SceneManager.LoadScene(Escenas.Selector);
            return;
        }

        for (int i = 0; i < botones.Length; i++)
            botones[i].Inicializar(i, Responder);

        StartCoroutine(Partida());
    }

    private IEnumerator Partida()
    {
        for (int i = 0; i < kahoot.NumPreguntas; i++)
            yield return JugarPregunta(i, kahoot.datos.questions[i]);

        FinDePartida();
    }

    private IEnumerator JugarPregunta(int indice, Pregunta p)
    {
        panelFeedback.SetActive(false);
        textoPregunta.text = p.question;
        textoProgreso.text = "Question " + (indice + 1) + " / " + kahoot.NumPreguntas;
        for (int i = 0; i < botones.Length; i++)
            botones[i].Configurar(i < p.answers.Length ? p.answers[i] : "");

        respuestaElegida = -1;
        esperandoRespuesta = true;
        float restante = p.duration;

        while (esperandoRespuesta && restante > 0f)
        {
            restante -= Time.deltaTime;
            ActualizarTiempo(Mathf.Max(restante, 0f), p.duration);
            yield return null;
        }
        esperandoRespuesta = false;

        float usado = Mathf.Clamp(p.duration - restante, 0f, p.duration);
        tiempoTotal += usado;

        bool correcta = respuestaElegida == p.correctAnswer;
        int puntos = Puntuacion.Calcular(correcta, usado, p.duration);
        puntuacion += puntos;
        if (correcta) aciertos++;
        textoPuntuacion.text = puntuacion.ToString();

        for (int i = 0; i < botones.Length; i++)
            botones[i].MostrarResultado(i == p.correctAnswer, i == respuestaElegida);

        if (respuestaElegida < 0) MostrarFeedback("Time's up!", Gris);
        else if (correcta) MostrarFeedback("Correct!  +" + puntos, Verde);
        else MostrarFeedback("Incorrect", Rojo);

        yield return new WaitForSeconds(segundosRevelando);
    }

    private void Responder(int indice)
    {
        if (!esperandoRespuesta) return;
        respuestaElegida = indice;
        esperandoRespuesta = false;
        foreach (BotonRespuesta b in botones) b.Bloquear();
    }

    private void ActualizarTiempo(float restante, float duracion)
    {
        textoTiempo.text = Mathf.CeilToInt(restante).ToString();
        anilloTiempo.fillAmount = duracion > 0f ? restante / duracion : 0f;
        anilloTiempo.color = restante <= 5f ? Rojo : Color.white; // aviso cuando queda poco
    }

    private void MostrarFeedback(string texto, Color color)
    {
        textoFeedback.text = texto;
        fondoFeedback.color = color;
        panelFeedback.SetActive(true);
    }

    private void FinDePartida()
    {
        var entrada = new EntradaLeaderboard
        {
            jugador = string.IsNullOrWhiteSpace(Sesion.Usuario) ? "Player" : Sesion.Usuario,
            puntuacion = puntuacion,
            aciertos = aciertos,
            totalPreguntas = kahoot.NumPreguntas,
            tiempo = Mathf.Round(tiempoTotal * 10f) / 10f,
            fecha = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
        };

        Sesion.UltimaEntrada = entrada;
        Sesion.UltimaPosicion = LeaderboardRepository.Agregar(kahoot.id, entrada);
        SceneManager.LoadScene(Escenas.Resultados);
    }
}
