
using UnityEngine;
using UnityEngine.UI;

public class ObjectGrabController : MonoBehaviour
{
    [Header("Referencias")]
    //public UDPReceive udpReceive;
    UDPReceive udpReceive => UDPReceive.Instance;
    public Camera camara;
    public TextAnimationController controladorTexto;

    [Header("Configuración")]
    public float tiempoParaAgarrar = 2f;
    public float distanciaRaycast = 100f;

    [Header("Conexión física")]
    public float tiempoParaConectar = 3f;
    public float radioDeteccionConexion = 2f;

    [Header("Estado")]
    public bool objetoAgarrado = false;

    private InteractableObject objetoActual;
    private InteractableObject objetoEnMano;
    private InteractableObject objetoDebajo;

    private float tiempoSobreObjeto = 0f;
    private float tiempoSobreConexion = 0f;

    private Vector3 offsetObjeto;
    private Vector3 posicionObjetivoObjeto;

    private Rigidbody rbObjeto;
    private bool estadoKinematicAnterior;

    private Transform padreOriginal;
    private Outline outlineActual;

    [Header("Colores del Outline")]
    public Color colorNormal = Color.white;
    public Color colorConectando = Color.green;

    private Color colorOriginalOutline;



    [Header("Indicador circular del cursor")]
    public float tamanoIndicador = 75f;
    public float transparenciaIndicador = 0.65f;
    public Color colorIndicador = Color.white;
    public Color colorIndicadorCompleto = Color.green;

