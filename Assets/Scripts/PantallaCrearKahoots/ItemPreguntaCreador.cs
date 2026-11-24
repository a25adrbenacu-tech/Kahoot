using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Tarjeta de una pregunta en el creador (prefab ItemPreguntaCreador).
public class ItemPreguntaCreador : MonoBehaviour
{
    [SerializeField] private TMP_Text textoTitulo;
    [SerializeField] private TMP_InputField inputPregunta;
    [SerializeField] private TMP_InputField[] inputRespuestas = new TMP_InputField[Pregunta.NumRespuestas];
    [SerializeField] private TMP_Dropdown dropdownCorrecta;
    [SerializeField] private TMP_InputField inputDuracion;
    [SerializeField] private Button botonBorrar;
    [SerializeField] private Image fondo;

    private Color colorNormal;
    private static readonly Color ColorError = new Color(0.55f, 0.08f, 0.15f, 1f);

    public void Inicializar(Action<ItemPreguntaCreador> alBorrar)
    {
        colorNormal = fondo.color;
        botonBorrar.onClick.RemoveAllListeners();
        botonBorrar.onClick.AddListener(() => alBorrar(this));

        inputPregunta.characterLimit = 120;
        foreach (TMP_InputField r in inputRespuestas) r.characterLimit = 75;
        inputDuracion.contentType = TMP_InputField.ContentType.DecimalNumber;
        inputDuracion.characterLimit = 5;

        // Al editar se quita la marca de error.
        inputPregunta.onValueChanged.AddListener(_ => MarcarError(false));
        foreach (TMP_InputField r in inputRespuestas) r.onValueChanged.AddListener(_ => MarcarError(false));
        dropdownCorrecta.onValueChanged.AddListener(_ => MarcarError(false));
        inputDuracion.onValueChanged.AddListener(_ => MarcarError(false));
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
        fondo.color = error ? ColorError : colorNormal;
    }

    // Devuelve los datos tal cual están escritos (la validación la hace GuardarKahoot).
    public Pregunta Leer(out bool duracionValida)
    {
        var p = new Pregunta
        {
            question = inputPregunta.text.Trim(),
            correctAnswer = dropdownCorrecta.value,
            answers = new string[Pregunta.NumRespuestas]
        };
        for (int i = 0; i < Pregunta.NumRespuestas; i++)
            p.answers[i] = inputRespuestas[i].text.Trim();

        string d = inputDuracion.text.Trim().Replace(',', '.');
        if (d.Length == 0)
        {
            p.duration = Pregunta.DuracionPorDefecto;
            duracionValida = true;
        }
        else
        {
            duracionValida = float.TryParse(d, NumberStyles.Float, CultureInfo.InvariantCulture, out float valor);
            p.duration = valor;
        }
        return p;
    }
}
