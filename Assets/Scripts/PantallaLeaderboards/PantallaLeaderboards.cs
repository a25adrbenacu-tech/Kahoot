using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PantallaLeaderboards : MonoBehaviour
{
    [SerializeField] private Transform contenidoKahoots;
    [SerializeField] private ItemKahoot prefabItem;
    [SerializeField] private TMP_Text textoTituloKahoot;
    [SerializeField] private TMP_Text textoSinKahoots;
    [SerializeField] private VistaLeaderboard vistaLeaderboard;

    private List<ItemKahoot> items = new List<ItemKahoot>();

    void Start()
    {
        foreach (Transform hijo in contenidoKahoots)
        {
            hijo.gameObject.SetActive(false);
            Destroy(hijo.gameObject);
        }

        List<KahootCargado> kahoots = ListaKahoots.leerKahoots();
        textoSinKahoots.gameObject.SetActive(kahoots.Count == 0);

        foreach (KahootCargado kahoot in kahoots)
        {
            ItemKahoot item = Instantiate(prefabItem, contenidoKahoots);
            item.name = "Item_" + kahoot.id;
            item.Configurar(kahoot, "View", mostrar);
            items.Add(item);
        }

        if (kahoots.Count == 0)
        {
            textoTituloKahoot.text = "No kahoots available";
            vistaLeaderboard.Mostrar(null, null);
            return;
        }

        KahootCargado primero = kahoots[0];
        if (KahootSeleccionado.kahoot != null)
        {
            foreach (KahootCargado kahoot in kahoots)
            {
                if (kahoot.id == KahootSeleccionado.kahoot.id)
                    primero = kahoot;
            }
        }
        mostrar(primero);
    }

    private void mostrar(KahootCargado kahoot)
    {
        textoTituloKahoot.text = kahoot.datos.nombre;
        vistaLeaderboard.Mostrar(Leaderboard.cargar(kahoot.id), null);

        foreach (ItemKahoot item in items)
            item.Resaltar(item.name == "Item_" + kahoot.id);
    }
}
