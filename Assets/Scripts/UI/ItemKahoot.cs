using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemKahoot : MonoBehaviour
{
    [SerializeField] private TMP_Text textoNombre;
    [SerializeField] private TMP_Text textoInfo;
    [SerializeField] private Button boton;
    [SerializeField] private TMP_Text textoBoton;
    [SerializeField] private Image fondo;

    private Color colorNormal;

    public void Configurar(KahootCargado kahoot, string accion, System.Action<KahootCargado> alPulsar)
    {
        colorNormal = fondo.color;

        int preguntas = kahoot.datos.questions.Count;
        textoNombre.text = kahoot.datos.nombre;
        if (preguntas == 1)
            textoInfo.text = "1 question   -   " + kahoot.id + ".json";
        else
            textoInfo.text = preguntas + " questions   -   " + kahoot.id + ".json";

        textoBoton.text = accion;
        boton.onClick.RemoveAllListeners();
        boton.onClick.AddListener(() => alPulsar(kahoot));
    }

    public void Resaltar(bool activo)
    {
        if (activo)
            fondo.color = new Color(0.27f, 0.09f, 0.56f);
        else
            fondo.color = colorNormal;
    }
}
