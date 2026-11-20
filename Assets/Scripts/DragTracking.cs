
using UnityEngine;
using System.Globalization;

public class DragTracking : MonoBehaviour
{
    UDPReceive udpReceive => UDPReceive.Instance;

    [Header("Sprites del cursor")]
    public SpriteRenderer spriteRenderer;
    public Sprite spriteGesto0;
    public Sprite spriteGesto5;

    private int ultimoGesto = -1;

    [Header("Mensajes")]
    public TextAnimationController controladorTexto;

    private enum EstadoTutorial
    {
        EsperandoCentro,
        EsperandoDialogoCentro,
        EsperandoGestos,
        EsperandoDialogoGestos,
        MovimientoHorizontal,
        EsperandoDialogoHorizontal,
        MovimientoVertical,
        EsperandoDialogoVertical,
        MovimientoProfundidad,
        EsperandoDialogoFinal,
        Completado
    }

    private EstadoTutorial estadoActual = EstadoTutorial.EsperandoCentro;

    private bool mensajeCentroMostrado = false;
    private bool manoCentrada = false;

    private bool gesto0Detectado = false;
    private bool gesto5Detectado = false;

    private bool derechaDetectada = false;
    private bool izquierdaDetectada = false;

    private bool arribaDetectada = false;
    private bool abajoDetectada = false;

    private bool adelanteDetectado = false;
    private bool atrasDetectado = false;

    [Header("Detección de movimientos")]
    public float umbralMovimientoX = 0.08f;
    public float umbralMovimientoY = 0.08f;
    public float umbralMovimientoZ = 0.08f;

    private bool referenciaMovimientoInicializada = false;
    private Vector2 referenciaMovimientoXY;
    private float referenciaMovimientoZ;

    [Header("Posición en profundidad")]
    public float zLejano = 30f;
    public float zCercano = -30f;

    [Header("Suavizado")]
    public float suavizado = 4f;

    [Header("Control de sensibilidad")]
    [Range(0f, 0.1f)]
    public float zonaMuerta = 0.01f;

    [Header("Sensibilidad del cursor")]
    [Range(0.1f, 1f)]
    public float sensibilidadX = 0.5f;

    [Range(0.1f, 1f)]
    public float sensibilidadY = 0.5f;

    [Header("Límites de pantalla")]
    public Camera camara;

    [Range(0f, 0.1f)]
    public float margenPantalla = 0.03f;

    private Vector3 posicionObjetivo;

    private Vector2 ultimaEntrada;
    private bool entradaInicializada = false;

    [Header("Validación del centro")]
    public float centroX = 0.05f;
    public float centroY = 0.05f;
    public float toleranciaCentro = 0.10f;


    [Header("Rotación de cámara")]
public Transform camaraRotacion;

public float rotacionMaxima = 12f;
public float suavizadoRotacion = 3f;

private Quaternion rotacionInicialCamara;

    private void Start()
    {
        if (controladorTexto == null)
        {
            Debug.LogError("DragTracking: No se ha asignado TextAnimationController.");
        }

        if (udpReceive == null)
        {
            Debug.LogError("DragTracking: No se ha asignado UDPReceive.");
        }

        if (camaraRotacion != null)
        {
            rotacionInicialCamara = camaraRotacion.localRotation;
        }
        else
        {
            Debug.LogWarning("DragTracking: No se ha asignado la cámara para rotación.");
        }
    }

    void MostrarMensaje(string id, string mensaje)
    {
        if (controladorTexto != null)
        {
            controladorTexto.MostrarMensajeUnaVez(id, mensaje);
        }
    }

    bool DialogoTerminado()
    {
        return controladorTexto == null ||
               !controladorTexto.EstaProcesandoCola;
    }

    // =====================================
    // CENTRAR LA MANO
    // =====================================

