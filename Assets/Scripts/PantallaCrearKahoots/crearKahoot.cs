using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public class crearKahoot : MonoBehaviour
{
    [System.Serializable]
    public class Questions
    {
        public string question;
        public int correctAnswer;
        public string[] answers = new string[4];
        public float duration = 15;
    }

    public class preguntasKahoot { }
    private string rutaKahootPrueba1;
    private string rutaimportarkahoots;
    private string nombrearchivo;
    private int contadorKahoots;
    private string nombreFinalArchivo;
    List<Questions> questionsontheKahoot = new List<Questions>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rutaimportarkahoots = Application.persistentDataPath;

        /**rutaKahootPrueba1 = Application.persistentDataPath + "/KahootPrueba1.json";
        
        Debug.Log(rutaKahootPrueba1);

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
        Debug.Log(pruebaCnvertirJsonAString.answers);**/
        List<Questions> importedJson = new List<Questions>();
        
        foreach (string ruta in revisarCarpetaParaVerJson())
        {
            string texto = File.ReadAllText(ruta);
            Questions p = JsonUtility.FromJson<Questions>(texto);

            importedJson.Add(p);
        }
        Debug.Log(rutaimportarkahoots);
        
    }
     public string[] revisarCarpetaParaVerJson()
    {
        string[] lista = Directory.GetFiles(rutaimportarkahoots, "*.json");

        return lista;
    }

    public void saveQuestions(string question, string answer1, string answer2, string answer3, string answer4, float duration, int correct)
    {
        Questions save = new Questions();

        

        save.question = question;
        save.answers[0] = answer1;
        save.answers[1] = answer2;
        save.answers[2] = answer3;
        save.answers[3] = answer4;
        save.duration = duration;
        save.correctAnswer = correct;

        questionsontheKahoot.Add(save);
        
    }

    public void saveKahoot()
    {
        nombrearchivo = "kahoot" + contadorKahoots + ".json";
        nombreFinalArchivo = Path.Combine(rutaimportarkahoots, nombrearchivo);


        while (File.Exists(nombreFinalArchivo))
        {
            contadorKahoots++;
            nombrearchivo = "kahoot" + contadorKahoots + ".json";
            nombreFinalArchivo = Path.Combine(rutaimportarkahoots, nombrearchivo);
        }
        string json = JsonUtility.ToJson(questionsontheKahoot);
        File.WriteAllText(nombreFinalArchivo, json);
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
