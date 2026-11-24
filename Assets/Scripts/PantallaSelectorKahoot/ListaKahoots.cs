using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Escena SelectorKahoot: lista dinámica de los .json de persistentDataPath/Kahoots + nombre de usuario.
public class ListaKahoots : MonoBehaviour
{
    [SerializeField] private Transform contenido;          // Content del ScrollView
    [SerializeField] private ItemKahoot prefabItem;
    [SerializeField] private TMP_InputField inputUsuario;
    [SerializeField] private TMP_Text textoAviso;
    [SerializeField] private TMP_Text textoVacio;
    [SerializeField] private TMP_Text textoRuta;

    private static readonly Color ColorError = new Color(1f, 0.45f, 0.5f);
    private static readonly Color ColorInfo = new Color(1f, 1f, 1f, 0.8f);

    void Start()
    {
        inputUsuario.characterLimit = 20;
        inputUsuario.text = Sesion.Usuario;
        inputUsuario.onValueChanged.AddListener(_ => MostrarAviso("", ColorInfo));
        if (textoRuta != null) textoRuta.text = "Folder: " + Rutas.CarpetaKahoots;
        CargarLista();
    }

    public void CargarLista()
    {
        foreach (Transform hijo in contenido)
            { hijo.gameObject.SetActive(false); Destroy(hijo.gameObject); }

        List<KahootInfo> kahoots = KahootRepository.CargarTodos();
        foreach (KahootInfo k in kahoots)
        {
            ItemKahoot item = Instantiate(prefabItem, contenido);
            item.name = "Item_" + k.id;
            item.Configurar(k, "Play", Seleccionar);
        }
        textoVacio.gameObject.SetActive(kahoots.Count == 0);
    }

    private void Seleccionar(KahootInfo kahoot)
    {
        string usuario = inputUsuario.text.Trim();
        if (usuario.Length == 0)
        {
            MostrarAviso("Write your username before playing", ColorError);
            inputUsuario.Select();
            return;
        }

        Sesion.Usuario = usuario;
        Sesion.KahootSeleccionado = kahoot;
        SceneManager.LoadScene(Escenas.Juego);
    }

    private void MostrarAviso(string texto, Color color)
    {
        textoAviso.text = texto;
        textoAviso.color = color;
    }
}
