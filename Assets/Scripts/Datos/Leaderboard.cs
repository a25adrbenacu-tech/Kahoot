using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
public class EntradaLeaderboard
{
    public string id = Guid.NewGuid().ToString("N");
    public string jugador;
    public int puntuacion;
    public int aciertos;
    public int totalPreguntas;
    public float tiempo;      // segundos totales empleados en responder
    public string fecha;      // yyyy-MM-dd HH:mm
}

[Serializable]
[XmlRoot("Leaderboard")]
public class Leaderboard
{
    public string kahoot;

    [XmlArray("Entradas")]
    [XmlArrayItem("Entrada")]
    public List<EntradaLeaderboard> entradas = new List<EntradaLeaderboard>();

    // Mayor puntuación primero; a igualdad, menos tiempo.
    public void Ordenar()
    {
        entradas.Sort((a, b) =>
        {
            int c = b.puntuacion.CompareTo(a.puntuacion);
            return c != 0 ? c : a.tiempo.CompareTo(b.tiempo);
        });
    }
}
