using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Fila de leaderboard (prefab ItemLeaderboard).
public class ItemLeaderboard : MonoBehaviour
{
    [SerializeField] private Image fondo;
    [SerializeField] private TMP_Text textoPosicion;
    [SerializeField] private TMP_Text textoJugador;
    [SerializeField] private TMP_Text textoPuntuacion;
    [SerializeField] private TMP_Text textoAciertos;
    [SerializeField] private TMP_Text textoTiempo;

    private static readonly Color Oro = new Color(1f, 0.82f, 0.2f);
    private static readonly Color Plata = new Color(0.85f, 0.85f, 0.9f);
    private static readonly Color Bronce = new Color(0.86f, 0.55f, 0.3f);

    public void Configurar(int posicion, EntradaLeaderboard e, bool resaltar)
    {
        textoPosicion.text = "#" + posicion;
        textoJugador.text = e.jugador;
        textoPuntuacion.text = e.puntuacion.ToString();
        textoAciertos.text = e.aciertos + "/" + e.totalPreguntas;
        textoTiempo.text = e.tiempo.ToString("0.0") + " s";

        textoPosicion.color = posicion == 1 ? Oro : posicion == 2 ? Plata : posicion == 3 ? Bronce : Color.white;

        if (resaltar)
            fondo.color = new Color(0.85f, 0.62f, 0f, 1f); // amarillo Kahoot = "esta es tu partida"
        else
            fondo.color = posicion % 2 == 0 ? new Color(0.29f, 0.05f, 0.29f, 1f) : new Color(0.37f, 0.047f, 0.36f, 1f);
    }
}
