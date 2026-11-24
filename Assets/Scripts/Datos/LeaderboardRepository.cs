using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

// Un XML de resultados por kahoot en Application.persistentDataPath/Leaderboards/<idKahoot>.xml
public static class LeaderboardRepository
{
    private static readonly XmlSerializer serializer = new XmlSerializer(typeof(Leaderboard));

    // Si el XML no existe o no se puede leer, devuelve una leaderboard vacía.
    public static Leaderboard Cargar(string idKahoot)
    {
        string ruta = Rutas.Leaderboard(idKahoot);
        if (!File.Exists(ruta))
            return new Leaderboard { kahoot = idKahoot };

        try
        {
            using (var stream = File.OpenRead(ruta))
            {
                var lb = (Leaderboard)serializer.Deserialize(stream);
                if (lb == null) return new Leaderboard { kahoot = idKahoot };
                if (lb.entradas == null) lb.entradas = new System.Collections.Generic.List<EntradaLeaderboard>();
                lb.Ordenar();
                return lb;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("No se ha podido leer " + ruta + ": " + e.Message);
            return new Leaderboard { kahoot = idKahoot };
        }
    }

    // Añade la entrada, guarda el XML y devuelve la posición (1 = primero).
    public static int Agregar(string idKahoot, EntradaLeaderboard entrada)
    {
        Leaderboard lb = Cargar(idKahoot);
        lb.kahoot = idKahoot;
        lb.entradas.Add(entrada);
        lb.Ordenar();

        try
        {
            Directory.CreateDirectory(Rutas.CarpetaLeaderboards);
            using (var stream = File.Create(Rutas.Leaderboard(idKahoot)))
                serializer.Serialize(stream, lb);
        }
        catch (Exception e)
        {
            Debug.LogWarning("No se ha podido guardar la leaderboard de " + idKahoot + ": " + e.Message);
        }

        return lb.entradas.IndexOf(entrada) + 1;
    }
}
