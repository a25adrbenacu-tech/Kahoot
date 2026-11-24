using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Rellena un ScrollView con las entradas de una leaderboard. Reutilizado en Resultados y Leaderboards.
public class VistaLeaderboard : MonoBehaviour
{
    [SerializeField] private Transform contenido;
    [SerializeField] private ItemLeaderboard prefabItem;
    [SerializeField] private TMP_Text textoVacio;
    [SerializeField] private ScrollRect scroll;

    public void Mostrar(Leaderboard lb, string idResaltado = null)
    {
        foreach (Transform hijo in contenido)
            { hijo.gameObject.SetActive(false); Destroy(hijo.gameObject); }

        List<EntradaLeaderboard> entradas = lb != null ? lb.entradas : new List<EntradaLeaderboard>();
        textoVacio.gameObject.SetActive(entradas.Count == 0);

        RectTransform resaltada = null;
        for (int i = 0; i < entradas.Count; i++)
        {
            ItemLeaderboard item = Instantiate(prefabItem, contenido);
            bool esLaMia = idResaltado != null && entradas[i].id == idResaltado;
            item.Configurar(i + 1, entradas[i], esLaMia);
            if (esLaMia) resaltada = (RectTransform)item.transform;
        }

        Canvas.ForceUpdateCanvases();
        if (scroll != null)
            scroll.verticalNormalizedPosition = resaltada != null ? PosicionPara(resaltada) : 1f;
    }

    // Desplaza el scroll para que se vea la fila resaltada.
    private float PosicionPara(RectTransform item)
    {
        RectTransform content = scroll.content;
        float altoContenido = content.rect.height;
        float altoVista = scroll.viewport != null ? scroll.viewport.rect.height : ((RectTransform)scroll.transform).rect.height;
        if (altoContenido <= altoVista) return 1f;
        float y = -item.anchoredPosition.y - item.rect.height * 0.5f; // distancia desde arriba
        return Mathf.Clamp01(1f - (y - altoVista * 0.5f) / (altoContenido - altoVista));
    }
}
