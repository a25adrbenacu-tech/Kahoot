using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Escena Leaderboards: ver la leaderboard (XML independiente) de cualquier kahoot sin jugar.
public class PantallaLeaderboards : MonoBehaviour
{
    [SerializeField] private Transform contenidoKahoots;
    [SerializeField] private ItemKahoot prefabItem;
    [SerializeField] private TMP_Text textoTituloKahoot;
    [SerializeField] private TMP_Text textoSinKahoots;
    [SerializeField] private VistaLeaderboard vistaLeaderboard;

    private readonly List<ItemKahoot> items = new List<ItemKahoot>();

    void Start()
    {
        List<KahootInfo> kahoots = KahootRepository.CargarTodos();
        textoSinKahoots.gameObject.SetActive(kahoots.Count == 0);

        foreach (Transform hijo in contenidoKahoots)
            { hijo.gameObject.SetActive(false); Destroy(hijo.gameObject); }

        foreach (KahootInfo k in kahoots)
        {
            ItemKahoot item = Instantiate(prefabItem, contenidoKahoots);
            item.name = "Item_" + k.id;
            item.Configurar(k, "View", Mostrar);
            items.Add(item);
        }

        if (kahoots.Count > 0)
        {
            KahootInfo inicial = kahoots[0];
            if (Sesion.KahootSeleccionado != null)
            {
                KahootInfo previo = kahoots.Find(k => k.id == Sesion.KahootSeleccionado.id);
                if (previo != null) inicial = previo;
            }
            Mostrar(inicial);
        }
        else
        {
            textoTituloKahoot.text = "No kahoots available";
            vistaLeaderboard.Mostrar(null);
        }
    }

    private void Mostrar(KahootInfo kahoot)
    {
        textoTituloKahoot.text = kahoot.Nombre;
        vistaLeaderboard.Mostrar(LeaderboardRepository.Cargar(kahoot.id));

        for (int i = 0; i < items.Count; i++)
            items[i].Resaltar(items[i].name == "Item_" + kahoot.id);
    }
}
