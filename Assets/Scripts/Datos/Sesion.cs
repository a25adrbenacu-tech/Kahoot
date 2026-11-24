using UnityEngine;

// Estado compartido entre escenas.
public static class Sesion
{
    private const string ClaveUsuario = "kahoot_usuario";

    public static string Usuario
    {
        get => PlayerPrefs.GetString(ClaveUsuario, "");
        set { PlayerPrefs.SetString(ClaveUsuario, value); PlayerPrefs.Save(); }
    }

    public static KahootInfo KahootSeleccionado;

    // Resultado de la última partida (para la escena de resultados)
    public static EntradaLeaderboard UltimaEntrada;
    public static int UltimaPosicion;
}
