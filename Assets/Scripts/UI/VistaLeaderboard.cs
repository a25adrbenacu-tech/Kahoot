using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VistaLeaderboard : MonoBehaviour
{
    [SerializeField] private Transform contenido;
    [SerializeField] private ItemLeaderboard prefabItem;
    [SerializeField] private TMP_Text textoVacio;
    [SerializeField] private ScrollRect scroll;

    public void Mostrar(Leaderboard leaderboard, string idMiPartida)
    {
        foreach (Transform hijo in contenido)
        {
            hijo.gameObject.SetActive(false);
            Destroy(hijo.gameObject);
        }

        int total = 0;
        int miPosicion = -1;

        if (leaderboard != null)
        {
            total = leaderboard.entradas.Count;
            for (int i = 0; i < total; i++)
            {
                EntradaLeaderboard entrada = leaderboard.entradas[i];
                bool esLaMia = idMiPartida != null && entrada.id == idMiPartida;
                if (esLaMia)
                    miPosicion = i;

                ItemLeaderboard item = Instantiate(prefabItem, contenido);
                item.Configurar(i + 1, entrada, esLaMia);
            }
        }

        textoVacio.gameObject.SetActive(total == 0);

        if (miPosicion > 0 && total > 1)
            scroll.verticalNormalizedPosition = 1f - (float)miPosicion / (total - 1);
        else
            scroll.verticalNormalizedPosition = 1f;
    }
}
