using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Fila de la lista de kahoots (prefab ItemKahoot). Se usa en el selector y en Leaderboards.
public class ItemKahoot : MonoBehaviour
{
    [SerializeField] private TMP_Text textoNombre;
    [SerializeField] private TMP_Text textoInfo;
    [SerializeField] private Button boton;
    [SerializeField] private TMP_Text textoBoton;
    [SerializeField] private Image fondo;

    private Color colorNormal;

    public void Configurar(KahootInfo kahoot, string textoAccion, Action<KahootInfo> alPulsar)
    {
        if (fondo != null) colorNormal = fondo.color;
        textoNombre.text = kahoot.Nombre;
        textoInfo.text = kahoot.NumPreguntas + (kahoot.NumPreguntas == 1 ? " question" : " questions") + "   ·   " + kahoot.id + ".json";
        textoBoton.text = textoAccion;
        boton.onClick.RemoveAllListeners();
        boton.onClick.AddListener(() => alPulsar(kahoot));
    }

    // Feedback visual del elemento seleccionado (Leaderboards).
    public void Resaltar(bool activo)
    {
        if (fondo == null) return;
        fondo.color = activo ? new Color(0.27f, 0.09f, 0.56f, 1f) : colorNormal;
    }
}
