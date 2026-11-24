using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BotonRespuesta : MonoBehaviour
{
    [SerializeField] private Button boton;
    [SerializeField] private TMP_Text texto;
    [SerializeField] private Image iconoResultado;
    [SerializeField] private CanvasGroup grupo;
    [SerializeField] private Sprite spriteCorrecto;
    [SerializeField] private Sprite spriteIncorrecto;

    private int numero;
    private bool visible;

    public void Inicializar(int numeroRespuesta, System.Action<int> alPulsar)
    {
        numero = numeroRespuesta;
        boton.onClick.RemoveAllListeners();
        boton.onClick.AddListener(() => alPulsar(numero));
    }

    public void Configurar(string respuesta)
    {
        visible = respuesta != null && respuesta.Trim() != "";

        if (visible)
        {
            texto.text = respuesta;
            grupo.alpha = 1f;
        }
        else
        {
            texto.text = "";
            grupo.alpha = 0f;
        }

        grupo.blocksRaycasts = visible;
        boton.interactable = visible;
        iconoResultado.gameObject.SetActive(false);
        transform.localScale = Vector3.one;
    }

    public void Bloquear()
    {
        boton.interactable = false;
    }

    public void MostrarResultado(bool esCorrecta, bool laHeElegido)
    {
        if (!visible)
            return;

        boton.interactable = false;

        if (esCorrecta)
        {
            grupo.alpha = 1f;
            iconoResultado.sprite = spriteCorrecto;
            iconoResultado.gameObject.SetActive(true);
            transform.localScale = new Vector3(1.03f, 1.03f, 1f);
        }
        else
        {
            if (laHeElegido)
                grupo.alpha = 0.75f;
            else
                grupo.alpha = 0.3f;

            iconoResultado.sprite = spriteIncorrecto;
            iconoResultado.gameObject.SetActive(laHeElegido);
        }
    }
}