    private Canvas canvasIndicador;
    private Image imagenProgreso;
    private Image puntoCursor;


    

void CrearIndicador()
{
    // Canvas que siempre se dibuja por encima de la escena.
    GameObject canvasGO = new GameObject("CanvasIndicadorCursor");

    canvasIndicador = canvasGO.AddComponent<Canvas>();
    canvasIndicador.renderMode = RenderMode.ScreenSpaceOverlay;
    canvasIndicador.sortingOrder = 32767;

    canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();

    // Imagen circular de progreso.
    GameObject anilloGO = new GameObject("AnilloProgreso");
    anilloGO.transform.SetParent(canvasGO.transform, false);

    imagenProgreso = anilloGO.AddComponent<Image>();

    imagenProgreso.sprite = CrearSpriteAnillo();
    imagenProgreso.type = Image.Type.Filled;
    imagenProgreso.fillMethod = Image.FillMethod.Radial360;
    imagenProgreso.fillOrigin = (int)Image.Origin360.Top;
    imagenProgreso.fillClockwise = true;
    imagenProgreso.fillAmount = 0;

    imagenProgreso.color = colorIndicador;

    RectTransform rectAnillo =
        imagenProgreso.rectTransform;

    rectAnillo.sizeDelta =
        new Vector2(tamanoIndicador, tamanoIndicador);

    // Punto central del cursor.
    GameObject puntoGO = new GameObject("PuntoCursor");
    puntoGO.transform.SetParent(canvasGO.transform, false);

    puntoCursor = puntoGO.AddComponent<Image>();
    puntoCursor.sprite = CrearSpriteCirculo();

    puntoCursor.color = new Color(
        1f, 1f, 1f, transparenciaIndicador
    );

    puntoCursor.rectTransform.sizeDelta =
        new Vector2(9f, 9f);

    canvasGO.SetActive(false);
}

Sprite CrearSpriteAnillo()
{
    int resolucion = 128;
    Texture2D textura = new Texture2D(
        resolucion, resolucion,
        TextureFormat.RGBA32, false
    );

    for (int y = 0; y < resolucion; y++)
    {
        for (int x = 0; x < resolucion; x++)
        {
            float dx = (x - resolucion / 2f) /
                       (resolucion / 2f);

            float dy = (y - resolucion / 2f) /
                       (resolucion / 2f);

            float distancia = Mathf.Sqrt(dx * dx + dy * dy);

            bool dentro = distancia <= 0.95f &&
                          distancia >= 0.78f;

            textura.SetPixel(
                x, y,
                dentro ? Color.white : Color.clear
            );
        }
    }

    textura.Apply();

    return Sprite.Create(
        textura,
        new Rect(0, 0, resolucion, resolucion),
        new Vector2(0.5f, 0.5f)
    );
}

Sprite CrearSpriteCirculo()
{
    int resolucion = 64;
    Texture2D textura = new Texture2D(
        resolucion, resolucion,
        TextureFormat.RGBA32, false
    );

    for (int y = 0; y < resolucion; y++)
    {
        for (int x = 0; x < resolucion; x++)
        {
            float dx = (x - resolucion / 2f) /
                       (resolucion / 2f);

            float dy = (y - resolucion / 2f) /
                       (resolucion / 2f);

            float distancia = Mathf.Sqrt(dx * dx + dy * dy);

            textura.SetPixel(
                x, y,
                distancia <= 0.9f ? Color.white : Color.clear
            );
        }
    }

    textura.Apply();

    return Sprite.Create(
        textura,
        new Rect(0, 0, resolucion, resolucion),
        new Vector2(0.5f, 0.5f)
    );
}


void ActualizarIndicador()
{
    if (imagenProgreso == null)
        return;

    Vector3 posicionPantalla =
        camara.WorldToScreenPoint(transform.position);

    if (posicionPantalla.z <= 0)
    {
        canvasIndicador.gameObject.SetActive(false);
        return;
    }

    canvasIndicador.gameObject.SetActive(true);

    imagenProgreso.rectTransform.position =
        posicionPantalla;

    puntoCursor.rectTransform.position =
        posicionPantalla;

    float progreso = Mathf.Clamp01(
        tiempoSobreObjeto / tiempoParaAgarrar
    );

    imagenProgreso.fillAmount = progreso;

    imagenProgreso.color = Color.Lerp(
        colorIndicador,
        colorIndicadorCompleto,
        progreso
    );

    Color color = imagenProgreso.color;
    color.a = transparenciaIndicador;
    imagenProgreso.color = color;
}

void OcultarIndicador()
{
    if (canvasIndicador != null)
        canvasIndicador.gameObject.SetActive(false);
}


void AlinearObjetos(
    InteractableObject superior,
    InteractableObject inferior)
{
    Collider colliderSuperior =
        superior.GetComponentInChildren<Collider>();

    Collider colliderInferior =
        inferior.GetComponentInChildren<Collider>();

    if (colliderSuperior == null || colliderInferior == null)
    {
        Debug.LogWarning("Falta un Collider para alinear los objetos.");
        return;
    }

    Bounds boundsSuperior = colliderSuperior.bounds;
    Bounds boundsInferior = colliderInferior.bounds;

    // Centrar el objeto superior en X y Z.
    float diferenciaX =
        boundsInferior.center.x - boundsSuperior.center.x;

    float diferenciaZ =
        boundsInferior.center.z - boundsSuperior.center.z;

    // Colocar la base del superior exactamente sobre
    // la parte superior del inferior.
    float diferenciaY =
        boundsInferior.max.y - boundsSuperior.min.y;

    Vector3 desplazamiento = new Vector3(
        diferenciaX,
        diferenciaY,
        diferenciaZ
    );

    superior.transform.position += desplazamiento;

    // Actualizar la física de Unity.
    Physics.SyncTransforms();

    // Actualizar el objetivo del objeto agarrado para que
    // el cursor no lo devuelva a su posición anterior.
    offsetObjeto =
        superior.transform.position - transform.position;

    posicionObjetivoObjeto = superior.transform.position;

    Debug.Log("Objetos alineados correctamente.");
}


void Start()
{
    CrearIndicador();
}
    void Update()
    {
        if (udpReceive == null || camara == null)
            return;

        int gesto = udpReceive.digit;

        // Gesto 5: soltar objeto
        if (gesto == 5 && objetoAgarrado)
        {
            SoltarObjeto();
            return;
        }

        // Si ya tenemos un objeto agarrado
        if (objetoAgarrado)
        {
            posicionObjetivoObjeto = transform.position + offsetObjeto;

            // Solo conectar mientras se mantiene el gesto 0
            if (gesto == 0)
                ComprobarConexion();
            else
                ReiniciarConexion();

            return;
        }

        // Gesto 0: buscar objeto
        if (gesto == 0)
        {
            DetectarObjeto();
        }
        else
        {
            objetoActual = null;
            tiempoSobreObjeto = 0f;
            ActualizarOutline(null);
            OcultarIndicador();
        }
    }

    void FixedUpdate()
    {
        if (!objetoAgarrado || objetoEnMano == null)
            return;

        // Mover el objeto mediante el sistema físico de Unity
        if (rbObjeto != null)
        {
            rbObjeto.MovePosition(posicionObjetivoObjeto);
        }
        else
        {
            objetoEnMano.transform.position = posicionObjetivoObjeto;
        }
    }

    // --------------------------------------------------
    // DETECCIÓN DE OBJETOS
    // --------------------------------------------------

    [Header("Detección")]
    public float radioDeteccion = 0.15f;

