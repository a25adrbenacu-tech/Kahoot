using UnityEngine;

// Fórmula estilo Kahoot: hasta 1000 puntos, que bajan hasta 500 cuanto más tardas.
public static class Puntuacion
{
    public const int Maxima = 1000;

    public static int Calcular(bool correcta, float tiempoUsado, float duracion)
    {
        if (!correcta || duracion <= 0f) return 0;
        float fraccion = Mathf.Clamp01(tiempoUsado / duracion);
        return Mathf.RoundToInt(Maxima * (1f - fraccion / 2f));
    }
}
