using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Menú "Kahoot > Generar escenas y prefabs".
// Construye todas las escenas, los prefabs de la UI y la lista de Build Settings.
// Botones de menú = botón por defecto de Unity (UI > Button - TextMeshPro).
// Botones de respuesta = estilo Kahoot (rojo/azul/amarillo/verde con triángulo/rombo/círculo/cuadrado).
// Fondo y estilo = los de la pantalla SelectorKahoot (Selector fondo, títulos blancos, paneles oscuros, items morados).
public static class KahootSceneBuilder
{
    private const string DirImagenes = "Assets/Imagenes/Kahoot/";
    private const string DirPrefabs = "Assets/Prefabs/";
    private const string DirEscenas = "Assets/Scenes/";
    private const string Wallpaper = "Assets/Imagenes/Selector fondo.png";

    private static readonly Color ColorPanel = new Color(0.047f, 0.0073f, 0.0073f, 0.85f);
    private static readonly Color ColorItem = new Color(0.37254903f, 0.04705883f, 0.36078432f, 1f);
    private static readonly Color Morado = new Color32(0x46, 0x17, 0x8F, 0xFF);
    private static readonly Color[] ColoresKahoot =
    {
        new Color32(0xE2, 0x1B, 0x3C, 0xFF), // rojo   - triángulo
        new Color32(0x13, 0x68, 0xCE, 0xFF), // azul   - rombo
        new Color32(0xD8, 0x9E, 0x00, 0xFF), // amarillo - círculo
        new Color32(0x26, 0x89, 0x0C, 0xFF), // verde  - cuadrado
    };
    private static readonly string[] NombresColor = { "Red", "Blue", "Yellow", "Green" };

    private static readonly string[] OrdenEscenas =
    {
        Escenas.Titulo, Escenas.Selector, Escenas.Juego, Escenas.Resultados,
        Escenas.Leaderboards, Escenas.Creador, Escenas.About
    };

    private static TMP_DefaultControls.Resources tmpRes;
    private static DefaultControls.Resources uiRes;
    private static Sprite sprFondo, sprRedondeado, sprCirculo, sprCheck, sprCruz;
    private static Sprite[] sprFormas;

    // Traza paso a paso en Logs/KahootGenerador.log (se escribe y cierra en cada línea,
    // así queda registrada aunque Unity se cierre de golpe).
    private const string RutaTraza = "Logs/KahootGenerador.log";

    private static void T(string mensaje)
    {
        try
        {
            System.IO.Directory.CreateDirectory("Logs");
            System.IO.File.AppendAllText(RutaTraza, System.DateTime.Now.ToString("HH:mm:ss.fff") + "  " + mensaje + "\n");
        }
        catch { }
    }

    private static void CapturarLog(string condicion, string stack, LogType tipo)
    {
        if (tipo == LogType.Error || tipo == LogType.Exception || tipo == LogType.Assert)
            T("[" + tipo + "] " + condicion + "\n" + stack);
    }

    [MenuItem("Kahoot/Generar escenas y prefabs")]
    public static void Generar()
    {
        if (!EditorUtility.DisplayDialog("Generar escenas",
                "Se van a (re)generar las escenas " + string.Join(", ", OrdenEscenas) +
                " y los prefabs de Assets/Prefabs. Las escenas con ese nombre se sobrescriben. ¿Continuar?",
                "Generar", "Cancelar"))
            return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        T("==================== INICIO GENERADOR (Unity " + Application.unityVersion + ")");
        Application.logMessageReceived += CapturarLog;
        try
        {
        CrearCarpeta("Assets", "Prefabs");
        CrearCarpeta("Assets", "Scenes");
        T("PrepararSprites");
        PrepararSprites();
        T("CargarRecursosPorDefecto");
        CargarRecursosPorDefecto();

        // Escena temporal para montar los prefabs
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        T("Prefab ItemKahoot");
        ItemKahoot prefabKahoot = CrearPrefabItemKahoot();
        T("Prefab ItemLeaderboard");
        ItemLeaderboard prefabLeaderboard = CrearPrefabItemLeaderboard();
        T("Prefab ItemPreguntaCreador");
        ItemPreguntaCreador prefabPregunta = CrearPrefabItemPregunta();

        var errores = new List<string>();
        Paso(errores, Escenas.Titulo, EscenaTitulo);
        Paso(errores, Escenas.Selector, () => EscenaSelector(prefabKahoot));
        Paso(errores, Escenas.Juego, EscenaJuego);
        Paso(errores, Escenas.Resultados, () => EscenaResultados(prefabLeaderboard));
        Paso(errores, Escenas.Leaderboards, () => EscenaLeaderboards(prefabKahoot, prefabLeaderboard));
        Paso(errores, Escenas.Creador, () => EscenaCreador(prefabPregunta));
        Paso(errores, Escenas.About, EscenaAbout);

        // Build Settings: siempre las 7 escenas, con PantallaTitulo primero
        EditorBuildSettings.scenes = OrdenEscenas
            .Where(n => System.IO.File.Exists(DirEscenas + n + ".unity"))
            .Select(n => new EditorBuildSettingsScene(DirEscenas + n + ".unity", true))
            .ToArray();

        T("Build settings + SaveAssets");
        AssetDatabase.SaveAssets();
        T("OpenScene final");
        EditorSceneManager.OpenScene(DirEscenas + Escenas.Titulo + ".unity");

        if (errores.Count == 0)
            EditorUtility.DisplayDialog("Kahoot", "Escenas y prefabs generados. Pulsa Play desde PantallaTitulo.", "OK");
        else
            EditorUtility.DisplayDialog("Kahoot - errores",
                "No se han podido generar estas escenas:\n\n" + string.Join("\n\n", errores) +
                "\n\n(El detalle completo está en la Console.)", "OK");
        T("FIN GENERADOR. Errores: " + errores.Count);
        }
        finally
        {
            Application.logMessageReceived -= CapturarLog;
        }
    }

    private static void Paso(List<string> errores, string escena, System.Action accion)
    {
        try
        {
            T(">> Escena " + escena);
            accion();
            T("<< Escena " + escena + " OK");
        }
        catch (System.Exception e)
        {
            T("!! Escena " + escena + " EXCEPCION: " + e);
            Debug.LogException(e);
            errores.Add(escena + ": " + e.GetType().Name + " - " + e.Message);
        }
    }

    // ------------------------------------------------------------------ recursos

