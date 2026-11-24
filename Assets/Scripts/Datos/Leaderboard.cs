using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

[System.Serializable]
public class EntradaLeaderboard
{
    public string id;
    public string jugador;
    public int puntuacion;
    public int aciertos;
    public int totalPreguntas;
    public float tiempo;
    public string fecha;
}

[System.Serializable]
[XmlRoot("Leaderboard")]
public class Leaderboard
{
    public string kahoot;

    [XmlArray("Entradas")]
    [XmlArrayItem("Entrada")]
    public List<EntradaLeaderboard> entradas = new List<EntradaLeaderboard>();

    public static string rutaXml(string idKahoot)
    {
        return Path.Combine(Application.persistentDataPath, "Leaderboards", idKahoot + ".xml");
    }

    public static Leaderboard cargar(string idKahoot)
    {
        string ruta = rutaXml(idKahoot);
        Leaderboard leaderboard = null;

        if (File.Exists(ruta))
        {
            try
            {
                XmlSerializer xml = new XmlSerializer(typeof(Leaderboard));
                using (FileStream archivo = File.OpenRead(ruta))
                {
                    leaderboard = (Leaderboard)xml.Deserialize(archivo);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("No se ha podido leer " + ruta + ": " + e.Message);
            }
        }

        if (leaderboard == null)
        {
            leaderboard = new Leaderboard();
            leaderboard.kahoot = idKahoot;
        }
        if (leaderboard.entradas == null)
            leaderboard.entradas = new List<EntradaLeaderboard>();

        leaderboard.ordenar();
        return leaderboard;
    }

    public static int guardarPartida(string idKahoot, EntradaLeaderboard entrada)
    {
        Leaderboard leaderboard = cargar(idKahoot);
        leaderboard.entradas.Add(entrada);
        leaderboard.ordenar();

        try
        {
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "Leaderboards"));
            XmlSerializer xml = new XmlSerializer(typeof(Leaderboard));
            using (FileStream archivo = File.Create(rutaXml(idKahoot)))
            {
                xml.Serialize(archivo, leaderboard);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("No se ha podido guardar la leaderboard: " + e.Message);
        }

        return leaderboard.entradas.IndexOf(entrada) + 1;
    }

    public void ordenar()
    {
        entradas.Sort(compararEntradas);
    }

    private static int compararEntradas(EntradaLeaderboard a, EntradaLeaderboard b)
    {
        if (a.puntuacion != b.puntuacion)
            return b.puntuacion.CompareTo(a.puntuacion);

        return a.tiempo.CompareTo(b.tiempo);
    }
}
