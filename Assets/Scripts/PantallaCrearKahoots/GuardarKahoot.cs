using Unity.VisualScripting;
using UnityEngine;

public class GuardarKahoot : MonoBehaviour
{
    [SerializeField] private crearKahoot crearKahoot;
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

    public void saveKahoot()
    {
        crearKahoot.saveQuestions(question, answer1, answer2 , answer3, answer4, duration, correctAnswer);
        crearKahoot.revisarCarpetaParaVerJson();
    }
}