    private static void CrearCarpeta(string padre, string nombre)
    {
        if (!AssetDatabase.IsValidFolder(padre + "/" + nombre))
            AssetDatabase.CreateFolder(padre, nombre);
    }

    private static void PrepararSprites()
    {
        sprRedondeado = ConfigurarSprite(DirImagenes + "Redondeado.png", new Vector4(20, 20, 20, 20));
        sprCirculo = ConfigurarSprite(DirImagenes + "Circulo.png", Vector4.zero);
        sprCheck = ConfigurarSprite(DirImagenes + "Check.png", Vector4.zero);
        sprCruz = ConfigurarSprite(DirImagenes + "Cruz.png", Vector4.zero);
        sprFormas = new[]
        {
            ConfigurarSprite(DirImagenes + "Triangulo.png", Vector4.zero),
            ConfigurarSprite(DirImagenes + "Rombo.png", Vector4.zero),
            ConfigurarSprite(DirImagenes + "CirculoForma.png", Vector4.zero),
            ConfigurarSprite(DirImagenes + "Cuadrado.png", Vector4.zero),
        };
        sprFondo = AssetDatabase.LoadAllAssetsAtPath(Wallpaper).OfType<Sprite>().FirstOrDefault();
        if (sprFondo == null)
            Debug.LogWarning("No se encuentra el sprite de fondo en " + Wallpaper);
    }

    private static Sprite ConfigurarSprite(string ruta, Vector4 borde)
    {
        var imp = AssetImporter.GetAtPath(ruta) as TextureImporter;
        if (imp == null)
        {
            Debug.LogError("Falta la imagen " + ruta);
            return null;
        }
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.alphaIsTransparency = true;
        imp.mipmapEnabled = false;
        imp.spriteBorder = borde;
        imp.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
    }

    private static Sprite Builtin(string ruta) => AssetDatabase.GetBuiltinExtraResource<Sprite>(ruta);

    private static void CargarRecursosPorDefecto()
    {
        Sprite standard = Builtin("UI/Skin/UISprite.psd");
        Sprite background = Builtin("UI/Skin/Background.psd");
        Sprite input = Builtin("UI/Skin/InputFieldBackground.psd");
        Sprite knob = Builtin("UI/Skin/Knob.psd");
        Sprite check = Builtin("UI/Skin/Checkmark.psd");
        Sprite arrow = Builtin("UI/Skin/DropdownArrow.psd");
        Sprite mask = Builtin("UI/Skin/UIMask.psd");

        tmpRes = new TMP_DefaultControls.Resources
        {
            standard = standard, background = background, inputField = input, knob = knob,
            checkmark = check, dropdown = arrow, mask = mask
        };
        uiRes = new DefaultControls.Resources
        {
            standard = standard, background = background, inputField = input, knob = knob,
            checkmark = check, dropdown = arrow, mask = mask
        };
    }

    // ------------------------------------------------------------------ helpers UI

    private static RectTransform Nuevo(string nombre, Transform padre)
    {
        var go = new GameObject(nombre, typeof(RectTransform));
        go.layer = 5; // UI
        var rt = (RectTransform)go.transform;
        if (padre != null) rt.SetParent(padre, false);
        return rt;
    }

    private static void Anclar(RectTransform rt, Vector2 min, Vector2 max, Vector2 offMin = default, Vector2 offMax = default)
    {
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = offMin;
        rt.offsetMax = offMax;
    }

    private static void Colocar(RectTransform rt, Vector2 ancla, Vector2 pos, Vector2 tam)
    {
        rt.anchorMin = rt.anchorMax = ancla;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = tam;
    }

    private static void Rellenar(RectTransform rt, float margen = 0f)
    {
        Anclar(rt, Vector2.zero, Vector2.one, new Vector2(margen, margen), new Vector2(-margen, -margen));
    }

    private static TextMeshProUGUI Texto(Transform padre, string nombre, string texto, float tam, Color color,
        TextAlignmentOptions alineacion = TextAlignmentOptions.Center, bool negrita = false)
    {
        RectTransform rt = Nuevo(nombre, padre);
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.text = texto;
        t.fontSize = tam;
        t.color = color;
        t.alignment = alineacion;
        if (negrita) t.fontStyle = FontStyles.Bold;
        t.raycastTarget = false;
        return t;
    }

    private static Image Imagen(Transform padre, string nombre, Sprite sprite, Color color, bool sliced = false)
    {
        RectTransform rt = Nuevo(nombre, padre);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        if (sliced) img.type = Image.Type.Sliced;
        return img;
    }

    private static LayoutElement LE(Component c, float ancho = -1, float alto = -1, float flexAncho = -1, float flexAlto = -1)
    {
        var le = c.GetComponent<LayoutElement>();
        if (le == null) le = c.gameObject.AddComponent<LayoutElement>();
        le.preferredWidth = ancho;
        le.preferredHeight = alto;
        le.flexibleWidth = flexAncho;
        le.flexibleHeight = flexAlto;
        return le;
    }

    private static HorizontalLayoutGroup HLG(Component c, float espacio, RectOffset padding = null, bool expandirAlto = false)
    {
        var g = c.gameObject.AddComponent<HorizontalLayoutGroup>();
        g.spacing = espacio;
        g.padding = padding ?? new RectOffset(0, 0, 0, 0);
        g.childAlignment = TextAnchor.MiddleLeft;
        g.childControlWidth = true;
        g.childControlHeight = true;
        g.childForceExpandWidth = false;
        g.childForceExpandHeight = expandirAlto;
        return g;
    }

    private static VerticalLayoutGroup VLG(Component c, float espacio, RectOffset padding = null)
    {
        var g = c.gameObject.AddComponent<VerticalLayoutGroup>();
        g.spacing = espacio;
        g.padding = padding ?? new RectOffset(0, 0, 0, 0);
        g.childAlignment = TextAnchor.UpperCenter;
        g.childControlWidth = true;
        g.childControlHeight = true;
        g.childForceExpandWidth = true;
        g.childForceExpandHeight = false;
        return g;
    }

    // Botón por defecto de Unity (UI > Button - TextMeshPro)
    private static Button Boton(Transform padre, string nombre, string texto, float tamTexto = 36)
    {
        T("   Boton " + nombre);
        GameObject go = TMP_DefaultControls.CreateButton(tmpRes);
        go.name = nombre;
        go.layer = 5;
        go.transform.SetParent(padre, false);
        TMP_Text t = go.GetComponentInChildren<TMP_Text>();
        t.text = texto;
        t.fontSize = tamTexto;
        return go.GetComponent<Button>();
    }

