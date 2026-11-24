using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Lectura / escritura de kahoots (.json) en Application.persistentDataPath/Kahoots.
public static class KahootRepository
{
    private static bool defaultsCopiados;

    public static List<KahootInfo> CargarTodos()
    {
        var lista = new List<KahootInfo>();
        try
        {
            Directory.CreateDirectory(Rutas.CarpetaKahoots);
            CopiarKahootsDefault();

            string[] archivos = Directory.GetFiles(Rutas.CarpetaKahoots, "*.json");
            Array.Sort(archivos, StringComparer.OrdinalIgnoreCase);

            foreach (string ruta in archivos)
                if (TryCargar(ruta, out KahootInfo info))
                    lista.Add(info);
        }
        catch (Exception e)
        {
            Debug.LogWarning("No se ha podido leer la carpeta de kahoots: " + e.Message);
        }
        return lista;
    }

    public static bool TryCargar(string ruta, out KahootInfo info)
    {
        info = null;
        try
        {
            Kahoot k = JsonUtility.FromJson<Kahoot>(File.ReadAllText(ruta));
            if (k == null || k.questions == null || k.questions.Count == 0)
            {
                Debug.LogWarning("El archivo no contiene un kahoot válido, se ignora: " + ruta);
                return false;
            }
            foreach (Pregunta p in k.questions) p.Normalizar();

            info = new KahootInfo { id = Path.GetFileNameWithoutExtension(ruta), ruta = ruta, datos = k };
            return true;
        }
        catch (Exception e) // ArgumentException (JSON mal formado), IOException, UnauthorizedAccessException...
        {
            Debug.LogWarning("JSON no válido, se ignora: " + ruta + " -> " + e.Message);
            return false;
        }
    }

    // Guarda el kahoot con un nombre de archivo único y devuelve la ruta.
    public static string Guardar(Kahoot kahoot)
    {
        Directory.CreateDirectory(Rutas.CarpetaKahoots);

        string baseNombre = NombreArchivoSeguro(kahoot.nombre);
        string ruta = Path.Combine(Rutas.CarpetaKahoots, baseNombre + ".json");
        for (int i = 1; File.Exists(ruta); i++)
            ruta = Path.Combine(Rutas.CarpetaKahoots, baseNombre + "_" + i + ".json");

        File.WriteAllText(ruta, JsonUtility.ToJson(kahoot, true));
        return ruta;
    }

    // Los kahoots "default" viven en Assets/StreamingAssets/Kahoots y se copian
    // a la carpeta de lectura dinámica la primera vez.
    private static void CopiarKahootsDefault()
    {
        if (defaultsCopiados) return;
        defaultsCopiados = true;

        if (!Directory.Exists(Rutas.CarpetaKahootsDefault)) return;

        foreach (string origen in Directory.GetFiles(Rutas.CarpetaKahootsDefault, "*.json"))
        {
            string destino = Path.Combine(Rutas.CarpetaKahoots, Path.GetFileName(origen));
            if (!File.Exists(destino))
                File.Copy(origen, destino);
        }
    }

    private static string NombreArchivoSeguro(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre)) return "kahoot";
        char[] invalidos = Path.GetInvalidFileNameChars();
        var chars = nombre.Trim().ToCharArray();
        for (int i = 0; i < chars.Length; i++)
            if (Array.IndexOf(invalidos, chars[i]) >= 0 || chars[i] == ' ')
                chars[i] = '_';
        return new string(chars);
    }
}
