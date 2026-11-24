using System;
using System.Collections.Generic;

// Modelo de datos de un Kahoot. Los nombres de los campos coinciden con el JSON
// que ya generaba el creador original (nombre, questions, question, answers, correctAnswer, duration).
[Serializable]
public class Pregunta
{
    public const int NumRespuestas = 4;
    public const float DuracionPorDefecto = 20f;

    public string question;
    public int correctAnswer;
    public string[] answers = new string[NumRespuestas];
    public float duration = DuracionPorDefecto;

    // Corrige datos incompletos de un JSON escrito a mano para que el juego nunca se rompa.
    public void Normalizar()
    {
        if (question == null) question = "";
        if (answers == null) answers = new string[NumRespuestas];
        if (answers.Length != NumRespuestas) Array.Resize(ref answers, NumRespuestas);
        for (int i = 0; i < NumRespuestas; i++)
            if (answers[i] == null) answers[i] = "";
        if (duration <= 0f) duration = DuracionPorDefecto;
        if (correctAnswer < 0 || correctAnswer >= NumRespuestas) correctAnswer = 0;
    }
}

[Serializable]
public class Kahoot
{
    public string nombre;
    public List<Pregunta> questions = new List<Pregunta>();
}

// Kahoot cargado desde disco: el id es el nombre del archivo .json sin extensión
// y sirve también para nombrar su XML de leaderboard (1 JSON -> 1 XML).
public class KahootInfo
{
    public string id;
    public string ruta;
    public Kahoot datos;

    public string Nombre => string.IsNullOrWhiteSpace(datos.nombre) ? id : datos.nombre;
    public int NumPreguntas => datos.questions.Count;
}
