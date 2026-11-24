using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        if (KahootSeleccionado.kahoot == null || KahootSeleccionado.ultimaPartida == null)
        {
            SceneManager.LoadScene("PantallaTitulo");
            return;
        }

        EntradaLeaderboard partida = KahootSeleccionado.ultimaPartida;
        Leaderboard leaderboard = Leaderboard.cargar(KahootSeleccionado.kahoot.id);

        textoKahoot.text = KahootSeleccionado.kahoot.datos.nombre;
        textoJugador.text = partida.jugador;
        textoPuntuacion.text = partida.puntuacion + " pts";
        textoAciertos.text = "Correct answers: " + partida.aciertos + " / " + partida.totalPreguntas;
        textoTiempo.text = "Time: " + partida.tiempo.ToString("0.0") + " s";
        textoPosicion.text = "Position #" + KahootSeleccionado.posicion + " of " + leaderboard.entradas.Count;

        vistaLeaderboard.Mostrar(leaderboard, partida.id);
    }

    public void JugarOtraVez()
    {
        SceneManager.LoadScene("Juego");
    }
}
