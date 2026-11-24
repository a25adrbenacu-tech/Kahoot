using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Escena Resultados: resumen de la partida + posición en la leaderboard de ese kahoot.
public class PantallaResultados : MonoBehaviour
{
    [SerializeField] private TMP_Text textoKahoot;
    [SerializeField] private TMP_Text textoJugador;
    [SerializeField] private TMP_Text textoPuntuacion;
    [SerializeField] private TMP_Text textoAciertos;
    [SerializeField] private TMP_Text textoTiempo;
    [SerializeField] private TMP_Text textoPosicion;
    [SerializeField] private VistaLeaderboard vistaLeaderboard;

    void Start()
    {
        KahootInfo kahoot = Sesion.KahootSeleccionado;
        EntradaLeaderboard e = Sesion.UltimaEntrada;
        if (kahoot == null || e == null)
        {
            SceneManager.LoadScene(Escenas.Titulo);
            return;
        }

        Leaderboard lb = LeaderboardRepository.Cargar(kahoot.id);

        textoKahoot.text = kahoot.Nombre;
        textoJugador.text = e.jugador;
        textoPuntuacion.text = e.puntuacion + " pts";
        textoAciertos.text = "Correct answers: " + e.aciertos + " / " + e.totalPreguntas;
        textoTiempo.text = "Time: " + e.tiempo.ToString("0.0") + " s";
        textoPosicion.text = "Position #" + Sesion.UltimaPosicion + " of " + Mathf.Max(lb.entradas.Count, 1);

        vistaLeaderboard.Mostrar(lb, e.id);
    }

    public void JugarOtraVez() => SceneManager.LoadScene(Escenas.Juego);
}