    void DetectarObjeto()
    {
        Collider[] impactos = Physics.OverlapSphere(
            transform.position,
            radioDeteccion
        );

        InteractableObject objetoDetectado = null;
        float distanciaCercana = float.MaxValue;

        foreach (Collider impacto in impactos)
        {
            InteractableObject interactuable =
                impacto.GetComponentInParent<InteractableObject>();

            if (interactuable == null)
                continue;

            float distancia = Vector3.Distance(
                transform.position,
                impacto.ClosestPoint(transform.position)
            );

            if (distancia < distanciaCercana)
            {
                objetoDetectado = interactuable;
                distanciaCercana = distancia;
            }
        }

        if (objetoDetectado == null)
        {
            objetoActual = null;
            tiempoSobreObjeto = 0f;
            ActualizarOutline(null);
            OcultarIndicador();
            return;
        }

        if (objetoActual != objetoDetectado)
        {
            objetoActual = objetoDetectado;
            tiempoSobreObjeto = 0f;
            ActualizarOutline(objetoActual);
            OcultarIndicador();
        }

        tiempoSobreObjeto += Time.deltaTime;

// Actualizar el círculo de progreso.
ActualizarIndicador();

// Cuando se completa el círculo, agarrar.
if (tiempoSobreObjeto >= tiempoParaAgarrar)
{
    OcultarIndicador();
    AgarrarObjeto(objetoActual);
}
    }

    // --------------------------------------------------
    // OUTLINE
    // --------------------------------------------------

    void ActualizarOutline(InteractableObject nuevoObjeto)
    {
        if (outlineActual != null)
        {
            outlineActual.enabled = false;
            outlineActual = null;
        }

        if (nuevoObjeto == null)
            return;

        outlineActual = nuevoObjeto.GetComponentInChildren<Outline>();

        if (outlineActual != null)
        {
            outlineActual.enabled = true;
        }


        outlineActual = nuevoObjeto.GetComponent<Outline>();

        if (outlineActual != null)
        {
            colorOriginalOutline = outlineActual.OutlineColor;
            outlineActual.OutlineColor = colorNormal;
            outlineActual.enabled = true;
        }
    }

    // --------------------------------------------------
    // AGARRAR OBJETO
    // --------------------------------------------------

    void AgarrarObjeto(InteractableObject objeto)
    {
        if (objeto == null)
            return;

        // Al agarrar un objeto conectado, se eliminan
        // sus conexiones físicas para permitir separarlo.
        DesconectarObjeto(objeto);

        objetoEnMano = objeto;
        objetoActual = objeto;

        ActualizarOutline(objeto);

        objetoAgarrado = true;

        if (controladorTexto != null)
        {
            controladorTexto.MostrarMensajeUnaVez(
                "objeto_agarrado",
                "¡Ya lo tienes!"
            );

            controladorTexto.MostrarMensajeUnaVez(
                "boton_spawn",
                "Ves ese boton arriba...Pasa el objeto por encima"
            );
        }

        offsetObjeto =
            objeto.transform.position - transform.position;

        posicionObjetivoObjeto = objeto.transform.position;

        padreOriginal = objeto.transform.parent;

        rbObjeto = objeto.GetComponent<Rigidbody>();

        if (rbObjeto != null)
        {
            estadoKinematicAnterior = rbObjeto.isKinematic;

            rbObjeto.linearVelocity = Vector3.zero;
            rbObjeto.angularVelocity = Vector3.zero;

            // Mientras está agarrado, el cursor controla
            // su posición.
            rbObjeto.isKinematic = true;
        }

        tiempoSobreObjeto = 0f;

        Debug.Log("Objeto agarrado: " + objeto.name);
        OcultarIndicador();
    }

    // --------------------------------------------------
    // CONEXIÓN FÍSICA
    // --------------------------------------------------

void ComprobarConexion()
    {
        if (objetoEnMano == null || rbObjeto == null)
            return;

        Collider[] impactos = Physics.OverlapSphere(
            objetoEnMano.transform.position,
            radioDeteccionConexion
        );

        InteractableObject candidato = null;
        float distanciaCercana = float.MaxValue;

        foreach (Collider impacto in impactos)
        {
            InteractableObject posibleObjeto =
                impacto.GetComponentInParent<InteractableObject>();

            if (posibleObjeto == null || posibleObjeto == objetoEnMano)
                continue;

            // Ambos cubos deben tener el mismo valor.
            if (posibleObjeto.value != objetoEnMano.value)
                continue;

            Rigidbody rbCandidato =
                posibleObjeto.GetComponent<Rigidbody>();

            // Ambos objetos necesitan Rigidbody.
            if (rbCandidato == null)
                continue;

            // Evitar conectar objetos del mismo conjunto físico.
            if (rbCandidato == rbObjeto)
                continue;

            // Buscar un objeto que esté debajo del objeto agarrado.
            if (posibleObjeto.transform.position.y >=
                objetoEnMano.transform.position.y)
                continue;

            float distancia = Vector3.Distance(
                objetoEnMano.transform.position,
                impacto.ClosestPoint(objetoEnMano.transform.position)
            );

            if (distancia < distanciaCercana)
            {
                candidato = posibleObjeto;
                distanciaCercana = distancia;
            }
        }

        if (candidato == null)
        {
            ReiniciarConexion();
            return;
        }

        if (objetoDebajo != candidato)
        {
            objetoDebajo = candidato;
            tiempoSobreConexion = 0f;

            if (outlineActual != null)
                outlineActual.OutlineColor = colorConectando;

            Debug.Log("Preparando conexión con: " + candidato.name);
        }

        tiempoSobreConexion += Time.deltaTime;

        if (tiempoSobreConexion >= tiempoParaConectar)
        {
            ConectarObjetos(objetoEnMano, objetoDebajo);
        }
    }