    // Input Field por defecto de Unity (UI > Input Field - TextMeshPro)
    private static TMP_InputField CampoTexto(Transform padre, string nombre, string placeholder, float tamTexto = 30)
    {
        T("   CampoTexto " + nombre);
        GameObject go = TMP_DefaultControls.CreateInputField(tmpRes);
        go.name = nombre;
        go.layer = 5;
        go.transform.SetParent(padre, false);
        var input = go.GetComponent<TMP_InputField>();
        input.pointSize = tamTexto;
        input.textComponent.fontSize = tamTexto;
        input.textComponent.alignment = TextAlignmentOptions.MidlineLeft;
        var ph = (TMP_Text)input.placeholder;
        ph.text = placeholder;
        ph.fontSize = tamTexto;
        ph.alignment = TextAlignmentOptions.MidlineLeft;
        input.lineType = TMP_InputField.LineType.SingleLine;
        return input;
    }

    // Dropdown por defecto de Unity (UI > Dropdown - TextMeshPro)
    private static TMP_Dropdown Desplegable(Transform padre, string nombre, List<string> opciones, float tamTexto = 26)
    {
        T("   Desplegable " + nombre);
        GameObject go = TMP_DefaultControls.CreateDropdown(tmpRes);
        go.name = nombre;
        go.layer = 5;
        go.transform.SetParent(padre, false);
        var dd = go.GetComponent<TMP_Dropdown>();
        dd.ClearOptions();
        dd.AddOptions(opciones);
        dd.captionText.fontSize = tamTexto;
        dd.itemText.fontSize = tamTexto;

        var item = (RectTransform)dd.itemText.transform.parent;
        item.sizeDelta = new Vector2(item.sizeDelta.x, 48);
        var content = (RectTransform)item.parent;
        content.sizeDelta = new Vector2(content.sizeDelta.x, 52);
        dd.template.sizeDelta = new Vector2(dd.template.sizeDelta.x, opciones.Count * 48 + 12);
        return dd;
    }

    // Scroll View montado a mano (ScrollRect + Viewport con RectMask2D + Content + Scrollbar por defecto).
    // Content con Vertical Layout Group + Content Size Fitter: crece con la cantidad de elementos.
    private static ScrollRect Scroll(Transform padre, string nombre, float espacio = 12)
    {
        T("   Scroll " + nombre);
        Image fondo = Imagen(padre, nombre, null, new Color(0, 0, 0, 0));
        RectTransform root = fondo.rectTransform;
        Rellenar(root, 10);

        // Viewport (deja 30 px a la derecha para la barra)
        RectTransform viewport = Nuevo("Viewport", root);
        Anclar(viewport, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-30, 0));
        viewport.gameObject.AddComponent<RectMask2D>();

        // Content
        RectTransform content = Nuevo("Content", viewport);
        content.anchorMin = new Vector2(0, 1);
        content.anchorMax = new Vector2(1, 1);
        content.pivot = new Vector2(0.5f, 1);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = Vector2.zero;
        VLG(content, espacio, new RectOffset(10, 10, 10, 10));
        var csf = content.gameObject.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Scrollbar vertical por defecto de Unity
        GameObject barraGo = DefaultControls.CreateScrollbar(uiRes);
        barraGo.name = "Scrollbar Vertical";
        barraGo.layer = 5;
        var barraRt = (RectTransform)barraGo.transform;
        barraRt.SetParent(root, false);
        barraRt.anchorMin = new Vector2(1, 0);
        barraRt.anchorMax = new Vector2(1, 1);
        barraRt.pivot = new Vector2(1, 0.5f);
        barraRt.anchoredPosition = Vector2.zero;
        barraRt.sizeDelta = new Vector2(22, 0);
        var barra = barraGo.GetComponent<Scrollbar>();
        barra.direction = Scrollbar.Direction.BottomToTop;