    void DetectarPrimeraMano(float x, float y)
    {
        if (estadoActual != EstadoTutorial.EsperandoCentro)
            return;

        if (!mensajeCentroMostrado)
        {
            mensajeCentroMostrado = true;

            MostrarMensaje(
                "centrar_mano",
                "¡Coloca tu mano en el centro de la pantalla!"
            );
        }

        bool estaEnCentro =
            Mathf.Abs(x - centroX) <= toleranciaCentro &&
            Mathf.Abs(y - centroY) <= toleranciaCentro;

        if (!estaEnCentro)
            return;

        manoCentrada = true;

        estadoActual = EstadoTutorial.EsperandoDialogoCentro;

        MostrarMensaje(
            "mano_centrada",
            "¡Muy bien! Ahora abre y cierra tu mano."
        );
    }

    // =====================================
    // CONTROL DE LOS DIÁLOGOS
    // =====================================

    void ActualizarEstadoDialogos()
    {
        if (!DialogoTerminado())
            return;

        switch (estadoActual)
        {
            case EstadoTutorial.EsperandoDialogoCentro:

                estadoActual = EstadoTutorial.EsperandoGestos;
                Debug.Log("Tutorial: esperando los gestos 0 y 5.");
                break;

            case EstadoTutorial.EsperandoDialogoGestos:

                ReiniciarReferenciaMovimiento();
                estadoActual = EstadoTutorial.MovimientoHorizontal;

                Debug.Log("Tutorial: movimiento horizontal habilitado.");
                break;

            case EstadoTutorial.EsperandoDialogoHorizontal:

                ReiniciarReferenciaMovimiento();
                estadoActual = EstadoTutorial.MovimientoVertical;

                Debug.Log("Tutorial: movimiento vertical habilitado.");
                break;

            case EstadoTutorial.EsperandoDialogoVertical:

                ReiniciarReferenciaMovimiento();
                estadoActual = EstadoTutorial.MovimientoProfundidad;

                Debug.Log("Tutorial: movimiento de profundidad habilitado.");
                break;

            case EstadoTutorial.EsperandoDialogoFinal:

                estadoActual = EstadoTutorial.Completado;
                OperationsManager.Instance.operacionesHabilitadas = true;

                Debug.Log("Tutorial de movimientos completado.");
                break;
        }
    }

    // =====================================
    // REINICIAR REFERENCIA DE MOVIMIENTO
    // =====================================

    void ReiniciarReferenciaMovimiento()
    {
        referenciaMovimientoInicializada = false;
    }

    // =====================================
    // DETECTAR MOVIMIENTOS
    // =====================================

