using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemPreguntaCreador : MonoBehaviour
{
    [SerializeField] private TMP_Text textoTitulo;
    [SerializeField] private TMP_InputField inputPregunta;
    [SerializeField] private TMP_InputField[] inputRespuestas = new TMP_InputField[4];
    [SerializeField] private TMP_Dropdown dropdownCorrecta;
    [SerializeField] private TMP_InputField inputDuracion;
    [SerializeField] private Button botonBorrar;
    [SerializeField] private Image fondo;

    private Color colorNormal;

    public void Inicializar(System.Action<ItemPreguntaCreador> alBorrar)
    {
        colorNormal = fondo.color;

        botonBorrar.onClick.RemoveAllListeners();
        botonBorrar.onClick.AddListener(() => alBorrar(this));

        inputPregunta.characterLimit = 120;
        inputPregunta.onValueChanged.AddListener(quitarError);

        for (int i = 0; i < inputRespuestas.Length; i++)
        {
            inputRespuestas[i].characterLimit = 75;
            inputRespuestas[i].onValueChanged.AddListener(quitarError);
        }

        inputDuracion.contentType = TMP_InputField.ContentType.DecimalNumber;
        inputDuracion.characterLimit = 5;
        inputDuracion.onValueChanged.AddListener(quitarError);
        dropdownCorrecta.onValueChanged.AddListener(quitarErrorDropdown);
    }

    public void SetNumero(int numero)
    {
        textoTitulo.text = "Question " + numero;
    }

    public void SetBorrable(bool borrable)
    {
        botonBorrar.interactable = borrable;
    }

    public void MarcarError(bool error)
    {
        if (error)
            fondo.color = new Color(0.55f, 0.08f, 0.15f);
        else
            fondo.color = colorNormal;
    }

    public crearKahoot.Questions leerPregunta(out bool duracionBien)
    {
        crearKahoot.Questions pregunta = new crearKahoot.Questions();
        pregunta.question = inputPregunta.text.Trim();
        pregunta.correctAnswer = dropdownCorrecta.value;

        for (int i = 0; i < 4; i++)
            pregunta.answers[i] = inputRespuestas[i].text.Trim();

        string duracion = inputDuracion.text.Trim().Replace(',', '.');
        if (duracion == "")
        {
            pregunta.duration = 20;
            duracionBien = true;
        }
        else
        {
            duracionBien = float.TryParse(duracion, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out pregunta.duration);
        }

        return pregunta;
    }

    private void quitarError(string texto)
    {
        MarcarError(false);
    }

    private void quitarErrorDropdown(int valor)
    {
        MarcarError(false);
    }
}
