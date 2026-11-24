using System.IO;
using UnityEngine;

// Única fuente de rutas de la aplicación.
public static class Rutas
{
    public static string CarpetaKahoots => Path.Combine(Application.persistentDataPath, "Kahoots");
    public static string CarpetaLeaderboards => Path.Combine(Application.persistentDataPath, "Leaderboards");
    public static string CarpetaKahootsDefault => Path.Combine(Application.streamingAssetsPath, "Kahoots");

    public static string Leaderboard(string idKahoot) => Path.Combine(CarpetaLeaderboards, idKahoot + ".xml");
}
