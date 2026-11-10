using UnityEngine;
using System.IO;

public class crearKahoot : MonoBehaviour
{
    [System.Serializable]
    public class Questions
    {
        public string question;
        public int correctAnswer;
        public string[] answers;
        public int duration = 15;
    }
    private string rutaKahootPrueba1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rutaKahootPrueba1 = Application.persistentDataPath + "/KahootPrueba1.json";

        Questions firstQuestionKahootPrueba1 = new Questions();
        firstQuestionKahootPrueba1.question = "alo";
        firstQuestionKahootPrueba1.answers = new string[] { "elo", "pelo", "qwefwe", "nose" };
        firstQuestionKahootPrueba1.correctAnswer = 1;

        string Json = JsonUtility.ToJson(firstQuestionKahootPrueba1, true);
        File.WriteAllText(rutaKahootPrueba1, Json);
        Debug.Log(rutaKahootPrueba1);

        string conversion = File.ReadAllText(rutaKahootPrueba1);
        Questions pruebaCnvertirJsonAString = JsonUtility.FromJson<Questions>(conversion);
        Debug.Log(pruebaCnvertirJsonAString.question);
        Debug.Log(pruebaCnvertirJsonAString.correctAnswer);
        Debug.Log(pruebaCnvertirJsonAString.answers);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
