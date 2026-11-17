using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GuardarKahoot : MonoBehaviour
{
    [SerializeField] private crearKahoot crearKahoot;
    [SerializeField] private TMP_InputField textopregunta;
    [SerializeField] private TMP_InputField respuesta1;
    [SerializeField] private TMP_InputField respuesta2;
    [SerializeField] private TMP_InputField respuesta3;
    [SerializeField] private TMP_InputField respuesta4;
    [SerializeField] private TMP_InputField duracion;
    [SerializeField] private TMP_Dropdown rescorrecta;

    public string question;
    public int correctAnswer;
    public string answer1;
    public string answer2;
    public string answer3;
    public string answer4;
    public float duration = 15;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void enterQuestion(string respuesta)
    {
        question = respuesta;
    }
    public void enterAnswer1(string respuesta)
    {
        answer1 = respuesta;
    }
    public void enterAnswer2(string respuesta)
    {
        answer2 = respuesta;
    }
    public void enterAnswer3(string respuesta)
    {
        answer3 = respuesta;
    }
    public void enterAnswer4(string respuesta)
    {
        answer4 = respuesta;
    }
    public void enterDuration(string respuesta)
    {
        duration = float.Parse(respuesta);
    }
    public void enterCorrectAnswer(int respuesta)
    {
        correctAnswer = respuesta;
    }

    public void saveQuestion()
    {
        crearKahoot.saveQuestions(question, answer1, answer2 , answer3, answer4, duration, correctAnswer);
        textopregunta.text ="";   
        respuesta1.text ="";   
        respuesta2.text ="";   
        respuesta3.text ="";   
        respuesta4.text ="";   
        duracion.text ="";
        rescorrecta.value = 0;
        
    }

    public void saveKahoot()
    {
        crearKahoot.revisarCarpetaParaVerJson();
        SceneManager.LoadScene("PantallaTitulo");
    }
}
