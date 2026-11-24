// Validación de un kahoot antes de guardarlo. Separado de la UI para poder reutilizarlo.
public static class GuardarKahoot
{
    public const float DuracionMinima = 5f;
    public const float DuracionMaxima = 120f;

    // Devuelve null si es válido, o el mensaje de error y el índice de la pregunta con el problema (-1 = general).
    public static string Validar(Kahoot kahoot, bool[] duracionesValidas, out int preguntaConError)
    {
        preguntaConError = -1;

        if (string.IsNullOrWhiteSpace(kahoot.nombre))
            return "Write a name for the kahoot";
        if (kahoot.questions.Count == 0)
            return "Add at least one question";

        for (int i = 0; i < kahoot.questions.Count; i++)
        {
            Pregunta p = kahoot.questions[i];
            preguntaConError = i;
            string n = "Question " + (i + 1) + ": ";

            if (string.IsNullOrWhiteSpace(p.question))
                return n + "the statement is empty";

            int rellenas = 0;
            foreach (string r in p.answers)
                if (!string.IsNullOrWhiteSpace(r)) rellenas++;
            if (rellenas < 2)
                return n + "fill in at least two answers";

            if (string.IsNullOrWhiteSpace(p.answers[p.correctAnswer]))
                return n + "the correct answer (" + (p.correctAnswer + 1) + ") is empty";

            if (!duracionesValidas[i] || p.duration < DuracionMinima || p.duration > DuracionMaxima)
                return n + "time must be a number between " + DuracionMinima + " and " + DuracionMaxima + " seconds";
        }

        preguntaConError = -1;
        return null;
    }
}