    void DetectarMovimientos(float x, float y, float z)
    {
        if (!manoCentrada || !gesto0Detectado || !gesto5Detectado)
            return;

        if (!DialogoTerminado())
            return;

        Vector2 posicionActual = new Vector2(x, y);

        // Establecer referencia al comenzar cada ejercicio
        if (!referenciaMovimientoInicializada)
        {
            referenciaMovimientoXY = posicionActual;
            referenciaMovimientoZ = z;

            referenciaMovimientoInicializada = true;
            return;
        }

        float diferenciaX = x - referenciaMovimientoXY.x;
        float diferenciaY = y - referenciaMovimientoXY.y;
        float diferenciaZ = z - referenciaMovimientoZ;

        // =====================================
        // MOVIMIENTO HORIZONTAL
        // =====================================

        if (estadoActual == EstadoTutorial.MovimientoHorizontal)
        {
            if (diferenciaX >= umbralMovimientoX)
                derechaDetectada = true;

            if (diferenciaX <= -umbralMovimientoX)
                izquierdaDetectada = true;

            if (derechaDetectada && izquierdaDetectada)
            {
                estadoActual = EstadoTutorial.EsperandoDialogoHorizontal;

                MostrarMensaje(
                    "movimiento_horizontal",
                    "¡Eso! Ahora mueve tu mano de arriba a abajo."
                );

                Debug.Log("Movimiento horizontal completado.");
            }

            return;
        }

        // =====================================
        // MOVIMIENTO VERTICAL
        // =====================================

        if (estadoActual == EstadoTutorial.MovimientoVertical)
        {
            if (diferenciaY >= umbralMovimientoY)
                arribaDetectada = true;

            if (diferenciaY <= -umbralMovimientoY)
                abajoDetectada = true;

            if (arribaDetectada && abajoDetectada)
            {
                estadoActual = EstadoTutorial.EsperandoDialogoVertical;

                MostrarMensaje(
                    "movimiento_vertical",
                    "¡Excelente! Ahora mueve tu mano hacia delante y atrás."
                );

                Debug.Log("Movimiento vertical completado.");
            }

            return;
        }

        // =====================================
        // MOVIMIENTO DE PROFUNDIDAD
        // =====================================

        if (estadoActual == EstadoTutorial.MovimientoProfundidad)
        {
            if (diferenciaZ >= umbralMovimientoZ)
                adelanteDetectado = true;

            if (diferenciaZ <= -umbralMovimientoZ)
                atrasDetectado = true;

            if (adelanteDetectado && atrasDetectado)
            {
                estadoActual = EstadoTutorial.EsperandoDialogoFinal;

                MostrarMensaje(
                    "movimiento_profundidad",
                    "¡Excelente!"
                );

                MostrarMensaje(
                    "ejercicios_completados",
                    "¡Bien hecho! Ahora, intenta agarrar un objeto."
                );

                Debug.Log("Movimiento de profundidad completado.");
            }
        }
    }





// =====================================
// ROTACIÓN DE CÁMARA SEGÚN LA MANO
// =====================================

void ActualizarRotacionCamara(float x)
{
    if (camaraRotacion == null)
        return;

    // Convertir la posición horizontal de la mano
    // (0 a 1) en un ángulo de rotación.
    float anguloY = (x - 0.5f) * 2f * rotacionMaxima;

    // Crear la rotación objetivo a partir
    // de la rotación inicial de la cámara.
    Quaternion rotacionObjetivo =
        rotacionInicialCamara *
        Quaternion.Euler(0f, anguloY, 0f);

    // Suavizar la rotación.
    camaraRotacion.localRotation = Quaternion.Slerp(
        camaraRotacion.localRotation,
        rotacionObjetivo,
        suavizadoRotacion * Time.deltaTime
    );
}
    // =====================================
    // ACTUALIZACIÓN PRINCIPAL
    // =====================================

    void Update()
    {
        if (udpReceive == null)
            return;

        string data = udpReceive.dataLandmarks;

        if (string.IsNullOrEmpty(data))
            return;

        data = data.Trim('[', ']');

        string[] coordinates = data.Split(',');

        if (coordinates.Length < 5)
            return;

        float x, y0, y9;

        if (!float.TryParse(
            coordinates[0],
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out x))
            return;

        if (!float.TryParse(
            coordinates[1],
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out y0))
            return;

        if (!float.TryParse(
            coordinates[4],
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out y9))
            return;

        // =====================================
        // 1. NORMALIZAR COORDENADAS
        // =====================================

        x = 1f - x;

        float y = y9;

        ActualizarRotacionCamara(x);

        // =====================================
        // 2. VALIDAR CENTRO
        // =====================================

        DetectarPrimeraMano(x, y);

        // Actualizar el estado según los diálogos
        ActualizarEstadoDialogos();

        // =====================================
        // 3. RECONOCER GESTOS
        // =====================================

        if (manoCentrada && spriteRenderer != null)
        {
            ActualizarSprite();
        }

        // Actualizar nuevamente por si se acaba de reconocer
        // el último gesto y se ha iniciado un nuevo diálogo.
        ActualizarEstadoDialogos();

        // =====================================
        // 4. ZONA MUERTA
        // =====================================

        Vector2 entradaActual = new Vector2(x, y);

        if (!entradaInicializada)
        {
            ultimaEntrada = entradaActual;
            entradaInicializada = true;
        }
        else
        {
            float diferencia = Vector2.Distance(
                entradaActual,
                ultimaEntrada
            );

            if (diferencia >= zonaMuerta)
            {
                ultimaEntrada = entradaActual;
            }
        }

        x = ultimaEntrada.x;
        y = ultimaEntrada.y;

        // =====================================
        // 5. CONVERSIÓN A COORDENADAS UNITY
        // =====================================

        float centroCursorX = 2.5f;
        float centroCursorY = 2.5f;

        float x0 = centroCursorX +
            ((x - 0.5f) * 55f * sensibilidadX);

        float vertical0 = centroCursorY +
            ((0.5f - y) * 35f * sensibilidadY);

        // =====================================
        // 6. PROFUNDIDAD
        // =====================================

        float depth = Mathf.Clamp01(udpReceive.dataDepth);

        float profundidadMedia = 0.5f;

        float zCentro = (zLejano + zCercano) / 2f;

        float desplazamientoZ =
            (depth - profundidadMedia) * 10f;

        float z = Mathf.Clamp(
            zCentro + desplazamientoZ,
            zCercano,
            zLejano
        );

        // =====================================
        // 7. DETECCIÓN DEL EJERCICIO ACTUAL
        // =====================================

        DetectarMovimientos(x, y, depth);

        // =====================================
        // 8. POSICIÓN OBJETIVO DEL CURSOR
        // =====================================

        posicionObjetivo = new Vector3(
            x0,
            vertical0,
            z
        );

        Vector3 posicionSuavizada = Vector3.Lerp(
            transform.localPosition,
            posicionObjetivo,
            suavizado * Time.deltaTime
        );

        // =====================================
        // 9. LIMITAR PANTALLA
        // =====================================

        transform.localPosition = LimitarPantalla(
            posicionSuavizada
        );
    }

