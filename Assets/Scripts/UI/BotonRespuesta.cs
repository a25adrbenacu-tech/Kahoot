using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Botón de respuesta del juego con el estilo de Kahoot (color + forma).
public class BotonRespuesta : MonoBehaviour
{
    [SerializeField] private Button boton;
    [SerializeField] private TMP_Text texto;
    [SerializeField] private Image iconoResultado;
    [SerializeField] private CanvasGroup grupo;
    [SerializeField] private Sprite spriteCorrecto;
    [SerializeField] private Sprite spriteIncorrecto;

    public int Indice { get; private set; }
    public bool Visible { get; private set; }

    public void Inicializar(int indice, Action<int> alPulsar)
    {
        Indice = indice;
        boton.onClick.RemoveAllListeners();
        boton.onClick.AddListener(() => alPulsar(Indice));
    }

    // Las respuestas vacías no se muestran, pero el hueco se mantiene (como en Kahoot).
    public void Configurar(string respuesta)
    {
        Visible = !string.IsNullOrWhiteSpace(respuesta);
        texto.text = Visible ? respuesta : "";
        grupo.alpha = Visible ? 1f : 0f;
        grupo.blocksRaycasts = Visible;
        boton.interactable = Visible;
        iconoResultado.gameObject.SetActive(false);
        transform.localScale = Vector3.one;
    }

    public void Bloquear()
    {
        boton.interactable = false;
    }

    public void MostrarResultado(bool esCorrecta, bool fueElegida)
    {
        if (!Visible) return;
        boton.interactable = false;

        if (esCorrecta)
        {
            grupo.alpha = 1f;
            iconoResultado.sprite = spriteCorrecto;
            iconoResultado.gameObject.SetActive(true);
            transform.localScale = Vector3.one * 1.03f;
        }
        else
        {
            grupo.alpha = fueElegida ? 0.75f : 0.3f;
            iconoResultado.sprite = spriteIncorrecto;
            iconoResultado.gameObject.SetActive(fueElegida);
        }
    }
}
