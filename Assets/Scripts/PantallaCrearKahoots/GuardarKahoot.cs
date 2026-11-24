public static class GuardarKahoot
{
    public static string comprobarKahoot(crearKahoot.preguntasKahoot kahoot, bool[] duracionesBien, out int preguntaMal)
    {
        preguntaMal = -1;

        if (kahoot.nombre == "")
            return "Write a name for the kahoot";

        if (kahoot.questions.Count == 0)
            return "Add at least one question";

        for (int i = 0; i < kahoot.questions.Count; i++)
        {
            crearKahoot.Questions pregunta = kahoot.questions[i];
            string numero = "Question " + (i + 1) + ": ";
            preguntaMal = i;

            if (pregunta.question == "")
                return numero + "the statement is empty";

            int respuestasRellenas = 0;
            for (int r = 0; r < pregunta.answers.Length; r++)
            {
                if (pregunta.answers[r] != "")
                    respuestasRellenas++;
            }

            if (respuestasRellenas < 2)
                return numero + "fill in at least two answers";

            if (pregunta.answers[pregunta.correctAnswer] == "")
                return numero + "the correct answer (" + (pregunta.correctAnswer + 1) + ") is empty";

            if (!duracionesBien[i] || pregunta.duration < 5 || pregunta.duration > 120)
                return numero + "time must be a number between 5 and 120 seconds";
        }

        preguntaMal = -1;
        return "";
    }
}