    // =====================================
    // LIMITAR POSICIÓN EN PANTALLA
    // =====================================

    Vector3 LimitarPantalla(Vector3 posicionLocal)
    {
        if (camara == null)
            return posicionLocal;

        Vector3 posicionMundo = transform.parent != null
            ? transform.parent.TransformPoint(posicionLocal)
            : posicionLocal;

        Vector3 viewport = camara.WorldToViewportPoint(
            posicionMundo
        );

        if (viewport.z <= 0f)
            return posicionLocal;

        viewport.x = Mathf.Clamp(
            viewport.x,
            margenPantalla,
            1f - margenPantalla
        );

        viewport.y = Mathf.Clamp(
            viewport.y,
            margenPantalla,
            1f - margenPantalla
        );

        Vector3 posicionLimitada =
            camara.ViewportToWorldPoint(viewport);

        if (transform.parent != null)
        {
            return transform.parent.InverseTransformPoint(
                posicionLimitada
            );
        }

        return posicionLimitada;
    }

    // =====================================
    // RECONOCIMIENTO DE GESTOS Y SPRITES
    // =====================================

    void ActualizarSprite()
    {
        if (!manoCentrada)
            return;

        int gesto = udpReceive.digit;

        // Solo registrar los gestos durante su etapa.
        if (estadoActual == EstadoTutorial.EsperandoGestos &&
            DialogoTerminado())
        {
            if (gesto == 0)
                gesto0Detectado = true;

            if (gesto == 5)
                gesto5Detectado = true;

            if (gesto0Detectado && gesto5Detectado)
            {
                estadoActual = EstadoTutorial.EsperandoDialogoGestos;

                MostrarMensaje(
                    "gestos_reconocidos",
                    "¡Muy bien! Ahora mueve tu mano de izquierda a derecha."
                );

                Debug.Log("Gestos 0 y 5 reconocidos.");
            }
        }

        // Actualizar el sprite aunque el tutorial esté mostrando diálogos.
        if (gesto == ultimoGesto)
            return;

        ultimoGesto = gesto;

        if (gesto == 0 && spriteGesto0 != null)
        {
            spriteRenderer.sprite = spriteGesto0;
        }
        else if (gesto == 5 && spriteGesto5 != null)
        {
            spriteRenderer.sprite = spriteGesto5;
        }
    }
}