    void ConectarObjetos(
        InteractableObject superior,
        InteractableObject inferior)
    {
        if (superior == null || inferior == null)
            return;

        Rigidbody rbSuperior = superior.GetComponent<Rigidbody>();
        Rigidbody rbInferior = inferior.GetComponent<Rigidbody>();

        if (rbSuperior == null || rbInferior == null)
        {
            Debug.LogWarning("Los dos objetos necesitan Rigidbody.");
            ReiniciarConexion();
            return;
        }

        AlinearObjetos(superior, inferior);

        // Evitar crear una conexión duplicada.
        FixedJoint[] conexiones =
            superior.GetComponents<FixedJoint>();

        foreach (FixedJoint conexion in conexiones)
        {
            if (conexion.connectedBody == rbInferior)
            {
                Debug.Log("Los objetos ya están conectados.");
                ReiniciarConexion();
                return;
            }
        }

        // Crear una conexión física entre ambos objetos.
        FixedJoint joint = superior.gameObject.AddComponent<FixedJoint>();

        joint.connectedBody = rbInferior;
        joint.enableCollision = true;
        joint.breakForce = Mathf.Infinity;
        joint.breakTorque = Mathf.Infinity;

        Debug.Log(
            "Conexión física creada: " +
            superior.name + " -> " + inferior.name
        );

        ReiniciarConexion();
    }

    void ReiniciarConexion()
    {
        objetoDebajo = null;
        tiempoSobreConexion = 0f;

        if (outlineActual != null)
            outlineActual.OutlineColor = colorNormal;
    }
    // --------------------------------------------------
    // DESCONECTAR OBJETOS
    // --------------------------------------------------

    void DesconectarObjeto(InteractableObject objeto)
    {
        if (objeto == null)
            return;

        Rigidbody rb = objeto.GetComponent<Rigidbody>();

        if (rb == null)
            return;

        // Buscar todos los FixedJoint de la escena.
        FixedJoint[] conexiones =
            FindObjectsOfType<FixedJoint>();

        foreach (FixedJoint conexion in conexiones)
        {
            Rigidbody rbPropio =
                conexion.GetComponent<Rigidbody>();

            // Eliminar conexiones donde el objeto agarrado
            // sea el cuerpo principal o el cuerpo conectado.
            if (rbPropio == rb || conexion.connectedBody == rb)
            {
                Debug.Log(
                    "Conexión eliminada de: " + objeto.name
                );

                Destroy(conexion);
            }
        }
    }

    // --------------------------------------------------
    // SOLTAR OBJETO
    // --------------------------------------------------

    public void SoltarObjeto()
    {
        if (objetoEnMano == null)
            return;

        objetoEnMano.transform.SetParent(padreOriginal, true);

        if (rbObjeto != null)
        {
            rbObjeto.isKinematic = estadoKinematicAnterior;
        }

        Debug.Log("Objeto soltado: " + objetoEnMano.name);

        objetoEnMano = null;
        objetoActual = null;

        rbObjeto = null;
        padreOriginal = null;

        objetoAgarrado = false;
        tiempoSobreObjeto = 0f;

        ReiniciarConexion();
        ActualizarOutline(null);
    }

    // --------------------------------------------------
    // REFERENCIA PARA OTROS SCRIPTS
    // --------------------------------------------------

    public SpawnerContainer getObjetoMano()
    {
        if (objetoAgarrado == true && objetoEnMano != null)
        {
            Debug.Log("Se obtuvo");
            return objetoEnMano.SOButton;
        }

        return null;
    }


    public void ReiniciarAgarre()
{
    if (objetoAgarrado)
        SoltarObjeto();

    objetoActual = null;
    objetoEnMano = null;
    objetoDebajo = null;

    tiempoSobreObjeto = 0f;
    tiempoSobreConexion = 0f;

    ReiniciarConexion();
    ActualizarOutline(null);
    OcultarIndicador();
}
}
