using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemLeaderboard : MonoBehaviour
{
    [SerializeField] private Image fondo;
    [SerializeField] private TMP_Text textoPosicion;
    [SerializeField] private TMP_Text textoJugador;
    [SerializeField] private TMP_Text textoPuntuacion;
    [SerializeField] private TMP_Text textoAciertos;
    [SerializeField] private TMP_Text textoTiempo;

    public void Configurar(int posicion, EntradaLeaderboard entrada, bool esLaMia)
    {
        textoPosicion.text = "#" + posicion;
        textoJugador.text = entrada.jugador;
        textoPuntuacion.text = entrada.puntuacion.ToString();
        textoAciertos.text = entrada.aciertos + "/" + entrada.totalPreguntas;
        textoTiempo.text = entrada.tiempo.ToString("0.0") + " s";

        if (posicion == 1)
            textoPosicion.color = new Color(1f, 0.82f, 0.2f);
        else if (posicion == 2)
            textoPosicion.color = new Color(0.85f, 0.85f, 0.9f);
        else if (posicion == 3)
            textoPosicion.color = new Color(0.86f, 0.55f, 0.3f);
        else
            textoPosicion.color = Color.white;

        if (esLaMia)
            fondo.color = new Color(0.85f, 0.62f, 0f);
        else if (posicion % 2 == 0)
            fondo.color = new Color(0.29f, 0.05f, 0.29f);
        else
            fondo.color = new Color(0.37f, 0.047f, 0.36f);
    }
}