        var sr = root.gameObject.AddComponent<ScrollRect>();
        sr.content = content;
        sr.viewport = viewport;
        sr.horizontal = false;
        sr.vertical = true;
        sr.movementType = ScrollRect.MovementType.Clamped;
        sr.scrollSensitivity = 40;
        sr.verticalScrollbar = barra;
        sr.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
        return sr;
    }

    private static RectTransform Panel(Transform padre, string nombre, Vector2 min, Vector2 max)
    {
        T("   Panel " + nombre);
        Image img = Imagen(padre, nombre, sprRedondeado, ColorPanel, true);
        Anclar(img.rectTransform, min, max);
        return img.rectTransform;
    }

    private static void Set(Object objeto, string campo, Object valor)
    {
        T("   Set " + objeto.GetType().Name + "." + campo);
        var so = new SerializedObject(objeto);
        SerializedProperty p = so.FindProperty(campo);
        if (p == null)
        {
            Debug.LogError("Campo " + campo + " no encontrado en " + objeto.GetType().Name);
            return;
        }
        p.objectReferenceValue = valor;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetArray(Object objeto, string campo, Object[] valores)
    {
        T("   SetArray " + objeto.GetType().Name + "." + campo);
        var so = new SerializedObject(objeto);
        SerializedProperty p = so.FindProperty(campo);
        p.arraySize = valores.Length;
        for (int i = 0; i < valores.Length; i++)
            p.GetArrayElementAtIndex(i).objectReferenceValue = valores[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // Al abrir una escena nueva (NewSceneMode.Single) Unity descarga los assets en memoria,
    // así que la referencia al prefab se vuelve null: se recarga del disco justo antes de usarla.
    private static T CargarPrefab<T>(string nombre) where T : Component
    {
        var go = AssetDatabase.LoadAssetAtPath<GameObject>(DirPrefabs + nombre + ".prefab");
        T comp = go != null ? go.GetComponent<T>() : null;
        KahootSceneBuilder.T("   CargarPrefab " + nombre + " -> " + (comp != null ? "OK" : "NULL"));
        return comp;
    }

    private static T GuardarPrefab<T>(GameObject go, string nombre) where T : Component
    {
        KahootSceneBuilder.T("   SaveAsPrefabAsset " + nombre);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, DirPrefabs + nombre + ".prefab");
        Object.DestroyImmediate(go);
        return prefab.GetComponent<T>();
    }

    // Cuadradito de color + forma Kahoot (para asociar respuesta <-> color en el creador)
    private static Image MarcaRespuesta(Transform padre, int i, float tam)
    {
        Image marca = Imagen(padre, "Marca" + (i + 1), sprRedondeado, ColoresKahoot[i], true);
        marca.rectTransform.sizeDelta = new Vector2(tam, tam);
        Image forma = Imagen(marca.transform, "Forma", sprFormas[i], Color.white);
        Rellenar(forma.rectTransform, tam * 0.2f);
        forma.raycastTarget = false;
        return marca;
    }

    // ------------------------------------------------------------------ escena base

    private static Transform NuevaEscena(string titulo, out GameObject managers)
    {
        T("   NewScene");
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var cam = new GameObject("Main Camera");
        cam.tag = "MainCamera";
        cam.transform.position = new Vector3(0, 0, -10);
        var c = cam.AddComponent<Camera>();
        c.orthographic = true;
        c.clearFlags = CameraClearFlags.SolidColor;
        c.backgroundColor = Color.black;

        T("   NuevaEscena: var es");
        var es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        ConfigurarInputModule(es.GetComponent<InputSystemUIInputModule>());

        T("   NuevaEscena: var cgo");
        var cgo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        cgo.layer = 5;
        cgo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = cgo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // Fondo igual que la pantalla de cargar kahoot; Envelope para que nunca deje huecos ni se deforme
        T("   NuevaEscena: Image wp");
        Image wp = Imagen(cgo.transform, "Wallpaper", sprFondo, Color.white);
        wp.raycastTarget = false;
        Colocar(wp.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1920, 1080));
        var arf = wp.gameObject.AddComponent<AspectRatioFitter>();
        arf.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        arf.aspectRatio = 16f / 9f;

        if (titulo != null)
        {
            TextMeshProUGUI t = Texto(cgo.transform, "Titulo", titulo, 80, Color.white, TextAlignmentOptions.Center, true);
            Anclar(t.rectTransform, new Vector2(0.2f, 1), new Vector2(0.8f, 1), new Vector2(0, -160), new Vector2(0, -30));
        }

        T("   NuevaEscena: managers");
        managers = new GameObject("Managers");
        return cgo.transform;
    }

    // Asigna al módulo de UI las acciones del asset del proyecto (InputSystem_Actions > mapa UI),
    // igual que haría el Inspector. AssignDefaultActions() falla al llamarlo varias veces seguidas.
    private static void ConfigurarInputModule(InputSystemUIInputModule modulo)
    {
        const string rutaAcciones = "Assets/InputSystem_Actions.inputactions";
        var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(rutaAcciones);
        if (asset == null)
        {
            T("   InputSystem_Actions no encontrado: el módulo usará las acciones por defecto en runtime");
            return;
        }
        var refs = AssetDatabase.LoadAllAssetsAtPath(rutaAcciones).OfType<InputActionReference>().ToList();
        InputActionReference R(string accion)
        {
            return refs.FirstOrDefault(r => r != null && r.action != null &&
                                            r.action.actionMap != null && r.action.actionMap.name == "UI" &&
                                            r.action.name == accion);
        }

        modulo.actionsAsset = asset;
        modulo.point = R("Point");
        modulo.leftClick = R("Click");
        modulo.rightClick = R("RightClick");
        modulo.middleClick = R("MiddleClick");
        modulo.scrollWheel = R("ScrollWheel");
        modulo.move = R("Navigate");
        modulo.submit = R("Submit");
        modulo.cancel = R("Cancel");
        T("   InputModule: point=" + (modulo.point != null) + " click=" + (modulo.leftClick != null) + " scroll=" + (modulo.scrollWheel != null));
    }

    private static Button BotonVolver(Transform canvas, Navegacion nav, string escena, string texto = "Go Back")
    {
        T("   BotonVolver -> " + escena);
        Button b = Boton(canvas, "BotonVolver", texto, 36);
        Colocar((RectTransform)b.transform, new Vector2(0, 1), new Vector2(190, -80), new Vector2(280, 80));
        UnityEventTools.AddStringPersistentListener(b.onClick, nav.IrA, escena);
        return b;
    }

    private static void GuardarEscena(string nombre)
    {
        T("   SaveScene " + nombre);
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), DirEscenas + nombre + ".unity");
    }

    // ------------------------------------------------------------------ prefabs

    private static ItemKahoot CrearPrefabItemKahoot()
    {
        Image fondo = Imagen(null, "ItemKahoot", sprRedondeado, ColorItem, true);
        RectTransform root = fondo.rectTransform;
        root.sizeDelta = new Vector2(900, 120);
        LE(root, -1, 120);
        HLG(root, 20, new RectOffset(28, 20, 14, 14));

        RectTransform textos = Nuevo("Textos", root);
        VLG(textos, 2).childAlignment = TextAnchor.MiddleLeft;
        LE(textos, -1, -1, 1);
        TextMeshProUGUI nombre = Texto(textos, "Nombre", "Kahoot", 40, Color.white, TextAlignmentOptions.MidlineLeft, true);
        nombre.overflowMode = TextOverflowModes.Ellipsis;
        LE(nombre, -1, 52);
        TextMeshProUGUI info = Texto(textos, "Info", "10 questions", 24, new Color(1, 1, 1, 0.75f), TextAlignmentOptions.MidlineLeft);
        info.overflowMode = TextOverflowModes.Ellipsis;
        LE(info, -1, 32);

        Button boton = Boton(root, "Boton", "Play", 34);
        LE(boton, 200, 80);

        var item = root.gameObject.AddComponent<ItemKahoot>();
        Set(item, "textoNombre", nombre);
        Set(item, "textoInfo", info);
        Set(item, "boton", boton);
        Set(item, "textoBoton", boton.GetComponentInChildren<TMP_Text>());
        Set(item, "fondo", fondo);
        return GuardarPrefab<ItemKahoot>(root.gameObject, "ItemKahoot");
    }

    private static TMP_Text[] FilaLeaderboard(Transform padre, string nombre, bool cabecera, out Image fondo)
    {
        fondo = Imagen(padre, nombre, sprRedondeado, cabecera ? new Color(0, 0, 0, 0) : ColorItem, true);
        RectTransform root = fondo.rectTransform;
        root.sizeDelta = new Vector2(900, 64);
        LE(root, -1, cabecera ? 48 : 64);
        HLG(root, 10, new RectOffset(20, 20, 4, 4), true);

        float tam = cabecera ? 26 : 32;
        Color color = cabecera ? new Color(1, 1, 1, 0.7f) : Color.white;
        string[] titulos = { "Pos", "Player", "Score", "Correct", "Time" };
        string[] ejemplo = { "#1", "Player", "0", "0/0", "0.0 s" };
        float[] anchos = { 100, -1, 180, 150, 150 };
        var textos = new TMP_Text[titulos.Length];
        for (int i = 0; i < titulos.Length; i++)
        {
            var t = Texto(root, titulos[i], cabecera ? titulos[i] : ejemplo[i], tam, color,
                i == 1 ? TextAlignmentOptions.MidlineLeft : TextAlignmentOptions.Center, i < 3 || cabecera);
            t.overflowMode = TextOverflowModes.Ellipsis;
            LE(t, anchos[i], -1, anchos[i] < 0 ? 1 : -1);
            textos[i] = t;
        }
        return textos;
    }

    private static ItemLeaderboard CrearPrefabItemLeaderboard()
    {
        TMP_Text[] t = FilaLeaderboard(null, "ItemLeaderboard", false, out Image fondo);
        var item = fondo.gameObject.AddComponent<ItemLeaderboard>();
        Set(item, "fondo", fondo);
        Set(item, "textoPosicion", t[0]);
        Set(item, "textoJugador", t[1]);
        Set(item, "textoPuntuacion", t[2]);
        Set(item, "textoAciertos", t[3]);
        Set(item, "textoTiempo", t[4]);
        return GuardarPrefab<ItemLeaderboard>(fondo.gameObject, "ItemLeaderboard");
    }

    private static ItemPreguntaCreador CrearPrefabItemPregunta()
    {
        Image fondo = Imagen(null, "ItemPreguntaCreador", sprRedondeado, ColorItem, true);
        RectTransform root = fondo.rectTransform;
        root.sizeDelta = new Vector2(1600, 420);
        VLG(root, 12, new RectOffset(24, 24, 18, 22));

        // Cabecera
        RectTransform cab = Nuevo("Cabecera", root);
        HLG(cab, 20, null, true);
        LE(cab, -1, 56);
        TextMeshProUGUI titulo = Texto(cab, "Titulo", "Question 1", 40, Color.white, TextAlignmentOptions.MidlineLeft, true);
        LE(titulo, -1, -1, 1);
        Button borrar = Boton(cab, "BotonBorrar", "Delete", 30);
        LE(borrar, 200, -1);

        // Enunciado
        TMP_InputField pregunta = CampoTexto(root, "InputPregunta", "Write the question...", 30);
        LE(pregunta, -1, 64);

        // Respuestas 2x2, cada una con su color/forma Kahoot
        var respuestas = new TMP_InputField[Pregunta.NumRespuestas];
        for (int fila = 0; fila < 2; fila++)
        {
            RectTransform rf = Nuevo("Respuestas" + (fila + 1), root);
            HLG(rf, 14, null, true);
            LE(rf, -1, 64);
            for (int col = 0; col < 2; col++)
            {
                int i = fila * 2 + col;
                LE(MarcaRespuesta(rf, i, 64), 64, -1);
                respuestas[i] = CampoTexto(rf, "InputRespuesta" + (i + 1), "Answer " + (i + 1) + (i >= 2 ? " (optional)" : ""), 28);
                LE(respuestas[i], -1, -1, 1);
            }
        }

        // Ajustes
        RectTransform aj = Nuevo("Ajustes", root);
        HLG(aj, 16, null, true);
        LE(aj, -1, 60);
        LE(Texto(aj, "LabelCorrecta", "Correct answer:", 30, Color.white, TextAlignmentOptions.MidlineRight, true), 250, -1);
        var opciones = new List<string>();
        for (int i = 0; i < Pregunta.NumRespuestas; i++) opciones.Add((i + 1) + " - " + NombresColor[i]);
        TMP_Dropdown correcta = Desplegable(aj, "DropdownCorrecta", opciones, 28);
        LE(correcta, 260, -1);
        LE(Texto(aj, "LabelTiempo", "Time (s):", 30, Color.white, TextAlignmentOptions.MidlineRight, true), 170, -1);
        TMP_InputField duracion = CampoTexto(aj, "InputDuracion", "20", 28);
        LE(duracion, 140, -1);
        LE(Texto(aj, "Ayuda", "5 - 120 s (empty = 20). Empty answers are hidden.", 24, new Color(1, 1, 1, 0.7f), TextAlignmentOptions.MidlineLeft), -1, -1, 1);

        var item = root.gameObject.AddComponent<ItemPreguntaCreador>();
        Set(item, "textoTitulo", titulo);
        Set(item, "inputPregunta", pregunta);
        SetArray(item, "inputRespuestas", respuestas);
        Set(item, "dropdownCorrecta", correcta);
        Set(item, "inputDuracion", duracion);
        Set(item, "botonBorrar", borrar);
        Set(item, "fondo", fondo);
        return GuardarPrefab<ItemPreguntaCreador>(root.gameObject, "ItemPreguntaCreador");
    }

    // Panel con título + cabecera + scroll de leaderboard (Resultados y Leaderboards)
    private static VistaLeaderboard PanelLeaderboard(Transform canvas, Vector2 min, Vector2 max, ItemLeaderboard prefab, string titulo, out TMP_Text textoTitulo)
    {
        RectTransform panel = Panel(canvas, "PanelLeaderboard", min, max);

        textoTitulo = Texto(panel, "TituloLeaderboard", titulo, 44, Color.white, TextAlignmentOptions.Center, true);
        Anclar(textoTitulo.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -76), new Vector2(-20, -12));

        FilaLeaderboard(panel, "Cabecera", true, out Image cab);
        Anclar(cab.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -130), new Vector2(-42, -82));

        RectTransform zona = Nuevo("Zona", panel);
        Anclar(zona, Vector2.zero, Vector2.one, new Vector2(10, 10), new Vector2(-10, -134));
        ScrollRect sr = Scroll(zona, "ScrollLeaderboard");

        TextMeshProUGUI vacio = Texto(zona, "TextoVacio", "No scores yet.\nBe the first one!", 40, new Color(1, 1, 1, 0.8f));
        Rellenar(vacio.rectTransform);

        var vista = panel.gameObject.AddComponent<VistaLeaderboard>();
        Set(vista, "contenido", sr.content);
        Set(vista, "prefabItem", prefab);
        Set(vista, "textoVacio", vacio);
        Set(vista, "scroll", sr);
        return vista;
    }

    // ------------------------------------------------------------------ escenas

    private static void EscenaTitulo()
    {
        Transform canvas = NuevaEscena(null, out GameObject managers);
        var menu = managers.AddComponent<BotontesTitulo>();

        TextMeshProUGUI titulo = Texto(canvas, "Titulo", "Kahoot!", 170, Color.white, TextAlignmentOptions.Center, true);
        Anclar(titulo.rectTransform, new Vector2(0.1f, 0.68f), new Vector2(0.9f, 0.93f));
        TextMeshProUGUI sub = Texto(canvas, "Subtitulo", "Main menu", 40, new Color(1, 1, 1, 0.8f));
        Anclar(sub.rectTransform, new Vector2(0.1f, 0.62f), new Vector2(0.9f, 0.69f));

        RectTransform caja = Nuevo("Botones", canvas);
        Colocar(caja, new Vector2(0.5f, 0.33f), Vector2.zero, new Vector2(520, 5 * 90 + 4 * 22));
        VerticalLayoutGroup v = VLG(caja, 22);
        v.childForceExpandHeight = true;

        UnityEventTools.AddPersistentListener(Boton(caja, "BotonJugar", "Play", 40).onClick, menu.Jugar);
        UnityEventTools.AddPersistentListener(Boton(caja, "BotonLeaderboards", "Leaderboards", 40).onClick, menu.VerLeaderboards);
        UnityEventTools.AddPersistentListener(Boton(caja, "BotonCrear", "Create Kahoot", 40).onClick, menu.cambiarEscenaCreacionKahoot);
        UnityEventTools.AddPersistentListener(Boton(caja, "BotonAbout", "About", 40).onClick, menu.VerAbout);
        UnityEventTools.AddPersistentListener(Boton(caja, "BotonSalir", "Exit", 40).onClick, menu.Salir);

        GuardarEscena(Escenas.Titulo);
    }

    private static void EscenaSelector(ItemKahoot prefab)
    {
        Transform canvas = NuevaEscena("Select Kahoot to play", out GameObject managers);
        prefab = CargarPrefab<ItemKahoot>("ItemKahoot");
        var nav = managers.AddComponent<Navegacion>();
        var lista = managers.AddComponent<ListaKahoots>();
        BotonVolver(canvas, nav, Escenas.Titulo);

        RectTransform fila = Nuevo("FilaUsuario", canvas);
        Anclar(fila, new Vector2(0.2f, 1), new Vector2(0.8f, 1), new Vector2(0, -245), new Vector2(0, -175));
        HLG(fila, 20, null, true);
        LE(Texto(fila, "LabelUsuario", "Username:", 40, Color.white, TextAlignmentOptions.MidlineRight, true), 250, -1);
        TMP_InputField usuario = CampoTexto(fila, "InputUsuario", "Write your name...", 32);
        LE(usuario, -1, -1, 1);

        TextMeshProUGUI aviso = Texto(canvas, "TextoAviso", "", 30, Color.white);
        Anclar(aviso.rectTransform, new Vector2(0.2f, 1), new Vector2(0.8f, 1), new Vector2(0, -295), new Vector2(0, -250));

        RectTransform panel = Panel(canvas, "Panel", new Vector2(0.2f, 0.07f), new Vector2(0.8f, 0.71f));
        ScrollRect sr = Scroll(panel, "ScrollKahoots");
        TextMeshProUGUI vacio = Texto(panel, "TextoVacio", "No kahoots found.\nCreate one from the main menu!", 40, new Color(1, 1, 1, 0.8f));
        Rellenar(vacio.rectTransform, 20);

        TextMeshProUGUI ruta = Texto(canvas, "TextoRuta", "Folder:", 22, new Color(1, 1, 1, 0.6f));
        Anclar(ruta.rectTransform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(20, 6), new Vector2(-20, 40));

        Set(lista, "contenido", sr.content);
        Set(lista, "prefabItem", prefab);
        Set(lista, "inputUsuario", usuario);
        Set(lista, "textoAviso", aviso);
        Set(lista, "textoVacio", vacio);
        Set(lista, "textoRuta", ruta);

        GuardarEscena(Escenas.Selector);
    }

    private static void EscenaJuego()
    {
        Transform canvas = NuevaEscena(null, out GameObject managers);
        var juego = managers.AddComponent<JuegoKahoot>();

        // Pregunta
        Image barra = Imagen(canvas, "BarraPregunta", sprRedondeado, Color.white, true);
        Anclar(barra.rectTransform, new Vector2(0.03f, 0.78f), new Vector2(0.97f, 0.965f));
        barra.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(0, -6);
        TextMeshProUGUI pregunta = Texto(barra.transform, "TextoPregunta", "Question", 60, new Color(0.1f, 0.1f, 0.1f), TextAlignmentOptions.Center, true);
        Rellenar(pregunta.rectTransform, 20);
        pregunta.enableAutoSizing = true;
        pregunta.fontSizeMin = 28;
        pregunta.fontSizeMax = 64;

        TextMeshProUGUI progreso = Texto(canvas, "TextoProgreso", "Question 1 / 1", 36, Color.white, TextAlignmentOptions.MidlineLeft, true);
        Anclar(progreso.rectTransform, new Vector2(0.03f, 0.70f), new Vector2(0.5f, 0.77f));

        // Temporizador circular
        Image reloj = Imagen(canvas, "Temporizador", sprCirculo, Morado);
        Colocar(reloj.rectTransform, new Vector2(0.11f, 0.6f), Vector2.zero, new Vector2(220, 220));
        Image anillo = Imagen(reloj.transform, "Anillo", sprCirculo, Color.white);
        Rellenar(anillo.rectTransform);
        anillo.type = Image.Type.Filled;
        anillo.fillMethod = Image.FillMethod.Radial360;
        anillo.fillOrigin = (int)Image.Origin360.Top;
        anillo.fillClockwise = false;
        Image interior = Imagen(reloj.transform, "Interior", sprCirculo, Morado);
        Rellenar(interior.rectTransform, 16);
        TextMeshProUGUI tiempo = Texto(reloj.transform, "TextoTiempo", "20", 90, Color.white, TextAlignmentOptions.Center, true);
        Rellenar(tiempo.rectTransform);

        // Puntuación
        RectTransform caja = Panel(canvas, "Puntuacion", Vector2.zero, Vector2.zero);
        Colocar(caja, new Vector2(0.89f, 0.6f), Vector2.zero, new Vector2(280, 160));
        TextMeshProUGUI lbl = Texto(caja, "Label", "Score", 32, new Color(1, 1, 1, 0.8f));
        Anclar(lbl.rectTransform, new Vector2(0, 0.62f), new Vector2(1, 1));
        TextMeshProUGUI puntos = Texto(caja, "TextoPuntuacion", "0", 64, Color.white, TextAlignmentOptions.Center, true);
        Anclar(puntos.rectTransform, new Vector2(0, 0), new Vector2(1, 0.7f));

        // Feedback
        Image feedback = Imagen(canvas, "PanelFeedback", sprRedondeado, ColoresKahoot[3], true);
        Colocar(feedback.rectTransform, new Vector2(0.5f, 0.6f), Vector2.zero, new Vector2(820, 130));
        feedback.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(0, -6);
        TextMeshProUGUI textoFeedback = Texto(feedback.transform, "TextoFeedback", "Correct!", 60, Color.white, TextAlignmentOptions.Center, true);
        Rellenar(textoFeedback.rectTransform);

        // Respuestas estilo Kahoot
        RectTransform zona = Nuevo("Respuestas", canvas);
        Anclar(zona, new Vector2(0.03f, 0.04f), new Vector2(0.97f, 0.49f));
        Vector2[] min = { new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(0.5f, 0) };
        Vector2[] max = { new Vector2(0.5f, 1), new Vector2(1, 1), new Vector2(0.5f, 0.5f), new Vector2(1, 0.5f) };
        Vector2[] offMin = { new Vector2(0, 8), new Vector2(8, 8), Vector2.zero, new Vector2(8, 0) };
        Vector2[] offMax = { new Vector2(-8, 0), Vector2.zero, new Vector2(-8, -8), new Vector2(0, -8) };
        var botones = new BotonRespuesta[4];
        for (int i = 0; i < 4; i++)
        {
            Image img = Imagen(zona, "Respuesta" + (i + 1) + "_" + NombresColor[i], sprRedondeado, ColoresKahoot[i], true);
            Anclar(img.rectTransform, min[i], max[i], offMin[i], offMax[i]);
            img.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(0, -6);
            var grupo = img.gameObject.AddComponent<CanvasGroup>();
            var btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
            cb.pressedColor = new Color(0.75f, 0.75f, 0.75f);
            cb.selectedColor = Color.white;
            cb.disabledColor = Color.white;
            btn.colors = cb;
            var nav = btn.navigation;
            nav.mode = Navigation.Mode.None; // solo ratón
            btn.navigation = nav;

            Image forma = Imagen(img.transform, "Forma", sprFormas[i], Color.white);
            Colocar(forma.rectTransform, new Vector2(0, 0.5f), new Vector2(75, 0), new Vector2(90, 90));
            forma.raycastTarget = false;
            TextMeshProUGUI texto = Texto(img.transform, "Texto", "Answer", 44, Color.white, TextAlignmentOptions.MidlineLeft, true);
            Anclar(texto.rectTransform, Vector2.zero, Vector2.one, new Vector2(150, 10), new Vector2(-120, -10));
            texto.enableAutoSizing = true;
            texto.fontSizeMin = 24;
            texto.fontSizeMax = 46;
            Image resultado = Imagen(img.transform, "Resultado", sprCheck, Color.white);
            Colocar(resultado.rectTransform, new Vector2(1, 0.5f), new Vector2(-65, 0), new Vector2(80, 80));
            resultado.raycastTarget = false;
            resultado.gameObject.SetActive(false);

            var br = img.gameObject.AddComponent<BotonRespuesta>();
            Set(br, "boton", btn);
            Set(br, "texto", texto);
            Set(br, "iconoResultado", resultado);
            Set(br, "grupo", grupo);
            Set(br, "spriteCorrecto", sprCheck);
            Set(br, "spriteIncorrecto", sprCruz);
            botones[i] = br;
        }

        Set(juego, "textoPregunta", pregunta);
        Set(juego, "textoProgreso", progreso);
        Set(juego, "textoTiempo", tiempo);
        Set(juego, "anilloTiempo", anillo);
        Set(juego, "textoPuntuacion", puntos);
        Set(juego, "panelFeedback", feedback.gameObject);
        Set(juego, "fondoFeedback", feedback);
        Set(juego, "textoFeedback", textoFeedback);
        SetArray(juego, "botones", botones);
        feedback.gameObject.SetActive(false);

        GuardarEscena(Escenas.Juego);
    }

    private static void EscenaResultados(ItemLeaderboard prefab)
    {
        Transform canvas = NuevaEscena("Results", out GameObject managers);
        prefab = CargarPrefab<ItemLeaderboard>("ItemLeaderboard");
        var nav = managers.AddComponent<Navegacion>();
        var res = managers.AddComponent<PantallaResultados>();

        RectTransform panel = Panel(canvas, "PanelResumen", new Vector2(0.04f, 0.07f), new Vector2(0.4f, 0.84f));
        VLG(panel, 8, new RectOffset(30, 30, 26, 30));
        TextMeshProUGUI kahoot = Texto(panel, "TextoKahoot", "Kahoot", 44, Color.white, TextAlignmentOptions.Center, true);
        kahoot.overflowMode = TextOverflowModes.Ellipsis;
        LE(kahoot, -1, 60);
        TextMeshProUGUI jugador = Texto(panel, "TextoJugador", "Player", 34, new Color(1, 1, 1, 0.8f));
        LE(jugador, -1, 46);
        TextMeshProUGUI puntos = Texto(panel, "TextoPuntuacion", "0 pts", 90, new Color(1f, 0.82f, 0.2f), TextAlignmentOptions.Center, true);
        LE(puntos, -1, 110);
        TextMeshProUGUI posicion = Texto(panel, "TextoPosicion", "Position #1", 44, Color.white, TextAlignmentOptions.Center, true);
        LE(posicion, -1, 60);
        TextMeshProUGUI aciertos = Texto(panel, "TextoAciertos", "Correct answers", 32, Color.white);
        LE(aciertos, -1, 44);
        TextMeshProUGUI tiempo = Texto(panel, "TextoTiempo", "Time", 32, Color.white);
        LE(tiempo, -1, 44);
        LE(Nuevo("Espacio", panel), -1, 10, -1, 1);

        Button otra = Boton(panel, "BotonOtraVez", "Play again", 36);
        LE(otra, -1, 80);
        UnityEventTools.AddPersistentListener(otra.onClick, res.JugarOtraVez);
        Button lbs = Boton(panel, "BotonLeaderboards", "Leaderboards", 36);
        LE(lbs, -1, 80);
        UnityEventTools.AddStringPersistentListener(lbs.onClick, nav.IrA, Escenas.Leaderboards);
        Button menu = Boton(panel, "BotonMenu", "Main menu", 36);
        LE(menu, -1, 80);
        UnityEventTools.AddStringPersistentListener(menu.onClick, nav.IrA, Escenas.Titulo);

        VistaLeaderboard vista = PanelLeaderboard(canvas, new Vector2(0.43f, 0.07f), new Vector2(0.96f, 0.84f), prefab, "Leaderboard", out _);

        Set(res, "textoKahoot", kahoot);
        Set(res, "textoJugador", jugador);
        Set(res, "textoPuntuacion", puntos);
        Set(res, "textoAciertos", aciertos);
        Set(res, "textoTiempo", tiempo);
        Set(res, "textoPosicion", posicion);
        Set(res, "vistaLeaderboard", vista);

        GuardarEscena(Escenas.Resultados);
    }

    private static void EscenaLeaderboards(ItemKahoot prefabKahoot, ItemLeaderboard prefabLb)
    {
        Transform canvas = NuevaEscena("Leaderboards", out GameObject managers);
        prefabKahoot = CargarPrefab<ItemKahoot>("ItemKahoot");
        prefabLb = CargarPrefab<ItemLeaderboard>("ItemLeaderboard");
        var nav = managers.AddComponent<Navegacion>();
        var pantalla = managers.AddComponent<PantallaLeaderboards>();
        BotonVolver(canvas, nav, Escenas.Titulo);

        RectTransform izq = Panel(canvas, "PanelKahoots", new Vector2(0.03f, 0.06f), new Vector2(0.37f, 0.84f));
        TextMeshProUGUI lbl = Texto(izq, "TituloKahoots", "Kahoots", 44, Color.white, TextAlignmentOptions.Center, true);
        Anclar(lbl.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -76), new Vector2(-20, -12));
        RectTransform zona = Nuevo("Zona", izq);
        Anclar(zona, Vector2.zero, Vector2.one, new Vector2(0, 0), new Vector2(0, -80));
        ScrollRect sr = Scroll(zona, "ScrollKahoots");
        TextMeshProUGUI sinKahoots = Texto(zona, "TextoVacio", "No kahoots found", 36, new Color(1, 1, 1, 0.8f));
        Rellenar(sinKahoots.rectTransform, 20);

        VistaLeaderboard vista = PanelLeaderboard(canvas, new Vector2(0.39f, 0.06f), new Vector2(0.97f, 0.84f), prefabLb, "Kahoot", out TMP_Text titulo);

        Set(pantalla, "contenidoKahoots", sr.content);
        Set(pantalla, "prefabItem", prefabKahoot);
        Set(pantalla, "textoTituloKahoot", titulo);
        Set(pantalla, "textoSinKahoots", sinKahoots);
        Set(pantalla, "vistaLeaderboard", vista);

        GuardarEscena(Escenas.Leaderboards);
    }

    private static void EscenaCreador(ItemPreguntaCreador prefab)
    {
        Transform canvas = NuevaEscena("Kahoot creator", out GameObject managers);
        prefab = CargarPrefab<ItemPreguntaCreador>("ItemPreguntaCreador");
        var nav = managers.AddComponent<Navegacion>();
        var creador = managers.AddComponent<crearKahoot>();
        BotonVolver(canvas, nav, Escenas.Titulo);

        RectTransform barra = Nuevo("BarraSuperior", canvas);
        Anclar(barra, new Vector2(0.04f, 1), new Vector2(0.96f, 1), new Vector2(0, -252), new Vector2(0, -178));
        HLG(barra, 20, null, true);
        LE(Texto(barra, "LabelNombre", "Name:", 40, Color.white, TextAlignmentOptions.MidlineRight, true), 140, -1);
        TMP_InputField nombre = CampoTexto(barra, "InputNombre", "Kahoot name...", 32);
        LE(nombre, -1, -1, 1);
        TextMeshProUGUI contador = Texto(barra, "TextoContador", "1 question", 30, Color.white);
        LE(contador, 200, -1);
        Button anadir = Boton(barra, "BotonAnadir", "+ Add question", 32);
        LE(anadir, 290, -1);
        UnityEventTools.AddPersistentListener(anadir.onClick, creador.AnadirPregunta);
        Button guardar = Boton(barra, "BotonGuardar", "Save Kahoot", 32);
        LE(guardar, 260, -1);
        UnityEventTools.AddPersistentListener(guardar.onClick, creador.Guardar);

        TextMeshProUGUI feedback = Texto(canvas, "TextoFeedback", "", 30, Color.white);
        Anclar(feedback.rectTransform, new Vector2(0.04f, 1), new Vector2(0.96f, 1), new Vector2(0, -302), new Vector2(0, -258));

        RectTransform panel = Panel(canvas, "Panel", new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.71f));
        ScrollRect sr = Scroll(panel, "ScrollPreguntas", 16);

        Set(creador, "inputNombre", nombre);
        Set(creador, "contenido", sr.content);
        Set(creador, "prefabPregunta", prefab);
        Set(creador, "scroll", sr);
        Set(creador, "textoFeedback", feedback);
        Set(creador, "textoContador", contador);
        Set(creador, "botonGuardar", guardar);

        GuardarEscena(Escenas.Creador);
    }

    private static void EscenaAbout()
    {
        Transform canvas = NuevaEscena("About", out GameObject managers);
        var nav = managers.AddComponent<Navegacion>();
        BotonVolver(canvas, nav, Escenas.Titulo);

        RectTransform panel = Panel(canvas, "Panel", new Vector2(0.18f, 0.1f), new Vector2(0.82f, 0.8f));
        TextMeshProUGUI texto = Texto(panel, "TextoAbout",
            "<b><size=56>Kahoot! – Projecte II</size></b>\n\n" +
            "Quiz game inspired by Kahoot made with Unity.\n\n" +
            "• Kahoots are read from <b>.json</b> files in the persistent data folder (Kahoots).\n" +
            "• Each kahoot has its own <b>.xml</b> leaderboard (Leaderboards folder).\n" +
            "• Points: up to 1000 per question, the faster you answer the more you get.\n" +
            "• Create your own kahoots with as many questions as you want.\n\n" +
            "<b>Author:</b> ____________\n" +
            "<b>Course:</b> Accés a dades – Projecte II",
            34, Color.white, TextAlignmentOptions.TopLeft);
        Rellenar(texto.rectTransform, 50);

        GuardarEscena(Escenas.About);
    }
}
